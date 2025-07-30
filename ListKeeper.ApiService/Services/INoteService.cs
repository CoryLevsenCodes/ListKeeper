using ListKeeper.ApiService.Models.ViewModels;

namespace ListKeeper.ApiService.Services.ListKeeperWebApi.WebApi.Services
{
    public interface INoteService
    {
        Task<NoteViewModel?> CreateNoteAsync(NoteViewModel createNoteVm);
        Task<bool> DeleteNoteAsync(int id);
        Task<bool> DeleteNoteAsync(NoteViewModel noteVm);
        Task<IEnumerable<NoteViewModel>> GetAllNotesAsync();
        Task<IEnumerable<NoteViewModel>> GetAllNotesAsync(int userId);
        Task<IEnumerable<NoteViewModel>> GetAllNotesBySearchCriteriaAsync(SearchCriteriaViewModel searchCriteria);
        Task<IEnumerable<NoteViewModel>> GetAllNotesBySearchCriteriaAsync(SearchCriteriaViewModel searchCriteria, int userId);
        Task<NoteViewModel?> GetNoteByIdAsync(int id);
        Task<NoteViewModel?> GetNoteByIdAsync(int id, int userId);
        Task<NoteViewModel?> UpdateNoteAsync(NoteViewModel noteVm);
    }
}