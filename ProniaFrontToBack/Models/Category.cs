using ProniaFrontToBack.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace ProniaFrontToBack.Models
{
    public class Category : BaseEntity
    {
        [Required, StringLength(10,ErrorMessage="Max 10 element ola biler!"),MinLength(3,ErrorMessage="Min 3 element olmalidir!")]
        public string Name { get; set; }
        public List<Product>? Products { get; set; }


    }
}
