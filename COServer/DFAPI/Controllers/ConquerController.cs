using API.Models;
using API.Models.AccountServer;
using API.Models.GameServer;
using AutoMapper;
using Core.Interfaces.GameServer;
using Core.Models.GameServer;
using Core.Models.SharedConfig;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api")]
    [ApiController]
    public class ConquerController : ControllerBase
    {
        private readonly ILogger<ConquerController> _logger;
        private readonly JsonAuthStore _Auth;
        private readonly GameDbContext _GameDbContext;
        private readonly IMapper _mapper;

        public ConquerController(ILogger<ConquerController> logger, JsonAuthStore authStore, GameDbContext gameDbContext, IMapper mapper)
        {
            _logger = logger;
            _Auth = authStore;
            _GameDbContext = gameDbContext;
            _mapper = mapper;
        }

        #region Accounts
        /// <summary>Nunca devolve o hash/senha da conta.</summary>
        private static Account Sanitize(Account a)
        {
            if (a == null) return null;
            return new Account { EntityID = a.EntityID, Username = a.Username, Email = a.Email ?? "", IP = a.IP ?? "", State = a.State, Password = "" };
        }

        [HttpGet("Accounts")]
        public async Task<List<Account>> GetAccounts()
        {
            return (await _Auth.GetAccountsAsync()).Select(Sanitize).ToList();
        }

        [HttpGet("Account")]
        public async Task<Account> GetAccountByUsername(string Username)
        {
            return Sanitize(await _Auth.GetAccountByUsernameAsync(Username));
        }

        [HttpGet("AccountByUID")]
        public async Task<Account> GetAccountByUID(uint UID)
        {
            return Sanitize(await _Auth.GetAccountByUidAsync(UID));
        }

        /// <summary>
        /// Login: confere usuario e senha no servidor (a senha vai no corpo, nunca na URL).
        /// Devolve a conta sem senha, ou 401. Conta antiga com senha em texto puro e' convertida para hash no primeiro login correto.
        /// </summary>
        [HttpPost("AccountLogin")]
        public async Task<ActionResult<Account>> AccountLogin([FromBody] LoginRequest Login)
        {
            if (Login == null || string.IsNullOrEmpty(Login.Username) || Login.Password == null) return Unauthorized();
            Account acc = await _Auth.GetAccountByUsernameAsync(Login.Username);
            if (acc == null)
            {
                // gasta o mesmo tempo de um login real, para nao revelar se o usuario existe
                Core.Security.PasswordHasher.Verify(Core.Security.PasswordHasher.Hash("x"), Login.Password, out _);
                return Unauthorized();
            }
            if (!Core.Security.PasswordHasher.Verify(acc.Password, Login.Password, out bool needsUpgrade)) return Unauthorized();
            if (needsUpgrade)
            {
                acc.Password = Core.Security.PasswordHasher.Hash(Login.Password);
                await _Auth.SaveAccountAsync(acc, false);
            }
            return Sanitize(acc);
        }

        /// <summary>
        /// Cria a conta (senha guardada como hash) ou, se ela ja existir, atualiza so IP e estado.
        /// A senha de uma conta existente NUNCA muda por aqui: use AccountChangePassword (exige a senha atual).
        /// </summary>
        [HttpPost("Account")]
        public async Task<ActionResult<Account>> PostAccount([FromBody]Account Account)
        {
            if (Account == null || string.IsNullOrWhiteSpace(Account.Username)) return BadRequest();
            Account acc = await _Auth.GetAccountByUsernameAsync(Account.Username);
            if (acc != null)
            {
                acc.IP = Account.IP;
                acc.State = Account.State;
                return Ok(Sanitize(await _Auth.SaveAccountAsync(acc, false)));
            }
            if (string.IsNullOrEmpty(Account.Password)) return BadRequest("Password required");
            Account.Password = Core.Security.PasswordHasher.Hash(Account.Password);
            return Ok(Sanitize(await _Auth.SaveAccountAsync(Account, true)));
        }

        /// <summary>Troca de senha: exige a senha atual.</summary>
        [HttpPost("AccountChangePassword")]
        public async Task<IActionResult> AccountChangePassword([FromBody] ChangePasswordRequest r)
        {
            if (r == null || string.IsNullOrEmpty(r.Username) || string.IsNullOrEmpty(r.NewPassword)) return BadRequest();
            Account acc = await _Auth.GetAccountByUsernameAsync(r.Username);
            if (acc == null || !Core.Security.PasswordHasher.Verify(acc.Password, r.OldPassword, out _)) return Unauthorized();
            acc.Password = Core.Security.PasswordHasher.Hash(r.NewPassword);
            await _Auth.SaveAccountAsync(acc, false);
            return NoContent();
        }

        [HttpDelete("Account/Delete")]
        public async Task<IActionResult> DeleteAccount([FromBody] Account Account)
        {
            return await _Auth.DeleteAccountAsync(Account.Username) ? NoContent() : Forbid();
        }
        #endregion

        #region Configurations
        [HttpGet("Configurations")]
        public async Task<List<Configuration>> GetConfigurations()
        {
            return await _Auth.GetConfigurationsAsync();
        }
        [HttpGet("Configuration")]
        public async Task<Configuration> GetConfiguration(string Key)
        {
            return await _Auth.GetConfigurationAsync(Key);
        }

        [HttpPost("Configuration")]
        public async Task<Configuration> PostConfiguration([FromBody] Configuration Configuration)
        {
            return await _Auth.SaveConfigurationAsync(Configuration);
        }
        #endregion

        #region Characters
        [HttpGet("Character")]
        public async Task<Character> GetCharacter(string CharacterUID)
        {
            Character l = new Character();
            var gsConfig = (await GetGSConfig());
            string DbLocation = gsConfig.GeneralDatabaseLocation;
            if (gsConfig.DbFromFiles != null && (bool)gsConfig.DbFromFiles)
            {
                string CharacterLocationFile = Path.Combine(DbLocation, "Users", CharacterUID + ".ini");
                if (System.IO.File.Exists(CharacterLocationFile))
                {
                    Core.IniFileHelper iniHelper = new Core.IniFileHelper(CharacterLocationFile);
                    l.UID = iniHelper.ReadUInt32("Character", "UID", 0);
                    l.Body = iniHelper.ReadUInt16("Character", "Body", 0);
                    l.Face = iniHelper.ReadUInt16("Character", "Face", 0);
                    l.Name = iniHelper.ReadString("Character", "Name", "None");
                    l.Class = iniHelper.ReadUInt16("Character", "Class", 0);
                    l.Level = iniHelper.ReadUInt16("Character", "Level", 0);
                    l.Reborn = iniHelper.ReadUInt16("Character", "Reborn", 0);
                    l.Map = iniHelper.ReadUInt16("Character", "Map", 0);
                    l.X = iniHelper.ReadUInt16("Character", "X", 0);
                    l.Y = iniHelper.ReadUInt16("Character", "Y", 0);
                    l.RacePoints = iniHelper.ReadUInt32("Character", "RacePoints", 0);
                }
            } else
            {
                ApiPlayer player = await GetPlayer(uint.Parse(CharacterUID));
                if (player != null)
                {
                    l.UID = player.UID;
                    l.Body = player.Body;
                    l.Face = player.Face;
                    l.Name = player.Name;
                    l.Class = player.Class;
                    l.Level = player.Level;
                    l.Reborn = player.Reborn;
                    l.Map = player.Map;
                    l.X = player.X;
                    l.Y = player.Y;
                    l.RacePoints = player.DonationPoints;
                }
            }
            return l;
        }
        #endregion

        #region Players
        [HttpGet("Players/Get")]
        public async Task<List<ApiPlayer>> GetPlayers()
        {
            return await _GameDbContext.Players.AsNoTracking().ToListAsync();
        }
        [HttpGet("Players/GetOne")]
        public async Task<ApiPlayer> GetPlayer(uint UID)
        {
            return await _GameDbContext.Players.Where(x => x.UID == UID).AsNoTracking().FirstOrDefaultAsync();
        }
        [HttpPost("Players/Set")]
        public async Task<bool> PostPlayers(List<ApiPlayer> players)
        {
            await _GameDbContext.Players.AddRangeAsync(players);
            return await _GameDbContext.SaveChangesAsync() > 0;
        }
        [HttpPost("Players/Update")]
        public async Task<bool> PostPlayersUpdate(List<ApiPlayer> players)
        {
            if (players == null || !players.Any())
                return false;
            foreach (var apiPlayer in players)
            {
                var existingPlayer = await _GameDbContext.Players.FirstOrDefaultAsync(p => p.UID == apiPlayer.UID);
                if (existingPlayer != null)
                {
                    foreach (var property in typeof(ApiPlayer).GetProperties())
                    {
                        if (property.CanWrite && property.Name != "Id")
                        {
                            var newValue = property.GetValue(apiPlayer);
                            property.SetValue(existingPlayer, newValue);
                        }
                    }
                }
            }
            return await _GameDbContext.SaveChangesAsync() > 0;
        }
        #endregion

        #region PlayerItems
        [HttpGet("PlayerItems/Get")]
        public async Task<List<ApiPlayerItem>> GetPlayerItems(uint EntityID)
        {
            return await _GameDbContext.PlayerItems.AsNoTracking().Where(x => x.EntityID == EntityID).ToListAsync();
        }
        [HttpPut("PlayerItems/Update")]
        public async Task<int> PutPlayerItems(UpdatePlayerItem updateItemsFromPlayer)
        {
            uint EntityID = updateItemsFromPlayer.EntityUid;
            List<uint> ExcludeFromRemoveUids = new List<uint>();
            foreach (PlayerItem item in updateItemsFromPlayer.PlayerItems)
            {
                ExcludeFromRemoveUids.Add(item.Uid);
                ApiPlayerItem itemx = await _GameDbContext.PlayerItems.Where(x => x.Uid == item.Uid).FirstOrDefaultAsync();
                if (itemx != null)
                {
                    itemx.Uid = item.Uid;
                    itemx.ItemId = item.ItemId;
                    itemx.EntityID = item.EntityID;
                    itemx.Durability = item.Durability;
                    itemx.MaxDurability = item.MaxDurability;
                    itemx.Position = item.Position;
                    itemx.SocketProgress = item.SocketProgress;
                    itemx.SocketOne = item.SocketOne;
                    itemx.SocketTwo = item.SocketTwo;
                    itemx.Effect = item.Effect;
                    itemx.Plus = item.Plus;
                    itemx.Bless = item.Bless;
                    itemx.Bound = item.Bound;
                    itemx.Enchant = item.Enchant;
                    itemx.Suspicious = item.Suspicious;
                    itemx.Locked = item.Locked;
                    itemx.PlusProgress = item.PlusProgress;
                    itemx.Inscribed = item.Inscribed;
                    itemx.Activate = item.Activate;
                    itemx.TimeLeftInMinutes = item.TimeLeftInMinutes;
                    itemx.StackSize = item.StackSize;
                    itemx.WarehouseId = item.WarehouseId;
                    itemx.Color = item.Color;
                    itemx.IDEvent = item.IDEvent;
                    itemx.PurificationItemID = item.PurificationItemID;
                    itemx.PurificationAddedOn = item.PurificationAddedOn;
                    itemx.PurificationDuration = item.PurificationDuration;
                    itemx.PurificationLevel = item.PurificationLevel;
                    itemx.EffectID = item.EffectID;
                    itemx.EffectAddedOn = item.EffectAddedOn;
                    itemx.EffectDuration = item.EffectDuration;
                    itemx.EffectLevel = item.EffectLevel;
                    itemx.EffectPercent = item.EffectPercent;
                    itemx.EffectPercent2 = item.EffectPercent2;
                    itemx.Fake = item.Fake;
                    itemx.UnlockTimer = item.UnlockTimer;
                    itemx.Locked = item.Locked;
                    itemx.Expiration = item.Expiration;
                }
                else
                {
                    ApiPlayerItem apiNewItem = _mapper.Map<ApiPlayerItem>(item);
                    await _GameDbContext.PlayerItems.AddAsync(apiNewItem);
                }
            }
            List<ApiPlayerItem> ToRemove = await _GameDbContext.PlayerItems.Where(x => x.EntityID == EntityID && !ExcludeFromRemoveUids.Contains(x.Uid)).ToListAsync();
            _GameDbContext.PlayerItems.RemoveRange(ToRemove);
            return await _GameDbContext.SaveChangesAsync();
        }
        #endregion

        #region PlayerSpells (Api Restfull)
        [HttpGet("PlayerSpells")]
        public async Task<List<ApiPlayerSpell>> GetPlayerSpells(uint PlayerUID = 0)
        {
            if (PlayerUID == 0)
            {
                return await _GameDbContext.PlayerSpells.ToListAsync();
            }
            return await _GameDbContext.PlayerSpells.Where(x => x.PlayerUid == PlayerUID).ToListAsync();
        }
        [HttpPost("PlayerSpells/{playerUid}")]
        public async Task<int> PostPlayerSpells(uint PlayerUid, List<ApiPlayerSpell> PlayerSpells)
        {
            if (PlayerSpells == null || PlayerSpells.Count == 0)
                return 0;
            foreach (var spell in PlayerSpells)
            {
                spell.PlayerUid = PlayerUid;
            }
            var existingTypeIds = await _GameDbContext.PlayerSpells
                .Where(x => x.PlayerUid == PlayerUid)
                .Select(x => x.TypeID)
                .ToListAsync();
            var spellsToAdd = PlayerSpells
                .Where(spell => !existingTypeIds.Contains(spell.TypeID))
                .ToList();
            if (spellsToAdd.Count == 0)
                return 0;
            await _GameDbContext.PlayerSpells.AddRangeAsync(spellsToAdd);
            return await _GameDbContext.SaveChangesAsync();
        }

        [HttpPut("PlayerSpells/{playerUid}")]
        public async Task<int> PutPlayerSpells(uint PlayerUid, List<ApiPlayerSpell> PlayerSpells)
        {
            var existingSpells = await _GameDbContext.PlayerSpells
                .Where(x => x.PlayerUid == PlayerUid)
                .ToListAsync();

            _GameDbContext.PlayerSpells.RemoveRange(existingSpells);
            foreach (var spell in PlayerSpells)
            {
                spell.PlayerUid = PlayerUid; // prevent send playerspells with incorrect playeruid
            }
            await _GameDbContext.PlayerSpells.AddRangeAsync(PlayerSpells);
            return await _GameDbContext.SaveChangesAsync();
        }
        [HttpDelete("PlayerSpells/{TypeId}")]
        public async Task<int> DeletePlayerSpells(ushort TypeId)
        {
            var toDelete = await _GameDbContext.PlayerSpells.Where(x => x.TypeID == TypeId).ToListAsync();
            if (toDelete.Count == 0)
                return 0;
            _GameDbContext.PlayerSpells.RemoveRange(toDelete);
            return await _GameDbContext.SaveChangesAsync();
        }
        #endregion

        #region PlayerProfs (Api Restfull)
        [HttpGet("PlayerProfs")]
        public async Task<List<ApiPlayerProficiency>> GetPlayerProfs(uint PlayerUID = 0)
        {
            if (PlayerUID == 0)
            {
                return await _GameDbContext.PlayerProfs.ToListAsync();
            }
            return await _GameDbContext.PlayerProfs.Where(x => x.PlayerUid == PlayerUID).ToListAsync();
        }
        [HttpPost("PlayerProfs/{playerUid}")]
        public async Task<int> PostPlayerProfs(uint PlayerUid, List<ApiPlayerProficiency> PlayerProfs)
        {
            if (PlayerProfs == null || PlayerProfs.Count == 0)
                return 0;
            foreach (var spell in PlayerProfs)
            {
                spell.PlayerUid = PlayerUid;
            }
            var existingTypeIds = await _GameDbContext.PlayerProfs
                .Where(x => x.PlayerUid == PlayerUid)
                .Select(x => x.TypeID)
                .ToListAsync();
            var profsToAdd = PlayerProfs
                .Where(spell => !existingTypeIds.Contains(spell.TypeID))
                .ToList();
            if (profsToAdd.Count == 0)
                return 0;
            await _GameDbContext.PlayerProfs.AddRangeAsync(profsToAdd);
            return await _GameDbContext.SaveChangesAsync();
        }

        [HttpPut("PlayerProfs/{playerUid}")]
        public async Task<int> PutPlayerProfs(uint PlayerUid, List<ApiPlayerProficiency> PlayerProfs)
        {
            var existingProfs = await _GameDbContext.PlayerProfs
                .Where(x => x.PlayerUid == PlayerUid)
                .ToListAsync();

            _GameDbContext.PlayerProfs.RemoveRange(existingProfs);
            foreach (var spell in PlayerProfs)
            {
                spell.PlayerUid = PlayerUid; // prevent send playerprofs with incorrect playeruid
            }
            await _GameDbContext.PlayerProfs.AddRangeAsync(PlayerProfs);
            return await _GameDbContext.SaveChangesAsync();
        }
        [HttpDelete("PlayerProfs/{TypeId}")]
        public async Task<int> DeletePlayerProfs(ushort TypeId)
        {
            var toDelete = await _GameDbContext.PlayerProfs.Where(x => x.TypeID == TypeId).ToListAsync();
            if (toDelete.Count == 0)
                return 0;
            _GameDbContext.PlayerProfs.RemoveRange(toDelete);
            return await _GameDbContext.SaveChangesAsync();
        }
        #endregion

        #region VendorShops (Api Restfull)
        [HttpGet("VendorShops")]
        public async Task<List<ApiVendorShop>> GetVendorShops()
        {
            return await _GameDbContext.VendorShops.ToListAsync();
        }
        [HttpPost("VendorShops")]
        public async Task<int> PostVendorShops(List<ApiVendorShop> vendorShops)
        {
            if (vendorShops == null || vendorShops.Count == 0)
                return 0;
            var existing = await _GameDbContext.VendorShops.Select(x => x.UID).ToListAsync();
            var toAdd = vendorShops.Where(x => !existing.Contains(x.UID)).ToList();
            if (toAdd.Count() == 0) return 0;
            await _GameDbContext.VendorShops.AddRangeAsync(toAdd);
            return await _GameDbContext.SaveChangesAsync();
        }

        [HttpPut("VendorShops")]
        public async Task<int> PutVendorShops(uint playerUid, List<ApiVendorShop> vendorShops)
        {
            foreach (var shop in vendorShops)
            {
                var existingShop = await _GameDbContext.VendorShops.Where(x => x.UID == shop.UID).FirstOrDefaultAsync();
                if (existingShop != null)
                {
                    existingShop.UID = shop.UID;
                    existingShop.Mesh = shop.Mesh;
                    existingShop.Name = shop.Name;
                    existingShop.Map = shop.Map;
                    existingShop.X = shop.X;
                    existingShop.Y = shop.Y;
                    existingShop.Items = shop.Items;
                    existingShop.CostType = shop.CostType;
                    _GameDbContext.VendorShops.Update(existingShop);
                }
                else
                {
                    _GameDbContext.VendorShops.Add(shop);
                }
            }
            return await _GameDbContext.SaveChangesAsync();
        }
        [HttpDelete("VendorShops/{UID}")]
        public async Task<int> DeleteVendorShops(ushort UID)
        {
            var toDelete = await _GameDbContext.VendorShops.Where(x => x.UID == UID).ToListAsync();
            if (toDelete.Count == 0)
                return 0;
            _GameDbContext.VendorShops.RemoveRange(toDelete);
            return await _GameDbContext.SaveChangesAsync();
        }
        #endregion

        #region TeamElitePK (Api Restfull)
        [HttpGet("TeamElitePKS/{Type}")]
        public async Task<List<ApiTeamElitePK>> GetTeamElitePKS(EliteTournamentType Type)
        {
            return await _GameDbContext.TeamElitePKs.Where(x => x.Type == Type).ToListAsync();
        }
        [HttpPost("TeamElitePKS")]
        public async Task<int> PostTeamElitePKS(List<ApiTeamElitePK> TeamElitePKS)
        {
            if (TeamElitePKS == null || TeamElitePKS.Count == 0)
                return 0;
            var existing = await _GameDbContext.TeamElitePKs.Where(x => x.Type == TeamElitePKS.First().Type).Select(x => x.Tournament).ToListAsync();
            var toAdd = TeamElitePKS.Where(x => !existing.Contains(x.Tournament) && x.Type == TeamElitePKS.First().Type).ToList();
            if (toAdd.Count() == 0) return 0;
            await _GameDbContext.TeamElitePKs.AddRangeAsync(toAdd);
            return await _GameDbContext.SaveChangesAsync();
        }

        [HttpPut("TeamElitePKS")]
        public async Task<int> PutTeamElitePKS(List<ApiTeamElitePK> TeamElitePKS)
        {
            foreach (var TeamElitePK in TeamElitePKS)
            {
                var existingItem = await _GameDbContext.TeamElitePKs.Where(x => x.Tournament == TeamElitePK.Tournament && x.Type == TeamElitePK.Type).FirstOrDefaultAsync();
                if (existingItem != null)
                {
                    existingItem.Tournament = TeamElitePK.Tournament;
                    existingItem.Rank = TeamElitePK.Rank;
                    existingItem.PlayerId = TeamElitePK.PlayerId;
                    existingItem.PlayerName = TeamElitePK.PlayerName;
                    existingItem.PlayerMesh = TeamElitePK.PlayerMesh;
                    existingItem.ClaimReward = TeamElitePK.ClaimReward;
                    existingItem.LeaderUID = TeamElitePK.LeaderUID;
                    _GameDbContext.TeamElitePKs.Update(existingItem);
                }
                else
                {
                    _GameDbContext.TeamElitePKs.Add(TeamElitePK);
                }
            }
            return await _GameDbContext.SaveChangesAsync();
        }
        [HttpDelete("TeamElitePKS/{Tournament}/{Type}")]
        public async Task<int> DeleteTeamElitePKS(ushort Tournament, EliteTournamentType Type)
        {
            var toDelete = await _GameDbContext.TeamElitePKs.Where(x => x.Tournament == Tournament && x.Type == Type).ToListAsync();
            if (toDelete.Count == 0)
                return 0;
            _GameDbContext.TeamElitePKs.RemoveRange(toDelete);
            return await _GameDbContext.SaveChangesAsync();
        }
        #endregion

        #region ClassPKWar (Api Restfull)
        [HttpGet("ClassPKWars")]
        public async Task<List<ApiClassPKWar>> GetClassPKWars()
        {
            return await _GameDbContext.ClassPKWars.ToListAsync();
        }
        [HttpPost("ClassPKWars")]
        public async Task<int> PostClassPKWars(List<ApiClassPKWar> ClassPKWars)
        {
            if (ClassPKWars == null || ClassPKWars.Count == 0)
                return 0;
            List<ApiClassPKWar> existing = new List<ApiClassPKWar>();
            foreach (var ClassPKWar in ClassPKWars)
            {
                ApiClassPKWar? found = await _GameDbContext.ClassPKWars.FirstOrDefaultAsync(x => x.Type == ClassPKWar.Type && x.Level == ClassPKWar.Level);
                if (found == null)
                {
                    await _GameDbContext.ClassPKWars.AddAsync(ClassPKWar);
                }
            }
            return await _GameDbContext.SaveChangesAsync();
        }

        [HttpPut("ClassPKWars")]
        public async Task<int> PutClassPKWars(List<ApiClassPKWar> ClassPKWars)
        {
            foreach (var ClassPKWar in ClassPKWars)
            {
                var existingItem = await _GameDbContext.ClassPKWars.Where(x => x.Type == ClassPKWar.Type && x.Level == ClassPKWar.Level).FirstOrDefaultAsync();
                if (existingItem != null)
                {
                    existingItem.Type = ClassPKWar.Type;
                    existingItem.Level = ClassPKWar.Level;
                    existingItem.Winner = ClassPKWar.Winner;
                    existingItem.LastFlag = ClassPKWar.LastFlag;
                    _GameDbContext.ClassPKWars.Update(existingItem);
                }
                else
                {
                    _GameDbContext.ClassPKWars.Add(ClassPKWar);
                }
            }
            return await _GameDbContext.SaveChangesAsync();
        }
        [HttpDelete("ClassPKWars/{Type}/{Level}")]
        public async Task<int> DeleteTeamElitePKS(ushort Type, byte Level)
        {
            var toDelete = await _GameDbContext.ClassPKWars.Where(x => x.Type == Type && x.Level == Level).ToListAsync();
            if (toDelete.Count == 0)
                return 0;
            _GameDbContext.ClassPKWars.RemoveRange(toDelete);
            return await _GameDbContext.SaveChangesAsync();
        }
        #endregion

        #region ElitePK (Api Restfull)
        [HttpGet("ElitePKS")]
        public async Task<List<ApiElitePK>> GetElitePKS()
        {
            return await _GameDbContext.ElitePKs.ToListAsync();
        }
        [HttpPost("ElitePKS")]
        public async Task<int> PostElitePKS(List<ApiElitePK> ElitePKS)
        {
            if (ElitePKS == null || ElitePKS.Count == 0)
                return 0;
            var existing = await _GameDbContext.ElitePKs.Select(x => x.Tournament).ToListAsync();
            var toAdd = ElitePKS.Where(x => !existing.Contains(x.Tournament)).ToList();
            if (toAdd.Count() == 0) return 0;
            await _GameDbContext.ElitePKs.AddRangeAsync(toAdd);
            return await _GameDbContext.SaveChangesAsync();
        }

        [HttpPut("ElitePKS")]
        public async Task<int> PutElitePKS(List<ApiElitePK> ElitePKS)
        {
            foreach (var ElitePK in ElitePKS)
            {
                var existingItem = await _GameDbContext.ElitePKs.Where(x => x.Tournament == ElitePK.Tournament).FirstOrDefaultAsync();
                if (existingItem != null)
                {
                    existingItem.Tournament = ElitePK.Tournament;
                    existingItem.Rank = ElitePK.Rank;
                    existingItem.PlayerId = ElitePK.PlayerId;
                    existingItem.PlayerName = ElitePK.PlayerName;
                    existingItem.PlayerMesh = ElitePK.PlayerMesh;
                    existingItem.ClaimReward = ElitePK.ClaimReward;
                    _GameDbContext.ElitePKs.Update(existingItem);
                }
                else
                {
                    _GameDbContext.ElitePKs.Add(ElitePK);
                }
            }
            return await _GameDbContext.SaveChangesAsync();
        }
        [HttpDelete("ElitePKS/{Tournament}")]
        public async Task<int> DeleteElitePKS(ushort Tournament)
        {
            var toDelete = await _GameDbContext.ElitePKs.Where(x => x.Tournament == Tournament).ToListAsync();
            if (toDelete.Count == 0)
                return 0;
            _GameDbContext.ElitePKs.RemoveRange(toDelete);
            return await _GameDbContext.SaveChangesAsync();
        }
        #endregion

        #region Couples (Api Restfull)
        [HttpGet("Couples")]
        public async Task<List<ApiCouple>> GetCouples()
        {
            return await _GameDbContext.Couples.ToListAsync();
        }

        [HttpPut("Couples")]
        public async Task<int> PutCouples(List<ApiCouple> Couples)
        {
            _GameDbContext.Couples.RemoveRange(_GameDbContext.Couples); // Remove all existing couples
            foreach (var Couple in Couples)
            {
                _GameDbContext.Couples.Add(Couple); // Add the new couples
            }
            return await _GameDbContext.SaveChangesAsync();
        }
        #endregion

        #region CityWars (Api Restfull)
        [HttpGet("CityWars")]
        public async Task<List<ApiCityWar>> GetCityWars()
        {
            return await _GameDbContext.CityWars.ToListAsync();
        }

        [HttpPut("CityWars")]
        public async Task<int> PutCityWars(List<ApiCityWar> CityWars)
        {
            foreach (var CityWar in CityWars)
            {
                var existingItem = await _GameDbContext.CityWars.Where(x => x.CityWarType == CityWar.CityWarType).FirstOrDefaultAsync();
                if (existingItem != null)
                {
                    existingItem.GuildId = CityWar.GuildId;
                    existingItem.GuildName = CityWar.GuildName;
                    existingItem.PoleHitPoints = CityWar.PoleHitPoints;
                    _GameDbContext.CityWars.Update(existingItem);
                }
                else
                {
                    _GameDbContext.CityWars.Add(CityWar);
                }
            }
            return await _GameDbContext.SaveChangesAsync();
        }
        #endregion

        #region GuildWars (Api Restfull)
        [HttpGet("GuildWars/{Type}")]
        public async Task<List<ApiGuildWar>> GetGuildWars(GuildWarType Type)
        {
            return await _GameDbContext.GuildWars.Where(x => x.Type == Type).ToListAsync();
        }

        [HttpPut("GuildWars")]
        public async Task<int> PutGuildWars(List<ApiGuildWar> GuildWars)
        {
            foreach (var GuildWar in GuildWars)
            {
                var existingItem = await _GameDbContext.GuildWars.Where(x => x.Type == GuildWar.Type).FirstOrDefaultAsync();
                if (existingItem != null)
                {
                    existingItem.WinnerGuildID = GuildWar.WinnerGuildID;
                    existingItem.WinnerName = GuildWar.WinnerName;
                    existingItem.LeaderReward = GuildWar.LeaderReward;
                    existingItem.DeputiLeaderReward = GuildWar.DeputiLeaderReward;
                    existingItem.RewardDeputies = GuildWar.RewardLeaders;
                    existingItem.RewardLeaders = GuildWar.RewardDeputies;
                    existingItem.PoleHitPoints = GuildWar.PoleHitPoints;
                    existingItem.GuildConductor1 = GuildWar.GuildConductor1;
                    existingItem.GuildConductor2 = GuildWar.GuildConductor2;
                    existingItem.GuildConductor3 = GuildWar.GuildConductor3;
                    existingItem.GuildConductor4 = GuildWar.GuildConductor4;
                    _GameDbContext.GuildWars.Update(existingItem);
                }
                else
                {
                    _GameDbContext.GuildWars.Add(GuildWar);
                }
            }
            return await _GameDbContext.SaveChangesAsync();
        }
        #endregion

        #region VIPShares (Api Restfull)
        [HttpGet("VIPShares")]
        public async Task<List<ApiVIPShare>> GetVIPShares()
        {
            return await _GameDbContext.VIPShares.ToListAsync();
        }

        [HttpPut("VIPShares")]
        public async Task<int> PutVIPShares(List<ApiVIPShare> VIPShares)
        {
            foreach (var VIPShare in VIPShares)
            {
                var existingItem = await _GameDbContext.VIPShares.Where(x => x.PlayerUID == VIPShare.PlayerUID).FirstOrDefaultAsync();
                if (existingItem != null)
                {
                    existingItem.PlayerUID = VIPShare.PlayerUID;
                    existingItem.ShareUID = VIPShare.ShareUID;
                    existingItem.ShareName = VIPShare.ShareName;
                    existingItem.ShareLevel = VIPShare.ShareLevel;
                    existingItem.ShareExpiration = VIPShare.ShareExpiration;
                    _GameDbContext.VIPShares.Update(existingItem);
                }
                else
                {
                    _GameDbContext.VIPShares.Add(VIPShare);
                }
            }
            return await _GameDbContext.SaveChangesAsync();
        }
        #endregion

        #region KOBoardRanks (Api Restfull)
        [HttpGet("KOBoardRanks")]
        public async Task<List<ApiKOBoardRank>> GetKOBoardRanks()
        {
            return await _GameDbContext.KOBoardRanks.ToListAsync();
        }

        [HttpPut("KOBoardRanks")]
        public async Task<int> PutKOBoardRanks(List<ApiKOBoardRank> KOBoardRanks)
        {
            foreach (var KOBoardRank in KOBoardRanks)
            {
                var existingItem = await _GameDbContext.KOBoardRanks.Where(x => x.PlayerUID == KOBoardRank.PlayerUID).FirstOrDefaultAsync();
                if (existingItem != null)
                {
                    existingItem.PlayerUID = KOBoardRank.PlayerUID;
                    existingItem.PlayerName = KOBoardRank.PlayerName;
                    existingItem.Points = KOBoardRank.Points;
                    _GameDbContext.KOBoardRanks.Update(existingItem);
                }
                else
                {
                    _GameDbContext.KOBoardRanks.Add(KOBoardRank);
                }
            }
            return await _GameDbContext.SaveChangesAsync();
        }
        #endregion

        #region BanUIDS (Api Restfull)
        [HttpGet("BanUIDs")]
        public async Task<List<ApiBanUID>> GetBanUIDs()
        {
            return await _GameDbContext.BanUIDs.ToListAsync();
        }

        [HttpPut("BanUIDs")]
        public async Task<int> PutBanUIDs(List<ApiBanUID> BanUIDs)
        {
            foreach (var item in BanUIDs)
            {
                var existingItem = await _GameDbContext.BanUIDs.Where(x => x.PlayerUID == item.PlayerUID).FirstOrDefaultAsync();
                if (existingItem != null)
                {
                    existingItem.PlayerUID = item.PlayerUID;
                    _GameDbContext.BanUIDs.Update(existingItem);
                }
                else
                {
                    _GameDbContext.BanUIDs.Add(item);
                }
            }
            return await _GameDbContext.SaveChangesAsync();
        }

        [HttpDelete("BanUIDs/{PlayerUID}")]
        public async Task<int> DeleteBanUID(uint PlayerUID)
        {
            var toDelete = await _GameDbContext.BanUIDs.Where(x => x.PlayerUID == PlayerUID).ToListAsync();
            if (toDelete.Count == 0)
                return 0;
            _GameDbContext.BanUIDs.RemoveRange(toDelete);
            return await _GameDbContext.SaveChangesAsync();
        }
        #endregion

        #region BanIPS (Api Restfull)
        [HttpGet("BanIPs")]
        public async Task<List<ApiBanIP>> GetBanIPs()
        {
            return await _GameDbContext.BanIPs.ToListAsync();
        }

        [HttpPut("BanIPs")]
        public async Task<int> PutBanIPs(List<ApiBanIP> BanIPs)
        {
            foreach (var item in BanIPs)
            {
                var existingItem = await _GameDbContext.BanIPs.Where(x => x.IP == item.IP).FirstOrDefaultAsync();
                if (existingItem != null)
                {
                    existingItem.IP = item.IP;
                    existingItem.Hours = item.Hours;
                    existingItem.StartBan = item.StartBan;
                    _GameDbContext.BanIPs.Update(existingItem);
                }
                else
                {
                    _GameDbContext.BanIPs.Add(item);
                }
            }
            return await _GameDbContext.SaveChangesAsync();
        }

        [HttpDelete("BanIPs/{IP}")]
        public async Task<int> DeleteBanIP(string IP)
        {
            var toDelete = await _GameDbContext.BanIPs.Where(x => x.IP == IP).ToListAsync();
            if (toDelete.Count == 0)
                return 0;
            _GameDbContext.BanIPs.RemoveRange(toDelete);
            return await _GameDbContext.SaveChangesAsync();
        }
        #endregion

        #region Associates (Api Restfull)
        [HttpGet("Associates")]
        public async Task<List<ApiAssociate>> GetAssociates()
        {
            return await _GameDbContext.Associates.Include(x => x.Members).ToListAsync();
        }

        [HttpPut("Associates")]
        public async Task<int> PutAssociates(List<ApiAssociate> Associates)
        {
            foreach (var item in Associates)
            {
                var existingItem = await _GameDbContext.Associates.Include(x => x.Members).Where(x => x.PlayerUID == item.PlayerUID).FirstOrDefaultAsync();
                if (existingItem != null)
                {
                    existingItem.PlayerUID = item.PlayerUID;
                    existingItem.MentorExpballs = item.MentorExpballs;
                    existingItem.MentorBlessing = item.MentorBlessing;
                    existingItem.MentorStones = item.MentorStones;
                    foreach(ApiAssociateMember member in item.Members)
                    {
                        ApiAssociateMember memberCurrentOnDb = existingItem.Members.FirstOrDefault(x => x.UID == member.UID && x.Type == member.Type);
                        if (memberCurrentOnDb != null)
                        {
                            memberCurrentOnDb.Timer = member.Timer;
                            memberCurrentOnDb.ExpBalls = member.ExpBalls;
                            memberCurrentOnDb.Blessing = member.Blessing;
                            memberCurrentOnDb.Stone = member.Stone;
                            memberCurrentOnDb.MapName = member.MapName;
                            memberCurrentOnDb.Name = member.Name;
                            memberCurrentOnDb.KillsCount = member.KillsCount;
                            memberCurrentOnDb.BattlePower = member.BattlePower;
                        }
                        else
                        {
                            existingItem.Members.Add(member);
                        }
                    }
                    _GameDbContext.Associates.Update(existingItem);
                }
                else
                {
                    if (item.Members == null)
                    {
                        item.Members = [];
                    }
                    _GameDbContext.Associates.Add(item);
                }
            }
            return await _GameDbContext.SaveChangesAsync();
        }

        [HttpDelete("Associates/{Type}/{MemberUID}")]
        public async Task<int> DeleteAssociates(AssociateMemberType Type, uint MemberUID)
        {
            List<ApiAssociate> associates = await _GameDbContext.Associates.Include(x => x.Members).ToListAsync();
            foreach(var associate in associates)
            {
                associate.Members.RemoveAll(x => x.UID == MemberUID && x.Type == Type);
            }
            return await _GameDbContext.SaveChangesAsync();
        }
        #endregion

        #region ClanWars (Api Restfull)
        [HttpGet("ClanWars")]
        public async Task<List<ApiClanWar>> GetClanWars()
        {
            return await _GameDbContext.ClanWars.ToListAsync();
        }

        [HttpPut("ClanWars")]
        public async Task<int> PutClanWars(List<ApiClanWar> ClanWars)
        {
            foreach (var ClanWar in ClanWars)
            {
                var existingItem = await _GameDbContext.ClanWars.Where(x => x.Type == ClanWar.Type).FirstOrDefaultAsync();
                if (existingItem != null)
                {
                    existingItem.ClanId = ClanWar.ClanId;
                    existingItem.PoleName = ClanWar.PoleName;
                    existingItem.WinnerClaimReward = ClanWar.WinnerClaimReward;
                    existingItem.WinnerNextReward = ClanWar.WinnerNextReward;
                    existingItem.WinnerOccupationDays = ClanWar.WinnerOccupationDays;
                    existingItem.WinnerReward = ClanWar.WinnerReward;
                    existingItem.BestWinnerClanId = ClanWar.BestWinnerClanId;
                    existingItem.BestWinnerName = ClanWar.BestWinnerName;
                    existingItem.BestWinnerClaimReward = ClanWar.BestWinnerClaimReward;
                    existingItem.BestWinnerNextReward = ClanWar.BestWinnerNextReward;
                    existingItem.BestWinnerOccupationDays = ClanWar.BestWinnerOccupationDays;
                    existingItem.BestWinnerReward = ClanWar.BestWinnerReward;
                    _GameDbContext.ClanWars.Update(existingItem);
                }
                else
                {
                    _GameDbContext.ClanWars.Add(ClanWar);
                }
            }
            return await _GameDbContext.SaveChangesAsync();
        }
        #endregion

        #region PlayerHouses (Api Restfull)
        [HttpGet("PlayerHouses/{PlayerUID}")]
        public async Task<List<ApiPlayerHouse>> GetPlayerHouses(uint PlayerUID)
        {
            return await _GameDbContext.PlayerHouses.Include(x => x.Furnitures).Where(x => x.PlayerUID == PlayerUID).ToListAsync();
        }

        [HttpPut("PlayerHouses")]
        public async Task<int> PutPlayerHouses(List<ApiPlayerHouse> playerHouses)
        {
            if (playerHouses == null || playerHouses.Count == 0)
                return 0;

            foreach (var newHouse in playerHouses)
            {
                var existingHouse = await _GameDbContext.PlayerHouses
                    .Include(h => h.Furnitures)
                    .FirstOrDefaultAsync(h => h.PlayerUID == newHouse.PlayerUID);

                if (existingHouse != null)
                {
                    existingHouse.Level = newHouse.Level;
                    _GameDbContext.PlayerHouseFurnitures.RemoveRange(existingHouse.Furnitures);
                    existingHouse.Furnitures = newHouse.Furnitures ?? new List<ApiPlayerHouseFurniture>();
                }
                else
                {
                    var houseToAdd = new ApiPlayerHouse
                    {
                        PlayerUID = newHouse.PlayerUID,
                        Level = newHouse.Level,
                        Furnitures = newHouse.Furnitures ?? new List<ApiPlayerHouseFurniture>()
                    };

                    _GameDbContext.PlayerHouses.Add(houseToAdd);
                }
            }
            return await _GameDbContext.SaveChangesAsync();
        }

        #endregion

        #region Quests
        [HttpGet("Quests/Get")]
        public async Task<List<ApiQuest>> GetQuests()
        {
            return await _GameDbContext.Quests.AsNoTracking().ToListAsync();
        }
        [HttpPost("Quests/Set")]
        public async Task<bool> SetQuests(List<ApiQuest> quests)
        {
            await _GameDbContext.Quests.AddRangeAsync(quests);
            return await _GameDbContext.SaveChangesAsync() > 0;
        }
        [HttpPost("Quests/Update")]
        public async Task<bool> UpdateQuests(List<ApiQuest> quests)
        {
            if (quests == null || !quests.Any())
                return false;
            foreach (var apiItem in quests)
            {
                var existing = await _GameDbContext.Quests.FirstOrDefaultAsync(p => p.UID == apiItem.UID && p.PlayerUID == apiItem.PlayerUID);
                if (existing != null)
                {
                    existing.UID = apiItem.UID;
                    existing.PlayerUID = apiItem.PlayerUID;
                    existing.Intentions = apiItem.Intentions;
                    existing.Status = apiItem.Status;
                    existing.Time = apiItem.Time;
                } else
                {
                    _GameDbContext.Quests.Add(apiItem);
                }
            }
            return await _GameDbContext.SaveChangesAsync() > 0;
        }
        [HttpPost("Quests/Remove")]
        public async Task<bool> RemoveQuests(List<ApiQuest> quests)
        {
            if (quests == null || !quests.Any())
                return false;
            List<ApiQuest> toRemove = new List<ApiQuest>();
            foreach (var apiItem in quests)
            {
                var existing = await _GameDbContext.Quests.FirstOrDefaultAsync(p => p.UID == apiItem.UID && p.PlayerUID == apiItem.PlayerUID);
                if (existing != null)
                {
                    toRemove.Add(existing);
                }
            }
            _GameDbContext.Quests.RemoveRange(toRemove);
            return await _GameDbContext.SaveChangesAsync() > 0;
        }
        #endregion

        #region Servers
        [HttpGet("Servers")]
        public async Task<List<Server>> GetServerByName()
        {
            return await _Auth.GetServersAsync();
        }

        [HttpGet("Server")]
        public async Task<Server> GetServerByName(string Name)
        {
            return await _Auth.GetServerByNameAsync(Name);
        }
        #endregion

        #region Votes

        [HttpGet("Votes/Get")]
        public async Task<Vote> GetVotesByUID(uint UID)
        {
            return await _Auth.GetVoteByUidAsync(UID);
        }

        [HttpPost("Votes/Add")]
        public async Task<Vote> AddVoteByUID([FromBody]Core.Models.AddVote AddVote)
        {
            return await _Auth.AddVoteAsync(AddVote.UID);
        }
        /// <summary>
        /// Remove a vote.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST api/RemoveVotes
        ///     {        
        ///       "UID": "1000000",
        ///       "VotesToRemove": 10,
        ///     }
        /// </remarks>
        /// <param name="RemoveVote"></param>     
        [HttpPost("Votes/Remove")]
        public async Task<Vote> RemoveVotesByUID([FromBody] RemoveVote RemoveVote)
        {
            return await _Auth.RemoveVotesAsync(RemoveVote.UID, RemoveVote.VotesToRemove);
        }
        #endregion

        #region SobNPC/NPC/Furniture
        [HttpGet("SobNPCs/Get")]
        public async Task<List<ApiSobNPC>> GetSobNPCs()
        {
            return await _GameDbContext.SobNPCs.AsNoTracking().ToListAsync();
        }

        [HttpPost("SobNPCs/Set")]
        public async Task<bool> SetSobNPCs(List<ApiSobNPC> SobNPCs)
        {
            _GameDbContext.SobNPCs.AddRange(SobNPCs);
            return await _GameDbContext.SaveChangesAsync() > 0;
        }
        [HttpGet("NPCs/Get")]
        public async Task<List<ApiNPC>> GetNPCs()
        {
            return await _GameDbContext.NPCs.AsNoTracking().ToListAsync();
        }

        [HttpPost("NPCs/Set")]
        public async Task<bool> SetNPCs(List<ApiNPC> NPCs)
        {
            _GameDbContext.NPCs.AddRange(NPCs);
            return await _GameDbContext.SaveChangesAsync() > 0;
        }
        [HttpGet("Furnitures/Get")]
        public async Task<List<ApiFurniture>> GetFurnitures()
        {
            return await _GameDbContext.Furnitures.AsNoTracking().ToListAsync();
        }

        [HttpPost("Furnitures/Set")]
        public async Task<bool> SetFurnitures(List<ApiFurniture> furnitures)
        {
            _GameDbContext.Furnitures.AddRange(furnitures);
            return await _GameDbContext.SaveChangesAsync() > 0;
        }
        #endregion

        #region Trap

        [HttpGet("Traps/Get")]
        public async Task<List<ApiTrap>> GetTraps()
        {
            return await _GameDbContext.Traps.AsNoTracking().ToListAsync();
        }

        [HttpPost("Traps/Set")]
        public async Task<bool> SetTraps(List<ApiTrap> Traps)
        {
            _GameDbContext.Traps.AddRange(Traps);
            return await _GameDbContext.SaveChangesAsync() > 0;
        }
        #endregion

        #region Portal

        [HttpGet("Portals/Get")]
        public async Task<List<ApiPortal>> GetPortals()
        {
            return await _GameDbContext.Portals.AsNoTracking().ToListAsync();
        }

        [HttpPost("Portals/Set")]
        public async Task<bool> SetTraps(List<ApiPortal> Portals)
        {
            _GameDbContext.Portals.AddRange(Portals);
            return await _GameDbContext.SaveChangesAsync() > 0;
        }
        #endregion

        #region Guild

        [HttpGet("Guilds/Get")]
        public async Task<List<ApiGuild>> GetGuilds()
        {
            return await _GameDbContext.Guilds
                .AsNoTracking().ToListAsync();
        }

        [HttpPost("Guilds/Set")]
        public async Task<bool> SetGuilds(List<ApiGuild> Guilds)
        {
            _GameDbContext.Guilds.AddRange(Guilds);
            return await _GameDbContext.SaveChangesAsync() > 0;
        }

        [HttpPost("Guilds/Update")]
        public async Task<bool> UpdateGuilds(List<ApiGuild> Guilds)
        {
            foreach(ApiGuild g in Guilds)
            {
                ApiGuild apiCurrentGuild = await _GameDbContext.Guilds.Where(x => x.GuildID == g.GuildID).FirstOrDefaultAsync();
                if (apiCurrentGuild != null)
                {
                    apiCurrentGuild.GuildID = g.GuildID;
                    apiCurrentGuild.SilverFund = g.SilverFund;
                    apiCurrentGuild.ConquerPointFund = g.ConquerPointFund;
                    apiCurrentGuild.MembersCount = g.MembersCount;
                    apiCurrentGuild.MyRank = g.MyRank;
                    apiCurrentGuild.Level = g.Level;
                    apiCurrentGuild.CreateTime = g.CreateTime;
                    apiCurrentGuild.LeaderName = g.LeaderName;
                    apiCurrentGuild.Recruit = g.Recruit;
                    apiCurrentGuild.AdvertiseRecruit = g.AdvertiseRecruit;
                    apiCurrentGuild.Enemies = g.Enemies;
                    apiCurrentGuild.Arsenal = g.Arsenal;
                    apiCurrentGuild.Bulletin = g.Bulletin;
                    apiCurrentGuild.BuletinEnrole = g.BuletinEnrole;
                    apiCurrentGuild.CTFRank = g.CTFRank;
                    apiCurrentGuild.CTFNextMoney = g.CTFNextMoney;
                    apiCurrentGuild.CTFNextConquerPoints = g.CTFNextConquerPoints;
                    apiCurrentGuild.ClaimCtfReward = g.ClaimCtfReward;
                    apiCurrentGuild.UseAdvertise = g.UseAdvertise;
                    apiCurrentGuild.Allies = g.Allies;
                    apiCurrentGuild.Enemies = g.Enemies;
                } else
                {
                    _GameDbContext.Guilds.Add(g);
                }
            }
            return await _GameDbContext.SaveChangesAsync() > 0;
        }
        #endregion

        #region Clan

        [HttpGet("Clans/Get")]
        public async Task<List<ApiClan>> GetClans()
        {
            return await _GameDbContext.Clans
                .AsNoTracking().ToListAsync();
        }

        [HttpPost("Clans/Set")]
        public async Task<bool> SetClans(List<ApiClan> Clans)
        {
            _GameDbContext.Clans.AddRange(Clans);
            return await _GameDbContext.SaveChangesAsync() > 0;
        }

        [HttpPost("Clans/Update")]
        public async Task<bool> UpdateClans(List<ApiClan> Clans)
        {
            foreach (ApiClan c in _GameDbContext.Clans.AsNoTracking())
            {
                ApiClan apiC = Clans.Find(x => x.ClanID == c.ClanID);
                if (apiC != null)
                {
                    c.Name = apiC.Name;
                    c.LeaderName = apiC.LeaderName;
                    c.Level = apiC.Level;
                    c.Donation = apiC.Donation;
                    c.ClanBulletin = apiC.ClanBulletin;
                    c.BP = apiC.BP;
                    c.Allies = apiC.Allies;
                    c.Enemies = apiC.Enemies;
                    _GameDbContext.Update(c);
                }
            }
            return await _GameDbContext.SaveChangesAsync() > 0;
        }
        #endregion

        #region TutorType && TutorBattleLimitType
        [HttpGet("TutorType/Get")]
        public async Task<List<ApiTutorType>> GetTutorType()
        {
            return await _GameDbContext.TutorTypes
                .AsNoTracking().ToListAsync();
        }
        [HttpGet("TutorBattleLimitType/Get")]
        public async Task<List<ApiTutorBattleLimitType>> GetTutorBattleLimitType()
        {
            return await _GameDbContext.TutorBattleLimitTypes
                .AsNoTracking().ToListAsync();
        }

        [HttpPost("TutorType/Set")]
        public async Task<bool> SetTutorTypes(List<ApiTutorType> tutorTypes)
        {
            _GameDbContext.TutorTypes.AddRange(tutorTypes);
            return await _GameDbContext.SaveChangesAsync() > 0;
        }
        [HttpPost("TutorBattleLimitType/Set")]
        public async Task<bool> SetTutorBattleLimitType(List<ApiTutorBattleLimitType> tutorBattleLimitTypes)
        {
            _GameDbContext.TutorBattleLimitTypes.AddRange(tutorBattleLimitTypes);
            return await _GameDbContext.SaveChangesAsync() > 0;
        }
        #endregion

        #region Transformation

        [HttpGet("Transformation/Get")]
        public async Task<List<ApiTransformation>> GetTransformations()
        {
            return await _GameDbContext.Transformations
                .AsNoTracking().ToListAsync();
        }

        [HttpPost("Transformation/Set")]
        public async Task<bool> SetTransformations(List<ApiTransformation> transformations)
        {
            _GameDbContext.Transformations.AddRange(transformations);
            return await _GameDbContext.SaveChangesAsync() > 0;
        }
        #endregion

        #region Crime

        [HttpGet("Crime/Get")]
        public async Task<List<ApiCrime>> GetCrimes()
        {
            return await _GameDbContext.Crimes
                .AsNoTracking().ToListAsync();
        }

        [HttpPost("Crime/Set")]
        public async Task<bool> SetCrimes(List<ApiCrime> crimes)
        {
            _GameDbContext.Crimes.AddRange(crimes);
            return await _GameDbContext.SaveChangesAsync() > 0;
        }

        [HttpPost("Crime/Update")]
        public async Task<bool> UpdateCrimes(List<ApiCrime> Crimes)
        {
            foreach (ApiCrime aCrime in _GameDbContext.Crimes.AsNoTracking())
            {
                ApiCrime apiC = Crimes.Find(x => x.OwnerUID == aCrime.OwnerUID);
                if (apiC != null)
                {
                    aCrime.OwnerName = aCrime.OwnerName;
                    aCrime.Money = aCrime.Money;
                    _GameDbContext.Update(aCrime);
                }
            }
            return await _GameDbContext.SaveChangesAsync() > 0;
        }
        #endregion

        #region ArenaUser && TeamArenaUser

        [HttpGet("ArenaUser/Get")]
        public async Task<List<ApiArenaUser>> GetArenaUsers()
        {
            return await _GameDbContext.ArenaUsers
                .AsNoTracking().ToListAsync();
        }

        [HttpPost("ArenaUser/Set")]
        public async Task<bool> SetArenaUsers(List<ApiArenaUser> arenaUsers)
        {
            _GameDbContext.ArenaUsers.AddRange(arenaUsers);
            return await _GameDbContext.SaveChangesAsync() > 0;
        }

        [HttpPut("ArenaUser/Update")]
        public async Task<int> UpdateArenaUser(List<ApiArenaUser> arenaUsers)
        {
            var uids = arenaUsers.Select(x => x.UID).ToList();
            var existingUsers = await _GameDbContext.ArenaUsers
                .Where(x => uids.Contains(x.UID))
                .ToDictionaryAsync(x => x.UID);
            foreach (var arenaUser in arenaUsers)
            {
                if (existingUsers.TryGetValue(arenaUser.UID, out var existing))
                {
                    existing.TotalWin = arenaUser.TotalWin;
                    existing.TotalLose = arenaUser.TotalLose;
                    existing.LastSeasonWin = arenaUser.LastSeasonWin;
                    existing.LastSeasonLose = arenaUser.LastSeasonLose;
                    existing.LastSeasonRank = arenaUser.LastSeasonRank;
                    existing.LastSeasonArenaPoints = arenaUser.LastSeasonArenaPoints;
                    existing.Name = arenaUser.Name;
                    existing.Class = arenaUser.Class;
                    existing.Level = arenaUser.Level;
                    existing.Mesh = arenaUser.Mesh;
                    existing.HistoryHonor = arenaUser.HistoryHonor;
                    existing.TodayWin = arenaUser.TodayWin;
                    existing.TodayBattles = arenaUser.TodayBattles;
                }
                else
                {
                    _GameDbContext.ArenaUsers.Add(arenaUser);
                }
            }
            return await _GameDbContext.SaveChangesAsync();
        }

        [HttpGet("TeamArenaUser/Get")]
        public async Task<List<ApiTeamArenaUser>> GetTeamArenaUsers()
        {
            return await _GameDbContext.TeamArenaUsers
                .AsNoTracking().ToListAsync();
        }

        [HttpPost("TeamArenaUser/Set")]
        public async Task<bool> SetTeamArenaUsers(List<ApiTeamArenaUser> teamArenaUsers)
        {
            _GameDbContext.TeamArenaUsers.AddRange(teamArenaUsers);
            return await _GameDbContext.SaveChangesAsync() > 0;
        }

        [HttpPut("TeamArenaUser/Update")]
        public async Task<int> UpdateTeamArenaUser(List<ApiTeamArenaUser> teamArenaUsers)
        {
            var uids = teamArenaUsers.Select(x => x.UID).ToList();
            var existingUsers = await _GameDbContext.TeamArenaUsers
                .Where(x => uids.Contains(x.UID))
                .ToDictionaryAsync(x => x.UID);
            foreach (var incoming in teamArenaUsers)
            {
                if (existingUsers.TryGetValue(incoming.UID, out var existing))
                {
                    existing.TotalWin = incoming.TotalWin;
                    existing.TotalLose = incoming.TotalLose;
                    existing.LastSeasonWin = incoming.LastSeasonWin;
                    existing.LastSeasonLose = incoming.LastSeasonLose;
                    existing.LastSeasonRank = incoming.LastSeasonRank;
                    existing.LastSeasonArenaPoints = incoming.LastSeasonArenaPoints;
                    existing.Name = incoming.Name;
                    existing.Class = incoming.Class;
                    existing.Level = incoming.Level;
                    existing.Mesh = incoming.Mesh;
                    existing.HistoryHonor = incoming.HistoryHonor;
                    existing.TodayWin = incoming.TodayWin;
                    existing.TodayBattles = incoming.TodayBattles;
                }
                else
                {
                    _GameDbContext.TeamArenaUsers.Add(incoming);
                }
            }
            return await _GameDbContext.SaveChangesAsync();
        }
        #endregion

        #region StaticStatue

        [HttpGet("StaticStatue/Get")]
        public async Task<List<ApiStaticStatue>> GetStaticStatues()
        {
            return await _GameDbContext.StaticStatues
                .AsNoTracking().ToListAsync();
        }

        [HttpPost("StaticStatue/Set")]
        public async Task<bool> SetStaticStaues(List<ApiStaticStatue> staticStatues)
        {
            _GameDbContext.StaticStatues.AddRange(staticStatues);
            return await _GameDbContext.SaveChangesAsync() > 0;
        }

        [HttpPost("StaticStatue/Update")]
        public async Task<bool> UpdateStaticStatues(List<ApiStaticStatue> staticStatues)
        {
            foreach (ApiStaticStatue arenaUser in _GameDbContext.StaticStatues.AsNoTracking())
            {
                ApiStaticStatue apiObj = staticStatues.Find(x => x.UID == arenaUser.UID);
                if (apiObj != null)
                {
                    apiObj.StatueSize = arenaUser.StatueSize;
                    apiObj.StatuePackets = arenaUser.StatuePackets;
                    apiObj.Name = arenaUser.Name;
                    apiObj.Map = arenaUser.Map;
                    apiObj.Sort = arenaUser.Sort;
                    apiObj.Mesh = arenaUser.Mesh;
                    apiObj.HitPoints = arenaUser.HitPoints;
                    apiObj.MaxHitPoints = arenaUser.MaxHitPoints;
                    apiObj.X = arenaUser.X;
                    apiObj.Y = arenaUser.Y;
                    apiObj.ObjType = arenaUser.ObjType;
                    apiObj.Type = arenaUser.Type;
                    apiObj.ShowName = arenaUser.ShowName;
                    _GameDbContext.Update(apiObj);
                }
            }
            return await _GameDbContext.SaveChangesAsync() > 0;
        }
        #endregion

        #region GameMap
        [HttpGet("GameMaps/Get")]
        public async Task<List<ApiGameMap>> GetGameMaps()
        {
            return await _GameDbContext.GameMaps.AsNoTracking().ToListAsync();
        }

        [HttpPost("GameMaps/Set")]
        public async Task<bool> SetGameMaps(List<ApiGameMap> Gamemap)
        {
            _GameDbContext.GameMaps.AddRange(Gamemap);
            return await _GameDbContext.SaveChangesAsync() > 0;
        }
        #endregion

        #region Other
        [HttpGet("CanConnect")]
        public async Task<InitialConnectionStatus> CanConnect()
        {
            InitialConnectionStatus initialDbConnect = new InitialConnectionStatus();
            if (!Utils.ExistAccountServerConfig())
            {
                Utils.LockedConfigCreator();
                return initialDbConnect;
            }
            initialDbConnect.IsConnected = await _Auth.PingAsync();
            return initialDbConnect;
        }
        [HttpGet("CanConnectGS")]
        public async Task<InitialConnectionStatus> CanConnectGS()
        {
            InitialConnectionStatus initialDbConnect = new InitialConnectionStatus();
            if (!Utils.ExistGameServerConfig())
            {
                return initialDbConnect;
            }
            initialDbConnect.IsConnected = await _GameDbContext.Database.CanConnectAsync();
            return initialDbConnect;
        }
        [HttpGet("ASMigrationInit")]
        public async Task ASMigrationInit()
        {
            await _Auth.EnsureIndexesAsync();
        }
        [HttpGet("GSMigrationInit")]
        public async Task GSMigrationInit()
        {
            await _GameDbContext.Database.EnsureCreatedAsync();
        }
        [HttpGet("GetOnlinePlayers")]
        public async Task<Online> GetOnlinePlayers(string Servername)
        {
            return await _Auth.GetOnlineAsync(Servername);
        }
        [HttpPost("SetOnlinePlayers")]
        public async Task<Online> SetOnlinePlayers(Online Online)
        {
            return await _Auth.SaveOnlineAsync(Online);
        }
        [HttpGet("GetServerByName")]
        public async Task<Server> SetServerNameAsync(string serverName)
        {
            return await _Auth.GetServerByNameAsync(serverName);
        }
        [HttpPost("SetServerName")]
        public async Task SetServerNameAsync(SetServerName setServerName)
        {
            await _Auth.SetServerNameAsync(setServerName.ServerID, setServerName.ServerName);
        }
        [HttpGet("GetASConfig")]
        public async Task<AccountServerConfig> GetASConfig()
        {
            AccountServerConfig configObject = null;
            bool ExistAccountServer = Utils.ExistAccountServerConfig();
            if (ExistAccountServer)
            {
                configObject = Utils.ReadAccountServerConfig();
            }
            return configObject;
        }
        [HttpGet("GetGSConfig")]
        public async Task<GameServerConfig> GetGSConfig()
        {
            GameServerConfig configObject = null;
            bool ExistGameServer = Utils.ExistAccountServerConfig();
            if (ExistGameServer)
            {
                configObject = Utils.ReadGameServerConfig();
            }
            return configObject;
        }
        [HttpPost("SetGSConfig")]
        public async Task<bool> SetGSConfig(GameServerConfig GSConfig)
        {
            return Utils.SaveGameServerConfig(GSConfig);
        }
        #endregion
    }
}
