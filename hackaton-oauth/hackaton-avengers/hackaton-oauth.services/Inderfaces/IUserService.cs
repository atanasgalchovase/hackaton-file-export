using hackaton_oauth.api.Controllers;
using hackaton_oauth.common.Dtos;

namespace hackaton_oauth.services.Inderfaces
{
    public interface IUserService
    {
        Task<AuthenticateResponseDto> AuthenticateS2SUserAsync(AuthenticateRequestDto dto);

        Task<bool> VerifyUserTokenS2sAsync(VerifyTokenS2SrequestDto request);

        Task<AuthenticateResponseDto> AuthenticateAsync(AuthenticateRequestDto dto);

        Task<CreateUserResponseDto> CreateAsync(Guid userId, RegisterUserRequestDto dto);

        (byte[] passwordHash, byte[] passwordSalt) CreateHash(string password);
    }
}
