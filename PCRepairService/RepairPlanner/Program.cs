using MessengerLibrary;
using Microsoft.EntityFrameworkCore;
using PCRepairService.Models;
using RepairPlanner;
using RepairPlanner.DataAccess;

//RepairMessagingHandler handler = new RepairMessagingHandler();

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("PCRepairDB");
builder.Services.AddDbContext<PlannerDBContext>(x => x.UseNpgsql(connectionString));

//Add Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddHostedService<PlannerMessagingHandler>();
builder.Services.AddScoped<IDA_Planner, DA_Planner>();

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
