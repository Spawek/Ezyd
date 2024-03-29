﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ezyd.Models.helpers
{
    public class AjaxOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsAjaxRequest())
                filterContext.HttpContext.Response.Redirect("/error/Error404");
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {

        }
    }
}
