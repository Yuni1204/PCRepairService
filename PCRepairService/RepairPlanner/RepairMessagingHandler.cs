using MessengerLibrary;
using Microsoft.AspNetCore.Connections;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace RepairPlanner
{
    public class RepairMessagingHandler : BackgroundService, IMessenger
    {
        public ConnectionFactory _factory;
        public IConnection _connection;
        public IModel _channel;
        public IBasicProperties _props;
        public EventingBasicConsumer _consumer;
        private ILogger _logger;

        public RepairMessagingHandler(ILogger<RepairMessagingHandler> logger)
        {
            _factory = new ConnectionFactory { HostName = "localhost" };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
            _props = _channel.CreateBasicProperties();
            _consumer = new EventingBasicConsumer(_channel);
            LoggerFactory factory = new LoggerFactory();
            _logger = logger;

        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            HandleMessages();
            return Task.CompletedTask;
        }
        public void HandleMessages(string exchange = "ServiceOrders")
        {
            _channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Fanout);

            var queueName = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(queue: queueName,
                              exchange: exchange,
                              routingKey: string.Empty);

            _logger.LogInformation("Waiting for messages.");

            _consumer.Received += (model, ea) =>
            {
                //Object test;
                byte[]? headerMessType;
                string? messageType;
                long sagaId;
                if(ea.BasicProperties.IsHeadersPresent())
                {
                    headerMessType = (ea.BasicProperties.Headers.ContainsKey("MessageType")) ? (byte[])ea.BasicProperties.Headers["MessageType"] : Encoding.UTF8.GetBytes("null");
                    messageType = Encoding.UTF8.GetString(headerMessType);
                    sagaId = (ea.BasicProperties.Headers.ContainsKey("SagaId")) ? (long)ea.BasicProperties.Headers["SagaId"] : -1;
                }
                else
                { //is not allowed to happen, every message should have a type
                    throw new Exception();
                }
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                if (messageType == "null")
                {
                    _logger.LogInformation($" [x] MessageType: null, Received: {message}");
                }
                if(messageType == "ServiceOrderCreated")
                {
                    //Handle the message content (for example create service order with all necessary data) imaginary for now
                    //_logger.LogInformation("");
                    //after completion send back response
                    var response = new Message
                    {
                        exchange = "ServiceOrderReply",
                        messageType = "AppointmentDatesConfirmed",
                        content = "Success",
                        Timestamp = DateTime.UtcNow,
                        SagaId = sagaId
                    };
                    _logger.LogInformation($"New ServiceOrder Received: {message}! at {DateTimeOffset.Now}");
                    SendMessage(response);
                }
                else
                {
                    //var messageType = Encoding.UTF8.GetString();
                    //header.TryGetValue("MessageType");
                    _logger.LogInformation($" [x] RepairMessagingHandler_HandleMessages_else");
                }
            };
            _channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: _consumer);

            //Console.WriteLine(" Press [enter] to exit.");
            //Console.ReadLine();
        }

        public async Task SendMessageAsync(Message messageobj)
        {
            //_props.Headers = new Dictionary<string, object>
            //{
            //    { "MessageType", "Reply" }
            //};
            //_channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Fanout);

            ////string jsonstring = JsonSerializer.Serialize(aiso);
            ////var message = GetMessage(jsonstring);
            ////var body = Encoding.UTF8.GetBytes(jsonstring);
            //string bodytext = "ServiceOrderCreated message received and processed successfully!";
            //var body = Encoding.UTF8.GetBytes(bodytext);
            //_channel.BasicPublish(exchange: exchange,
            //                     routingKey: string.Empty,
            //                     basicProperties: _props,
            //                     body: body);
            await Task.CompletedTask;
        }
        public void SendMessage(Message messageobj)
        {
            _logger.LogInformation($"SendMessage()");
            if (messageobj.messageType == null) messageobj.messageType = "null";
            if (messageobj.SagaId == null) messageobj.SagaId = -1;
            _props.Headers = new Dictionary<string, object>
            {
                { "MessageType", messageobj.messageType },
                { "SagaId", messageobj.SagaId }
            };

            _channel.ExchangeDeclare(exchange: "ServiceOrderReply", type: ExchangeType.Fanout);

            //string jsonstring = JsonSerializer.Serialize(aiso);
            //var message = GetMessage(jsonstring);
            //var body = Encoding.UTF8.GetBytes(jsonstring);

            var bodytext = (messageobj.content == null) ? "null" : messageobj.content;
            var body = Encoding.UTF8.GetBytes(bodytext);
            _channel.BasicPublish(exchange: "ServiceOrderReply",
                                 routingKey: string.Empty,
                                 basicProperties: _props,
                                 body: body);
            _logger.LogInformation($"published Message at {DateTimeOffset.Now} ");
        }

    }
}
