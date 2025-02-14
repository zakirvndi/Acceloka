using Acceloka.Entities;
using Acceloka.Mapping;
using Acceloka.Services;
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

//services
builder.Services.AddTransient<TicketService>();
builder.Services.AddTransient<BookService>();
builder.Services.AddTransient<BookedTicketService>();
//Mapper
builder.Services.AddAutoMapper([typeof(TicketProfile), typeof(BookedTicketProfile)]);


// Konfigurasi Serilog Sink File
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(
        path: "logs/Log-.txt",
        rollingInterval: RollingInterval.Day,  // File per hari
        retainedFileCountLimit: 7, // Hapus log lebih dari 7 hari
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
