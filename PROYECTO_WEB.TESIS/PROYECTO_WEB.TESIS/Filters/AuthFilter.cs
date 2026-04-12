using PROYECTO_WEB.TESIS.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;


namespace PROYECTO_WEB.TESIS.Filters
{
    public class AuthFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            if (!SessionHelper.EstaLogueado(session))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }
            base.OnActionExecuting(context);
        }
    }

    public class AdminOnlyFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            if (!SessionHelper.EstaLogueado(session) || !SessionHelper.EsAdministrador(session))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }
            base.OnActionExecuting(context);
        }
    }
}