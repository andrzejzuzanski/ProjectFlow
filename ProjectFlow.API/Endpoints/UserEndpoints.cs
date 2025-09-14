using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ProjectFlow.Core.DTOs;
using ProjectFlow.Core.Entities;
using ProjectFlow.Core.Interfaces;
using ProjectFlow.Infrastructure.Data;
using ProjectFlow.Core.Validators;

namespace ProjectFlow.API.Endpoints
{
    public static class UserEndpoints
    {
        public static void MapUserEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/users").WithTags("Users");

            group.MapGet("/", GetUsers);
            group.MapGet("/{id}", GetUser);
            group.MapPost("/", CreateUser);
        }

        private static async Task<IResult> GetUsers(IUserRepository repository)
        {
            var users = await repository.GetAllActiveUsersAsync();

            var userDtos = users.Select(user => new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            });

            return Results.Ok(userDtos);
        }
        private static async Task<IResult> GetUser(int id, IUserRepository repository)
        {
            var user = await repository.GetByIdAsync(id);

            if (user == null)
                return Results.NotFound();

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };

            return Results.Ok(userDto);
        }
        private static async Task<IResult> CreateUser(CreateUserDto createUserDto, IUserRepository repository, IValidator<CreateUserDto> validator)
        {

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

            var user = new User
            {
                Email = createUserDto.Email,
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password),
                Role = createUserDto.Role,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var createdUser = await repository.CreateAsync(user);

            var newUserDto = new UserDto
            {
                Id = createdUser.Id,
                Email = createdUser.Email,
                FirstName = createdUser.FirstName,
                LastName = createdUser.LastName,
                Role = createdUser.Role,
                CreatedAt = createdUser.CreatedAt,
                IsActive = createdUser.IsActive
            };

            return Results.Created($"/api/users/{user.Id}", newUserDto);
        }
    }
}
