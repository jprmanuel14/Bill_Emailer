using System;
using System.Threading.Tasks;
using BillColl_Main.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BillColl_Main.Filters
{
    /// <summary>
    /// Applies the admin role check to every action on the controller, not just the page.
    ///
    /// Placed on the controller rather than on Index alone because a regular user can
    /// still reach the data endpoints by typing the URL — including the write actions
    /// that grant roles. The sidebar gate is not authorization.
    ///
    /// Resolved through IUserContext so the nav and the enforcement share one source
    /// of truth. Returns 403 (Forbid) for non-admins.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AdminOnlyAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;

            var userContext = httpContext.RequestServices.GetService(typeof(IUserContext)) as IUserContext;

            if (userContext == null)
            {
                // No DI hook — fail closed rather than silently allow.
                context.Result = new ForbidResult();
                return;
            }

            var principal = httpContext.User;
            var currentUser = userContext.Resolve(principal);
            if (!currentUser.IsAdmin)
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }
    }
}