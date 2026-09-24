using ConquerSite.Models;
using ConquerSite.Models.DTOs;
using Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.IO;

namespace ConquerSite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DowloadsSettings _downloadsSettings;

        public HomeController(ILogger<HomeController> logger, DowloadsSettings downloadsSettings)
        {
            _logger = logger;
            _downloadsSettings = downloadsSettings;
        }

        public IActionResult Index()
        {
            ServerStatusDTO status = new ServerStatusDTO
            {
                ApiOnline = false,
                GameServerOnline = false,
                ServerName = "Placebo",
                ClientPatch = _downloadsSettings.GetLastPatch() ?? "-"
            };

            try
            {
                ServerStatusDTO apiStatus = RestApiHelper.GetRequest<ServerStatusDTO>("status");
                if (apiStatus != null)
                {
                    status = apiStatus;
                    status.ClientPatch = _downloadsSettings.GetLastPatch() ?? "-";
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogWarning(ex, "Placebo status API unavailable.");
            }

            return View(status);
        }

        public IActionResult Downloads()
        {
            return View(_downloadsSettings);
        }

        public IActionResult ServerInfo()
        {
            return View();
        }

        public IActionResult Rankings()
        {
            RankingsDTO rankings = new RankingsDTO { Available = false };
            try
            {
                RankingsDTO apiRankings = RestApiHelper.GetRequest<RankingsDTO>("rankings");
                if (apiRankings != null)
                {
                    rankings = apiRankings;
                    rankings.Available = true;
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogWarning(ex, "Placebo rankings API unavailable.");
            }

            return View(rankings);
        }

        public IActionResult Updates()
        {
            return View(new UpdatesPageDTO { Posts = LoadUpdates() });
        }

        public IActionResult Update(string id)
        {
            UpdatePostDTO post = LoadUpdates()
                .FirstOrDefault(x => x.Slug.Equals(id ?? "", System.StringComparison.OrdinalIgnoreCase));

            if (post == null)
                return NotFound();

            return View(post);
        }

        private List<UpdatePostDTO> LoadUpdates()
        {
            try
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "updates.json");
                if (!System.IO.File.Exists(path))
                    return new List<UpdatePostDTO>();

                string json = System.IO.File.ReadAllText(path);
                return JsonSerializer.Deserialize<List<UpdatePostDTO>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })?.OrderByDescending(x => x.PublishedAt).ToList() ?? new List<UpdatePostDTO>();
            }
            catch (System.Exception ex)
            {
                _logger.LogWarning(ex, "Unable to load Placebo updates.");
                return new List<UpdatePostDTO>();
            }
        }

        public IActionResult Guides()
        {
            return View();
        }
        public IActionResult Shop()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet("xtremetop100.vote")]
        public IActionResult VotePostBack(string Custom, string VotingIP)
        {
            List<Message> Messages = new();
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("UID", Custom);
            Account acc = RestApiHelper.GetRequest<Account>("AccountByUID", param);
            if (acc != null)
            {
                Vote v = RestApiHelper.GetRequest<Vote>("Votes/Get", param);
                if (v != null)
                {
                    v.LastVoteDate = System.DateTime.Now;
                    v.Votes++;
                }
                RestApiHelper.PostRequestSuccessful("Votes/Add", new Core.Models.AddVote() { UID = uint.Parse(Custom) });
                Messages.Add(new Message() { Text = $"Has voted and obtained 1 VotePoints with your account with EntityID:{Custom} [IP: {VotingIP}].", Type = TypeMessage.Success });
            }
            else
            {
                Messages.Add(new Message() { Text = $"Cannot find the EntityID:{Custom} [IP: {VotingIP}] for apply the reward for vote.", Type = TypeMessage.Danger });
            }
            foreach (Message v in Messages)
            {
                System.IO.File.AppendAllText("Vote.log", $"[{System.DateTime.Now.ToShortDateString()}] {Messages.First().Text}{System.Environment.NewLine}");
            }
            return View("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
