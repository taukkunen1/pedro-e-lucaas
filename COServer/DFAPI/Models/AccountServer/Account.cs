using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models.AccountServer
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
