namespace ListKeeper.ApiService.Models
{
    /// <summary>
    /// Domain model for search criteria used at the repository/data access layer
    /// </summary>
    public class SearchCriteria
    {
        /// <summary>
        /// Optional search text to filter notes by title or content
        /// </summary>
        public string? SearchText { get; set; }

        /// <summary>
        /// Optional flag to show only completed notes
        /// </summary>
        public bool? ShowOnlyCompleted { get; set; }

        /// <summary>
        /// Status filters: 0=All, 1=Upcoming, 2=Past Due, 3=Completed
        /// Can contain multiple values for checkbox list
        /// </summary>
        public int[] Statuses { get; set; } = new int[] { 0 }; // Default to "All"
    }
}
