namespace ListKeeper.ApiService.Models.ViewModels
{
    public class NoteCategoryViewModel
    {
        public int NoteCategoryId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public ICollection<NoteViewModel> Notes { get; set; }

        public NoteCategoryViewModel()
        {
            Name = string.Empty;
            Description = string.Empty;
        }
    }
}
