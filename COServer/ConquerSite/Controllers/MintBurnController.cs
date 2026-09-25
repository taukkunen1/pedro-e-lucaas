using ConquerSite.Models;
using Core;
using Core.Models.GameServer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ConquerSite.Controllers
{
    public class MintBurnController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { WriteIndented = true };

        public MintBurnController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public IActionResult Index()
        {
            Account account = Utils.CurrentLoggedAccount(HttpContext.Session);
            if (account == null || account.EntityID == 0)
                return RedirectToAction("Login", "Account");

            Character character = LoadCharacter(account.EntityID);
            List<PlayerItem> items = character?.UID > 0 ? RestApiHelper.GetPlayerItems(character.UID) ?? new List<PlayerItem>() : new List<PlayerItem>();
            List<MintBurnRecord> records = LoadRecords()
                .Where(x => x.AccountUid == account.EntityID)
                .OrderByDescending(x => x.MintedAt)
                .ToList();

            ViewBag.Account = account;
            ViewBag.Character = character;
            ViewBag.Items = items.OrderBy(x => x.Position).ThenBy(x => x.ItemId).ToList();
            ViewBag.ActiveMintedItemUids = records
                .Where(x => x.Status == MintBurnStatus.Minted)
                .Select(x => x.ItemUid)
                .ToHashSet();
            ViewBag.Records = records;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Mint(uint itemUid)
        {
            Account account = Utils.CurrentLoggedAccount(HttpContext.Session);
            if (account == null || account.EntityID == 0)
                return RedirectToAction("Login", "Account");

            Character character = LoadCharacter(account.EntityID);
            if (character == null || character.UID == 0)
                return RedirectWithMessage("Character not found.", TypeMessage.Danger);

            List<PlayerItem> items = RestApiHelper.GetPlayerItems(character.UID) ?? new List<PlayerItem>();
            PlayerItem item = items.FirstOrDefault(x => x.Uid == itemUid && x.EntityID == character.UID);
            if (item == null)
                return RedirectWithMessage("Item not found for your character.", TypeMessage.Danger);

            List<MintBurnRecord> records = LoadRecords();
            if (records.Any(x => x.ItemUid == itemUid && x.Status == MintBurnStatus.Minted))
                return RedirectWithMessage("This item is already minted.", TypeMessage.Danger);

            item.Locked = 1;
            item.ItemPoints = CalculateMintPower(item);
            if (RestApiHelper.UpdatePlayerItems(new UpdatePlayerItem { EntityUid = character.UID, PlayerItems = items }) < 0)
                return RedirectWithMessage("Unable to update the item.", TypeMessage.Danger);

            records.Add(new MintBurnRecord
            {
                AccountUid = account.EntityID,
                AccountName = account.Username,
                CharacterUid = character.UID,
                CharacterName = character.Name,
                ItemUid = item.Uid,
                ItemId = item.ItemId,
                MintPower = item.ItemPoints,
                Plus = item.Plus,
                Bless = item.Bless,
                SocketOne = item.SocketOne,
                SocketTwo = item.SocketTwo,
                Position = item.Position
            });
            SaveRecords(records);

            return RedirectWithMessage("Item minted and locked successfully.", TypeMessage.Success);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Burn(Guid id)
        {
            Account account = Utils.CurrentLoggedAccount(HttpContext.Session);
            if (account == null || account.EntityID == 0)
                return RedirectToAction("Login", "Account");

            List<MintBurnRecord> records = LoadRecords();
            MintBurnRecord record = records.FirstOrDefault(x => x.Id == id && x.AccountUid == account.EntityID);
            if (record == null || record.Status != MintBurnStatus.Minted)
                return RedirectWithMessage("Mint record not found.", TypeMessage.Danger);

            List<PlayerItem> items = RestApiHelper.GetPlayerItems(record.CharacterUid) ?? new List<PlayerItem>();
            PlayerItem item = items.FirstOrDefault(x => x.Uid == record.ItemUid);
            if (item != null)
            {
                item.Locked = 0;
                item.ItemPoints = 0;
                RestApiHelper.UpdatePlayerItems(new UpdatePlayerItem { EntityUid = record.CharacterUid, PlayerItems = items });
            }

            record.Status = MintBurnStatus.Burned;
            record.BurnedAt = DateTime.UtcNow;
            SaveRecords(records);

            return RedirectWithMessage("Mint burned and item unlocked successfully.", TypeMessage.Success);
        }

        private Character LoadCharacter(uint accountUid)
        {
            Dictionary<string, string> param = new Dictionary<string, string>
            {
                { "CharacterUID", accountUid.ToString() }
            };
            return RestApiHelper.GetRequest<Character>("Character", param);
        }

        private IActionResult RedirectWithMessage(string text, TypeMessage type)
        {
            TempData["MintBurnMessage"] = text;
            TempData["MintBurnMessageType"] = type == TypeMessage.Success ? "success" : "danger";
            return RedirectToAction("Index");
        }

        private uint CalculateMintPower(PlayerItem item)
        {
            uint socketScore = (uint)((item.SocketOne > 0 ? 250 : 0) + (item.SocketTwo > 0 ? 500 : 0));
            uint plusScore = (uint)item.Plus * 100;
            uint blessScore = (uint)item.Bless * 50;
            uint qualityScore = item.ItemId % 10 >= 8 ? 200u : 50u;
            return Math.Max(100u, qualityScore + socketScore + plusScore + blessScore + item.ItemPoints);
        }

        private List<MintBurnRecord> LoadRecords()
        {
            string path = RecordsPath();
            if (!System.IO.File.Exists(path))
                return new List<MintBurnRecord>();

            string json = System.IO.File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<MintBurnRecord>>(json, _jsonOptions) ?? new List<MintBurnRecord>();
        }

        private void SaveRecords(List<MintBurnRecord> records)
        {
            string path = RecordsPath();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            System.IO.File.WriteAllText(path, JsonSerializer.Serialize(records, _jsonOptions));
        }

        private string RecordsPath()
        {
            return Path.Combine(_environment.ContentRootPath, "LocalData", "MintBurn", "minted-items.json");
        }
    }
}
