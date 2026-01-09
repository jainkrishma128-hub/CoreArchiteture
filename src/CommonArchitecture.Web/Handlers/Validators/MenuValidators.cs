using CommonArchitecture.Application.DTOs;
using FluentValidation;

namespace CommonArchitecture.Web.Validators;

public class CreateMenuDtoValidator : AbstractValidator<CreateMenuDto>
{
 public CreateMenuDtoValidator()
 {
 RuleFor(x => x.Name)
 .NotEmpty().WithMessage("Menu name is required")
 .MaximumLength(128).WithMessage("Menu name cannot exceed 128 characters");

 RuleFor(x => x.Url)
 .NotEmpty().WithMessage("Menu URL is required")
 .MaximumLength(256).WithMessage("Menu URL cannot exceed 256 characters");

 RuleFor(x => x.Icon)
 .MaximumLength(64).WithMessage("Icon class cannot exceed 64 characters");

 RuleFor(x => x.DisplayOrder)
 .GreaterThanOrEqualTo(0).WithMessage("Display order must be greater than or equal to 0");
 }
}

public class UpdateMenuDtoValidator : AbstractValidator<UpdateMenuDto>
{
 public UpdateMenuDtoValidator()
 {
 RuleFor(x => x.Name)
 .NotEmpty().WithMessage("Menu name is required")
 .MaximumLength(128).WithMessage("Menu name cannot exceed 128 characters");

 RuleFor(x => x.Url)
 .NotEmpty().WithMessage("Menu URL is required")
 .MaximumLength(256).WithMessage("Menu URL cannot exceed 256 characters");

 RuleFor(x => x.Icon)
 .MaximumLength(64).WithMessage("Icon class cannot exceed 64 characters");

 RuleFor(x => x.DisplayOrder)
 .GreaterThanOrEqualTo(0).WithMessage("Display order must be greater than or equal to 0");
 }
}
