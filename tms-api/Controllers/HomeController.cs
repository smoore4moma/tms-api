using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace tms_api.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Terms()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Examples()
        {
            return View();
        }
    }
}
