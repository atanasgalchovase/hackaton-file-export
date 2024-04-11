using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace hackaton_file_export.common.Interfaces
{
    public interface IAuthorizationService
    {
        Task<bool> VerifyToken(string token, string[] roles);
    }
}
