using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ezyd.Controllers
{
    [HandleError]
    public class TutorialController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult friendlistTutorial()
        {
            return View();
        }

        public ActionResult createNewTransaction()
        {
            return View();
        }

        public ActionResult accAndExeTransactionTutorial()
        {
            return View();
        }

        public ActionResult HowItWorksCartoon()
        {
            return View();
        }

        public ActionResult whyUErrorToMeCartooon()
        {
            return View();
        }

        public ActionResult suspendOptimization()
        {
            return View();
        }
    }
}
