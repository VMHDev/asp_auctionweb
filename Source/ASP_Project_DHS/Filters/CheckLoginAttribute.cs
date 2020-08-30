using ASP_Project_DHS.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ASP_Project_DHS.Filters
{
    public class CheckLoginAttribute: ActionFilterAttribute
    {
        public bool RequiredPermission { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (CurrentContext.IsLogged() == false)
            {
                filterContext.Result = new RedirectResult("~/Account/Login");
                return;
            }


            if(this.RequiredPermission==true && CurrentContext.GetCurUser().Quyen == false)
            {
                filterContext.Result = new RedirectResult("~/ThongBao/Permission");
                return;  
            }

            base.OnActionExecuting(filterContext);
        }
    }
}