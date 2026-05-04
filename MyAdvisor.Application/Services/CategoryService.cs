using MyAdvisor.Application.DTOs.Category;
using MyAdvisor.Application.Interfaces.Repositories;
using MyAdvisor.Application.Interfaces.Services.Domain;
using MyAdvisor.Application.Mappers;
using MyAdvisor.Domain.Entities;

namespace MyAdvisor.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly CategoryMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, CategoryMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<CategoryDto>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return categories.Select(_mapper.ToDto).ToList();
        }

        public async Task<CategoryDto> CreateAsync(CreateCategoryRequestDto request)
        {
            var all = await _categoryRepository.GetAllAsync();
            if (all.Any(c => string.Equals(c.Name, request.Name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Category '{request.Name}' already exists.");

            var category = new Category(request.Name, request.ParentCategoryId);
            await _categoryRepository.AddAsync(category);
            return _mapper.ToDto(category);
        }

        public async Task<CategoryDto> EnsureCreatedAsync(string name)
        {
            var all = await _categoryRepository.GetAllAsync();
            var existing = all.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
            if (existing is not null)
                return _mapper.ToDto(existing);

            var category = new Category(name);
            await _categoryRepository.AddAsync(category);
            return _mapper.ToDto(category);
        }
    }
}
