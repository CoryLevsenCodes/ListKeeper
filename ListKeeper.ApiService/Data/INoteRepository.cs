// These 'using' statements import necessary namespaces from the .NET framework and our project.
using ListKeeper.ApiService.Models;

namespace ListKeeperWebApi.WebApi.Data
{
    public interface INoteRepository
    {
        Task<Note> AddAsync(Note note);
        Task<bool> Delete(int id);
        Task<bool> Delete(Note note);
        Task<IEnumerable<Note>> GetAllAsync();
        Task<Note?> GetByIdAsync(int id);
        Task<Note> Update(Note note);
    }
}