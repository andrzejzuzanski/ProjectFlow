using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Win32;
using ProjectFlow.Core.DTOs;
using ProjectFlow.Core.DTOs.Auth;
using ProjectFlow.Core.Entities;
using ProjectFlow.Core.Interfaces;
using Serilog;

namespace ProjectFlow.API.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/auth").WithTags("Authentication");

            group.MapPost("/login", Login);
            group.MapPost("/register", Register);
        }

        private static async Task<IResult> Login(LoginDto loginDto, IUserRepository repository, IJwtService jwtService, IValidator<LoginDto> validator, IMapper mapper)
        {
            var validationResult = await validator.ValidateAsync(loginDto);

            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
            }

            var user = await repository.GetByEmailAsync(loginDto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return Results.BadRequest("Invalid email or password");
            }

            if(!user.IsActive)
            {
                return Results.BadRequest("Account is inactive");
            }

            var token = jwtService.GenerateToken(user);
            var refreshToken = jwtService.GenerateRefreshToken();

            var userDto = mapper.Map<UserDto>(user);
            var response = new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                User = userDto
            };

            return Results.Ok(response);
        }
        private static async Task<IResult> Register(
            RegisterDto registerDto,
            IUserRepository userRepository,
            IJwtService jwtService,
            IValidator<RegisterDto> validator,
            IMapper mapper)
        {
            var validationResult = await validator.ValidateAsync(registerDto);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors.Select(e => new
                {
                    Property = e.PropertyName,
                    Error = e.ErrorMessage
                }));
            }

            if (await userRepository.EmailExistsAsync(registerDto.Email))
            {
                return Results.BadRequest("User with this email already exists");
            }

            var user = mapper.Map<User>(registerDto);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            var createdUser = await userRepository.CreateAsync(user);
            var token = jwtService.GenerateToken(createdUser);
            var refreshToken = jwtService.GenerateRefreshToken();

            var userDto = mapper.Map<UserDto>(createdUser);
            var response = new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                User = userDto
            };

            return Results.Created($"/api/users/{createdUser.Id}", response);
        }
    }
}
