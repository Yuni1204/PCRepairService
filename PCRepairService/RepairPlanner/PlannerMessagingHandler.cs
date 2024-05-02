using MessengerLibrary;
using Microsoft.AspNetCore.Connections;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RepairPlanner.DataAccess;
using RepairPlanner.Models;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace RepairPlanner
{
    public class PlannerMessagingHandler : BackgroundService, IMessenger
    {
        public ConnectionFactory _factory;
        public IConnection _connection;
        public IModel _channel;
        public IBasicProperties _props;
        public EventingBasicConsumer _consumer;
        private ILogger _logger;

        private IServiceScopeFactory _serviceScopeFactory;
        //private readonly IDA_Planner _DAplanner;

        public PlannerMessagingHandler(ILogger<PlannerMessagingHandler> logger/*, IDA_Planner planner*/, IServiceScopeFactory serviceScopeFactory)
        {
            _factory = new ConnectionFactory { HostName = "localhost" };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
            _props = _channel.CreateBasicProperties();
            _consumer = new EventingBasicConsumer(_channel);
            //LoggerFactory factory = new LoggerFactory();
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;

            //_DAplanner = planner;
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
                var message = Encoding.UTF8.GetString(body); //serviceOrder as json

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
                    _logger.LogInformation($"[SagaId {sagaId}] New ServiceOrder Received: {message}! at {DateTimeOffset.Now.ToString("hh.mm.ss.ffffff")}");
                    SendMessage(response);
                }
                else if(messageType == "AppointmentSelected")
                {
                    saveNewServiceOrder(message);

                    var response = new Message
                    {
                        exchange = "ServiceOrderReply",
                        //messageType = "AppointmentDatesFailed",
                        messageType = "AppointmentDatesConfirmed",
                        content = message,
                        Timestamp = DateTime.UtcNow,
                        SagaId = sagaId
                    };
                    _logger.LogInformation($"[SagaId {sagaId}] ServiceOrder Appointment at {DateTimeOffset.Now.ToString("hh.mm.ss.ffffff")}");
                    SendMessage(response);
                }
                else if (messageType == "CancelAppointment")
                {
                    var content = RemoveAppointmentServiceOrder(message);
                    var response = new Message
                    {
                        exchange = "ServiceOrderReply",
                        messageType = "AppointmentDatesFailed",
                        //messageType = "AppointmentDatesConfirmed",
                        content = content,
                        Timestamp = DateTime.UtcNow,
                        SagaId = sagaId
                    };
                    _logger.LogInformation($"[SagaId {sagaId}] ServiceOrder Appointment (FAIL) at {DateTimeOffset.Now.ToString("hh.mm.ss.ffffff")}");
                    SendMessage(response);
                }
                else if(messageType == "ReserveSpareCar")
                {
                    
                    var response = new Message
                    {
                        exchange = "ServiceOrderReply",
                        //messageType = "SpareCarConfirmed",
                        messageType = "SpareCarFailed",
                        content = message,
                        Timestamp = DateTime.UtcNow,
                        SagaId = sagaId
                    };
                    if (response.messageType == "SpareCarConfirmed")
                    {
                        response.content = AddSpareCarServiceOrder(message);
                    }
                    _logger.LogInformation($"[SagaId {sagaId}] SpareCar Reserve at {DateTimeOffset.Now.ToString("hh.mm.ss.ffffff")}");
                    SendMessage(response);
                }
                else
                {
                    //var messageType = Encoding.UTF8.GetString();
                    //header.TryGetValue("MessageType");
                    _logger.LogInformation($"[SagaId {sagaId}] ({messageType}) RepairMessagingHandler_HandleMessages_else");
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
            _logger.LogInformation($"[SagaId {messageobj.SagaId}] published Message at {DateTimeOffset.Now.ToString("hh.mm.ss.ffffff")} ");
        }


        public void saveNewServiceOrder(string content)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var daPlanner = scope.ServiceProvider.GetService<IDA_Planner>();
                if (daPlanner != null)
                {
                    var serviceOrder = JsonSerializer.Deserialize<PServiceOrder>(content);
                    if (serviceOrder == null) throw new Exception(content);

                    var dbEntry = daPlanner.GetServiceOrder(serviceOrder.Id);
                    if (dbEntry == null) daPlanner.AddServiceOrder(serviceOrder);
                    else daPlanner.AddServiceOrderAppointments(serviceOrder, dbEntry);
                }
                else
                {
                    throw new Exception("no scope DAPlanner");
                }
            }
        }

        public string RemoveAppointmentServiceOrder(string content)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var daPlanner = scope.ServiceProvider.GetService<IDA_Planner>();
                if (daPlanner != null)
                {
                    var serviceOrder = JsonSerializer.Deserialize<PServiceOrder>(content);
                    if (serviceOrder == null) throw new Exception(content);
                    else 
                    {
                        serviceOrder.HandoverAppointment = null;
                        serviceOrder.ReturnDate = null;
                        daPlanner.EditServiceOrder(serviceOrder);
                        return JsonSerializer.Serialize(serviceOrder);
                    }
                }
                else throw new Exception("no scope DAPlanner");
            }
        }

        public string AddSpareCarServiceOrder(string content)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var daPlanner = scope.ServiceProvider.GetService<IDA_Planner>();
                if (daPlanner != null)
                {
                    var serviceOrder = JsonSerializer.Deserialize<PServiceOrder>(content);
                    if (serviceOrder == null) throw new Exception(content);
                    var dbEntry = daPlanner.GetServiceOrder(serviceOrder.Id);
                    if (dbEntry != null)
                    {
                        daPlanner.AddServiceOrderSpareCar(dbEntry);
                        dbEntry = daPlanner.GetServiceOrder(serviceOrder.Id);
                        if (dbEntry == null) throw new Exception("new dbentry after addsparecar N/A");
                        return JsonSerializer.Serialize(dbEntry);
                    }
                    else throw new Exception(content);
                }
                else throw new Exception("no scope DAPlanner");
            }
        }

    }
}
