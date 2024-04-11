using hackaton_oauth.common.Attributes;
using hackaton_oauth.common.Dtos;
using hackaton_oauth.common.Models;
using hackaton_oauth.services.Inderfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Security.Claims;

namespace hackaton_oauth.api.Controllers
{
    /// <summary>
    /// The controller for handling user related requests.
    /// <c>CreatedAtAction</c> will not work if you use "Async" suffix in controller action name.
    /// You must specify <c>ActionName</c> attribute to fix this known issue.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController"/> class.
        /// </summary>
        /// <param name="userService">The user service.</param>
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Authenticates the user.
        /// </summary>
        /// <param name="dto">The request data.</param>
        /// <returns>The result containing user info and authorization token, if authentication was successful.
        /// </returns>
        [AllowAnonymous]
        [HttpPost("authenticate")]
        [ActionName(nameof(AuthenticateAsync))]
        public async Task<ActionResult<AuthenticateResponseDto>> AuthenticateAsync([FromBody] AuthenticateRequestDto dto)
        {
            try
            {
                return Ok(await _userService.AuthenticateAsync(dto));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = ex.Message });
            }
        }

        /// <summary>
        /// Verify user token.
        /// </summary>
        /// <returns> returns Ok if user has authenticated.
        /// </returns>
        [AuthorizeRolesAttribute("S2S")]
        [HttpPost("verify-user-token-s2s")]
        [ActionName(nameof(AuthenticateAsync))]
        public async Task<ActionResult<bool>> VerifyTokenS2sAsync([FromBody] VerifyTokenS2SrequestDto request)
        {
            try
            {
                var result = await _userService.VerifyUserTokenS2sAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = ex.Message });
            }
        }

        /// <summary>
        /// Authenticates s2s.
        /// </summary>
        /// <param name="dto">The request data.</param>
        /// <returns>The result containing user info and authorization token, if authentication was successful.
        /// </returns>
        [AllowAnonymous]
        [HttpPost("authenticate-user-s2s")]
        [ActionName(nameof(AuthenticateAsync))]
        public async Task<ActionResult<AuthenticateResponseDto>> AuthenticateS2sAsync([FromBody] AuthenticateRequestDto dto)
        {
            try
            {
                return Ok(await _userService.AuthenticateS2SUserAsync(dto));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = ex.Message });
            }
        }

        /// <summary>
        /// Create user
        /// </summary>
        /// <param name="dto">The request data.</param>
        /// <returns>The result containing created user info
        /// </returns>
        [Authorize]
        [AuthorizeRolesAttribute("SuperAdmin")]
        [HttpPost("create")]
        [ActionName(nameof(CreateUserAsync))]
        public async Task<ActionResult<CreateUserResponseDto>> CreateUserAsync([FromBody] RegisterUserRequestDto dto)
        {
            try
            {
                string userId = User.Identity.Name;
                if(userId == null)
                    return Unauthorized();

                return Ok(await _userService.CreateAsync(new Guid(userId), dto));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseMessage { Message = ex.Message });
            }
        }
    }
}
