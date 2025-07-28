using ListKeeper.ApiService.Models;

namespace ListKeeperWebApi.WebApi.Data
{
    public interface INoteCategoryRepository
    {
        Task<NoteCategory?> GetByIdAsync(int id);
        Task<IEnumerable<NoteCategory>> GetAllAsync();
        Task<NoteCategory> AddAsync(NoteCategory category);
        Task<NoteCategory> Update(NoteCategory category);
        Task<bool> Delete(NoteCategory category);
        Task<bool> Delete(int id);
    }
}
