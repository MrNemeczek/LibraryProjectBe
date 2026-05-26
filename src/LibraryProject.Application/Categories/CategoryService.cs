using LibraryProject.Application.Repositories;

namespace LibraryProject.Application.Categories;

internal sealed class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryResponse>> GetAsync(CancellationToken cancellationToken)
    {
        var categories = await categoryRepository.GetAsync(cancellationToken);

        return categories
            .Select(category => new CategoryResponse(category.Id, category.Name))
            .ToList();
    }
}
