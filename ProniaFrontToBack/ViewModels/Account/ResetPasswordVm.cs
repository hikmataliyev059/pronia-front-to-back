using System.ComponentModel.DataAnnotations;

namespace ProniaFrontToBack.ViewModels.Account;

public class ResetPasswordVm
{
    [Required]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }

    [DataType(DataType.Password), Compare(nameof(NewPassword))]
    public string ConfirmNewPassword { get; set; }

    public string userId { get; set; }
    
    public string token { get; set; }
}