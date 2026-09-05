using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using ScytheButler.data.DatabaseModels.LoginModels;

namespace Dashboard.Web.Components.Pages.Login;

public partial class Register
{
    [Inject]
    private UserManager<ApplicationUser> UserManager { get; set; } = default!;

    [Inject]
    private SignInManager<ApplicationUser> SignInManager { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    protected RegisterModel Model { get; set; } = new();

    protected string? ErrorMessage { get; set; }
    protected async Task HandleRegister()
    {
        ErrorMessage = null;

        var existingUser = await UserManager.FindByNameAsync(Model.Username);

        if (existingUser != null)
        {
            ErrorMessage = "That username is already taken.";
            return;
        }

        var user = new ApplicationUser
        {
            UserName = Model.Username
        };

        var result = await UserManager.CreateAsync(
            user,
            Model.Password);

        if (!result.Succeeded)
        {
            ErrorMessage = string.Join(
                " ",
                result.Errors.Select(error => error.Description));

            return;
        }

        Navigation.NavigateTo("/login");
    }

    protected class RegisterModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(
            50,
            MinimumLength = 3,
            ErrorMessage = "Username must be between 3 and 50 characters.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare(
            nameof(Password),
            ErrorMessage = "The passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
