using EmailService_Core.Abstractions;
using EmailService_Core.Infrastructure;
using EmailService_Core.Models;
using EmailService.Core.Services;
using Serilog;
using Serilog.Core;
using EmailService.API.Middlewares;
using Microsoft.OpenApi.Models;
using EmailService.API.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
//��������� ������� ������
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "EmailService API",
        Description = "������ ��� �������� email-�����"
    });

    var basePath = AppContext.BaseDirectory;

    var xmlPath = Path.Combine(basePath, "EmailService.xml");
    opt.IncludeXmlComments(xmlPath);
});

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

builder.Services.AddHostedService<RabbitMQBackgroundService>();

var app = builder.Build();

app.UseExceptionHandler(o => { });

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();