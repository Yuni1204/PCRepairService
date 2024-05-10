using MessengerLibrary;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using PCRepairService;
using PCRepairService.DataAccess;
using PCRepairService.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("PCRepairDB");
builder.Services.AddScoped<IDA_ServiceOrder, DA_ServiceOrder>();
builder.Services.AddScoped<IDA_Timestamps, DA_Timestamps>();
builder.Services.AddScoped<IDA_StopTime, DA_StopTime>();
builder.Services.AddScoped<ISagaHandler, SagaHandler>();
builder.Services.AddDbContext<ServiceDBContext>(x => x.UseNpgsql(connectionString));
builder.Services.AddSingleton<IDbContextFactory<ServiceDBContext>, ServiceDBContextFactory>();
//builder.Services.AddSingleton<IMessenger, ServiceMessenger>();
builder.Services.AddSingleton<IRepairTimer, RepairTimer>();
builder.Services.AddSingleton<ISimpleMessenger, SimpleMessenger>();
builder.Services.AddHostedService<ServiceMessenger>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
