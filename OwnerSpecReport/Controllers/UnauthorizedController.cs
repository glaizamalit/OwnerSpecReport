using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OwnerSpecReport.Controllers
{
    public class UnauthorizedController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
