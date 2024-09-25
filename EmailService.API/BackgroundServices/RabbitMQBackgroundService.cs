using EmailService.Core.Models;
using EmailService_Core.Abstractions;
using Microsoft.AspNetCore.Connections;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace EmailService.API.BackgroundServices
{
    /// <summary>
    /// BackgroundService - Consumer
    /// </summary>
    public class RabbitMQBackgroundService : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        //private readonly IMailService _mailService;
        private readonly IServiceProvider _serviceProvider;

        public RabbitMQBackgroundService(
            //IMailService mailService, 
            IServiceProvider serviceProvider
            )
        {
            // Настройка подключения к RabbitMQ
            var factory = new ConnectionFactory() { HostName = "host.docker.internal", Port = 5672, UserName = "admin", Password = "admin" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            var exchangeName = "emailExchange";
            _channel.ExchangeDeclare(exchangeName, type: ExchangeType.Topic, durable: true);

            // Декларация очереди (если её нет)
            _channel.QueueDeclare(queue: "emailQueue",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            _channel.QueueBind(queue: "emailQueue", exchange: exchangeName, routingKey: "email.send");
            _channel.QueueBind(queue: "emailQueue", exchange: exchangeName, routingKey: "email.send.files");

            //_mailService = mailService;
            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            // Обработка сообщения
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;


                using(var scope = _serviceProvider.CreateScope()) 
                {
                    IMailService _mailService = scope
                    .ServiceProvider.GetRequiredService<IMailService>();

                    if (routingKey == "email.send")
                    {
                        SendMessageDto? emailModel = JsonConvert
                        .DeserializeObject<SendMessageDto>(message) ??
                            throw new ArgumentNullException(nameof(emailModel), "Контент сообщения оказался пустым");
                        // Логика отправки email без файлов
                        await _mailService.SendEmailAsync(emailModel, default);
                        Console.WriteLine("Ъуй");
                    }

                    else if (routingKey == "email.send.files")
                    {
                        SendMailWithFilesDto? emailData = JsonConvert
                        .DeserializeObject<SendMailWithFilesDto>(message) ??
                            throw new ArgumentNullException(nameof(emailData), "Контент сообщения с файлами оказался пустым");
                        // Логика отправки email с файлами
                        // TODO отправка файлов
                        //await _mailService.SendEmailWithFilesAsync(emailData.Files, emailData.Message, default);
                        Console.WriteLine("!!");
                    }
                }
                
            };

            _channel.BasicConsume(queue: "emailQueue",
                                 autoAck: true,
                                 consumer: consumer);

            return Task.CompletedTask;  // Фоновая задача завершена
        }


        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }

}
