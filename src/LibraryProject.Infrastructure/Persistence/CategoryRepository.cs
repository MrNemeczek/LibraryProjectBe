using LibraryProject.Application.Repositories;
using LibraryProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryProject.Infrastructure.Persistence;

internal sealed class CategoryRepository(LibraryDbContext dbContext) : ICategoryRepository
{
    public async Task<IReadOnlyList<Category>> GetAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .ThenBy(category => category.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        var normalizedName = name?.Trim() ?? string.Empty;
        return dbContext.Categories.SingleOrDefaultAsync(category => category.Name == normalizedName, cancellationToken);
    }

    public void Add(Category category)
    {
        dbContext.Categories.Add(category);
    }
}
