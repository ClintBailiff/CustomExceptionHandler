**Visual Studio 2015 - ASP.NET 5 RC1**  
Middleware project that can be added to a Web Application or Web API project or compiled into a NuGet package.
Includes a demo Web Application that is a slightly modified Standard ASP.NET 5 Template project with no authentication.

The CustomExceptionHandler middleware catches exceptions in the Http pipeline and redirects to an error page or returns a response that contains the error message.  It returns an error message when an exception occurs during a call to a Web API method.  It knows the call is to a Web API method by the base routes that are passed to it in the Starup.Configure() method of the parent application.  It also logs the exceptions and client information in a SQL database.  It does the logging in a separate thread to improve performance.  The logging functionality can be disabled by a boolean parameter that is supplied when adding the middleware to the Http pipeline in the Starup.Configure() method of the parent application.

If the hosting environment is set to Debug and an exception does not occur in a defined Web API route, the exception is passed up the Http pipeline to be handled by the ASP.NET Developer Exception Page.  So obviously for this to work the UseDeveloperExceptionPage() call needs to be above the UseCustomExceptionHandler() call in the Starup.Configure() method of the parent application.  The ASP.NET Developer Exception Page is very good at displaying details of the exception so I didn't see the point of trying to recreate that functionality.

If an exception occurs in a defined Web API route, the Response Body is replaced with the details of the exception when the hosting environment is set to Debug or with a generic "Internal server error" message when the hosting environment is set to something other than Debug.

If the hosting environment is not set to Debug and an exception does not occur in a defined Web API route, the response is redirected to the error page that is passed in via parameter when adding the middleware to the Http pipeline in the Starup.Configure() method of the parent application.

Key areas of the code are marked by **_*POI_** (point of interest) in the comments.  Just search the solution for **_*POI_** to read about these key areas.

This is the second in a series of posts on ASP.NET Core examples that I am creating.  I've worked through many issues porting an MVC 5 application to ASP.NET Core and I thought there might be people out there who could benefit from the things I've learned.  Also, I want to have some examples for my own reference.

I hope this helps someone and any comments or suggestions are appreciated.


