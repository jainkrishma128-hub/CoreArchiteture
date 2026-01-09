using CommonArchitecture.Application.DTOs;
using FluentValidation;

namespace CommonArchitecture.Web.Validators;

public class CreateRoleMenuDtoValidator : AbstractValidator<CreateRoleMenuDto>
{
 public CreateRoleMenuDtoValidator()
 {
 RuleFor(x => x.RoleId)
 .GreaterThan(0).WithMessage("Role ID is required");

 RuleFor(x => x.MenuId)
 .GreaterThan(0).WithMessage("Menu ID is required");

 RuleFor(x => x)
 .Custom((obj, context) =>
 {
 if (!obj.CanCreate && !obj.CanRead && !obj.CanUpdate && !obj.CanDelete && !obj.CanExecute)
 {
 context.AddFailure("At least one permission must be selected");
 }
 });
 }
}

public class UpdateRoleMenuDtoValidator : AbstractValidator<UpdateRoleMenuDto>
{
 public UpdateRoleMenuDtoValidator()
 {
 RuleFor(x => x)
 .Custom((obj, context) =>
 {
 if (!obj.CanCreate && !obj.CanRead && !obj.CanUpdate && !obj.CanDelete && !obj.CanExecute)
 {
 context.AddFailure("At least one permission must be selected");
 }
 });
 }
}
