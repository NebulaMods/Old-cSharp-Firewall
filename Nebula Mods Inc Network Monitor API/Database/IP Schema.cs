using System.ComponentModel.DataAnnotations;
using System.Net;

namespace NebulaMods.Database
{
    public class IPSchema
    {
        [Key]
        public IPAddress IP { get; set; }

        public string Geolocation { get; set; }

        public string Server { get; set; }

        public string Description { get; set; }

        public string EdgeRules { get; set; }

        public string FlagLink { get; set; }
    }
}
