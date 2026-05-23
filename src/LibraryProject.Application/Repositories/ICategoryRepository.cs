using LibraryProject.Domain.Entities;

namespace LibraryProject.Application.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken);
    void Add(Category category);
}
