using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models.DTO
{
    /// <summary>
    /// DTO לקטגוריה
    /// </summary>
    public class CategoryDTO
    {
        /// <summary>מזהה ייחודי של הקטגוריה</summary>
        [Range(1, int.MaxValue, ErrorMessage = "מזהה הקטגוריה חייב להיות חיובי")]
        public int Id { get; set; }

        /// <summary>שם הקטגוריה (למשל: "חשמל", "ריהוט", "נופש")</summary>
        [Required(ErrorMessage = "שם הקטגוריה הוא חובה")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "שם הקטגוריה חייב להיות בין 2 ל-50 תווים")]
        public string Name { get; set; } = string.Empty;
    }
}
