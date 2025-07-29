using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ListKeeper.ApiService.Models.ViewModels
{
    public class NoteViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public string Color { get; set; }

        /// <summary> 
        /// Gets or sets the ID of the user who owns this note 
        /// </summary> 
        public int UserId { get; set; }

        public NoteViewModel()
        {
            Title = string.Empty;
            Content = string.Empty;
            Color = string.Empty;
        }
    }
}
