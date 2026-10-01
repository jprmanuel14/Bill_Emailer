using System.ComponentModel.DataAnnotations;

namespace BillColl_Main.Options
{
    public class AuthSettings
    {
        /// <summary>
        /// When true, authentication is delegated to Azure Entra ID (OIDC).
        /// When false, the application exposes a local manual login form (development only).
        /// </summary>
        public bool EnableSso { get; set; } = true;

        /// <summary>
        /// Identity issued to the session when manual (non-SSO) login is used.
        /// No database lookup is performed against tblUsers for this account.
        /// </summary>
        [Required]
        public string DevDefaultEmail { get; set; } = "admin@pwc.com";
    }
}
