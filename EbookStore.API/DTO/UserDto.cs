namespace EbookStore.API.DTO
{
    public class UserDto
    {
        public int? Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }

        public string? Password { get; set; }
        public bool IsActive { get; set; }
        public string PhoneNumber { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public IFormFile? AvatarFile { get; set; }
    }
}
