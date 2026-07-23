using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

//Code attribution
//Write custom ASP.NET Core middleware
//Available at: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/write?view=aspnetcore-10.0
//[Accessed: 1 June 2026]

namespace ERP.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        // This middleware is designed to catch unhandled exceptions that occur during the processing of HTTP requests.
        // It logs the exception details for developers and then redirects the user back to the previous page
        // (or home page if no referer is available) while passing an error message using TempData.
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        // The InvokeAsync method is the core of the middleware. It wraps the execution of the next middleware in a try-catch block.
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        // This method handles the exception by logging it, setting an error message in TempData, and redirecting the user.
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // 1. Log for the developer
            _logger.LogError(exception, "Middleware caught exception. Redirecting with message.");

            // 2. Get TempData to store error messages
            var tempDataFactory = context.RequestServices.GetRequiredService<ITempDataDictionaryFactory>();
            var tempData = tempDataFactory.GetTempData(context);

            // 3. Set the messages
            tempData["ErrorMessage"] = exception.Message;

            // 4. Determine where to send the user back to
            string referer = context.Request.Headers["Referer"].ToString();
            string redirectUrl = string.IsNullOrEmpty(referer) ? "/" : referer;

            // 5. Perform the Redirect
            context.Response.Redirect(redirectUrl);

            await Task.CompletedTask;
        }
    }
}