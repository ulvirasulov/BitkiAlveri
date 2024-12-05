using ProniaFrontToBack.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProniaFrontToBack.Models
{
    public class Slider : BaseEntity
    {
        [Required, StringLength(20, ErrorMessage = "Title uzunlugu max 20 ola biler!")]
        public string Title { get; set; }
        [StringLength(100)]
        public string? ImgUrl { get; set; }
        public string SubTitle { get; set; }
        public string Description { get; set; }
        [NotMapped]
        public IFormFile File { get; set; }
    }
}
