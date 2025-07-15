using ListKeeper.ApiService.Models.ViewModels;
using ListKeeperWebApi.WebApi.Models;
using ListKeeperWebApi.WebApi.Models.ViewModels;

namespace ListKeeper.ApiService.Models.Extensions
{
    public static class NoteMappingExtensions
    {
        
        public static NoteViewModel? ToViewModel(this Note? note)
        {
            if (note == null)
            {
                return null;
            }

            return new NoteViewModel
            {
               Id = note.Id,
               Color = note.Color ?? string.Empty,
               Content = note.Content ?? string.Empty,
               DueDate = note.DueDate,
               IsCompleted = note.IsCompleted,
               Title = note.Title ?? string.Empty
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
                Title = viewModel.Title
            };
        }
    }
}
