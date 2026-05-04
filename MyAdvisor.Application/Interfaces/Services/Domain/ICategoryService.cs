using MyAdvisor.Application.DTOs.Category;

namespace MyAdvisor.Application.Interfaces.Services.Domain
{
    public interface ICategoryService
    {
        Task<IReadOnlyList<CategoryDto>> GetAllAsync();
        Task<CategoryDto> CreateAsync(CreateCategoryRequestDto request);
        Task<CategoryDto> EnsureCreatedAsync(string name);
    }
}
