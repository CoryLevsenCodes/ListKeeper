using System.ComponentModel.DataAnnotations;

namespace ListKeeper.ApiService.Models.ViewModels
{
    public class SearchCriteriaViewModel
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
