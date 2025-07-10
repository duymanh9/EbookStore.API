using EbookStore.API.DTO;
using EbookStore.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;


namespace EbookStore.API.Controllers;

[Route("api/[controller]")]
[ApiController]

public class UsersController : ControllerBase
{
    private readonly EbookStoreContext _context;
    private readonly IWebHostEnvironment _env;

    public UsersController(EbookStoreContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // GET: api/Users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> getUsers()
    {
        return await _context.Users.OrderByDescending(x => x.CreatedAt).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();
        return user;
    }
    [HttpPost]
    public async Task<ActionResult<User>> CreateUser([FromForm] UserDto userDto)
    {
        string avatarUrl = null;
        if(userDto.AvatarFile != null)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsFolder);
            var fileName = Guid.NewGuid() + Path.GetExtension(userDto.AvatarFile.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await userDto.AvatarFile.CopyToAsync(stream);
            avatarUrl = $"/uploads/{fileName}";
        }

        var salt = GenerateSalt();
        var hash = HashPassword(userDto.Password, salt);

        var user = new User
        {
            Username = userDto.Username,
            Email = userDto.Email,
            PasswordSalt = salt,
            PasswordHash = hash,
            IsActive = userDto.IsActive,
            PhoneNumber = userDto.PhoneNumber,
            TwoFactorEnabled = userDto.TwoFactorEnabled,
            Avatar = avatarUrl,
            CreatedAt = DateTime.Now,
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);

    }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser( int id, [FromForm] UserDto userDto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();
        user.Username = userDto.Username;
        user.Email = userDto.Email;
        user.IsActive = userDto.IsActive;
        user.PhoneNumber = userDto.PhoneNumber;
        user.TwoFactorEnabled = userDto.TwoFactorEnabled;
        user.UpdatedAt = DateTime.Now;

        if(userDto.AvatarFile != null)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsFolder);
            var fileName = Guid.NewGuid() + Path.GetExtension(userDto.AvatarFile.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await userDto.AvatarFile.CopyToAsync(stream);
            user.Avatar = $"/uploads/{fileName}";
        }
        // Nếu có password mới thì cập nhật hash và salt
        if(!string.IsNullOrEmpty(userDto.Password))
        {
            var salt = GenerateSalt();
            var hash = HashPassword(userDto.Password, salt);
            user.PasswordSalt = salt;
            user.PasswordHash = hash;
        }
        await _context.SaveChangesAsync();
        return NoContent();
    }
    [HttpDelete("{id}")]

    public async Task<ActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return NoContent();
    }
    // Hàm sinh salt ngẫu nhiên
    private string GenerateSalt(int size = 16)
    {
        var rng = RandomNumberGenerator.Create();
        var saltBytes = new byte[size];
        rng.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);  //chuyển mảng byte thành một chuỗi Base64
    }
    // Hàm hash password với salt bằng SHA-256
    private string HashPassword(string password, string salt)
    {
        using var sha256 = SHA256.Create();
        var combined = Encoding.UTF8.GetBytes(password + salt);
        var hash = sha256.ComputeHash(combined);
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}