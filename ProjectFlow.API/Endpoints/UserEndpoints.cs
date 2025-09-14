using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ProjectFlow.Core.DTOs;
using ProjectFlow.Core.Entities;
using ProjectFlow.Core.Interfaces;
using ProjectFlow.Core.Validators;
using ProjectFlow.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Serilog;

namespace ProjectFlow.API.Endpoints
{
    public static class UserEndpoints
    {
        public static void MapUserEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/users").WithTags("Users");

            group.MapGet("/", GetUsers).RequireAuthorization();
            group.MapGet("/{id}", GetUser).RequireAuthorization();
            group.MapPost("/", CreateUser).RequireAuthorization("Admin");
        }

        private static async Task<IResult> GetUsers(IUserRepository repository, IMapper mapper)
        {
            var users = await repository.GetAllActiveUsersAsync();
            var userDtos = mapper.Map<IEnumerable<UserDto>>(users);
            return Results.Ok(userDtos);
        }
        private static async Task<IResult> GetUser(int id, IUserRepository repository, IMapper mapper)
        {
            var user = await repository.GetByIdAsync(id);

            if (user == null)
                return Results.NotFound();

            var userDto = mapper.Map<UserDto>(user);
            return Results.Ok(userDto);
        }
        private static async Task<IResult> CreateUser(CreateUserDto createUserDto, IUserRepository repository, IValidator<CreateUserDto> validator, IMapper mapper)
        {
            Log.Information("Admin creating new user for {Email}", createUserDto.Email);

            var validationResult = await validator.ValidateAsync(createUserDto);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors.Select(e => new
                {
                    Property = e.PropertyName,
                    Error = e.ErrorMessage
                }));
            }

            if (await repository.EmailExistsAsync(createUserDto.Email))
                return Results.BadRequest("User with this email already exists");

            var user = mapper.Map<User>(createUserDto);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);

            var createdUser = await repository.CreateAsync(user);

            var newUserDto = mapper.Map<UserDto>(createdUser);

            Log.Information("New user created by admin: {UserId} ({Email}) with role {Role}",
                createdUser.Id, createdUser.Email, createdUser.Role);

            return Results.Created($"/api/users/{user.Id}", newUserDto);
        }
    }
}
