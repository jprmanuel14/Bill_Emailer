using System;

namespace BillColl_Main.Services
{
    /// <summary>
    /// Identity + role of the signed-in user, in the shape the front end expects.
    /// Property names are part of the contract with site.js / utilities.js.
    /// </summary>
    public sealed class CurrentUser
    {
        /// <summary>Role assigned in BCAT "User_Role". "0" means regular user.</summary>
        public const string RegularUserRole = "0";

        public string Username { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string UserRole { get; set; } = RegularUserRole;

        /// <summary>Anything other than the explicit regular-user role is treated as an admin.</summary>
        public bool IsAdmin => !string.IsNullOrWhiteSpace(UserRole)
                               && !string.Equals(UserRole, RegularUserRole, StringComparison.OrdinalIgnoreCase);

        public string ToJson()
        {
            // Cloud_Email is retained for backwards compatibility with the existing front end.
            return Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                Cloud_Email = Username,
                username = Username,
                User_Role = UserRole,
                employee_Name = EmployeeName
            });
        }
    }
}