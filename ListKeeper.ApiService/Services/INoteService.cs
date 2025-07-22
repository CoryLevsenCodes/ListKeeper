using ListKeeper.ApiService.Models.ViewModels;

namespace ListKeeper.ApiService.Services.ListKeeperWebApi.WebApi.Services
{
    public interface INoteService
    {
        Task<NoteViewModel?> CreateNoteAsync(NoteViewModel createNoteVm);
        Task<bool> DeleteNoteAsync(int id);
        Task<bool> DeleteNoteAsync(NoteViewModel noteVm);
        Task<IEnumerable<NoteViewModel>> GetAllNotesAsync();
        Task<IEnumerable<NoteViewModel>> GetAllNotesBySearchCriteriaAsync(SearchCriteriaViewModel searchCriteria);
        Task<NoteViewModel?> GetNoteByIdAsync(int id);
        Task<NoteViewModel?> UpdateNoteAsync(NoteViewModel noteVm);
    }
}