﻿using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using RabbitMQ.Client.Events;

namespace MessengerLibrary
{
    public class Messenger : IMessenger
    {
        //rabbitmq
        public ConnectionFactory _factory;
        public IConnection _connection;
        public IModel _channel;
        public IBasicProperties _props;
        public EventingBasicConsumer _consumer;
        public string queueName;
        public int count;

        public Messenger()
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
        }

        public async Task SendOutboxMessageAsync(Message messageobj)
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

        public void SendOutboxMessage(Message messageobj)
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

        public void HandleMessages(string exchange = "ServiceOrderReply")
        {



            _channel.QueueBind(queue: queueName,
                              exchange: exchange,
                              routingKey: string.Empty);


            _consumer.Received += (model, ea) =>
            {
                //Thread.Sleep(5000);
                //Object test;
                var header = (byte[])ea.BasicProperties.Headers["MessageType"];
                var messageType = Encoding.UTF8.GetString(header);
                if (messageType == "Reply")
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    ++count;
                    Console.WriteLine($" [x] Received: {message + count}");
                }
                else
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine($" [x] unknown message Received: {message}");
                }
            };
            _channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: _consumer);
        }
    }
}
