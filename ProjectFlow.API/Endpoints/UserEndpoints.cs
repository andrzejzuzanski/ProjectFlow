using Microsoft.EntityFrameworkCore;
using ProjectFlow.API.DTOs;
using ProjectFlow.Core.Entities;
using ProjectFlow.Infrastructure.Data;

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

        private static async Task<IResult> GetUsers(ProjectFlowDbContext context)
        {
            var users = await context.Users
                .Where(u => u.IsActive)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt,
                    IsActive = u.IsActive
                })
                .ToListAsync();

            return Results.Ok(users);
        }
        private static async Task<IResult> GetUser(int id, ProjectFlowDbContext context)
        {
            var user = await context.Users.FindAsync(id);

            if(user == null || !user.IsActive)
            {
                return Results.NotFound();
            }

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
        private static async Task<IResult> CreateUser(CreateUserDto createUserDto, ProjectFlowDbContext context)
        {
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

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var newUserDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };

            return Results.Created($"/api/users/{user.Id}", newUserDto);
        }
    }
}
