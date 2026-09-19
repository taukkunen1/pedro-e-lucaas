using ConquerSite.Models;
using Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace ConquerSite.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IWebHostEnvironment _environment;


        public AccountController(ILogger<AccountController> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult MyCharacter()
        {
            Account acc = @Utils.CurrentLoggedAccount(HttpContext.Session);
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("CharacterUID", acc.EntityID.ToString());
            Character c = RestApiHelper.GetRequest<Character>("Character", param);
            ViewBag.Account = acc;
            ViewBag.Character = c;
            string PathPlayerFace = System.IO.Path.Combine("images", "PlayerFace", c.Face.ToString() + ".png");
            string PlayerClassNames = System.IO.Path.Combine("conquer", "ProfessionalName.ini");
            string webRootPath = _environment.WebRootPath;
            string ClassNamesFilePath = System.IO.Path.Combine(webRootPath, PlayerClassNames);
            string AvatarFilePath = System.IO.Path.Combine(webRootPath, PathPlayerFace);
            string CharacterClassName = "Unknown";
            #region Obtain the Class name from file
            if (System.IO.File.Exists(ClassNamesFilePath))
            {
                foreach(string lineClassName in System.IO.File.ReadAllLines(ClassNamesFilePath))
                {
                    string classId = lineClassName.Split(',')[0];
                    if (classId == c.Class.ToString())
                    {
                        string className = lineClassName.Split(',')[1];
                        CharacterClassName = className;
                    }
                }
            }
            #endregion
            ViewBag.CharacterClassName = CharacterClassName;
            if (System.IO.File.Exists(AvatarFilePath)) {
                ViewBag.CharacterAvatar = "/" + PathPlayerFace;
            } else
            {
                ViewBag.CharacterAvatar = "/" + System.IO.Path.Combine("images", "PlayerFace", "296.png");
            }
            return View();
        }

        [HttpPost]
        public IActionResult RegisterPost(Account account)
        {
            List<Message> Messages = new();
            if (ModelState.IsValid)
            {
                Dictionary<string, string> param = new Dictionary<string, string>();
                param.Add("Username", account.Username);
                Account acc = RestApiHelper.GetRequest<Account>("Account", param);
                if (acc == null)
                {
                    account.IP = "";
                    if (RestApiHelper.PostRequestSuccessful("Account", account))
                    {
                        Messages.Add(new Message() { Text = $"Your account with username {account.Username} has been registered successfully", Type = TypeMessage.Success });
                    }
                }
                else
                {
                    Messages.Add(new Message() { Text = $"Username {account.Username} already exists, try with other.", Type = TypeMessage.Danger });
                }
            }
            else
            {
                foreach (ModelStateEntry value in ModelState.Values)
                {
                    if (value.Errors.Count > 0)
                    {
                        foreach (var errorMessage in value.Errors)
                        {
                            Messages.Add(new Message() { Text = errorMessage.ErrorMessage, Type = TypeMessage.Danger });
                        }
                    }
                }
            }
            ViewBag.Message = Messages;
            return View("Register");
        }

        [HttpPost]
        public IActionResult LoginPost(Account account)
        {
            List<Message> Messages = new();
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("Username", account.Username);
            Account acc = RestApiHelper.GetRequest<Account>("Account", param);
            if (acc != null)
            {
                // a senha vai no corpo do POST (nunca na URL) e a API confere o hash
                acc = RestApiHelper.PostRequestAs<Account>("AccountLogin", new { Username = account.Username, Password = account.Password });
                if (acc != null && acc.EntityID != 0)
                {
                    Utils.LoginAccount(HttpContext.Session, acc);
                    Messages.Add(new Message() { Text = $"Logged with your account {account.Username} successfully.", Type = TypeMessage.Success });
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    Messages.Add(new Message() { Text = $"Password for the account {account.Username} invalid.", Type = TypeMessage.Danger });
                }
            }
            else
            {
                Messages.Add(new Message() { Text = $"Username {account.Username} not exists.", Type = TypeMessage.Danger });
            }
            ViewBag.Message = Messages;
            return View("Login");
        }

        public IActionResult Logout()
        {
            Utils.LogoutAccount(HttpContext.Session);
            return RedirectToAction("Index", "Home");
        }
    }
}
