namespace LibraryProject.Application.Categories;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryResponse>> GetAsync(CancellationToken cancellationToken);
}
