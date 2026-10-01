using BillColl_Main.Options;
using BillColl_Main.Services;
using BillColl_Main.ViewModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BillColl_Main.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUsers users;
        private readonly AuthSettings authSettings;
        private Logger log = LogManager.GetCurrentClassLogger();

        private const string relayStateReturnUrl = "ReturnUrl";

        public AccountController(IUsers users, IOptions<AuthSettings> authSettingsAccessor)
        {
            this.users = users;
            authSettings = authSettingsAccessor.Value;
        }

        public IActionResult Index(string returnUrl = null)
        {
            if (!authSettings.EnableSso)
            {
                return View(new LoginViewModel { Username = authSettings.DevDefaultEmail });
            }

            // For OIDC, challenge to initiate the login flow
            var properties = new AuthenticationProperties
            {
                RedirectUri = returnUrl ?? Url.Content("~/")
            };
            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Local (non-SSO) login used when AuthSettings:EnableSso is false.
        /// Issues a cookie session for AuthSettings:DevDefaultEmail without querying the database,
        /// so the application remains usable even when the user tables are not yet created.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginManual(LoginViewModel model)
        {
            if (authSettings.EnableSso)
            {
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            var email = string.IsNullOrWhiteSpace(model.Username)
                ? authSettings.DevDefaultEmail
                : model.Username.Trim();

            await SignInAsync(email, email);
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// OIDC callback handler (signin-oidc).
        /// This is invoked automatically by the OpenIdConnect middleware after Azure Entra ID
        /// redirects back with the authorization code.
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        [Route("/signin-oidc")]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "/")
        {
            return await HandleExternalLogin(returnUrl);
        }

        /// <summary>
        /// OIDC callback handler (signout-callback-oidc).
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        [Route("/signout-callback-oidc")]
        public async Task<IActionResult> ExternalLogoutCallback(string returnUrl = "/")
        {
            return Redirect(returnUrl ?? "/");
        }

        private async Task<IActionResult> HandleExternalLogin(string returnUrl)
        {
            var result = await HttpContext.AuthenticateAsync(OpenIdConnectDefaults.AuthenticationScheme);
            if (!result.Succeeded || result.Principal == null)
            {
                return RedirectToAction("AccessDenied");
            }

            var userEmail = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                userEmail = result.Principal.FindFirst("preferred_username")?.Value;
            }

            var myuser = users.GetUser(userEmail);
            if (myuser == null)
            {
                return RedirectToAction("AccessDenied");
            }

            // Sign in with the cookie scheme using claims from the OIDC principal
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, result.Principal,
                new AuthenticationProperties { IsPersistent = false });

            return Redirect(returnUrl ?? "/");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return SignOut(
                new AuthenticationProperties { RedirectUri = "/" },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme
            );
        }

        private async Task SignInAsync(string username, string givenName)
        {
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, username));
            identity.AddClaim(new Claim(ClaimTypes.Name, username));
            identity.AddClaim(new Claim(ClaimTypes.Email, username));
            identity.AddClaim(new Claim(ClaimTypes.GivenName, string.IsNullOrEmpty(givenName) ? username : givenName));
            identity.AddClaim(new Claim(ClaimTypes.Role, "Administrator"));

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = false });
        }
    }
}
