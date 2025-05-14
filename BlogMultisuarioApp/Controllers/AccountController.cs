using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace BlogMultisuarioApp.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/Articulos/Index" },
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirigir a Cognito para el logout, pasando el parámetro client_id
            var clientId = "6nej8qanjv13q0kobf3glnr9d2"; // Tu Client ID de Cognito
            var redirectUri = "https://blogmultiusuarioapp-env.eba-h8cibmhk.us-east-1.elasticbeanstalk.com/";  // URL de redirección después de logout

            // La URL de logout de Cognito debe incluir el client_id y la URL de redirección
            var logoutUrl = $"https://us-east-1vtFErItEI.auth.us-east-1.amazoncognito.com/logout?client_id={clientId}&logout_uri={Uri.EscapeDataString(redirectUri)}";

            // Redirigir a Cognito para cerrar sesión
            return Redirect(logoutUrl);
        }

        public IActionResult Register()
        {
            var clientId = "6nej8qanjv13q0kobf3glnr9d2"; // tu Client ID
            var redirectUri = "https://blogmultiusuarioapp-env.eba-h8cibmhk.us-east-1.elasticbeanstalk.com/Account/Login"; // debe coincidir con lo configurado en Cognito
            var domain = "us-east-1vtFErItEI.auth.us-east-1.amazoncognito.com"; // tu dominio de Cognito

            var signupUrl = $"https://{domain}/signup?" +
                            $"client_id={clientId}&" +
                            $"response_type=code&" +
                            $"scope=openid+email+phone+profile&" +
                            $"redirect_uri={Uri.EscapeDataString(redirectUri)}";

            return Redirect(signupUrl);
        }
    }
}
