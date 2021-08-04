using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Coravel;
using Microsoft.Extensions.DependencyInjection;
using Vonage;
using Vonage.Request;

namespace FraudService
{
  public class Program
    {
        public static async Task Main(string[] args)
        {
            var provider = CreateHostBuilder(args).Build();
            provider.Services.UseScheduler(scheduler =>
            {
                scheduler
                    .Schedule<CheckPhoneNumberInvocable>()
                    .EveryTenSeconds()
                    .PreventOverlapping(nameof(CheckPhoneNumberInvocable));
            });
            await ConfigureTestPhoneNumbers(provider);
            provider.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = Credentials.FromApiKeyAndSecret(
                        "your_api_key",
                        "your_api_secret"
                    );
                    var vonageClient = new VonageClient(configuration);

                    services.AddSingleton(vonageClient);
                    services.AddTransient<MockEventBus>();
                    services.AddTransient<CheckPhoneNumberInvocable>();
                    services.AddScheduler();
                });

        private static async Task ConfigureTestPhoneNumbers(IHost provider)
        {
            var bus = provider.Services.GetRequiredService<MockEventBus>();
            await bus.SendTo(Constants.NumbersToValidateQueue, new MockEventBus.Event
            {
                Data = "15555555555" // Put your phone number here!
            });
        }
    }
}
