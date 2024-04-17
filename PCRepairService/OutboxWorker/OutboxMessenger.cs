using MessengerLibrary;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace OutboxWorker
{
    public class OutboxMessenger : IMessenger
    {
        private readonly ConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IBasicProperties _props;
        private readonly EventingBasicConsumer _consumer;
        private readonly string queueName;
        private readonly ILogger _logger;

        public OutboxMessenger(ILogger<OutboxMessenger> logger)
        {
            _factory = new ConnectionFactory { HostName = "rabbitmq", Port = 5672 };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
            _props = _channel.CreateBasicProperties();
            _channel.ExchangeDeclare(exchange: "ServiceOrders", type: ExchangeType.Fanout);
            _channel.ExchangeDeclare(exchange: "ServiceOrderReply", type: ExchangeType.Fanout);
            _consumer = new EventingBasicConsumer(_channel);
            queueName = _channel.QueueDeclare().QueueName;
            HandleMessages();
            _logger = logger;
        }

        public async Task SendMessageAsync(Message messageobj)
        {

            if (messageobj.messageType == null) messageobj.messageType = "null";
            if (messageobj.SagaId == null) messageobj.SagaId = -1;
            _props.Headers = new Dictionary<string, object>
            {
                { "MessageType", messageobj.messageType },
                { "SagaId", messageobj.SagaId }
            };


            string content = JsonSerializer.Serialize(messageobj.content);
            //var message = GetMessage(jsonstring);
            var body = Encoding.UTF8.GetBytes(content);
            _channel.BasicPublish(exchange: messageobj.exchange,
                                 routingKey: string.Empty,
                                 basicProperties: _props,
                                 body: body);
            _logger.LogInformation($" [x] Sent {content}");
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
            _logger.LogInformation($" [x] Sent {content}");
        }

        public void HandleMessages(string exchange = "_")
        {
            return;
        }
    }
}
