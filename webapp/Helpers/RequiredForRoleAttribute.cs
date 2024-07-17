using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using webapp.Areas.Identity.Data;

public class RequiredForRoleAttribute : ValidationAttribute
{
    private readonly string _role;

    public RequiredForRoleAttribute(string role)
    {
        _role = role;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var httpContextAccessor = validationContext.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
        var userManager = validationContext.GetService(typeof(UserManager<webappUser>)) as UserManager<webappUser>;

        if (httpContextAccessor == null || userManager == null)
        {
            return new ValidationResult("An error occurred while validating the role.");
        }

        var user = userManager.GetUserAsync(httpContextAccessor.HttpContext?.User).Result;
        if (user == null)
        {
            return new ValidationResult("User not found.");
        }

        var roles = userManager.GetRolesAsync(user).Result;

        if (roles.Contains(_role) && (value == null || string.IsNullOrEmpty(value.ToString())))
        {
            return new ValidationResult($"{validationContext.DisplayName} is required for role {_role}.");
        }

        return ValidationResult.Success;
    }
}
