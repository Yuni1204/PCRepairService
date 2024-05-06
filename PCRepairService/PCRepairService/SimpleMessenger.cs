using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using MessengerLibrary;
using System.Text.Json;

namespace PCRepairService
{
    public class SimpleMessenger
    {
        protected readonly ConnectionFactory _factory;
        protected readonly IConnection _connection;
        protected readonly IModel _channel;
        protected readonly IBasicProperties _props;
        protected readonly EventingBasicConsumer _consumer;
        protected readonly string queueName;

        public SimpleMessenger()
        {
            _factory = new ConnectionFactory { HostName = "localhost" };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
            _props = _channel.CreateBasicProperties();
            _channel.ExchangeDeclare(exchange: "ServiceOrders", type: ExchangeType.Fanout);
            _consumer = new EventingBasicConsumer(_channel);
            queueName = _channel.QueueDeclare().QueueName;
        }

        public void SendMessage(Message messageobj)
        {
            if (messageobj.messageType == null) messageobj.messageType = "null";
            _props.Headers = new Dictionary<string, object>
            {
                { "MessageType", messageobj.messageType }
            };

            string content = JsonSerializer.Serialize(messageobj.content);
            var body = Encoding.UTF8.GetBytes(content);
            _channel.BasicPublish(exchange: messageobj.exchange,
                                 routingKey: string.Empty,
                                 basicProperties: _props,
                                 body: body);
        }

    }
}
