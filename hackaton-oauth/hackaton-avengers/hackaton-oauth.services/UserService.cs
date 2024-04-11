using hackaton_oauth.common.Dtos;
using hackaton_oauth.common.Models;
using hackaton_oauth.services.Inderfaces;
using System.Reflection;
using System.Transactions;
using System;
using hackaton_oauth.data;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using hackaton_oauth.data.Models;
using hackaton_oauth.api.Controllers;
using Newtonsoft.Json.Linq;

namespace hackaton_oauth.services
{
    public class UserService : IUserService
    {
        private readonly DataContext _db;
        private readonly AppSettings _appSettings;

        public UserService(DataContext db, IOptions<AppSettings> appSettings)
        {         
            _db = db;
            _appSettings = appSettings.Value;
        }

        public async Task<AuthenticateResponseDto> AuthenticateS2SUserAsync(AuthenticateRequestDto dto)
        {
            var user = await _db.Users
                .Include(x => x.Role)
                .SingleOrDefaultAsync(x => x.Username == dto.Username && x.IsActive);

            if (user == null || user.Role.Name != "S2S")
                throw new InvalidOperationException("Username is incorrect.");

            if (!VerifyHash(dto.Password, user.PasswordHash, user.PasswordSalt))
            {
                await _db.SaveChangesAsync();
                throw new InvalidOperationException("Password is incorrect.");
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return new AuthenticateResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Token = CreateToken(user.Id.ToString())
            };
        }

        public async Task<AuthenticateResponseDto> AuthenticateAsync(AuthenticateRequestDto dto)
        {
            var user = await _db.Users.SingleOrDefaultAsync(x => x.Username == dto.Username && x.IsActive);

            if (user == null) 
                throw new InvalidOperationException("Username is incorrect.");

            if (!VerifyHash(dto.Password, user.PasswordHash, user.PasswordSalt))
            {
                await _db.SaveChangesAsync();
                throw new InvalidOperationException("Password is incorrect.");
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return new AuthenticateResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Token = CreateToken(user.Id.ToString())
            };
        }

        public async Task<CreateUserResponseDto> CreateAsync(Guid userId, RegisterUserRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new InvalidOperationException("Password is required.");

            var existingUser = await _db.Users.FirstOrDefaultAsync(
                x => x.Username == dto.Username || x.Email == dto.Email);

            if (existingUser?.Username == dto.Username)
                throw new InvalidOperationException(string.Format("Username '{0}' is already taken."));

            if (existingUser?.Email == dto.Email)
                throw new InvalidOperationException(string.Format("Email '{0}' is already taken."));

            var (passwordHash, passwordSalt) = CreateHash(dto.Password);

            var role = await _db.Roles.Where(x => x.Name == dto.RoleName).FirstOrDefaultAsync();
            if(role == null)
                throw new InvalidOperationException(string.Format("Must specify a valid role."));

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username,
                IsActive = true,
                Email = dto.Email,
                RoleId = role.Id,
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();

            return new CreateUserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = new RoleResponseDto 
                {
                    Id = role.Id,
                    Name = dto.RoleName
                },
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LastLoginAt = user.LastLoginAt,
                IsActive = user.IsActive
            };
        }

        public bool VerifyHash(string password, byte[] hash, byte[] salt)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty or whitespace only string.");
            if (hash.Length != 64)
                throw new ArgumentException("Invalid length of password hash (64 bytes expected).");
            if (salt.Length != 128)
                throw new ArgumentException("Invalid length of password salt (128 bytes expected).");

            using var hmac = new HMACSHA512(salt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            for (var i = 0; i < computedHash.Length; i++)
                if (computedHash[i] != hash[i])
                    return false;

            return true;
        }

        private string CreateToken(string userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, userId) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public (byte[] passwordHash, byte[] passwordSalt) CreateHash(string password)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Value cannot be empty or whitespace only string.");

            using var hmac = new HMACSHA512();
            return (
                passwordHash: hmac.ComputeHash(Encoding.UTF8.GetBytes(password)),
                passwordSalt: hmac.Key);
        }

        public async Task<bool> VerifyUserTokenS2sAsync(VerifyTokenS2SrequestDto request)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(request.UserToken);
              
                if (jwtSecurityToken.ValidTo < DateTime.UtcNow)
                    return false;
                
                var userId = jwtSecurityToken.Claims.FirstOrDefault(x => x.Type == "unique_user")?.Value; 
                if (userId == null)
                    return false;

                var user = await _db.Users
                   .Include(x => x.Role)
                   .SingleOrDefaultAsync(x => x.Id == Guid.Parse(userId) && x.IsActive);

                if(user == null || (request.UserRoles != null && !request.UserRoles.Any(x => x == user.Role.Name)))
                    return false;

                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }
    }
}
