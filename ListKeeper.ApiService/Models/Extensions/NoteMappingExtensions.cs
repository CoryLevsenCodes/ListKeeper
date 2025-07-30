using ListKeeper.ApiService.Models.ViewModels;
namespace ListKeeper.ApiService.Models.Extensions
{
    public static class NoteMappingExtensions
    {
        public static NoteViewModel? ToViewModel(this Note? note)
        {
            // First, handle the case where the input object itself is null.
            if (note == null)
            {
                return null;
            }

            // Create a new view model and map the properties.
            return new NoteViewModel
            {
                Id = note.Id,
                Color = note.Color,
                Content = note.Content,
                DueDate = note.DueDate,
                IsCompleted = note.IsCompleted,
                Title = note.Title,
                UserId = note.UserId
            };
        }

        public static Note? ToDomain(this NoteViewModel? viewModel)
        {
            if (viewModel == null)
            {
                return null;
            }

            return new Note
            {
                Id = viewModel.Id,
                Color = viewModel.Color,
                Content = viewModel.Content,
                DueDate = viewModel.DueDate,
                IsCompleted = viewModel.IsCompleted,
                Title = viewModel.Title,
                UserId = viewModel.UserId
            };
        }
    }
}
