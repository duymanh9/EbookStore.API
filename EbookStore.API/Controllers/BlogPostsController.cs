using EbookStore.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using EbookStore.API.DTO;

namespace EbookStore.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BlogPostsController : ControllerBase
{
    private readonly EbookStoreContext _context;
    private readonly IWebHostEnvironment _env;
    
    public BlogPostsController(EbookStoreContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
        
    }

   

    // GET: api/BlogPosts
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BlogPost>>> GetBlogPosts([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var query = _context.BlogPosts.AsQueryable();

        if (fromDate.HasValue)
        {
            // Lấy từ 00:00:00 của ngày fromDate
            query = query.Where(x => x.CreatedAt >= fromDate.Value.Date);
        }
        if (toDate.HasValue)
        {
            // Lấy đến 23:59:59 của ngày toDate
            query = query.Where(x => x.CreatedAt <= toDate.Value.Date.AddDays(1).AddTicks(-1));
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    // Get: api/BlogPosts/5
    [HttpGet("{id}")]
    public async Task<ActionResult<BlogPost>> GetBlogPost(int id)
    {
        var blogPost = await _context.BlogPosts.FindAsync(id);
        if (blogPost == null) 
            return NotFound();
        return blogPost;
    }
    // POST: api/BlogPosts
    [HttpPost]
    public async Task<ActionResult<BlogPost>> CreateBlog([FromForm] BlogPostDto blogPostDto)
    {
        string fileUrl = null;
        if (blogPostDto.File != null)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsFolder);
            var fileName = Guid.NewGuid() + Path.GetExtension(blogPostDto.File.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await blogPostDto.File.CopyToAsync(stream);
            fileUrl = $"/uploads/{fileName}";
        }

        var blog = new BlogPost
        {
            Title = blogPostDto.Title,
            Content = blogPostDto.Content,
            Image = fileUrl,
            CreatedAt = DateTime.Now

        };
        _context.BlogPosts.Add(blog);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetBlogPost), new { id = blog.PostId }, blog);
    }
    
    // PUT: api/BlogPosts/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBlog(int id, [FromForm] BlogPostDto blogPostDto)
    {
        var blog = await _context.BlogPosts.FindAsync(id); // Tìm bài viết cần cập nhật.
        if (blog == null)
            return NotFound();
        blog.Title = blogPostDto.Title;
        blog.Content = blogPostDto.Content; // Gán giá trị mới từ blogPostDto

        if (blogPostDto.File != null)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsFolder); // Tạo folder nếu chưa có
            var fileName = Guid.NewGuid() + Path.GetExtension(blogPostDto.File.FileName); // Tạo tên file duy nhất bằng Guid
            var filePath = Path.Combine(uploadsFolder, fileName); // Lưu ảnh vào thư mục.

            using var stream = new FileStream(filePath, FileMode.Create);
            await blogPostDto.File.CopyToAsync(stream);

            blog.Image = $"/uploads/{fileName}";
        }  
            await _context.SaveChangesAsync();
        return NoContent();
    }
    // DELETE: api/BlogPosts/5
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteBlogPost(int id)
    {
        var blogPost = await _context.BlogPosts.FindAsync(id); // Tìm bài viết trong database theo id
        if (blogPost == null) // Nếu không tìm thấy bài viết, trả về HTTP 404 
            return NotFound();

        _context.BlogPosts.Remove(blogPost); // Xóa bài viết khỏi database.
        await _context.SaveChangesAsync();
        return NoContent();
    }
    // ...existing code...

    // DELETE :api/BlogPosts/delete-multiple
    [HttpPost("delete-multipath")]
    public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
    {
        if (ids == null || !ids.Any())  // Kiểm tra nếu danh sách rỗng hoặc null, trả về mã lỗi 400 
            return BadRequest("Danh sách rỗng");
        var blogs = await _context.BlogPosts.Where(b => ids.Contains(b.PostId)).ToArrayAsync(); // Lấy tất cả các bài viết trong DB có PostId nằm trong danh sách ids
        if (!blogs.Any())
            return NotFound("Không tìm thấy blog nào"); // Nếu không tìm thấy bài viết nào khớp với danh sách ids, trả về 404 
        _context.BlogPosts.RemoveRange(blogs);
        await _context.SaveChangesAsync();
        return NoContent(); // xóa thành công nhưng không có nội dung trả về.
    }
}
