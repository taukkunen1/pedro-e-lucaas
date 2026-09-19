using System.ComponentModel.DataAnnotations;

namespace ConquerSite.Models
{
    public class LoginAccountDTO
    {
        [Required]
        public string Username { get; set; }
        [RegularExpression("^[a-zA-Z0-9]{1,8}$", ErrorMessage = "Please use password without special characters and 1 to 8 characters"), MinLength(1)]
        [Required]
        public string Password { get; set; }
        public LoginAccountDTO()
        {
        }
    }
}
