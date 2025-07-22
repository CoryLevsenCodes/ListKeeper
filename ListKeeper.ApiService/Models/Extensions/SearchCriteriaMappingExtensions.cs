using ListKeeper.ApiService.Models.ViewModels;

namespace ListKeeper.ApiService.Models.Extensions
{
    public static class SearchCriteriaMappingExtensions
    {
        /// <summary>
        /// Converts a SearchCriteriaViewModel to a domain SearchCriteria model
        /// </summary>
        public static SearchCriteria ToDomain(this SearchCriteriaViewModel? viewModel)
        {
            if (viewModel == null)
            {
                return new SearchCriteria();
            }

            return new SearchCriteria
            {
                SearchText = viewModel.SearchText,
                ShowOnlyCompleted = viewModel.ShowOnlyCompleted,
                Statuses = viewModel.Statuses ?? new int[] { 0 }
            };
        }

        /// <summary>
        /// Converts a domain SearchCriteria model to a SearchCriteriaViewModel
        /// </summary>
        public static SearchCriteriaViewModel ToViewModel(this SearchCriteria? model)
        {
            if (model == null)
            {
                return new SearchCriteriaViewModel();
            }

            return new SearchCriteriaViewModel
            {
                SearchText = model.SearchText,
                ShowOnlyCompleted = model.ShowOnlyCompleted,
                Statuses = model.Statuses ?? new int[] { 0 }
            };
        }
    }
}
