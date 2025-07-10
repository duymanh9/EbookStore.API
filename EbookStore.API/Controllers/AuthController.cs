using EbookStore.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using static System.Net.WebRequestMethods;

namespace EbookStore.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly EbookStoreContext _context;
    private string OTP;

    public AuthController(EbookStoreContext context)
    {
        _context = context;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (user == null)
            return Unauthorized("Sai tài khoản hoặc mật khẩu");

        var hash = HashPassword(dto.Password, user.PasswordSalt);

        if (user.PasswordHash != hash)
        {
            user.FailedLoginAttempts = (user.FailedLoginAttempts ?? 0) + 1;
            if (user.FailedLoginAttempts > 5) user.IsActive = false;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Unauthorized("Sai tài khoản hoặc mật khẩu");
        }
        if (user.IsActive == false)
            return Unauthorized("Tài khoản đã bị khóa");

        /// damg nhap da yeu to MFA -> Multiple factor authen
        /// viet ham gen OTP -> 6 so : 123456
        ///  viet ham gui mail, len mail dang ky dich vu gui mail, key_scret
        ///  gui cho nguoi ta opt Mail.send(user.usermail
        ///  so sanh cai OTP tren trinh duyet nguoi ta nhap -  bang nhau nhau tiep tuc
        ///  xeoxeehquogkgqda




        user.LastLoginAt = DateTime.Now;
        user.FailedLoginAttempts = 0;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        if (user.TwoFactorEnabled != null && user.TwoFactorEnabled.Value == true)
        {
            OTP = GenerateOtp();
            // Lưu OTP và thời gian hết hạn vào DB hoặc cache (nên dùng DB, không dùng biến toàn cục)
            user.Otp = OTP; // Tạm lưu vào PasswordSalt (khuyến nghị tạo trường riêng)
            user.UpdatedAt = DateTime.Now;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            await SendOtpEmailAsync(OTP, user.Email);

            return Ok(new
            {
                requireOtp = true,
                tempUser = new { user.Id, user.Username, user.Email }
            });
        }

        // Trả về thông tin user và role
        return Ok(new
        {
            user.Id,
            user.Username,
            user.Avatar,
            user.Email,
            user.Role // "admin" hoặc "user"
        });
    }



    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Username == dto.Username || u.Email == dto.Email))
            return BadRequest("Username hoặc Email đã tồn tại");

        var salt = GenerateSalt();
        var hash = HashPassword(dto.Password, salt);

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordSalt = salt,
            PasswordHash = hash,
            Role = "user", // Mặc định là user
            IsActive = true,
            CreatedAt = DateTime.Now
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.Id && u.Username == dto.Username && u.Email == dto.Email);
        if (user == null)
            return Unauthorized("Không tìm thấy user");

        // Kiểm tra OTP (ở đây tạm lưu trong PasswordSalt, nên tạo trường riêng cho OTP và thời gian hết hạn)
        if (user.Otp != dto.Otp)
            return Unauthorized("OTP không đúng");

        // Xóa OTP sau khi xác thực thành công
        user.Otp = null;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        // Trả về thông tin user như khi login thành công
        return Ok(new
        {
            user.Id,
            user.Username,
            user.Avatar,
            user.Email,
            user.Role
        });
    }

    public class VerifyOtpDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Otp { get; set; }
    }


    // Helper methods
    private string GenerateOtp()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString(); // từ 100000 -> 999999
    }


    private string GenerateSalt(int size = 16)
    {
        var rng = RandomNumberGenerator.Create();
        var saltBytes = new byte[size];
        rng.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }
    private string HashPassword(string password, string salt)
    {
        using var sha256 = SHA256.Create();
        var combined = Encoding.UTF8.GetBytes(password + salt);
        var hash = sha256.ComputeHash(combined);
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

    private async Task SendOtpEmailAsync(string otp, string recipientEmail)
    {
        string fromEmail = "manhndph49482@gmail.com";
        string fromPassword = "xeox eehq uogk gqda";

        using (var smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential(fromEmail, fromPassword),
            EnableSsl = true,
        })
        {
            MailMessage message = new MailMessage
            {
                From = new MailAddress(fromEmail, "Ebook Store - Xác thực OTP"),
                Subject = "Mã OTP xác thực đăng ký",
                IsBodyHtml = true,
                Body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; color: #333; }}
                        .container {{ max-width: 500px; margin: auto; background: #fff; padding: 20px; border-radius: 8px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }}
                        .otp-code {{ font-size: 24px; font-weight: bold; color: #007bff; }}
                        .footer {{ margin-top: 20px; font-size: 12px; color: #888; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h2>Xác thực đăng ký tài khoản</h2>
                        <p>Chúng tôi đã nhận được yêu cầu đăng ký tài khoản của bạn. Vui lòng sử dụng mã OTP dưới đây để hoàn tất quá trình xác thực:</p>
                        <p class='otp-code'>{otp}</p>
                        <p>Lưu ý: Mã OTP có hiệu lực trong vòng 5 phút.</p>
                        <div class='footer'>
                            Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này hoặc liên hệ với chúng tôi để được hỗ trợ.
                        </div>
                    </div>
                </body>
                </html>"
            };

            message.To.Add(new MailAddress(recipientEmail));

            await smtpClient.SendMailAsync(message);
        }
    }


}

// DTOs
public class LoginDto
{
    public string Username { get; set; }
    public string Password { get; set; }
}
public class RegisterDto
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}