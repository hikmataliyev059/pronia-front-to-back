using Microsoft.AspNetCore.Identity;

namespace ProniaFrontToBack.Models;

public class AppUser : IdentityUser
{
    public string Name { get; set; }
    public string SurName { get; set; }
    public string? VerificationCode { get; set; }
    public DateTime? VerificationCodeExpireTime { get; set; }
    public DateTime? VerificationCodeGeneratedAt { get; set; }
    public int? AccessFailedCount { get; set; }
    public bool IsVerified { get; set; }
}