using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using ProjectFlow.API.Services;
using ProjectFlow.Core.DTOs;
using ProjectFlow.Core.Entities;
using ProjectFlow.Core.Interfaces;
using System.Security.Claims;

namespace ProjectFlow.API.Endpoints
{
    public static class TimeEntryEndpoints
    {
        public static void MapTimeEntryEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/time-entries").WithTags("TimeEntries");

            group.MapGet("/task/{taskId}", GetTimeEntriesByTask).RequireAuthorization();
            group.MapGet("/active", GetActiveTimer).RequireAuthorization();
            group.MapPost("/", StartTimer).RequireAuthorization();
            group.MapPut("/{id}/stop", StopTimer).RequireAuthorization();
            group.MapPut("/{id}", UpdateTimeEntry).RequireAuthorization();
            group.MapDelete("/{id}", DeleteTimeEntry).RequireAuthorization();
        }

        private static async Task<IResult> GetTimeEntriesByTask(
            int taskId,
            ITimeEntryRepository repository,
            IMapper mapper)
        {
            var timeEntries = await repository.GetByTaskIdAsync(taskId);
            var timeEntryDtos = mapper.Map<IEnumerable<TimeEntryDto>>(timeEntries);
            return Results.Ok(timeEntryDtos);
        }

        private static async Task<IResult> GetActiveTimer(
            HttpContext context,
            ITimeEntryRepository repository,
            IMapper mapper)
        {
            var userId = GetCurrentUserId(context);
            if (userId == null) return Results.Unauthorized();

            var activeTimer = await repository.GetActiveTimerByUserIdAsync(userId.Value);
            if (activeTimer == null) return Results.Ok(null);

            var timeEntryDto = mapper.Map<TimeEntryDto>(activeTimer);
            return Results.Ok(timeEntryDto);
        }

        private static async Task<IResult> StartTimer(
            CreateTimeEntryDto createDto,
            HttpContext context,
            ITimeEntryRepository repository,
            ITaskRepository taskRepository,
            INotificationService notificationService,
            IMapper mapper)
        {
            var userId = GetCurrentUserId(context);
            if (userId == null) return Results.Unauthorized();

            // Check if user already has an active timer
            var existingTimer = await repository.GetActiveTimerByUserIdAsync(userId.Value);
            if (existingTimer != null)
                return Results.BadRequest("You already have an active timer. Stop it first.");

            // Verify task exists
            var task = await taskRepository.GetByIdAsync(createDto.TaskId);
            if (task == null) return Results.NotFound("Task not found");

            var timeEntry = mapper.Map<TimeEntry>(createDto);
            timeEntry.UserId = userId.Value;

            var createdEntry = await repository.CreateAsync(timeEntry);

            // DODAJ notification:
            await notificationService.NotifyTimerStarted(createdEntry);

            var timeEntryDto = mapper.Map<TimeEntryDto>(createdEntry);
            return Results.Created($"/api/time-entries/{createdEntry.Id}", timeEntryDto);
        }

        private static async Task<IResult> StopTimer(
            int id,
            HttpContext context,
            ITimeEntryRepository repository,
            INotificationService notificationService,
            IMapper mapper)
        {
            var userId = GetCurrentUserId(context);
            if (userId == null) return Results.Unauthorized();

            var timeEntry = await repository.GetByIdAsync(id);
            if (timeEntry == null) return Results.NotFound();

            if (timeEntry.UserId != userId.Value)
                return Results.Forbid();

            if (timeEntry.EndTime != null)
                return Results.BadRequest("Timer is already stopped");

            timeEntry.EndTime = DateTime.UtcNow;
            timeEntry.DurationMinutes = (int)(timeEntry.EndTime.Value - timeEntry.StartTime).TotalMinutes;

            var updatedEntry = await repository.UpdateAsync(timeEntry);
            await notificationService.NotifyTimerStopped(updatedEntry);
            var timeEntryDto = mapper.Map<TimeEntryDto>(updatedEntry);

            return Results.Ok(timeEntryDto);
        }

        private static async Task<IResult> UpdateTimeEntry(
            int id,
            UpdateTimeEntryDto updateDto,
            HttpContext context,
            ITimeEntryRepository repository,
            IMapper mapper)
        {
            var userId = GetCurrentUserId(context);
            if (userId == null) return Results.Unauthorized();

            var timeEntry = await repository.GetByIdAsync(id);
            if (timeEntry == null) return Results.NotFound();

            if (timeEntry.UserId != userId.Value)
                return Results.Forbid();

            mapper.Map(updateDto, timeEntry);
            var updatedEntry = await repository.UpdateAsync(timeEntry);
            var timeEntryDto = mapper.Map<TimeEntryDto>(updatedEntry);

            return Results.Ok(timeEntryDto);
        }

        private static async Task<IResult> DeleteTimeEntry(
            int id,
            HttpContext context,
            ITimeEntryRepository repository)
        {
            var userId = GetCurrentUserId(context);
            if (userId == null) return Results.Unauthorized();

            var timeEntry = await repository.GetByIdAsync(id);
            if (timeEntry == null) return Results.NotFound();

            if (timeEntry.UserId != userId.Value)
                return Results.Forbid();

            await repository.DeleteAsync(id);
            return Results.NoContent();
        }

        private static int? GetCurrentUserId(HttpContext context)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}