using ListKeeper.ApiService.Models.ViewModels;
namespace ListKeeper.ApiService.Models.Extensions
{
    public static class NoteCategoryMappingExtensions
    {
        public static NoteCategoryViewModel? ToViewModel(this NoteCategory? noteCategory)
        {
            // First, handle the case where the input object itself is null.
            if (noteCategory == null)
            {
                return null;
            }

            // Create a new view model and map the properties.
            return new NoteCategoryViewModel
            {
                NoteCategoryId = noteCategory.NoteCategoryId,
                Name = noteCategory.Name,
                Description = noteCategory.Description
            };
        }

        public static NoteCategory? ToDomain(this NoteCategoryViewModel? viewModel)
        {
            if (viewModel == null)
            {
                return null;
            }

            return new NoteCategory
            {
                NoteCategoryId = viewModel.NoteCategoryId,
                Name = viewModel.Name,
                Description = viewModel.Description
            };
        }
    }
}
