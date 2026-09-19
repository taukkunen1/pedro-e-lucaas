using ConquerSite.Models;
using Microsoft.AspNetCore.Http;

namespace ConquerSite
{
    public static class Utils
    {
        public static void LoginAccount(ISession CurrentSession, Account Account)
        {
            Extensions.SessionExtensions.Set<Account>(CurrentSession, "MyAccount", Account);
        }
        public static void LogoutAccount(ISession CurrentSession)
        {
            Extensions.SessionExtensions.Set<Account>(CurrentSession, "MyAccount", null);
        }
        public static Account CurrentLoggedAccount(ISession CurrentSession)
        {
            Account MyAccount = Extensions.SessionExtensions.Get<Account>(CurrentSession, "MyAccount");
            return MyAccount;
        }
    }
}
