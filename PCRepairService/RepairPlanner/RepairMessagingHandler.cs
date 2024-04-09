using Microsoft.AspNetCore.Connections;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace RepairPlanner
{
    public class RepairMessagingHandler
    {
        public ConnectionFactory _factory;
        public IConnection _connection;
        public IModel _channel;
        public IBasicProperties _props;
        public EventingBasicConsumer _consumer;

        public RepairMessagingHandler() 
        {
            _factory = new ConnectionFactory { HostName = "localhost" };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
            _props = _channel.CreateBasicProperties();
            _consumer = new EventingBasicConsumer(_channel);

            HandleMessages();
        }
        public void HandleMessages()
        {
            _channel.ExchangeDeclare(exchange: "ServiceOrders", type: ExchangeType.Fanout);

            var queueName = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(queue: queueName,
                              exchange: "ServiceOrders",
                              routingKey: string.Empty);

            Console.WriteLine(" [*] Waiting for messages.");


            _consumer.Received += (model, ea) =>
            {
                //Object test;
                byte[]? header;
                string? messageType;
                if(ea.BasicProperties.IsHeadersPresent())
                {
                    header = (ea.BasicProperties.Headers.ContainsKey("MessageType")) ? (byte[])ea.BasicProperties.Headers["MessageType"] : Encoding.UTF8.GetBytes("null");
                    messageType = Encoding.UTF8.GetString(header);
                }
                else
                { //is not allowed to happen, every message should have a type
                    throw new Exception();
                }
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                simulateProcessing();
                Thread.Sleep(12000);

                if (messageType == "null")
                {
                    Console.WriteLine($" [x] MessageType: null, Received: {message}");
                }
                if(messageType == "ServiceOrderCreated")
                {
                    Console.WriteLine($" [x] New ServiceOrder Received: {message}!");
                    SendReply();
                }
                else
                {
                    //var messageType = Encoding.UTF8.GetString();
                    //header.TryGetValue("MessageType");
                    Console.WriteLine($" [x] RepairMessagingHandler_HandleMessages_else");
                }
            };
            _channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: _consumer);

            //Console.WriteLine(" Press [enter] to exit.");
            //Console.ReadLine();
        }

        public void SendReply(string exchange = "ServiceOrderReply")
        {
            _props.Headers = new Dictionary<string, object>
            {
                { "MessageType", "Reply" }
            };
            _channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Fanout);

            //string jsonstring = JsonSerializer.Serialize(aiso);
            //var message = GetMessage(jsonstring);
            //var body = Encoding.UTF8.GetBytes(jsonstring);
            string bodytext = "ServiceOrderCreated message received and processed successfully!";
            var body = Encoding.UTF8.GetBytes(bodytext);
            _channel.BasicPublish(exchange: exchange,
                                 routingKey: string.Empty,
                                 basicProperties: _props,
                                 body: body);
        }

        public void HandleMessageByType(BasicDeliverEventArgs ea)
        {

        }

        public void simulateProcessing()
        {
            for(int i = 0; i < 1000;  ++i)
            {
                var something = 1;
                something += 1;
            }
        }
    }
}
