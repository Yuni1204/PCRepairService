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
    public class ServiceMessenger : BackgroundService, IMessenger 
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
            _factory = new ConnectionFactory { HostName = "localhost" };
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


            
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            HandleMessages();
            return Task.CompletedTask;
        }
        public void HandleMessages(string exchange = "ServiceOrderReply")
        {
            _logger.LogInformation($"HandleMessages started at {DateTimeOffset.Now}");
            _channel.QueueBind(queue: queueName,
                              exchange: exchange,
                              routingKey: string.Empty);

            _consumer.Received += async (model, ea) =>
            {
                byte[]? headerMessType;
                long sagaId;
                string messageType;
                if (ea.BasicProperties.IsHeadersPresent())
                {
                    headerMessType = (ea.BasicProperties.Headers.ContainsKey("MessageType")) ? (byte[])ea.BasicProperties.Headers["MessageType"] : Encoding.UTF8.GetBytes("null");
                    messageType = Encoding.UTF8.GetString(headerMessType);
                    sagaId = (ea.BasicProperties.Headers.ContainsKey("SagaId")) ? (long)ea.BasicProperties.Headers["SagaId"] : -1;
                }
                else
                { //is not allowed to happen, every message should have a type
                    throw new Exception();
                }
                //    //get MessageType
                //    var header = (byte[])ea.BasicProperties.Headers["MessageType"];
                //messageType = Encoding.UTF8.GetString(header);


                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                if (messageType == "Reply")
                {
                    _logger.LogInformation($" [x] Received: {message}");
                }
                else if(messageType == "AppointmentDatesConfirmed")
                {
                    //var messageobj = JsonSerializer.Deserialize<Message>(message);
                    //var messageobj = JsonSerializer.Deserialize<Message>(message);
                    //if(messageobj != null && sagaId != -1) 
                    if(message != null && sagaId != -1) 
                    {
                        _logger.LogInformation($"ending ServiceOrderSaga for sagaID {sagaId} at {DateTimeOffset.Now}");
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var sagaHandler = scope.ServiceProvider.GetService<ISagaHandler>();
                            if( sagaHandler != null )
                            {
                                await sagaHandler.EndServiceOrderSagaAsync(sagaId);
                                _logger.LogInformation($"ENDSERVICEORDERSAGAASYNC executed at {DateTimeOffset.Now} ");
                            }
                            else
                            {
                                _logger.LogError("Creating Scoped SagaHandler from serviceScopeFactory returned null!");
                            }
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"messageType AppointmentDatesConfirmed body was null or no sagaid");
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
            _channel.BasicPublish(exchange: messageobj.exchange,
                                 routingKey: string.Empty,
                                 basicProperties: _props,
                                 body: body);
            Console.WriteLine($" [x] Sent {content}");
        }

    }
}
