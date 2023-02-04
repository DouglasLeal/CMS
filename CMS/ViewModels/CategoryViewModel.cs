using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace CMS.ViewModels
{
    public class CategoryViewModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [MinLength(2, ErrorMessage = "O campo {0} deve ter no mínimo {1} caracteres.")]
        [DisplayName("Nome")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [MinLength(2, ErrorMessage = "O campo {0} deve ter no mínimo {1} caracteres.")]
        public string? Slug { get; set; }
    }
}
