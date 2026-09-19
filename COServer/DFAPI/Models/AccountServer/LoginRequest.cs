namespace API.Models.AccountServer
{
    /// <summary>Corpo do POST api/AccountLogin. So usuario e senha; qualquer outro campo enviado e' ignorado.</summary>
    public class LoginRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }

    /// <summary>Corpo do POST api/AccountChangePassword.</summary>
    public class ChangePasswordRequest
    {
        public string Username { get; set; } = "";
        public string OldPassword { get; set; } = "";
        public string NewPassword { get; set; } = "";
    }
}
