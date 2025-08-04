using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SwaggerDemo.Controllers;
using Serilog;
using SwaggerDemo.Services;
using SwaggerDemo.Middleware;
using SwaggerDemo.Interfaces;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(); 

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddHostedService<SwaggerDemo.Services.MessageProcessorService>();
builder.Services.AddSingleton<SwaggerDemo.Services.DatabaseInitializer>();
builder.Services.AddSingleton<SwaggerDemo.Interfaces.IMessageRepository,SwaggerDemo.Services.MessageRepository>();


var app = builder.Build();

app.UseMiddleware<SwaggerDemo.Middleware.GlobalExceptionHandler>();

var dbInit = app.Services.GetRequiredService<SwaggerDemo.Services.DatabaseInitializer>();
dbInit.EnsureTablesExist();
    
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => "Hello, Swagger works!");

app.MapControllers();

app.Run();
