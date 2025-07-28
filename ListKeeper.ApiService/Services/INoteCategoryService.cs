using ListKeeper.ApiService.Models.ViewModels;

namespace ListKeeperWebApi.WebApi.Services
{
    public interface INoteCategoryService
    {
        Task<NoteCategoryViewModel?> CreateCategoryAsync(NoteCategoryViewModel categoryVm);
        Task<NoteCategoryViewModel?> UpdateCategoryAsync(NoteCategoryViewModel categoryVm);
        Task<IEnumerable<NoteCategoryViewModel>> GetAllCategoriesAsync();
        Task<NoteCategoryViewModel?> GetCategoryByIdAsync(int id);
        Task<bool> DeleteCategoryAsync(int id);
        Task<bool> DeleteCategoryAsync(NoteCategoryViewModel categoryVm);
    }
}
