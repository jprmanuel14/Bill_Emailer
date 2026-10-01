namespace BillColl_Main.Services
{
    /// <summary>
    /// The single source of truth for "who is signed in and what may they see".
    /// Both the navigation (Views/Shared/_Layout.cshtml) and the Utilities page read from this,
    /// so the role shown in the nav cannot drift from the role the page enforces.
    /// </summary>
    public interface IUserContext
    {
        /// <summary>
        /// Resolves the signed-in user's identity and role. Never throws: if HRIS or the
        /// primary database is unreachable the role degrades to regular user ("0").
        /// </summary>
        CurrentUser Resolve(System.Security.Claims.ClaimsPrincipal principal);
    }
}