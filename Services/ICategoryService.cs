using FUDTOs;

namespace Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDTO>> GetCategories();
        Task<CategoryDTO> GetCategoryById(short id);
        Task Create(CategoryDTO category);
        Task Update(CategoryDTO category);
        /*Task Delete(short id);*/
        Task<bool> Delete(short id);
    }
}
