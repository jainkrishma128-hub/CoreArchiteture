using CommonArchitecture.Application.DTOs;
using FluentValidation;

namespace CommonArchitecture.Web.Validators;

public class CreateRoleDtoValidator : AbstractValidator<CreateRoleDto>
{
    public CreateRoleDtoValidator()
    {
        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("Role name is required")
            .MaximumLength(100).WithMessage("Role name must not exceed 100 characters");
    }
}

public class UpdateRoleDtoValidator : AbstractValidator<UpdateRoleDto>
{
    public UpdateRoleDtoValidator()
    {
        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("Role name is required")
            .MaximumLength(100).WithMessage("Role name must not exceed 100 characters");
    }
}

