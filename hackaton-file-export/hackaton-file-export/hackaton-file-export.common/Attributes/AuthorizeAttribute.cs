using hackaton_file_export.common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace hackaton_file_export.common.Attributes
{
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public AuthorizeAttribute(string[] Roles)
        {
            this.Roles = Roles;
        }

        public string[] Roles { get; }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context != null)
            {
                if (context.HttpContext.Request.Headers.Authorization.Count() == 0)
                {
                    context.Result = new UnauthorizedResult();
                }
                else 
                {
                    if (!context.HttpContext.Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
                    {
                        context.Result = new UnauthorizedResult();
                    }
                    else
                    {
                        var token = authorizationHeader.FirstOrDefault().Split(" ")[1];
                       
                        var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();
                        var isValid = authService.VerifyToken(token, Roles).Result;

                        if (!isValid)
                            context.Result = new UnauthorizedResult();
                    }
                }
            }
        }
    }
}
