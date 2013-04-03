using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ezyd.Views.ResourcesHelper
{
    //FROM: http://forums.asp.net/t/1655383.aspx/1?how+do+you+use+local+resources+with+MVC+3+Razor
    public static class Extentions
    {
        public static string LclRes(this WebViewPage page, string key)
        {
            //this part is written by me //its auto
           /* if(Thread.CurrentThread.CurrentUICulture.Equals(CultureInfo.CreateSpecificCulture("pl-PL")))
                return page.ViewContext.HttpContext.GetLocalResourceObject(page.VirtualPath, key, Thread.CurrentThread.CurrentUICulture) as string; */
            //end of this part
            
            //there was some problem with coding 'ó' letter - using page.Html.raw(resource) helps
            //idea FROM: http://stackoverflow.com/questions/9190496/showing-text-from-resources-resx-in-javascript
            //HtmlString outStr;
          //  string resStr = page.ViewContext.HttpContext.GetLocalResourceObject(page.VirtualPath, key) as string; 
           // if(resStr != null)
          //      outStr = page.Html.Raw(resStr) as HtmlString;
          //  else
          //      outStr = new HtmlString(string.Empty);

          //  return outStr;

            //an old one... FROM: http://forums.asp.net/t/1655383.aspx/1
            return page.ViewContext.HttpContext.GetLocalResourceObject(page.VirtualPath, key) as string; 
        }
    }
}