using System;
using System.Net;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Utilities;

namespace ApiGateway.API.Filters
{
    public class GatewayCustomExceptionFilter : IExceptionFilter
    {
        private const string serviceName = "Gateway Service Error";
        private readonly ILogger _logger;

        public GatewayCustomExceptionFilter(ILogger<GatewayCustomExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            var source = context.Exception.Source;
            var statusCode = string.Empty;
            var stackTrace = context.Exception.StackTrace;
            var message = $"Error Message: {TraverseException(context.Exception, out statusCode)}";

            _logger.LogError(new EventId(context.Exception.HResult),
                context.Exception,
                "Exception throw in {ServiceName} : {message}", serviceName, message);

            var exceptionType = context.Exception.GetType();

            var jsonErrorResponse = new JsonErrorResponse
            {
                Messages = new[] {message},
                StatusCode = statusCode,
                Source = source,
                StackTrace = stackTrace
            };

            if (statusCode == HttpStatusCode.NotFound.ToString())
            {
                context.Result = new NotFoundObjectResult(jsonErrorResponse);
                context.HttpContext.Response.StatusCode = (int) HttpStatusCode.NotFound;
                context.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = message;
            }
            else if (exceptionType == typeof(UnauthorizedAccessException))
            {
                context.Result = new UnauthorizedResult();
                context.HttpContext.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                context.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = message;
            }
            else if (exceptionType == typeof(ForbidResult))
            {
                context.Result = new ForbidResult(message);
                context.HttpContext.Response.StatusCode = (int) HttpStatusCode.Forbidden;
                context.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = message;
            }
            else
            {
                context.Result = new ObjectResult(jsonErrorResponse);
                context.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = message;
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            context.ExceptionHandled = true;
        }

        /// <summary>
        ///     Enumerates exception collection fetching the original exception, which would be the real
        ///     error. The outer exceptions are wrappers which we are not concerned.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        private static string TraverseException(Exception exception, out string statusCode)
        {
            var message = string.Empty;
            var innerException = exception;

            // Enumerate through exception stack to get to innermost exception
            do
            {
                message = string.IsNullOrEmpty(innerException.Message) ? string.Empty : innerException.Message;
                statusCode = innerException.Data["StatusCode"].ToSafeString();

                innerException = innerException.InnerException;
            } while (innerException != null);

            return message;
        }

        private class JsonErrorResponse
        {
            public string[] Messages { get; set; }
            public string StatusCode { get; set; }

            public string Source { get; set; }

            public string StackTrace { get; set; }

            public object DeveloperMeesage { get; set; }
        }
    }
}