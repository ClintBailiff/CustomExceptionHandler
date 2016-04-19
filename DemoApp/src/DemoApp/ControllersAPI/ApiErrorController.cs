using System;
using Microsoft.AspNet.Mvc;


namespace DemoApp.ControllersAPI
{
    [Route("API/ApiError")]
    public class ApiErrorController : Controller
    {
        // GET: API/ApiError
        [HttpGet]
        public IActionResult Get()
        {
            bool showError;

            showError = true;

            if (showError)
            {
                throw new Exception("Test exception for Web API calls");
            }

            return new ObjectResult("To test the API exception handling, set the showError bool to true in the ApiError controller.");
        }
    }
}
