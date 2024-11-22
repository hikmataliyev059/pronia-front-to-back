using System.ComponentModel.DataAnnotations;
using ProniaFrontToBack.Models.Base;

namespace ProniaFrontToBack.Models;

public class Slider : BaseEntity
{
    [Required, StringLength(20, ErrorMessage = "Length must be 20 characters or less.")]
    public string Title { get; set; }

    public string SubTitle { get; set; }
    public string Description { get; set; }
}