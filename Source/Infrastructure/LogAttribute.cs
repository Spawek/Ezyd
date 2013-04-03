using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using ezyd.Models.ezyd;

namespace ezyd.Infrastructure
{
    public class LogAttribute : ActionFilterAttribute
    {
        private static void LogException(Exception exMsg)
        {
            try
            {
                //log it in DB
                var DB = new EzydInstantDB();
                DB.SqlNonQuery(string.Format(
                    "INSERT INTO exceptions (`message`, `data`, `stackTrace`, `date`) VALUES ('{0}', '{1}', '{2}', {3} )",
                    exMsg.Message.Replace('\'','?'),
                    exMsg.Data.ToString().Replace('\'', '?'),
                    exMsg.StackTrace.Replace('\'', '?'),
                    "NOW()"
                ));
            }
            catch (Exception)
            {
                try
                {
                    //log it in text file (if DB fail)
                    TextWriter logOutput = new StreamWriter(System.Web.HttpContext.Current.Request.MapPath("~/Infrastructure/Log/ControllerExceptions.txt"), true);
                    logOutput.WriteLine(String.Format("At time: {0}, on action executed following exception was NOT handled: {1}", DateTime.Now, exMsg.ToString()));
                    logOutput.Close();
                }
                catch (IOException)
                {
                    //log it in 2nd text file (if 1st text file fail)
                    TextWriter logOutput = new StreamWriter(System.Web.HttpContext.Current.Request.MapPath("~/Infrastructure/Log/ControllerExceptions2.txt"), true);
                    logOutput.WriteLine(String.Format("At time: {0}, on action executed following exception was NOT handled: {1}", DateTime.Now, exMsg.ToString()));
                    logOutput.Close();
                }
            }
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);

            if (filterContext.Exception != null)
                LogException(filterContext.Exception);
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            base.OnResultExecuting(filterContext);
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            base.OnResultExecuted(filterContext);

            //TODO make database out of it
            if (filterContext.Exception != null)
                LogException(filterContext.Exception);
        }

    }

}