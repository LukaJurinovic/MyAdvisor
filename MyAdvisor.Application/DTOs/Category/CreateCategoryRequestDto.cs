using System.ComponentModel.DataAnnotations;

namespace MyAdvisor.Application.DTOs.Category
{
    public record CreateCategoryRequestDto(
        [Required, MinLength(1)] string Name,
        int? ParentCategoryId);
}
