using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using PCRepairService.DataAccess;
using PCRepairService.Interfaces;
using PCRepairService.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("PCRepairDB");
builder.Services.AddScoped<IDA_AISO, DA_AISO>();
builder.Services.AddDbContext<ServiceDBContext>(x => x.UseNpgsql(connectionString));
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
