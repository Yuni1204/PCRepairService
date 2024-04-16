using MessengerLibrary;
using Microsoft.Extensions.DependencyInjection;
using PCRepairService.Interfaces;
using PCRepairService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace PCRepairService
{
    public class ServiceMessenger : IMessenger
    {
        //rabbitmq
        protected readonly ConnectionFactory _factory;
        protected readonly IConnection _connection;
        protected readonly IModel _channel;
        protected readonly IBasicProperties _props;
        protected readonly EventingBasicConsumer _consumer;
        protected readonly string queueName;
        private IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger _logger;

        public ServiceMessenger(ILogger<ServiceMessenger> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _factory = new ConnectionFactory { HostName = "rabbitmq", Port = 5672 };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
            _props = _channel.CreateBasicProperties();
            _channel.ExchangeDeclare(exchange: "ServiceOrders", type: ExchangeType.Fanout);
            _channel.ExchangeDeclare(exchange: "ServiceOrderReply", type: ExchangeType.Fanout);
            _consumer = new EventingBasicConsumer(_channel);
            queueName = _channel.QueueDeclare().QueueName;

            _serviceScopeFactory = serviceScopeFactory;
            //maybe factory für sagaHandler weil messenger ist singleton und sagahandler sollte scoped sein
            _logger = logger;


            HandleMessages();
        }
        public void HandleMessages(string exchange = "ServiceOrderReply")
        {
            _channel.QueueBind(queue: queueName,
                              exchange: exchange,
                              routingKey: string.Empty);

            _consumer.Received += async (model, ea) =>
            {
                var header = (byte[])ea.BasicProperties.Headers["MessageType"];
                var messageType = Encoding.UTF8.GetString(header);
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                if (messageType == "Reply")
                {
                    _logger.LogInformation($" [x] Received: {message}");
                }
                else if(messageType == "AppointmentDatesConfirmed")
                {
                    var messageobj = JsonSerializer.Deserialize<Message>(message);
                    if(messageobj != null && messageobj.SagaId != null) 
                    {
                        _logger.LogInformation($"ending ServiceOrderSaga for {messageobj.SagaId}");
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var sagaHandler = scope.ServiceProvider.GetService<SagaHandler>();
                            if( sagaHandler != null )
                            {
                                await sagaHandler.EndServiceOrderSagaAsync((long)messageobj.SagaId);
                            }
                            else
                            {
                                _logger.LogError("Creating Scoped SagaHandler from serviceScopeFactory returned null!");
                            }
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"messageType AppointmentDatesConfirmed body was null");
                    }
                }
                else
                {
                    _logger.LogInformation($" [x] unknown message Received: {message}");
                }
            };
            _channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: _consumer);
        }

        public async Task SendMessageAsync(Message messageobj)
        {
            if (messageobj.messageType == null) messageobj.messageType = "null";
            _props.Headers = new Dictionary<string, object>
            {
                { "MessageType", messageobj.messageType }
            };


            string content = JsonSerializer.Serialize(messageobj.content);
            //var message = GetMessage(jsonstring);
            var body = Encoding.UTF8.GetBytes(content);
            //Thread.Sleep(100); //simulate processing delay
            _channel.BasicPublish(exchange: messageobj.exchange,
                                 routingKey: string.Empty,
                                 basicProperties: _props,
                                 body: body);
            Console.WriteLine($" [x] Sent {content}");
            await Task.CompletedTask;
        }


        public void SendMessage(Message messageobj)
        {
            if (messageobj.messageType == null) messageobj.messageType = "null";
            _props.Headers = new Dictionary<string, object>
            {
                { "MessageType", messageobj.messageType }
            };


            string content = JsonSerializer.Serialize(messageobj.content);
            //var message = GetMessage(jsonstring);
            var body = Encoding.UTF8.GetBytes(content);
            //Thread.Sleep(100); //simulate processing delay
            _channel.BasicPublish(exchange: messageobj.exchange,
                                 routingKey: string.Empty,
                                 basicProperties: _props,
                                 body: body);
            Console.WriteLine($" [x] Sent {content}");
        }

    }
}
