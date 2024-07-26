using EmailService_Core.Abstractions;
using EmailService_Core.Infrastructure;
using EmailService_Core.Models;
using EmailService.Core.Services;
using Serilog;
using Serilog.Core;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
EmailConfiguration? emailConfig = configuration
    .GetSection("EmailConfiguration")
    .Get<EmailConfiguration>();

Logger logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddSingleton(logger);
builder.Services.AddSingleton(emailConfig!);
builder.Services.AddScoped<IEmailDispatcher, EmailDispatcher>();
builder.Services.AddScoped<IMailService, MailService>();

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