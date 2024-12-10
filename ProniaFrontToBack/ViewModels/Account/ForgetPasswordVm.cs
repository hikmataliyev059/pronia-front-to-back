using System.ComponentModel.DataAnnotations;

namespace ProniaFrontToBack.ViewModels.Account;

public class ForgetPasswordVm
{
    [Required] [EmailAddress] public string Email { get; set; }
}