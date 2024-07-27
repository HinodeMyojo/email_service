using EmailService.Core.CustomExceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmailService.API.Middlewares
{
    public class GlobalExceptionHandler(IHostEnvironment env, ILogger<GlobalExceptionHandler> _logger) : IExceptionHandler
    {
        private const string UnhandledExceptionMsg = "An unhandled exception has occurred while executing the request.";

        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
        {
            Converters = {new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)}
        };

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext, 
            Exception exception, 
            CancellationToken cancellationToken)
        {

            _logger.LogError(
                exception, "Exception occurred: {Message}", exception.Message);

            ProblemDetails problemDetails;
            int statusCode;

            switch (exception)
            {
                case FormatException ex:
                    statusCode = StatusCodes.Status400BadRequest;
                    problemDetails = CreateProblemDetails(httpContext, ex, statusCode);
                    break;
                case ConnectionException ex:
                    statusCode = StatusCodes.Status503ServiceUnavailable;
                    problemDetails = CreateProblemDetails(httpContext, ex, statusCode);
                    break;
                case ArgumentNullException ex:
                    statusCode = StatusCodes.Status404NotFound;
                    problemDetails = CreateProblemDetails(httpContext, ex, statusCode);
                    break;
                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    problemDetails = CreateProblemDetails(httpContext, exception, statusCode);
                    break;
            }

            //httpContext.Response.StatusCode = problemDetails.Status.Value;
            var json = ToJson(problemDetails);

            const string contentType = "application/problem+json";
            httpContext.Response.ContentType = contentType;

            await httpContext.Response.WriteAsync(json, cancellationToken);

            return true;
        }

        private ProblemDetails CreateProblemDetails(in HttpContext context, in Exception exception, int statusCode)
        {
            var reasonPhrase = exception.Message;
            if (string.IsNullOrEmpty(reasonPhrase))
            {
                reasonPhrase = UnhandledExceptionMsg;
            }

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = reasonPhrase,
            };

            // в прод допускаем только короткую фразу
            if (env.IsProduction())
            {
                return problemDetails;
            }

            problemDetails.Detail = exception.ToString();
            problemDetails.Extensions["traceId"] = context.TraceIdentifier;
            problemDetails.Extensions["data"] = exception.Data;

            return problemDetails;
        }

        private string ToJson(in ProblemDetails problemDetails)
        {
            try
            {
                return JsonSerializer.Serialize(problemDetails, SerializerOptions);
            }
            catch (Exception ex)
            {
                const string msg = "An exception has occurred while serializing error to JSON";
                _logger.LogError(ex, msg);
            }

            return string.Empty;
        }
    }
}
