namespace ProniaFrontToBack.Models.Base;

public abstract class BaseEntity
{
    public int Id { get; set; }
    
   // public DateTime CreateDate { get; set; }  yaradilma vaxtini, yeni BaseEntity ne vaxt yaradilibi onu saxliyiriq
   // public DateTime UpdateDate { get; set; }  deyisilme vaxtini 
   // heleki bu 2 si lazim deyil deye bele yaziriq  
}