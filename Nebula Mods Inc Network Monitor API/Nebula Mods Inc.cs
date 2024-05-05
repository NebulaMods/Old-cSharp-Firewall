using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NebulaMods.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NebulaMods
{
    public class NebulaModsApplication
    {
        public NebulaModsApplication(string[] CommandLine)
        {

        }

        public static async Task RunAsync(string[] Arguments) => await new NebulaModsApplication(Arguments).RunAsync();

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<DatabaseService>()
            .AddSingleton(new Random())
            .AddSingleton<DDoSDetectionService>()
            .AddSingleton<CommandService>();
        }

        public async Task RunAsync()
        {
            try
            {
                var services = new ServiceCollection();
                ConfigureServices(services);
                var provider = services.BuildServiceProvider();
                provider.GetRequiredService<DatabaseService>().Database.Migrate();
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    provider.GetRequiredService<DDoSDetectionService>().Start();
                await provider.GetRequiredService<CommandService>().StartAsync();
                await Task.Delay(Timeout.Infinite);
            }
            catch(Exception error)
            {
                Utilities.ErrorDetection(error);
            }
        }
    }
}
