using System.ComponentModel.DataAnnotations;

namespace ProniaFrontToBack.ViewModels.Account;

public record RegisterVm
{
    [MinLength(3)] public string Name { get; set; }
    [Required] [MinLength(4)] public string Username { get; set; }
    [MinLength(3)] public string Surname { get; set; }

    [Required]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [DataType(DataType.Password), Compare(nameof(Password))]
    public string ConfirmPassword { get; set; }
}