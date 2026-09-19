using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConquerSite.Models
{
    [Table("accounts")]
    public class Account
    {
        [Key]
        public uint EntityID { get; set; }
        public enum AccountState : byte
        {
            ProjectManager = 2,
            Banned = 1,
            Player = 0
        }
        public string Username { get; set; }
        [RegularExpression("^[a-zA-Z0-9]{4,8}$", ErrorMessage = "Please use password without special characters and 4 to 8 characters"), MinLength(4)][Required]
        public string Password { get; set; }
        [Required]
        public string Email { get; set; }
        public string IP { get; set; }
        public AccountState State { get; set; }
        [NotMapped]
        public bool Banned { get; set; }
        public Account()
        {
                Banned = State == AccountState.Banned;
        }

        public bool Exists()
        {
            return EntityID != 0;
        }
    }
}
