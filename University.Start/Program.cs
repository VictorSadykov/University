using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.ComponentModel;
using Telegram.Bot;
using University.BLL;
using University.Bot;
using University.DLL.Sqlite;
using University.DLL.Sqlite.Repositories.Abstract;
using University.DLL.Sqlite.Repositories.Real;

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
                services.AddSingleton<UniversityDbContext>();
                services.AddSingleton<IGroupRepository, GroupRepository>();
                services.AddSingleton<IExamRepository, ExamRepository>();
                services.AddSingleton<ILessonRepository, LessonRepository>();

                services.AddTransient<ChatDataController>();
                services.AddTransient<ScheduleLoader>();
                services.AddTransient<InfoController>();


                services.AddSingleton<ITelegramBotClient>(provider => new TelegramBotClient(
                    "6293207321:AAGNx09cTsjC8r3Jvd1LsdsoyA75prhGa9o" // token
                    ));

                services.AddHostedService<BotService>();
            }
        }
    }
}