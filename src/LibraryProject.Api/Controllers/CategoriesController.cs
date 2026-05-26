using LibraryProject.Application.Categories;
using Microsoft.AspNetCore.Mvc;

namespace LibraryProject.Api.Controllers;

[Route("api/[controller]")]
public sealed class CategoriesController(ICategoryService categoryService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> Get(CancellationToken cancellationToken)
    {
        var response = await categoryService.GetAsync(cancellationToken);

        return Ok(response);
    }
}
