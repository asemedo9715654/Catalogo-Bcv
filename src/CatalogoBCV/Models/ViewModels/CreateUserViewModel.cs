using System.ComponentModel.DataAnnotations;

namespace CatalogoBCV.Models.ViewModels
{
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "O Username é obrigatório")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "A Password é obrigatória")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Password")]
        [Compare("Password", ErrorMessage = "As passwords não coincidem.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "A Função é obrigatória")]
        [Display(Name = "Função")]
        public int RoleId { get; set; }

        [Display(Name = "Conta Ativa")]
        public bool IsActive { get; set; } = true;
    }
}
