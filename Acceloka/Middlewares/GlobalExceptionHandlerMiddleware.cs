
using Newtonsoft.Json;
using System.Net;


public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (KeyNotFoundException ex)  
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.NotFound, "Not Found");
        }
        catch (ArgumentException ex)  
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.BadRequest, "Bad Request");
        }
        catch (Exception ex)  
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.InternalServerError, "Internal Server Error");
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex, HttpStatusCode statusCode, string title)
    {
        _logger.LogError(ex, "Exception occurred: {Message}", ex.Message);

        var errorResponse = new
        {
            type = $"https://yourapi.com/errors/{statusCode.ToString().ToLower()}",
            title = title,
            status = (int)statusCode,
            detail = ex.Message,
            instance = context.Request.Path // 🔹 Menggunakan 'instance' sesuai RFC 7807
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var jsonResponse = JsonConvert.SerializeObject(errorResponse, Formatting.Indented);
        await context.Response.WriteAsync(jsonResponse);
    }
}

