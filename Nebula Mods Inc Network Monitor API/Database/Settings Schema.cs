using System.ComponentModel.DataAnnotations;

namespace NebulaMods.Database
{
    public class SettingsSchema
    {
        [Key]
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
