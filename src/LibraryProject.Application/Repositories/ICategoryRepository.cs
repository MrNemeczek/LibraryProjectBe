using LibraryProject.Domain.Entities;

namespace LibraryProject.Application.Repositories;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAsync(CancellationToken cancellationToken);
    Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken);
    void Add(Category category);
}
