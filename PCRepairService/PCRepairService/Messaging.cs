using RabbitMQ.Client;
using System.Security.Policy;
using System.Text;
using System.Threading.Channels;

namespace PCRepairService
{
    public class Messaging
    {
        //rabbitmq
        public ConnectionFactory _factory;
        public IConnection _connection;
        public IModel _channel;

        public Messaging()
        {
            _factory = new ConnectionFactory { HostName = "localhost" };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void DoSomething(string[] args)
        {
            _channel.ExchangeDeclare(exchange: "ServiceOrders", type: ExchangeType.Fanout);

            var message = GetMessage(args);
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "ServiceOrders",
                                 routingKey: string.Empty,
                                 basicProperties: null,
                                 body: body);
            Console.WriteLine($" [x] Sent {message}");


            //_channel.QueueDeclare(queue: "hello",
            //         durable: false,
            //         exclusive: false,
            //         autoDelete: false,
            //         arguments: null);

            //const string message = "Hello World!";
            //var body = Encoding.UTF8.GetBytes(message);

            //_channel.BasicPublish(exchange: string.Empty,
            //                     routingKey: "hello",
            //                     basicProperties: null,
            //                     body: body);

            

        }
        static string GetMessage(string[] args)
        {
            return ((args.Length > 0) ? string.Join(" ", args) : "info: Hello World!");
        }


    }
}
