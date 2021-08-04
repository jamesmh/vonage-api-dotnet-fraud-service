using System.Threading.Tasks;
using Coravel.Invocable;
using Vonage;
using Vonage.NumberInsights;

namespace FraudService
{
    public class CheckPhoneNumberInvocable : IInvocable
    {
        private readonly MockEventBus _bus;
        private readonly VonageClient _client;

        public CheckPhoneNumberInvocable(MockEventBus bus, VonageClient client)
        {
            _bus = bus;
            _client = client;
        }

        public async Task Invoke()
        {
            var events = await _bus.RemoveFrom(Constants.NumbersToValidateQueue);

            foreach (var @event in events)
            {
                var phoneNumber = @event.Data;

                var request = new StandardNumberInsightRequest()
                {
                    Country = "",
                    Number = phoneNumber
                };
                var response = await _client.NumberInsightClient.GetNumberInsightStandardAsync(request);

                var carrierName = response.CurrentCarrier.Name ?? "Carrier not available";
                var country = response.CountryName ?? "Country not available";

                await _bus.SendTo(Constants.ValidatedNumbersQueue, new MockEventBus.Event
                {
                    Data = $"{phoneNumber}: {country}: {carrierName}"
                });
            }
        }
    }
}