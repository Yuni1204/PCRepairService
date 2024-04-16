using Microsoft.EntityFrameworkCore;
using OutboxWorker;
using MessengerLibrary;
using OutboxWorker.DataAccess;

var builder = Host.CreateApplicationBuilder(args);
//var connStr = builder.Configuration.GetConnectionString("PCRepairDB");
builder.Services.AddHostedService<OutboxHandler>();
builder.Services.AddSingleton<IMessenger, OutboxMessenger>();
builder.Services.AddSingleton<IDbContextFactory<OutboxDBContext>, OutboxDBContextFactory>();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var host = builder.Build();
host.Run();
