using Microsoft.EntityFrameworkCore;
using ListKeeper.ApiService.Models;

namespace ListKeeperWebApi.WebApi.Data
{
    /// <summary>
    /// Repository layer for NoteCategory entity. Handles direct database operations for note categories.
    /// </summary>
    public class NoteCategoryRepository : INoteCategoryRepository
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<NoteCategoryRepository> _logger;
        private readonly IConfiguration _configuration;

        public NoteCategoryRepository(DatabaseContext context, ILogger<NoteCategoryRepository> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<NoteCategory?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Attempting to find note category by ID: {CategoryId}", id);
            try
            {
                return await _context.NoteCategories.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting note category by ID: {CategoryId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<NoteCategory>> GetAllAsync()
        {
            _logger.LogInformation("Attempting to get all note categories");
            try
            {
                return await _context.NoteCategories.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting all note categories");
                throw;
            }
        }

        public async Task<NoteCategory> AddAsync(NoteCategory category)
        {
            _logger.LogInformation("Attempting to add a new note category with name: {CategoryName}", category.Name);
            try
            {
                await _context.NoteCategories.AddAsync(category);
                await _context.SaveChangesAsync();
                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding note category with name: {CategoryName}", category.Name);
                throw;
            }
        }

        public async Task<NoteCategory> Update(NoteCategory category)
        {
            _logger.LogInformation("Attempting to update note category with ID: {CategoryId}", category.NoteCategoryId);
            try
            {
                _context.NoteCategories.Update(category);
                await _context.SaveChangesAsync();
                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating note category with ID: {CategoryId}", category.NoteCategoryId);
                throw;
            }
        }

        public async Task<bool> Delete(NoteCategory category)
        {
            _logger.LogInformation("Attempting to delete note category with ID: {CategoryId}", category.NoteCategoryId);
            try
            {
                _context.NoteCategories.Remove(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting note category with ID: {CategoryId}", category.NoteCategoryId);
                throw;
            }
        }

        public async Task<bool> Delete(int id)
        {
            _logger.LogInformation("Attempting to delete note category by ID: {CategoryId}", id);
            try
            {
                var category = await _context.NoteCategories.FindAsync(id);
                if (category == null)
                {
                    _logger.LogWarning("Note category with ID: {CategoryId} not found to delete", id);
                    return false;
                }
                return await Delete(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting note category by ID: {CategoryId}", id);
                throw;
            }
        }
    }
}
