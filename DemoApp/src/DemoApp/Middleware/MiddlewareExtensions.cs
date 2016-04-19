using Microsoft.AspNet.Builder;
using Microsoft.Extensions.Logging;

using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DemoApp.Middleware
{
    /// <summary>
    /// I use one class to hold all the extension methods that add middleware modules to the ApplicationBuilder class.
    /// </summary>
    public static class MiddlewarExtensions
    {
        /// <summary>
        /// *POI
        /// This extension method does two things.
        /// 1.  Provides a method to add custom middleware to the Http Pipeline.
        /// 2.  Injects the required dependencies into the ApplicationBuilder class.
        /// </summary>
        /// <param name="builder">Required to extend the ApplicationBuilder class.</param>
        /// <param name="env">The IHostingEnvironment object from the calling application's Startup.Configure() method.</param>
        /// <param name="apiPaths">All the API base route paths in the application.  The paths need to start with a forward slash and should not end with a forward slash.</param>
        /// <param name="errorPage">The path to the generic error that should be returned when not in debug and the request was not for a Web API method.</param>
        /// <param name="logExceptions">Set to true to log all exceptions in a SQL Server database and create the log table if it doesn't exist.</param>
        /// <param name="conn">A connection string to the SQL Server database where the exceptions will be logged</param>
        /// <returns></returns>
        public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder builder, IHostingEnvironment env, string[] apiPaths, string errorPage, bool logExceptions, string conn)
        {
            //This method will accept a parameter array but I always explicitly define parameter arrays.
            return builder.UseMiddleware<CustomExceptionHandler.ExceptionHandler>(new object[] { env, apiPaths, errorPage, logExceptions, conn });
        }
    }
}
