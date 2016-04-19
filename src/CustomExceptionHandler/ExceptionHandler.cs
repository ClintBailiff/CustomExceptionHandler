using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.WebEncoders;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using CustomExceptionHandler.Models;


namespace CustomExceptionHandler
{
    /// <summary>
    /// *POI
    /// Middleware class that handles exceptions in the Http pipeline.  
    /// 
    /// If the hosting environment is set to Debug and an exception does not occur in a path defined as a Web API path, the exception
    /// is passed up the pipeline to be handled by the ASP.NET Developer Exception Page or Database Error Page. This of course requires
    /// that the UseDeveloperExceptionPage and UseDatabaseErrorPage calls exist above this middleware in the Starup.Configure() method
    /// of the parent application.
    /// 
    /// If an exception occurs in a path defined as a Web API path, the Response Body is replaced with the details of the exception when
    /// the hosting environment is set to Debug or with a generic "Internal server error" message when not is Debug.
    /// 
    /// If the hosting environment is not set to Debug and an exception does not occur in a path defined as a Web API path, the response
    /// is redirected to the error page that is defined by the errorPage parameter in the constructor.
    /// 
    /// If the logExceptions parameter in the constructor is set to true, a new thread is spawned that logs the exception in a SQL Server database.
    /// </summary>
    public class ExceptionHandler
    {
        private readonly ExceptionLogDbContext dbContext;
        private readonly RequestDelegate next;
        private readonly IHostingEnvironment env;
        private readonly string[] apiPaths;
        private readonly string errorPage;
        private readonly bool logExceptions;

        /// <summary>
        /// The constructor that is called when adding this middleware to the Http pipeline.  This is done in the Startup.Configure() 
        /// method of the parent application (see the DemoApp project).  If parameter logExceptions is true, the database specified in 
        /// parameter conn is created if that database doesn't exist (see the comments inside the if block below for an explanation).
        /// </summary>
        /// <param name="next">Represents the next module in the http pipeline and is required to be the first parameter in the constructor for all middleware classes.</param>
        /// <param name="env">The IHostingEnvironment object from the calling application's Startup.Configure() method.</param>
        /// <param name="apiPaths">All the API base route paths in the application.  The paths need to start with a forward slash and should not end with a forward slash.</param>
        /// <param name="errorPage">The path to the generic error that should be returned when not in debug and the request was not for a Web API method.</param>
        /// <param name="logExceptions">Set to true to log all exceptions in a SQL Server database and create the log table if it doesn't exist.</param>
        /// <param name="conn">A connection string to the SQL Server database where the exceptions will be logged</param>
        public ExceptionHandler(RequestDelegate next, IHostingEnvironment env, string[] apiPaths, string errorPage, bool logExceptions, string conn)
        {
            this.next = next;
            this.env = env;
            this.apiPaths = apiPaths;
            this.errorPage = errorPage;
            this.logExceptions = logExceptions;

            if (logExceptions)
            {
                //*POI
                //Setup the DbContext used to log exceptions.  If the database in the connection string does not exist, this will create the database
                //and the ExceptionLog table.  If the database exists, the ExceptionLog table must be created by running the ExceptionLog.sql file in
                //the root of this project against the database.  This is necessary because as of version 7.0.0-rc1-final EF7 does not support creating
                //migrations at runtime.  Also, since the connection string is passed in at runtime design time migrations can not be created either.
                this.dbContext = new ExceptionLogDbContext(conn);
            }
        }

        /// <summary>
        /// This is the method that is called by the framework when the execution of the Http pipeline
        /// reaches this middleware module. 
        /// </summary>
        /// <param name="context">A required parameter that the framework supplies.</param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await this.next(context);
            }
            catch (Exception e)
            {
                StackFrame frame;
                string fileName;
                string method;
                string apiPath;
                string requestPath;
                string message;
                int srcIndex;
                int lineNumber;
                bool apiRequest;

                //*POI
                //The use of this StackTrace object is the only reason this middleware is not compatible with DNX Core.
                frame = new StackTrace(e, true).GetFrame(0);

                fileName = frame.GetFileName();
                srcIndex = fileName.IndexOf("src");
                if (srcIndex >= 0)
                {
                    fileName = fileName.Substring(srcIndex + 3, fileName.Length - (srcIndex + 3));
                }
                else
                {
                    fileName = fileName.Replace(env.WebRootPath, "");
                }
                method = frame.GetMethod().DeclaringType.FullName + "." + e.TargetSite.Name + "()";
                lineNumber = frame.GetFileLineNumber();

                if (this.logExceptions)
                {
                    //Spawn a thread that logs the exception in a SQL Server database.
                    var t = Task.Run(() => { LogException(context, e, fileName, method, lineNumber); });
                }

                requestPath = (string)context.Request.Path;
                apiRequest = false;

                //Check if the request was for a Web API method by looping through all the Web API paths 
                //supplied by the parent application.
                foreach (string path in this.apiPaths)
                {
                    apiPath = path.Trim();

                    //Format the current Web API path so it can be reliably compared to the Request path
                    /*************************************************************************************/
                    //Make sure the path begins with a forward slash
                    if (apiPath.Substring(0, 1) != "/")
                    {
                        apiPath = "/" + path;
                    }

                    //Make sure the path does not end with a forward slash
                    if (apiPath.Substring(apiPath.Length - 1, 1) == "/")
                    {
                        apiPath = apiPath.Substring(0, apiPath.Length - 1);
                    }
                    /*************************************************************************************/

                    //Check if the current Web API path matches the base path of the Request.
                    if (apiPath.Length <= requestPath.Length && apiPath.ToLower() == requestPath.Substring(0, apiPath.Length).ToLower())
                    {
                        apiRequest = true;
                        break;
                    }
                }

                //Check if the Request was for a Web API method.
                if (apiRequest)
                {
                    context.Response.Clear();
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "text/html";
                    if (env.IsDevelopment())
                    {
                        //Return a detailed error message in the Response Body.
                        message = BuildExceptionHtml(e, fileName, method, lineNumber);
                    }
                    else
                    {
                        //Return a generic error message in the Response Body
                        message = "Internal server error";
                    }
                    await context.Response.Body.WriteAsync(System.Text.Encoding.UTF8.GetBytes(message), 0, message.Length);
                }
                else
                {
                    //The request was not for a Web API method.  The body of the response should be an HTML error page.
                    if (env.IsDevelopment())
                    {
                        //Allow the error handling middleware up the pipeline to generate a detailed error report.
                        ExceptionDispatchInfo.Capture(e).Throw();
                    }
                    else
                    {
                        //Redirect to the error page supplied by the parent application.
                        context.Response.Redirect(this.errorPage);
                    }

                }
            }
        }

        /// <summary>
        /// Inserts a record into the ExceptionLog table that contains the details of the exception and client.
        /// </summary>
        /// <param name="context">The current HttpContext.</param>
        /// <param name="e">The exception that is being logged.</param>
        /// <param name="fileName">The relative path of the file that the exception occurred in.</param>
        /// <param name="method">The method the exception occurred in.</param>
        /// <param name="lineNumber">The line number that the exception occurred at.</param>
        private void LogException(HttpContext context, Exception e, string fileName, string method, int lineNumber)
        {
            ExceptionLog logData;
            string userName;
            string message;
            string clientIp;

            message = (e.Message.Length > 512) ? e.Message.Substring(0, 512) : message = e.Message;
            
            //*POI
            //As of ASP.NET Core RC1 there is a bug that causes HttpContext.Connection.RemoteIpAddress to be randomly set to null.
            //When this happens, often HttpContext.User.Identity.Name is null also even if the user is authenticated.
            //Sometimes cleaning the solution and recompilling temporarily fixes the problem.
            if (context.Connection.RemoteIpAddress == null)
            {
                userName = (string.IsNullOrWhiteSpace(context.User.Identity.Name)) ? "Unknown" : context.User.Identity.Name;
                clientIp = "Unknown";
            }
            else
            {
                userName = (string.IsNullOrWhiteSpace(context.User.Identity.Name)) ? "Anonymous" : context.User.Identity.Name;
                clientIp = context.Connection.RemoteIpAddress.ToString();
            }

            logData = new ExceptionLog()
            {
                Id = Guid.NewGuid().ToString(),
                ExDate = DateTime.Now,
                Message = message,
                FileName = fileName,
                Method = method,
                LineNum = lineNumber,
                UserName = userName,
                ClientIp = clientIp.ToString()
            };

            this.dbContext.ExceptionLog.Add(logData);
            this.dbContext.SaveChanges();
        }

        /// <summary>
        /// Creates an html string for displaying the details of the exception in a div.
        /// </summary>
        /// <param name="e">The exception that is being handled.</param>
        /// <param name="fileName">The relative path of the file that the exception occurred in.</param>
        /// <param name="method">The method the exception occurred in.</param>
        /// <param name="lineNumber">The line number that the exception occurred at.</param>
        /// <returns>Details of the exception formatted as html.</returns>
        private string BuildExceptionHtml(Exception e, string fileName, string method, int lineNumber)
        {
            HtmlEncoder encoder;
            string html;

            encoder = new HtmlEncoder();

            html = "<style>" + Environment.NewLine +
                   "  .exceptiondetailmyapi td {" + Environment.NewLine +
                   "    vertical-align: top !important;" + Environment.NewLine +
                   "    padding-top: 5px !important;" + Environment.NewLine +
                   "  }" + Environment.NewLine + Environment.NewLine +
                   "  .exceptiondetailmyapi td:nth-child(1) {" + Environment.NewLine +
                   "    text-align: right; !important;" + Environment.NewLine +
                   "    font-weight: bold !important;" + Environment.NewLine +
                   "    white-space: nowrap !important;" + Environment.NewLine +
                   "    padding-right: 5px !important;" + Environment.NewLine +
                   "  }" + Environment.NewLine +
                   "</style>" + Environment.NewLine +
                   "<table class=\"exceptiondetailmyapi\"> " + Environment.NewLine +
                   "  <tr>" + Environment.NewLine +
                   "    <td>Exeception:</td>" + Environment.NewLine +
                   "    <td>" + e.Message + "</td>" + Environment.NewLine +
                   "  </tr>" + Environment.NewLine +
                   "  <tr>" + Environment.NewLine +
                   "    <td>&nbsp;</td>" + Environment.NewLine +
                   "    <td>&nbsp;</td>" + Environment.NewLine +
                   "  </tr>" + Environment.NewLine +
                   "  <tr>" + Environment.NewLine +
                   "    <td>File Name:</td>" + Environment.NewLine +
                   "    <td>" + fileName + "</td>" + Environment.NewLine +
                   "  </tr>" + Environment.NewLine +
                   "  <tr>" + Environment.NewLine +
                   "    <td>Method:</td>" + Environment.NewLine +
                   "    <td>" + encoder.HtmlEncode(method) + "</td>" + Environment.NewLine +
                   "  </tr>" + Environment.NewLine +
                   "  <tr>" + Environment.NewLine +
                   "    <td>Line Number:</td>" + Environment.NewLine +
                   "    <td>" + lineNumber.ToString() + "</td>" + Environment.NewLine +
                   "  </tr>" + Environment.NewLine +
                   "</table>";

            return html;
        }


    }
}
