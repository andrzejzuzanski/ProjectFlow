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
            Log.Information("Login attempt for {Email}", loginDto.Email);

            var validationResult = await validator.ValidateAsync(loginDto);

            if (!validationResult.IsValid)
            {
                Log.Warning("Login validation failed for {Email}: {Errors}",
                    loginDto.Email,
                    string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

                return Results.BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
            }

            var user = await repository.GetByEmailAsync(loginDto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                Log.Warning("Failed login attempt for {Email} - invalid credentials", loginDto.Email);
                return Results.BadRequest("Invalid email or password");
            }

            if(!user.IsActive)
            {
                Log.Warning("Login attempt for inactive user {Email} (ID: {UserId})", loginDto.Email, user.Id);
                return Results.BadRequest("Account is inactive");
            }

            var token = jwtService.GenerateToken(user);
            var refreshToken = jwtService.GenerateRefreshToken();

            Log.Information("User {UserId} ({Email}) successfully logged in", user.Id, user.Email);

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
            Log.Information("Registration attempt for {Email}", registerDto.Email);

            var validationResult = await validator.ValidateAsync(registerDto);
            if (!validationResult.IsValid)
            {
                Log.Warning("Registration validation failed for {Email}: {Errors}",
                    registerDto.Email,
                    string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                return Results.BadRequest(validationResult.Errors.Select(e => new
                {
                    Property = e.PropertyName,
                    Error = e.ErrorMessage
                }));
            }

            if (await userRepository.EmailExistsAsync(registerDto.Email))
            {
                Log.Warning("Registration failed for {Email} - email already exists", registerDto.Email);
                return Results.BadRequest("User with this email already exists");
            }

            var user = mapper.Map<User>(registerDto);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            var createdUser = await userRepository.CreateAsync(user);

            Log.Information("New user registered: {UserId} ({Email}) with role {Role}",
                  createdUser.Id, createdUser.Email, createdUser.Role);

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
