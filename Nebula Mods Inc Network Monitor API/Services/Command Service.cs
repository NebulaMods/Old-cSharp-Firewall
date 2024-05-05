using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NebulaMods.Services
{
    public class CommandService
    {
        private readonly DatabaseService _database;
        //private readonly DDoSDetectionService _DDoS;
        public CommandService(DatabaseService database)
        {
            _database = database;
        }
        public async Task StartAsync()
        {
            Console.WriteLine("Please enter a command :)");

            while (true)
            {
                string Input;
                switch (Input = Console.ReadLine().ToLower())
                {
                    case "test":
                        Console.WriteLine(Utilities.CommandExecuter("speedtest", "", VerifyFile: false));
                        break;
                    case "select interface":
                        break;
                    case "monitor":
                        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                            Console.WriteLine("Live monitoring is only available on Linux, sorry.");
                        else
                        {
                            DDoSDetectionService.LiveStats = true;
                            switch (Console.ReadKey(true).Key)
                            {
                                case ConsoleKey.Escape:
                                    DDoSDetectionService.LiveStats = false;
                                    Console.WriteLine("Server Monitoring is now disabled.");
                                    break;
                            }
                        }
                        break;
                    case "read dump":
                    case "read pcap":
                        break;
                    case "dump pcap":
                        Console.WriteLine("");
                        Console.Write("Pcap Filename: ");
                        string Pcap = Console.ReadLine();
                        Console.WriteLine("Dump Options: Amount, Time, Manual");
                        switch (Console.ReadLine().ToLower())
                        {
                            case "amount":
                                DDoSDetectionService.DumpPackets(Pcap, long.Parse(Console.ReadLine()));
                                break;
                            case "time":
                                DDoSDetectionService.DumpPackets(Pcap, 0, DumpTime: short.Parse(Console.ReadLine()));
                                break;
                            default:
                                Console.WriteLine("");
                                //Utilities.DumpPackets(Pcap, 0);
                                return;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
