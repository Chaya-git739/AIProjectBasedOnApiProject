using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models.DTO
{
    /// <summary>
    /// DTO לתורם
    /// </summary>
    public class DonorDTO
    {
       
        public int? Id { get; set; }
        
        /// <summary>שם התורם</summary>
        [Required(ErrorMessage = "שם התורם הוא חובה")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "שם התורם חייב להיות בין 2 ל-100 תווים")]
        public string Name { get; set; } = null!;
        
        /// <summary>דוא"ל של התורם</summary>
        [Required(ErrorMessage = "דוא״ל הוא חובה")]
        [EmailAddress(ErrorMessage = "דוא״ל אינו תקני")]
        public string Email { get; set; } = null!;
        
        /// <summary>כתובת של התורם</summary>
        [StringLength(200, ErrorMessage = "כתובת לא יכולה להיות יותר מ-200 תווים")]
        public string Address { get; set; } = string.Empty;

        /// <summary>רשימת המתנות שתרם התורם</summary>
        public List<GiftDTO> Gifts { get; set; } = new List<GiftDTO>();
    }
}
