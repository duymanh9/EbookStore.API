using EbookStore.API.DTO;
using EbookStore.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace EbookStore.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly EbookStoreContext _context;
    private readonly IWebHostEnvironment _env;

    public CategoryController(EbookStoreContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }
    [HttpGet]
    public async Task<ActionResult> GetCategory()
    {
        var categories = await _context.Categories
            .Select(c => new CategoryResponseDto { CategoryId = c.CategoryId, CategoryName = c.CategoryName })
       .ToListAsync();
        return Ok(categories);
    }
}
