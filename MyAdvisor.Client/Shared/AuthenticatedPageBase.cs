using Microsoft.AspNetCore.Components;
using MyAdvisor.Client.Services;

namespace MyAdvisor.Client.Shared
{
    public class AuthenticatedPageBase : ComponentBase
    {
        [Inject] protected AuthService AuthService { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            if (!await AuthService.IsAuthenticatedAsync())
                Nav.NavigateTo("/login");
        }

        protected async Task LogoutAsync()
        {
            await AuthService.LogoutAsync();
            Nav.NavigateTo("/login");
        }
    }
}
