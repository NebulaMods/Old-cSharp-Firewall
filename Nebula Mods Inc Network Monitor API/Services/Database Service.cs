using Microsoft.EntityFrameworkCore;
using NebulaMods.Database;

namespace NebulaMods.Services
{
    public class DatabaseService : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite("Data Source=Network-Monitor.db").UseLazyLoadingProxies();

        public DbSet<LogsSchema.ErrorLogs> Errors { get; set; }
        public DbSet<LogsSchema.AttackLogs> AttackLogs { get; set; }
        public DbSet<IPSchema> IPs { get; set; }
        public DbSet<SettingsSchema> Settings { get; set; }
    }
}
