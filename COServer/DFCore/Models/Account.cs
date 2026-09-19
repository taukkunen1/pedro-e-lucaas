using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models
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
        [RegularExpression("^[a-zA-Z0-9]{4,8}$", ErrorMessage = "Please use password without special characters and 4 to 8 characters"), MinLength(4)]
        public string Password { get; set; }
        public string Email { get; set; }
        public string IP { get; set; }
        public AccountState State { get; set; }
        public Account()
        {
        }

        public bool Exists()
        {
            return EntityID != 0;
        }
    }
}
