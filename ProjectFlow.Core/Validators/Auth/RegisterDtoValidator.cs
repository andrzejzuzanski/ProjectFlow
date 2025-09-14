using FluentValidation;
using ProjectFlow.Core.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectFlow.Core.Validators.Auth
{
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(100).WithMessage("First name cannot exceed 100 characters")
                .Matches("^[a-zA-ZąćęłńóśźżĄĆĘŁŃÓŚŹŻ ]+$").WithMessage("First name can only contain letters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters")
                .Matches("^[a-zA-ZąćęłńóśźżĄĆĘŁŃÓŚŹŻ ]+$").WithMessage("Last name can only contain letters");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long")
                .MaximumLength(100).WithMessage("Password cannot exceed 100 characters");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Password confirmation is required")
                .Equal(x => x.Password).WithMessage("Passwords do not match");

            RuleFor(x => x.Role)
                .IsInEnum().WithMessage("Invalid role selected");
        }
    }
}
