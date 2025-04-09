using EmailService.Core.Infrastructure;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace EmailService.API.BackgroundServices
{
    /// <summary>
    /// 
    /// </summary>
    public class RabbitMQBackgroundService : BackgroundService
    {
        private readonly RabbitMQConnection _connection;

        public RabbitMQBackgroundService(RabbitMQConnection connection)
        {
            _connection = connection;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            IConnection connection = null;
            IChannel channel = null;
            string consumerTag = null;

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        connection = await _connection.GetConnectionAsync(stoppingToken);
                        channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

                        const string exchangeName = "mailExchange";
                        const string queueName = "mailQueue";
                        const string routingKey = "mail.send";

                        var queueArgs = new Dictionary<string, object?>
                {
                    { "x-queue-type", "stream" }
                };

                        await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct, durable: true, cancellationToken: stoppingToken);
                        await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, arguments: queueArgs, cancellationToken: stoppingToken);
                        await channel.QueueBindAsync(queueName, exchangeName, routingKey, cancellationToken: stoppingToken);

                        var consumer = new AsyncEventingBasicConsumer(channel);
                        consumer.ReceivedAsync += async (ch, ea) =>
                        {
                            try
                            {
                                var body = ea.Body.ToArray();
                                var message = Encoding.UTF8.GetString(body);

                                // Обрабатываем сообщения тут

                                await channel.BasicAckAsync(ea.DeliveryTag, false);
                            }
                            catch
                            {
                                await channel.BasicNackAsync(ea.DeliveryTag, false, true);
                            }
                        };

                        consumerTag = await channel.BasicConsumeAsync(queueName, false, consumer);

                        var connectionShutdownTcs = new TaskCompletionSource();
                        var cancellationRegistration = stoppingToken.Register(() => connectionShutdownTcs.TrySetResult());

                        connection.ConnectionShutdownAsync += (sender, args) =>
                        {
                            connectionShutdownTcs.TrySetResult();
                            return Task.CompletedTask;
                        };

                        await connectionShutdownTcs.Task;
                        cancellationRegistration.Dispose();
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"RabbitMQ error: {ex.Message}");
                        await Task.Delay(5000, stoppingToken);
                    }
                    finally
                    {
                        if (channel != null && consumerTag != null)
                        {
                            try { await channel.BasicCancelAsync(consumerTag); } catch { }
                        }
                        channel?.CloseAsync();
                        channel?.Dispose();
                        connection?.CloseAsync();
                        connection?.Dispose();
                    }
                }
            }
            finally
            {
                channel?.Dispose();
                connection?.Dispose();
            }
        }
    }

}
