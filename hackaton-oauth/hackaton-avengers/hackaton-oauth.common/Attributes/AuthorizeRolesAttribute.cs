using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace hackaton_oauth.common.Attributes
{
    public class AuthorizeRolesAttribute : Attribute, IAuthorizationFilter
    {
        public AuthorizeRolesAttribute(string role)
        {
            Role = role;
        }

        public string Role { get; }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context != null)
            {
                List<Claim> roleClaims = context.HttpContext.User.FindAll(ClaimTypes.Role).ToList();
                if (!roleClaims.Any(x => x.Value == Role)) 
                {
                    context.Result = new UnauthorizedResult();
                }
            }
        }
    }
}
