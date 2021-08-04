using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace FraudService
{
    public class MockEventBus
    {
        public class Event
        {
            public string Data { get; set; }
        }

        public async Task SendTo(string queueName, Event @event)
        {
            await UsingStoredEvents(queueName, events =>
            {
                events.Add(@event);
            });
        }

        public async Task<List<Event>> RemoveFrom(string queueName)
        {
            var next = new List<Event>(0);

            await UsingStoredEvents(queueName, events =>
            {
                if (events.Any())
                {
                    next.AddRange(events);
                    events.Clear();
                }
            });

            return next;
        }

        private async Task UsingStoredEvents(string queueName, Action<List<Event>> action)
        {
            await using var fileRead = File.Open($"./mock_event_bus_{queueName}.json", FileMode.OpenOrCreate);
            var events = fileRead.Length > 0
                ? await JsonSerializer.DeserializeAsync<List<Event>>(fileRead)
                : new List<Event>();
            fileRead.Close();

            action(events);

            await using var fileWrite = File.Open($"./mock_event_bus_{queueName}.json", FileMode.Truncate);
            await JsonSerializer.SerializeAsync(fileWrite, events);
            fileWrite.Close();
        }
    }
}
