using BillColl_Main.Models;
using BillColl_Main.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;

namespace BillColl_Main.Services
{
    public class UserContext : IUserContext
    {
        private readonly IUserRole userRole;
        private readonly IEmployeeDirectory employeeDirectory;
        private readonly AuthSettings authSettings;
        private readonly bool isDevelopment;

        private static int devFallbackLogged;

        public UserContext(IUserRole userRole, IEmployeeDirectory employeeDirectory
            , IOptions<AuthSettings> authSettings, IHostEnvironment environment, ILogger<UserContext> logger)
        {
            this.userRole = userRole;
            this.employeeDirectory = employeeDirectory;
            this.authSettings = authSettings.Value;
            this.isDevelopment = environment.IsDevelopment();

            if (DevFallbackEnabled && Interlocked.Exchange(ref devFallbackLogged, 1) == 0)
            {
                logger.LogWarning(
                    "DEV role fallback is ACTIVE (Environment={Environment}, EnableSso={EnableSso}). " +
                    "User_Role rows are matched on signed-in name as well as employee code. " +
                    "This must never be enabled outside local development.",
                    environment.EnvironmentName, this.authSettings.EnableSso);
            }
        }

        /// <summary>
        /// The name-based User_Role lookup is a development convenience and nothing else.
        ///
        /// It exists because without HRIS no email can ever resolve to an employee code, which
        /// leaves no possible admin and makes the Utilities page unreachable locally. It is
        /// gated on BOTH conditions so that:
        ///
        ///   - SSO switched on  → the fallback is off; login behaves exactly as it always did.
        ///   - any non-Development host → the fallback is off, even if SSO was misconfigured.
        ///
        /// Both are configuration values, so switching between local dev and a deployed
        /// environment never requires a code change.
        /// </summary>
        private bool DevFallbackEnabled => isDevelopment && !authSettings.EnableSso;

        public CurrentUser Resolve(ClaimsPrincipal principal)
        {
            var username = principal?.Identity?.Name ?? string.Empty;

            var current = new CurrentUser
            {
                Username = username,
                EmployeeName = username,
                UserRole = CurrentUser.RegularUserRole
            };

            var roles = userRole.GetUserRoles().ToList();

            // Normal path. HRIS is the authority for the employee code and User_Role is keyed
            // on it. This is the only path that runs once SSO is on.
            var employeeCode = employeeDirectory.GetEmployeeCodeByEmail(username);

            if (!string.IsNullOrWhiteSpace(employeeCode))
            {
                current.EmployeeName = employeeDirectory.GetNameByEmployeeCode(employeeCode);

                var role = roles.FirstOrDefault(r => MatchesCode(r, employeeCode));

                if (role != null && !string.IsNullOrWhiteSpace(role.User_Role))
                {
                    current.UserRole = role.User_Role;
                    return current;
                }
            }

            if (!DevFallbackEnabled)
            {
                return current;
            }

            // Development only. Grants access by matching the signed-in name against User_Role
            // so a local admin can be created without HRIS. Dev-only; never reached under SSO.
            var byName = roles.FirstOrDefault(r => MatchesName(r, username));

            if (byName != null && !string.IsNullOrWhiteSpace(byName.User_Role))
            {
                current.UserRole = byName.User_Role;

                if (!string.IsNullOrWhiteSpace(byName.Employee_Name))
                {
                    current.EmployeeName = byName.Employee_Name;
                }
            }

            return current;
        }

        /// <summary>HRIS path. Kept as Id or Employee_Code, which is the legacy behaviour.</summary>
        private static bool MatchesCode(Nre_Uer_New role, string value)
        {
            if (role == null || string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return string.Equals(role.Id, value, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(role.Employee_Code, value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Development-only path. Matches the signed-in name, never an employee code lookup.</summary>
        private static bool MatchesName(Nre_Uer_New role, string value)
        {
            if (role == null || string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return string.Equals(role.Employee_Code, value, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(role.Employee_Name, value, StringComparison.OrdinalIgnoreCase);
        }
    }
}