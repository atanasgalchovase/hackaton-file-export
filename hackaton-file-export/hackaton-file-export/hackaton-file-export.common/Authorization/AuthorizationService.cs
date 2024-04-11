using hackaton_file_export.common.Dtos;
using hackaton_file_export.common.Interfaces;
using hackaton_file_export.common.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace hackaton_file_export.common.Authorization
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly HttpClient _httpClient;
        private readonly TokenValidationSettings _tokeValidationSettings;

        public AuthorizationService(IHttpClientFactory httpClientFactory, IOptions<TokenValidationSettings> tokenValidationOptions)
        {
            _httpClient = httpClientFactory.CreateClient("oauth");
            _tokeValidationSettings = tokenValidationOptions.Value;
        }

        public async Task<bool> VerifyToken(string token, string[] roles)
        {
            var requestContent = new StringContent(JsonSerializer.Serialize(new VerifyTokenRequestDto
            {
                UserToken = token,
                UserRoles = roles,
            }), Encoding.UTF8, "application/json");

            await AddDefaultHeaders();

            var response = await _httpClient.PostAsync(_tokeValidationSettings.VerifyTokenS2sPath, requestContent);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return false;

            var responseBody = await response.Content.ReadAsStringAsync();
            var isAuthenticated = JsonSerializer.Deserialize<bool>(responseBody);

            return isAuthenticated;
        }

        private async Task AddDefaultHeaders() 
        {
            var s2sToken = await GetS2sToken();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {s2sToken}");
        }

        private async Task<string> GetS2sToken()
        {
            var requestContent = new StringContent(JsonSerializer.Serialize(new AuthenticateS2SRequestDto
            {
                UserName = _tokeValidationSettings.UserName,
                Password = _tokeValidationSettings.Password
            }), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(_tokeValidationSettings.AuthenticateS2sPath, requestContent);
                var responseBody = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<AuthenticateResponseDto>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return responseObject.Token;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
