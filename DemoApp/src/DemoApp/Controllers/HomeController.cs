using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;

namespace DemoApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult PageError()
        {
            bool showError;

            showError = true;

            if (showError)
            {
                throw new Exception("Test exception used to view the exception page");
            }

            return View();
        }

        public IActionResult ApiError()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
