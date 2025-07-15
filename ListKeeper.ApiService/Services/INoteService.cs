// These 'using' statements import necessary namespaces.

using ListKeeper.ApiService.Models.ViewModels;

namespace ListKeeperWebApi.WebApi.Services
{
    public interface INoteService
    {
        Task<NoteViewModel?> CreateNoteAsync(NoteViewModel NoteVm);
        Task<bool> DeleteNoteAsync(int id);
        Task<bool> DeleteNoteAsync(NoteViewModel noteVm);
        Task<IEnumerable<NoteViewModel>> GetAllNotesAsync();
        Task<NoteViewModel?> GetNoteByIdAsync(int id);
        Task<NoteViewModel?> UpdateNoteAsync(NoteViewModel noteVm);
    }
}