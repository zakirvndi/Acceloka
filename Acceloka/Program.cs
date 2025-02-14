using System.Reflection;
using Acceloka.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//configure sql server & dbcontext
builder.Services.AddEntityFrameworkSqlServer();
builder.Services.AddDbContextPool<AccelokaContext>(options =>
{
    var conString = configuration.GetConnectionString("SQLDB");
    options.UseSqlServer(conString);
});


//Behaviors
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Register MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// Register FluentValidation
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());


//Mapper
builder.Services.AddAutoMapper([typeof(TicketProfile)]);


// Konfigurasi Serilog Sink File
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(
        path: "logs/Log-.txt",
        rollingInterval: RollingInterval.Day,  
        retainedFileCountLimit: 7,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}"
    )
    .CreateLogger();



builder.Host.UseSerilog();

var app = builder.Build();

//untuk middlware error handling
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
