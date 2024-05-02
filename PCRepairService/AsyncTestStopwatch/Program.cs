using AsyncTestStopwatch;
using AsyncTestStopwatch.DataAccess;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("PCRepairDB");
builder.Services.AddHostedService<Worker>();
builder.Services.AddDbContext<StopwatchDBContext>(x => x.UseNpgsql(connectionString));

var host = builder.Build();
host.Run();
