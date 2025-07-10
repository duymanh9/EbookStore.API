using EbookStore.API.DTO;
using EbookStore.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

namespace EbookStore.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EbookController : ControllerBase
{
    private readonly EbookStoreContext _context;
    private readonly IWebHostEnvironment _env;

    public EbookController(EbookStoreContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // GET: api/ebook
    [HttpGet]
    public async Task<ActionResult> GetEbooks([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var query = _context.Ebooks
            .Include(e => e.Category)
            .AsQueryable();

        if (fromDate.HasValue)
        {
            query = query.Where(e => e.CreatedAt >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            query = query.Where(e => e.CreatedAt < toDate.Value.Date.AddDays(1));
        }

        var ebooks = await query
            .Select(e => new EbookResponseDto
            {
                EbookId = e.EbookId,
                Title = e.Title,
                Description = e.Description,
                Author = e.Author,
                FileUrl = e.FileUrl,        
                FileType = e.FileType,
                Price = e.Price,
                CreatedAt = e.CreatedAt,
                CategoryId = e.CategoryId,
                CategoryName = e.Category.CategoryName
            })
            .ToListAsync();

        return Ok(ebooks);
    }


    // GET: api/ebook/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Ebook>> GetEbook(int id)
    {
        var ebook = await _context.Ebooks.FindAsync(id);

        if (ebook == null)
            return NotFound();

        return ebook;
    }

    // POST: api/ebook
    [HttpPost]
    public async Task<ActionResult<Ebook>> CreateEbook([FromForm] EbookDto ebookDto)
    {
        string fileUrl = null;

        if (ebookDto.File != null)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsFolder);
            // af9c0a15-daa3-4a3a-8e34-9b04db927c8cenglish1.pdf 
            var fileName = Guid.NewGuid() + Path.GetExtension(ebookDto.File.FileName); // tạo ra file guid
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await ebookDto.File.CopyToAsync(stream);
            fileUrl = $"/uploads/{fileName}";
        }

        var ebook = new Ebook
        {
            Title = ebookDto.Title,
            Description = ebookDto.Description,
            Author = ebookDto.Author,
            FileUrl = fileUrl,
            FileType = ebookDto.FileType,
            Price = ebookDto.Price,
            CreatedAt = DateTime.Now,
            CategoryId = ebookDto.CategoryId
        };

        _context.Ebooks.Add(ebook);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEbook), new { id = ebook.EbookId }, ebook);
    }

    [HttpGet("download/{fileName}")]
    public IActionResult Download(string fileName)
    {
        var filePath = Path.Combine(_env.WebRootPath, "uploads", fileName);

        if (!System.IO.File.Exists(filePath))
            return NotFound("File not found.");

        var contentType = GetContentType(filePath);
        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        return File(fileBytes, contentType, fileName);
    }

    private string GetContentType(string path)
    {
        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(path, out var contentType))
        {
            contentType = "application/octet-stream";
        }
        return contentType;
    }

    // PUT: api/ebook/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEbook(int id, [FromForm] EbookDto ebookDto)
    {
        var ebook = await _context.Ebooks.FindAsync(id);
        if (ebook == null)
            return NotFound();

        ebook.Title = ebookDto.Title;
        ebook.Description = ebookDto.Description;
        ebook.Author = ebookDto.Author;
        ebook.FileType = ebookDto.FileType;
        ebook.Price = ebookDto.Price;
        ebook.CategoryId = ebookDto.CategoryId;

        if (ebookDto.File != null)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + Path.GetExtension(ebookDto.File.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await ebookDto.File.CopyToAsync(stream);

            ebook.FileUrl = $"/uploads/{fileName}";
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/ebook/5
    [HttpDelete("delete-multiple")]
    public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
    {
        if (ids == null || !ids.Any())
            return BadRequest("Danh sách ID rỗng.");

        var ebooks = await _context.Ebooks.Where(e => ids.Contains(e.EbookId)).ToListAsync();

        if (!ebooks.Any())
            return NotFound("Không tìm thấy ebook nào.");

        _context.Ebooks.RemoveRange(ebooks);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
