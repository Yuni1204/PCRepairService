using MessengerLibrary;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using PCRepairService.Interfaces;
using PCRepairService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
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
        public IRepairTimer _repairTimer;

        public ServiceMessenger(ILogger<ServiceMessenger> logger, IServiceScopeFactory serviceScopeFactory, IRepairTimer repairtimer)
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
            _repairTimer = repairtimer;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            HandleMessages();
            return Task.CompletedTask;
        }

        public void HandleMessages(string exchange = "ServiceOrderReply")
        {
            _logger.LogInformation($"HandleMessages started at {DateTimeOffset.Now.ToString("hh.mm.ss.ffffff")}");
            _channel.QueueBind(queue: queueName,
                              exchange: exchange,
                              routingKey: string.Empty);

            _consumer.Received += async (model, ea) =>
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                //_logger.LogInformation("[*****] consumer.Received");
                byte[]? headerMessType;
                long sagaId;
                string messageType;
                var messagebody = ea.Body.ToArray();
                var messageString = Encoding.UTF8.GetString(messagebody);
                if (ea.BasicProperties.IsHeadersPresent())
                {
                    headerMessType = (ea.BasicProperties.Headers.ContainsKey("MessageType")) ? (byte[])ea.BasicProperties.Headers["MessageType"] : Encoding.UTF8.GetBytes("null");
                    messageType = Encoding.UTF8.GetString(headerMessType);
                    sagaId = (ea.BasicProperties.Headers.ContainsKey("SagaId")) ? (long)ea.BasicProperties.Headers["SagaId"] : -1;
                    await ProcessMessageByHeader(messageType, sagaId, messageString);
                }
                else
                { //is not allowed to happen, every message should have a type
                    throw new Exception();
                }
                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                var target = JsonSerializer.Deserialize<ServiceOrder>(messageString);
                var newstoptime = new RepairStopTime
                {
                    ServiceOrderId = target.Id,
                    StopTime = ts.Milliseconds
                };
                _repairTimer.AddStoppedTime(newstoptime);
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

        private async Task ProcessMessageByHeader(string messageType, long sagaId, string? content)
        {
            string? logmessage = null;
            switch (messageType)
            {
                case "Reply":
                    logmessage = $" [x] Received: {content}";
                    break;
                case "AppointmentDatesConfirmed":
                    logmessage = await CaseAppointmentSuccess(content, sagaId);
                    break;
                case "AppointmentDatesFailed":
                    logmessage = await CaseAppointmentFail(content, sagaId);
                    break;
                case "SpareCarConfirmed":
                    logmessage = await CaseSpareCarSuccess(content, sagaId);
                    break;
                case "SpareCarFailed":
                    logmessage = await CaseSpareCarFail(content, sagaId);
                    break;
                default:
                    logmessage = $" [x] unknown message Received: {content}";
                    break;
            }
            _logger.LogInformation($"{logmessage}");
        }

        private async Task<string> CaseAppointmentSuccess(string? content, long sagaId)
        {
            if (content != null && sagaId != -1)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var sagaHandler = scope.ServiceProvider.GetService<ISagaHandler>();
                    if (sagaHandler != null)
                    {
                        var serviceOrder = JsonSerializer.Deserialize<ServiceOrder>(content);
                        if (serviceOrder == null) throw new Exception(content);
                        await sagaHandler.ReserveSpareCar(serviceOrder, sagaId);
                        return $"CaseAppointmentSuccess for SagaId {sagaId} ended at {DateTimeOffset.Now.ToString("hh.mm.ss.ffffff")} ";
                    }
                    else
                    {
                        return "Creating Scoped SagaHandler from serviceScopeFactory returned null!";
                    }
                }
            }
            else
            {
                return $"messageType AppointmentDatesConfirmed body was null or no sagaid";
            }
        }

        private async Task<string> CaseAppointmentFail(string? messageString, long sagaId)
        {
            if (messageString != null && sagaId != -1)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var sagaHandler = scope.ServiceProvider.GetService<ISagaHandler>();
                    if (sagaHandler != null)
                    {
                        var serviceOrder = JsonSerializer.Deserialize<ServiceOrder>(messageString);
                        if (serviceOrder == null) throw new Exception(messageString);
                        await sagaHandler.CompensateConfirmAppointmentFail(serviceOrder, sagaId);
                        await _repairTimer.SaveStoppedTime(serviceOrder.Id);
                        var newTimeStamp = new Timestamps
                        {
                            ServiceOrderId = serviceOrder.Id,
                            Timestamp1 = DateTime.Now
                        };
                        _repairTimer.AddIrlDuration(newTimeStamp);
                        await _repairTimer.SaveDuration(serviceOrder.Id);
                        return $"CaseAppointmentFail for SagaId {sagaId} ended at {DateTimeOffset.Now.ToString("hh.mm.ss.ffffff")} ";
                    }
                    else
                    {
                        return "Creating Scoped SagaHandler from serviceScopeFactory returned null!";
                    }
                }
            }
            else
            {
                return $"messageType AppointmentDatesFail body was null or no sagaid";
            }
        }

        private async Task<string> CaseSpareCarSuccess(string? messageString, long sagaId)
        {
            if (messageString != null && sagaId != -1)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var sagaHandler = scope.ServiceProvider.GetService<ISagaHandler>();
                    if (sagaHandler != null)
                    {
                        var serviceOrder = JsonSerializer.Deserialize<ServiceOrder>(messageString);
                        if (serviceOrder == null) throw new Exception(messageString);
                        await sagaHandler.EndServiceOrderSagaAsync(serviceOrder, sagaId);
                        var newTimeStamp = new Timestamps
                        {
                            ServiceOrderId = serviceOrder.Id,
                            Timestamp1 = DateTime.Now
                        };
                        _repairTimer.AddIrlDuration(newTimeStamp);
                        await _repairTimer.SaveDuration(serviceOrder.Id);
                        return $"CaseSpareCarSuccess for SagaId {sagaId} ended at {DateTimeOffset.Now.ToString("hh.mm.ss.ffffff")} ";
                    }
                    else
                    {
                        return "Creating Scoped SagaHandler from serviceScopeFactory returned null!";
                    }
                }
            }
            else
            {
                return $"messageType SpareCarSuccess body was null or no sagaid";
            }
        }
        
        private async Task<string> CaseSpareCarFail(string? messageString, long sagaId)
        {
            if (messageString != null && sagaId != -1)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var sagaHandler = scope.ServiceProvider.GetService<ISagaHandler>();
                    if (sagaHandler != null)
                    {
                        var serviceOrder = JsonSerializer.Deserialize<ServiceOrder>(messageString);
                        if (serviceOrder == null) throw new Exception(messageString);
                        await sagaHandler.CompensateReserveSpareCarFail(serviceOrder, sagaId);
                        return $"CaseSpareCarFail for SagaId {sagaId} ended at {DateTimeOffset.Now.ToString("hh.mm.ss.ffffff")} ";
                    }
                    else
                    {
                        return "Creating Scoped SagaHandler from serviceScopeFactory returned null!";
                    }
                }
            }
            else
            {
                return $"messageType SpareCarFail body was null or no sagaid";
            }
        }

    }
}
