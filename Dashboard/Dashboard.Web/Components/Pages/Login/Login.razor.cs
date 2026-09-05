using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace Dashboard.Web.Components.Pages.Login;

public partial class Login
{
    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    protected LoginModel Model { get; set; } = new();

    protected string? ErrorMessage { get; set; }

    protected class LoginModel
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}
