using HospitalOrders.Infrastructure;
using HospitalOrders.Worker.Workers;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog((services, config) =>
    config.ReadFrom.Configuration(builder.Configuration));

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHostedService<OrderProcessingWorker>();

var host = builder.Build();
host.Run();
