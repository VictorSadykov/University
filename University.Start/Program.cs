using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.ComponentModel;
using Telegram.Bot;
using University.Bot;

namespace University.Start
{
    class Program
    {
        public static async Task Main()
        {
            var host = new HostBuilder().ConfigureServices((hostContext, services) =>
                 ConfigureServices(services))
                    .UseConsoleLifetime()
                    .Build();

            Console.WriteLine("Start");
            await host.RunAsync();
            Console.WriteLine("Stop");

            static void ConfigureServices(IServiceCollection services)
            {
                services.AddSingleton<ITelegramBotClient>(provider => new TelegramBotClient(
                    "6293207321:AAGNx09cTsjC8r3Jvd1LsdsoyA75prhGa9o" // token
                    ));

                services.AddHostedService<BotService>();
            }
        }
    }
}