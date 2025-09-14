using FluentValidation;
using ProjectFlow.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectFlow.Core.Validators
{
    public class CreateTaskDtoValidator : AbstractValidator<CreateTaskDto>
    {
        public CreateTaskDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Task title is required")
                .MaximumLength(300).WithMessage("Task title cannot exceed 300 characters");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

            RuleFor(x => x.ProjectId)
                .GreaterThan(0).WithMessage("Valid project ID is required");

            RuleFor(x => x.AssignedToId)
                .GreaterThan(0)
                .When(x => x.AssignedToId.HasValue)
                .WithMessage("Valid assignee ID is required when assigned");

            RuleFor(x => x.Priority)
                .IsInEnum().WithMessage("Invalid priority selected");

            RuleFor(x => x.DueDate)
                .GreaterThan(DateTime.Now)
                .When(x => x.DueDate.HasValue)
                .WithMessage("Due date must be in the future");
        }
    }
}
