namespace ListKeeper.ApiService.Helpers
{
    public interface ICurrentUserHelper
    {
        /// <summary>
        /// Gets the current user's ID from the JWT token claims
        /// </summary>
        /// <returns>The current user's ID, or null if not found</returns>
        int? GetCurrentUserId();

        /// <summary>
        /// Gets the current user's role from the JWT token claims
        /// </summary>
        /// <returns>The current user's role, or null if not found</returns>
        string? GetCurrentUserRole();

        /// <summary>
        /// Gets the current user's username from the JWT token claims
        /// </summary>
        /// <returns>The current user's username, or null if not found</returns>
        string? GetCurrentUserName();

        /// <summary>
        /// Gets the current user's email from the JWT token claims
        /// </summary>
        /// <returns>The current user's email, or null if not found</returns>
        string? GetCurrentUserEmail();

        /// <summary>
        /// Checks if the current user is an admin
        /// </summary>
        /// <returns>True if the current user is an admin, false otherwise</returns>
        bool IsCurrentUserAdmin();

        /// <summary>
        /// Gets the current user's username from the JWT token claims (alias for GetCurrentUserName)
        /// </summary>
        /// <returns>The current user's username, or null if not found</returns>
        string? GetCurrentUsername();
    }
}
