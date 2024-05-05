using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace NebulaMods.Database
{
    public class LogsSchema
    {
        public class ErrorLogs
        {
            [Key]
            public int ID { get; set; }

            public string Application { get; set; }

            public string Name { get; set; }

            public string Location { get; set; }

            public string Reason { get; set; }

            public DateTime ErrorTime { get; set; }
        }

        public class AttackLogs
        {
            [Key]
            public int ID { get; set; }

            public string Server { get; set; }

            public IPAddress IP { get; set; }

            public long InitialPPS { get; set; }

            public long InitialBPS { get; set; }

            public string Duration { get; set; }

            public long PeakPPS { get; set; }

            public long PeakBPS { get; set; }

            public long TotalPackets { get; set; }

            public long TotalBytes { get; set; }

            public long TotalUniqueIPs { get; set; }

            public DateTime DetectionTime { get; set; }
            
            public DateTime EndingTime { get; set; }

            public string PcapFile { get; set; }

            //public List<IPAddress> UniqueIPs { get; set; }
        }
    }
}
