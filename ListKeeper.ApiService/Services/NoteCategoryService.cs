using global::ListKeeperWebApi.WebApi.Data;
using ListKeeper.ApiService.Models;
using ListKeeper.ApiService.Models.ViewModels;
using ListKeeper.ApiService.Models.Extensions;

namespace ListKeeperWebApi.WebApi.Services
{
    /// <summary>
    /// Service Layer for note category-related operations.
    /// </summary>
    public class NoteCategoryService : INoteCategoryService
    {
        private readonly INoteCategoryRepository _repo;
        private readonly ILogger<NoteCategoryService> _logger;
        private readonly IConfiguration _config;

        public NoteCategoryService(INoteCategoryRepository repo, ILogger<NoteCategoryService> logger, IConfiguration config)
        {
            _repo = repo;
            _logger = logger;
            _config = config;
        }

        public async Task<NoteCategoryViewModel?> CreateCategoryAsync(NoteCategoryViewModel categoryVm)
        {
            if (categoryVm == null) return null;

            var entity = new NoteCategory
            {
                Name = categoryVm.Name,
                Description = categoryVm.Description,
                NoteCategoryId = categoryVm.NoteCategoryId
            };

            var created = await _repo.AddAsync(entity);
            return created?.ToViewModel();
        }

        public async Task<NoteCategoryViewModel?> UpdateCategoryAsync(NoteCategoryViewModel categoryVm)
        {
            if (categoryVm == null) return null;

            var entity = await _repo.GetByIdAsync(categoryVm.NoteCategoryId);
            if (entity == null) return null;

            entity.Name = categoryVm.Name;
            entity.Description = categoryVm.Description;

            var updated = await _repo.Update(entity);
            return updated?.ToViewModel();
        }

        public async Task<IEnumerable<NoteCategoryViewModel>> GetAllCategoriesAsync()
        {
            var entities = await _repo.GetAllAsync();
            return entities?.Select(e => e.ToViewModel()).Where(vm => vm != null).Cast<NoteCategoryViewModel>() ?? Enumerable.Empty<NoteCategoryViewModel>();
        }

        public async Task<NoteCategoryViewModel?> GetCategoryByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return entity?.ToViewModel();
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            return await _repo.Delete(id);
        }

        public async Task<bool> DeleteCategoryAsync(NoteCategoryViewModel categoryVm)
        {
            if (categoryVm == null) return false;
            var entity = categoryVm.ToDomain();
            if (entity == null) return false;
            return await _repo.Delete(entity);
        }
    }
}
