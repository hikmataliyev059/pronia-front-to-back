using ProniaFrontToBack.Models.Base;

namespace ProniaFrontToBack.Models;

public class Tag : BaseEntity
{
    public string Name { get; set; }    
    public List<TagProduct> TagProducts { get; set; }
}