using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AspNetCore
{
    class QueueBackgroundService : BackgroundService
    {
        private static readonly ActivitySource Activity = new(nameof(QueueBackgroundService));
        private static readonly TextMapPropagator Propagator = new TraceContextPropagator();
        private readonly ICoreMetrics metrics;

        IConnectionFactory factory;

        IConnection connection;
        IModel channel;

        public QueueBackgroundService(ICoreMetrics metrics)
        {
            this.metrics = metrics;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var activity = Activity.StartActivity("QueueService");

            await Task.Delay(TimeSpan.FromSeconds(10));
            this.factory = new ConnectionFactory() { HostName = "rabbitmq", DispatchConsumersAsync = true };
            this.connection = factory.CreateConnection();
            this.channel = connection.CreateModel();
            channel.QueueDeclare(queue: "hello",
                                durable: false,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                await ProcessMessage(ea);
            };

            channel.BasicConsume(queue: "hello",
                                 autoAck: true,
                                 consumer: consumer);

            await Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            this.channel?.Close();
            this.channel?.Dispose();
            this.connection?.Close();
            this.connection?.Dispose();

            await base.StopAsync(cancellationToken);
        }

        private static async Task ProcessMessage(BasicDeliverEventArgs ea)
        {
            try
            {
                //Extract the activity and set it into the current one
                var parentContext = Propagator.Extract(default, ea.BasicProperties, ExtractTraceContextFromBasicProperties);
                Baggage.Current = parentContext.Baggage;

                //Start a new Activity
                using var activity = Activity.StartActivity("Process Message", ActivityKind.Consumer, parentContext.ActivityContext);

                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                //Add Tags to the Activity
                AddActivityTags(activity);

                activity?.SetTag("Result", message);

                // simulate work
                await Task.Delay(TimeSpan.FromSeconds(4));
            }
            catch
            {
                throw;
            }
        }

        //Extract the Activity from the message header
        private static IEnumerable<string> ExtractTraceContextFromBasicProperties(IBasicProperties props, string key)
        {
            try
            {
                if (props.Headers.TryGetValue(key, out var value))
                {
                    var bytes = value as byte[];
                    return new[] { Encoding.UTF8.GetString(bytes) };
                }
            }
            catch
            {
                throw;
            }

            return Enumerable.Empty<string>();
        }

        //Add Tags to the Activity
        private static void AddActivityTags(Activity activity)
        {
            activity?.SetTag("messaging.system", "rabbitmq");
            activity?.SetTag("messaging.destination_kind", "queue");
            activity?.SetTag("messaging.rabbitmq.queue", "sample");
        }
    }
}