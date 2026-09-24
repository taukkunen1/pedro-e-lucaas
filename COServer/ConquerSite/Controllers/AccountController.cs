using ConquerSite.Models;
using ConquerSite.Models.DTOs;
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
            Account acc = Utils.CurrentLoggedAccount(HttpContext.Session);
            if (acc == null || acc.EntityID == 0)
                return RedirectToAction("Login");

            Character character = null;
            try
            {
                Dictionary<string, string> param = new Dictionary<string, string>
                {
                    { "CharacterUID", acc.EntityID.ToString() }
                };
                character = RestApiHelper.GetRequest<Character>("Character", param);
            }
            catch (System.Exception ex)
            {
                _logger.LogWarning(ex, "Unable to load Placebo character dashboard for UID {EntityID}.", acc.EntityID);
            }

            if (character == null || character.UID == 0)
            {
                return View(new CharacterDashboardDTO { Available = false });
            }

            string className = "Unknown";
            string classNamesFilePath = System.IO.Path.Combine(_environment.WebRootPath, "conquer", "ProfessionalName.ini");
            if (System.IO.File.Exists(classNamesFilePath))
            {
                foreach (string line in System.IO.File.ReadAllLines(classNamesFilePath))
                {
                    string[] parts = line.Split(',');
                    if (parts.Length >= 2 && parts[0] == character.Class.ToString())
                    {
                        className = parts[1];
                        break;
                    }
                }
            }

            string faceRelativePath = System.IO.Path.Combine("images", "PlayerFace", character.Face + ".png");
            string avatarFilePath = System.IO.Path.Combine(_environment.WebRootPath, faceRelativePath);
            string avatarUrl = System.IO.File.Exists(avatarFilePath)
                ? "/" + faceRelativePath.Replace('\\', '/')
                : "/images/PlayerFace/296.png";

            string uid = character.UID.ToString();
            string maskedUid = uid.Length > 4 ? "****" + uid.Substring(uid.Length - 4) : "****";
            string pkStatus = character.PkPoints >= 100 ? "Black name" : character.PkPoints >= 30 ? "Red name" : "Normal";
            uint hours = character.OnlineMinutes / 60;
            uint minutes = character.OnlineMinutes % 60;

            return View(new CharacterDashboardDTO
            {
                Available = true,
                Character = character,
                ClassName = className,
                AvatarUrl = avatarUrl,
                MaskedUid = maskedUid,
                PkStatus = pkStatus,
                OnlineTimeLabel = hours > 0 ? $"{hours}h {minutes}m" : $"{minutes}m",
                AutoJumpUnlocked = character.VipLevel >= 3,
                AutoPickupUnlocked = character.VipLevel >= 4
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
        public IActionResult LoginPost(LoginAccountDTO account)
        {
            List<Message> Messages = new();

            if (!ModelState.IsValid)
            {
                Messages.Add(new Message() { Text = "Invalid username or password.", Type = TypeMessage.Danger });
                ViewBag.Message = Messages;
                return View("Login", account);
            }

            // Use one generic authentication error so the login page does not reveal
            // whether a username exists.
            Account acc = RestApiHelper.PostRequestAs<Account>(
                "AccountLogin",
                new { Username = account.Username, Password = account.Password });

            if (acc != null && acc.EntityID != 0)
            {
                Utils.LoginAccount(HttpContext.Session, acc);
                return RedirectToAction("Index", "Home");
            }

            Messages.Add(new Message() { Text = "Invalid username or password.", Type = TypeMessage.Danger });
            ViewBag.Message = Messages;
            return View("Login", account);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            Utils.LogoutAccount(HttpContext.Session);
            return RedirectToAction("Index", "Home");
        }
    }
}
