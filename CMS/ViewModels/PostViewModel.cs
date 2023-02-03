using CMS.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace CMS.ViewModels
{
    public class PostViewModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [MinLength(2, ErrorMessage = "O campo {0} deve ter no mínimo {1} caracteres.")]
        [DisplayName("Título")]
        public string? Title { get; set; }

        [DisplayName("Imagem")]
        public IFormFile? ImageFile { get; set; }

        [DisplayName("Imagem")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [DisplayName("Conteúdo")]
        public string? Content { get; set; }

        [DisplayName("Categoria")]
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        public int CategoryId { get; set; }

        [DisplayName("Categoria")]
        public Category? Category { get; set; }
    }
}
