using Discord;
using Discord.Webhook;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace NebulaMods.Services
{
    public class DDoSDetectionService
    {
        private readonly DatabaseService _database;
        private static DateTime _specialtimer;
        private readonly DiscordWebhookClient _webHookClient = new(WebhookUrl());
        private static bool _detection = false, _underAttack = false, DumpingPackets = false;
        private ulong _webhookMessageID = 0;
        public static bool LiveStats = false;
        private static long BPS = 0, PPS = 0, PeakBPS = 0, PeakPPS = 0;
        private static string _interfaceName;
        private static int _interfaceID;

        public DDoSDetectionService(DatabaseService database)
        {
            _database = database;
        }

        public void Start()
        {
            if (!_detection)
            {
                if (!_database.Settings.Any(x => x.Name == "interface-name") || !_database.Settings.Any(x => x.Name == "interface-id"))
                    InterfaceSelector();
                else
                {
                    var intid = _database.Settings.FirstOrDefault(x => x.Name == "interface-id");
                    _interfaceID = Convert.ToInt32(intid.Value);
                    var intname = _database.Settings.FirstOrDefault(x => x.Name == "interface-name");
                    _interfaceName = intname.Value;
                }
                if (!_database.Settings.Any(x => x.Name == "discord-webhook-url"))
                {
                    Console.WriteLine("Please enter a webhook url:");
                    _database.Settings.Add(new Database.SettingsSchema
                    {
                        Name = "discord-webhook-url",
                        Value = Console.ReadLine()
                    });
                }
                _detection = true;
                new Thread(() => { NetworkStatistics(); }).Start();
                new Thread(() => { Detector(); }).Start();
            }
            else
                Console.WriteLine("DDoS Detection is already enabled.");
        }
        public void Stop()
        {
            if (_detection)
                _detection = false;
            else
                Console.WriteLine("DDoS Detection is no longer enabled.");
        }

        public static bool UnderAttack()
        {
            return _underAttack;
        }
        public static void NetworkStatistics()
        {
            while (true)
            {
                try
                {
                    long OldPPS = LocalPackets();
                    long OldBPS = LocalBytes();
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    PPS = LocalPackets() - OldPPS;
                    BPS = LocalBytes() - OldBPS;

                    if (UnderAttack())
                    {
                        if (PPS > PeakPPS)
                            PeakPPS = PPS;
                        if (BPS > PeakBPS)
                            PeakBPS = BPS;
                    }

                    if (LiveStats)
                    {
                        Console.Clear();
                        if (UnderAttack())
                            Console.WriteLine($"{DateTime.Now:h:mm:ss tt}\nCurrently under a DDoS attack\n@\nPackets Per Second: {PPS.LongConverter()}\n{BPS.ThroughputDetector()} Per Second: {BPS.ThroughputConverter()}");
                        else
                            Console.WriteLine($"{DateTime.Now:h:mm:ss tt}\n@\nPackets Per Second: {PPS.LongConverter()}\n{BPS.ThroughputDetector()} Per Second: {BPS.ThroughputConverter()}");
                    }
                }
                catch (Exception error)
                {
                    Utilities.ErrorDetection(error);
                }
            }
        }

        public static void DumpPackets(string PcapFileName, long DumpCount = 0, short DumpTime = 0, bool DisplayStats = false)
        {
            try
            {
                //get interface id
                var entry = new DatabaseService().Settings.FirstOrDefault(x => x.Name == "interface-id");
                var device = LibPcapLiveDeviceList.Instance[Convert.ToInt32(entry.Value)];

                // Open the device for capturing
                device.Open(mode: DeviceModes.Promiscuous | DeviceModes.DataTransferUdp | DeviceModes.NoCaptureLocal);

                // open the output file
                var captureFileWriter = new CaptureFileWriterDevice(PcapFileName);
                captureFileWriter.Open(device);

                PacketCapture packet;
                if (DumpTime != 0)
                {
                    new Task(() =>
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(DumpTime));
                        DumpTime = 0;
                    }).Start();
                    while (DumpTime != 0)
                    {
                        device.GetNextPacket(out packet);
                        if (packet.Data != null)
                            captureFileWriter.Write(packet.Data);
                    }
                }
                else if (DumpCount != 0)
                {
                    int i = 0;
                    while (i < DumpCount)
                    {
                        device.GetNextPacket(out packet);
                        if (packet.Data != null)
                        {
                            captureFileWriter.Write(packet.Data);
                            i++;
                        }
                    }
                }
                else
                {
                    while (DumpingPackets)
                    {
                        device.GetNextPacket(out packet);
                        if (packet.Data != null)
                            captureFileWriter.Write(packet.Data);
                    }
                }

                if (DisplayStats)
                    Console.WriteLine(device.Statistics.ToString());
                device.Close();
                captureFileWriter.Close();
            }
            catch (Exception e)
            {
                Utilities.ErrorDetection(e);
            }
        }
        private void Detector(long bpsThresold = 13000000, long ppsThresold = 50000, bool dumpInspection = false)
        {
            short durationVerification = 0;
            short durationVerify = 0;
            while (_detection)
            {
                try
                {
                    switch (BPS > bpsThresold || PPS > ppsThresold)
                    {
                        case true:
                            if (!_underAttack)//check if under attack
                            {
                                //attack detected?
                                switch (durationVerification)
                                {
                                    case 10:
                                        _underAttack = true;
                                        
                                        new Task(() => { Mitigation(); durationVerification = 0; }).Start();
                                        //if ((DateTime.Now - _specialtimer).TotalSeconds > 10)
                                        //{
                                        //    _underAttack = true;
                                        //    new Task(() => { Mitigation(); }).Start();
                                        //    durationVerify = 0;
                                        //}
                                        break;
                                    default:
                                        if (durationVerification < 10)
                                            durationVerification++;
                                        break;
                                }
                            }
                            break;
                        default:
                            if (_underAttack)//check if under attack
                            {
                                switch (durationVerify)
                                {
                                    case 15:
                                        //attack no longer detected?
                                        _underAttack = false;
                                        new Task(() => { ExitMitigation(); durationVerify = 0; }).Start();
                                        
                                        break;
                                    default:
                                        if (durationVerify < 15)
                                            durationVerify++;
                                        break;
                                }
                            }
                            break;
                    }
                    Thread.Sleep(30);//sleep for 30ms instead of 1min for more accurate detection
                }
                catch (Exception error)
                {
                    Utilities.ErrorDetection(error);
                    return;
                }
            }
        }

        private IPAddress isDDoSAttack(short DumpAmount)
        {
            try
            {
                //dump packets derp
                DumpPackets("/var/nebula-mods-inc/packet-dumps/SampleDump.pcap", DumpAmount);
                //read dump
                IPAddress Result = Sampleshit("/var/nebula-mods-inc/packet-dumps/SampleDump.pcap");
                //delete dump
                File.Delete("/var/nebula-mods-inc/packet-dumps/SampleDump.pcap");
                return Result;
            }
            catch(Exception error)
            {
                Utilities.ErrorDetection(error);
                return null;
            }
        }


        /// <summary>
        /// make attack detection last 5 mins no matter what & reset time each time pps/bps goes over limit for ~45secs
        /// </summary>


        private void Mitigation()
        {
            try
            {
                //log initial attack time
                IPAddress ipattacked;
                if ((ipattacked = isDDoSAttack(100)) != null)
                {
                    _specialtimer = DateTime.Now;
                    var ipentry = _database.IPs.FirstOrDefault(x => x.IP == ipattacked);
                    //log everything to database
                    _database.AttackLogs.Add(new Database.LogsSchema.AttackLogs()
                    {
                        Server = ipentry.Server,
                        IP = ipattacked,
                        DetectionTime = _specialtimer,
                        InitialPPS = PPS,
                        InitialBPS = BPS,
                        PeakPPS = 0,
                        PeakBPS = 0,
                        TotalBytes = LocalBytes(),
                        TotalPackets = LocalPackets(),
                        Duration = "N/A",
                        PcapFile = $"/var/nebula-mods-inc/packet-dumps/{_specialtimer:HH:mm:ss-D-dd-MM-yyyy}.pcap",
                        TotalUniqueIPs = 0,
                        EndingTime = DateTime.MinValue
                    });
                    _database.SaveChanges();

                    PostWebHook(DetectionNotify(_database.AttackLogs.FirstOrDefault(x => x.DetectionTime == _specialtimer), true));

                    //conntrack filtering
                    Task.Run(() => { IPSet(); Conntrack(ipattacked); });
                    //dump packets again
                    DumpPackets($"/var/nebula-mods-inc/packet-dumps/{_specialtimer:HH:mm:ss-D-dd-MM-yyyy}.pcap", 50000);
                }
            }
            catch (Exception error)
            {
                Utilities.ErrorDetection(error);
            }
        }

        private void ExitMitigation()
        {
            try
            {

                var attackentry = _database.AttackLogs.FirstOrDefault(x => x.DetectionTime == _specialtimer);

                //log ending time
                _specialtimer = DateTime.Now;
                attackentry.EndingTime = _specialtimer;

                //total bps/pps
                attackentry.TotalBytes = LocalBytes() - attackentry.TotalBytes;
                attackentry.TotalPackets = LocalPackets() - attackentry.TotalPackets;

                //read pcap for info
                attackentry.Duration = $"{Math.Round((attackentry.EndingTime - attackentry.DetectionTime).TotalSeconds, 2)} Seconds";
                attackentry.PeakBPS = PeakBPS;
                attackentry.PeakPPS = PeakPPS;
                //save database
                _database.SaveChanges();

                PostWebHook(DetectionNotify(attackentry, false));
                PeakPPS = 0; PeakBPS = 0;

                _specialtimer = DateTime.Now;
            }
            catch(Exception error)
            {
                Utilities.ErrorDetection(error);
            }
        }

        /// <summary>
        /// track all open port connections for the ip
        /// </summary>
        /// <param name="ipattacked"></param>
        private void Conntrack(IPAddress ipattacked)
        {
            var result = Utilities.CommandExecuter("conntrack", $"-L | grep {ipattacked}");
            File.WriteAllText($"/var/nebula-mods-inc/conntrack-logs/{_specialtimer:HH:mm:ss-D-dd-MM-yyyy}.txt", result);

        }

        /// <summary>
        /// check to see if the ratelimits r being hit
        /// </summary>
        private void IPSet()
        {

        }

        private IPAddress Sampleshit(string PcapFileName)
        {
            try
            {
                var database = new Services.DatabaseService();
                List<IPAddress> DestinationIPs = new();
                ICaptureDevice device;
                device = new CaptureFileReaderDevice(PcapFileName);
                device.Open();
                //50 packets to check
                GetPacketStatus retval;
                while ((retval = device.GetNextPacket(out PacketCapture capture)) == GetPacketStatus.PacketRead)
                {
                    var IPPacket = Packet.ParsePacket(capture.GetPacket().LinkLayerType, capture.GetPacket().Data).Extract<IPPacket>();
                    if (IPPacket != null)
                    {
                        var dstentry = database.IPs.FirstOrDefault(x => x.IP == IPPacket.DestinationAddress);
                        var srcentry = database.IPs.FirstOrDefault(x => x.IP == IPPacket.SourceAddress);
                        if (srcentry == null)
                            if (dstentry != null || dstentry.IP != IPPacket.DestinationAddress)
                                DestinationIPs.Add(IPPacket.DestinationAddress);
                    }
                    //
                }
                var AttackedIP = DestinationIPs.GroupBy(str => str)
                .OrderByDescending(x => x.Count())
                .Select(x => new { IP = x.Key, Count = x.Count() })
                .First();
                return AttackedIP.IP;
            }
            catch (Exception error)
            {
                Utilities.ErrorDetection(error);
                return null;
            }
        }

        private static long LocalBytes()
        {
            return long.Parse(File.ReadAllText($"/sys/class/net/{_interfaceName}/statistics/rx_bytes"));
        }
        private static long LocalPackets()
        {
            return long.Parse(File.ReadAllText($"/sys/class/net/{_interfaceName}/statistics/rx_packets"));
        }
        private static long LocalDroppedPackets()
        {
            return long.Parse(File.ReadAllText($"/sys/class/net/{_interfaceName}/statistics/rx_dropped"));
        }

        private void InterfaceSelector()
        {
            try
            {
                // Retrieve the device list
                var devices = LibPcapLiveDeviceList.Instance;
                int i = 0;

                // If no devices were found print an error
                if (devices.Count < 1)
                {
                    Console.WriteLine("No devices were found on this machine");
                    return;
                }

                Console.WriteLine();
                Console.WriteLine("Please select an interface from the list\n");

                // Print out the devices
                foreach (var dev in devices)
                {
                    /* Description */
                    if (string.IsNullOrWhiteSpace(dev.Description))
                        Console.WriteLine($"{i}) {dev.Name}");
                    else
                        Console.WriteLine($"{i}) {dev.Description}");
                    i++;
                }

                Console.WriteLine();
                Console.Write("Interface: ");
                i = int.Parse(Console.ReadLine());
                var device = devices[i];
                _interfaceID = i;
                _interfaceName = device.Name;
                Console.WriteLine("Interface selected.");
                _database.Settings.Add(new Database.SettingsSchema
                {
                    Name = "interface-name",
                    Value = _interfaceName
                });
                _database.Settings.Add(new Database.SettingsSchema
                {
                    Name = "interface-id",
                    Value = _interfaceID.ToString()
                });
                _database.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void AddIP()
        {

        }

        private Embed DetectionNotify(Database.LogsSchema.AttackLogs attackentry, bool isUnderAttack)
        {
            var ipentry = _database.IPs.FirstOrDefault(x => x.IP == attackentry.IP);
            Embed RichEmbed = isUnderAttack switch
            {
                true => new EmbedBuilder
                {
                    Title = "DDoS Attack Alert",
                    Description = "Our system has detected a possible DDoS attack, and is now attempting to mitigate the attack.",
                    Author = new EmbedAuthorBuilder().WithName($"Nebula Mods Inc. | Network Monitor Beta V{Assembly.GetExecutingAssembly().GetName().Version}").WithUrl("https://nebulamods.ca").WithIconUrl("https://nebulamods.ca/content/media/images/Home.png"),
                    Url = "https://nebulamods.ca",
                    Footer = new EmbedFooterBuilder().WithText($"{attackentry.DetectionTime:h:mm:ss tt MMMM dd/yyyy} ADT").WithIconUrl("https://nebulamods.ca/content/media/images/ddos-alert.png"),//need new picture
                    ThumbnailUrl = ipentry.FlagLink,
                    Color = Utilities.RandomDiscordColour(),
                    Fields = new List<EmbedFieldBuilder>()
                    {
                        new EmbedFieldBuilder().WithName("Server").WithValue(attackentry.Server).WithIsInline(true),//server name/description
                        new EmbedFieldBuilder().WithName("IP Address").WithValue(attackentry.IP.MaskIPAddress()).WithIsInline(true),

                        new EmbedFieldBuilder().WithName($"Initial {attackentry.InitialBPS.ThroughputDetector()}").WithValue(attackentry.InitialBPS.ThroughputConverter()).WithIsInline(true),
                        new EmbedFieldBuilder().WithName("Initial Packets\nPer Second").WithValue(attackentry.InitialPPS.LongConverter()).WithIsInline(true),
                    }
                }.Build(),
                _ => new EmbedBuilder
                {
                    Title = "DDoS Attack Alert",
                    Description = "Our system no longer detects a DDoS attack, and is no longer in mitigation.",
                    Author = new EmbedAuthorBuilder().WithName($"Nebula Mods Inc. | Network Monitor Beta V{Assembly.GetExecutingAssembly().GetName().Version}").WithUrl("https://nebulamods.ca").WithIconUrl("https://nebulamods.ca/content/media/images/Home.png"),
                    Url = "https://nebulamods.ca",
                    Footer = new EmbedFooterBuilder().WithText($"{attackentry.EndingTime:h:mm:ss tt MMMM dd/yyyy} ADT").WithIconUrl("https://nebulamods.ca/content/media/images/protection.png"),//need new picture
                    ThumbnailUrl = ipentry.FlagLink,
                    Color = Utilities.RandomDiscordColour(),
                    Fields = new List<EmbedFieldBuilder>()
                    {
                        new EmbedFieldBuilder().WithName("Server").WithValue(attackentry.Server).WithIsInline(true),//server name/description
                        new EmbedFieldBuilder().WithName("IP Address").WithValue(attackentry.IP.MaskIPAddress()).WithIsInline(true),
                        new EmbedFieldBuilder().WithName("Duration").WithValue(attackentry.Duration).WithIsInline(true),

                        new EmbedFieldBuilder().WithName($"Peak {attackentry.PeakBPS.ThroughputDetector()}").WithValue(attackentry.PeakBPS.ThroughputConverter()).WithIsInline(true),
                        new EmbedFieldBuilder().WithName("Peak Packets\nPer Second").WithValue(attackentry.PeakPPS.LongConverter()).WithIsInline(true),

                        new EmbedFieldBuilder().WithName($"Total {attackentry.TotalBytes.SizeDetector()}").WithValue(attackentry.TotalBytes.SizeConverter()).WithIsInline(true),
                        new EmbedFieldBuilder().WithName("Total Packets").WithValue(attackentry.TotalPackets.LongConverter()).WithIsInline(true),
                    }
                }.Build()
            };
            return RichEmbed;
        }
        private static string WebhookUrl()
        {
            Database.SettingsSchema entry;
            return (entry = new DatabaseService().Settings.FirstOrDefault(x => x.Name == "discord-webhook-url")).Name == "discord-webhook-url" ? entry.Value : "error: no webhook url";
        }
        private bool PostWebHook(Embed embed, short attempts = 3)
        {
            _webhookMessageID = _webHookClient.SendMessageAsync(embeds: new[] { embed }).Result;
            //for (int i = 0; i < attempts; i++)
            //{
            //    try
            //    {
            //        switch (_webhookMessageID)
            //        {
            //            case 0:
            //                _webhookMessageID = _webHookClient.SendMessageAsync(embeds: new[] { embed }).Result;
            //                if (_webhookMessageID != 0)
            //                    return true;
            //                break;
            //            default:
            //                _webHookClient.ModifyMessageAsync(_webhookMessageID, x => x.Embeds = new[] { embed });
            //                if (_webhookMessageID != 0)
            //                {
            //                    _webhookMessageID = 0;
            //                    return true;
            //                }
            //                break;
            //        }
            //    }
            //    catch { continue; }
            //}
            //_webhookMessageID = 0;
            return true;
        }
    }
}
