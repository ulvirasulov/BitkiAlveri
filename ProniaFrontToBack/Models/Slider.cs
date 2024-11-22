using ProniaFrontToBack.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace ProniaFrontToBack.Models
{
    public class Slider:BaseEntity
    {
        [Required, StringLength(20,ErrorMessage = "Title uzunlugu max 20 ola biler!")]
        public string Title{ get; set; }
        public string ImgUrl { get; set; }
        public string SubTitle { get; set; }
        public string Description { get; set; }

    }
}
