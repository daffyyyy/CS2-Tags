using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using Newtonsoft.Json.Linq;
using System.Numerics;
using System.Reflection;
using static CounterStrikeSharp.API.Core.Listeners;

namespace CS2_Tags;
public class CS2_Tags : BasePlugin
{
	public static JObject? JsonTags { get; private set; }
	public override string ModuleName => "CS2-Tags";
	public override string ModuleDescription => "Add player tags easily in cs2 game";
	public override string ModuleAuthor => "daffyy";
	public override string ModuleVersion => "1.0.2";

	public override void Load(bool hotReload)
	{
		CreateOrLoadJsonFile(ModuleDirectory + "/tags.json");

		RegisterListener<Listeners.OnClientAuthorized>(OnClientAuthorized);
		RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
		AddCommandListener("say", OnPlayerChat);
		AddCommandListener("say_team", OnPlayerChatTeam);
	}

	private static void CreateOrLoadJsonFile(string filepath)
	{
		if (!File.Exists(filepath))
		{
			var templateData = new JObject
			{
				["tags"] = new JObject
				{
					["@css/chat"] = new JObject
					{
						["prefix"] = "{GREEN}[CHAT]",
						["nick_color"] = "{RED}",
						["message_color"] = "{GOLD}",
						["scoreboard"] = "[CHAT]"
					},
					["76561197961430531"] = new JObject
					{
						["prefix"] = "{GREEN}[ADMIN]",
						["nick_color"] = "{RED}",
						["message_color"] = "{GOLD}",
						["scoreboard"] = "[ADMIN]"
					},
					["everyone"] = new JObject
					{
						["prefix"] = "{Grey}[Player]",
						["nick_color"] = "",
						["message_color"] = "",
						["scoreboard"] = "[Player]"
					},
				}
			};

			File.WriteAllText(filepath, templateData.ToString());
			var jsonData = File.ReadAllText(filepath);
			JsonTags = JObject.Parse(jsonData);
		}
		else
		{
			var jsonData = File.ReadAllText(filepath);
			JsonTags = JObject.Parse(jsonData);
		}
	}

	[ConsoleCommand("css_tags_reload")]
	public void OnReloadConfig(CCSPlayerController? player, CommandInfo info)
	{
		if (player != null) return;
		CreateOrLoadJsonFile(ModuleDirectory + "/tags.json");
	}

	private void OnClientAuthorized(int playerSlot, SteamID steamId)
	{
		CCSPlayerController? player = Utilities.GetPlayerFromSlot(playerSlot);

		if (player == null || !player.IsValid || player.IsBot) return;

		SetPlayerClanTag(player);
	}

	private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
	{
		CCSPlayerController? player = @event.Userid;
		if (player == null || !player.IsValid || player.IsBot) return HookResult.Continue;

		SetPlayerClanTag(player);

		return HookResult.Continue;
	}

	private HookResult OnPlayerChat(CCSPlayerController? player, CommandInfo info)
	{
		if (player == null || !player.IsValid) return HookResult.Continue;
		string steamid = new SteamID(player.SteamID).SteamId64.ToString();

		if (info.GetArg(1).StartsWith("!") || info.GetArg(1).StartsWith("/") || info.GetArg(1) == "rtv") return HookResult.Continue;

		if (JsonTags != null && JsonTags.TryGetValue("tags", out var tags) && tags is JObject tagsObject)
		{
			if (tagsObject.TryGetValue(steamid, out var playerTag) && playerTag is JObject)
			{
				string prefix = playerTag["prefix"]?.ToString() ?? "";
				string? nickColor = !string.IsNullOrEmpty(playerTag?["nick_color"]?.ToString()) ? playerTag?["nick_color"]?.ToString() : $"{ChatColors.Default}";
				string messageColor = playerTag?["message_color"]?.ToString() ?? $"{ChatColors.Default}";

				Server.PrintToChatAll(ReplaceTags($" {prefix}{nickColor}{player.PlayerName}{ChatColors.Default}: {messageColor}{info.GetArg(1)}", player.TeamNum));

				/* Temp fix for commands OLD
				if (info.GetArg(1).StartsWith("!") || info.GetArg(1).StartsWith("/"))
				{
					CBasePlayerPawn pawn = new CBasePlayerPawn(NativeAPI.GetEntityFromIndex((int)player.EntityIndex!.Value.Value));
					int playerIndex = (int)pawn.Controller.Value.EntityIndex!.Value.Value;

					NativeAPI.IssueClientCommand(playerIndex, $"css_{info.GetArg(1).Replace("!", "")}");
				}
				*/

				return HookResult.Handled;
			}

			foreach (var tagKey in tagsObject.Properties())
			{
				if (tagKey.Name.StartsWith("@"))
				{
					string permission = tagKey.Name;
					bool hasPermission = AdminManager.PlayerHasPermissions(player, permission);

					if (hasPermission)
					{
						if (tagsObject.TryGetValue(permission, out var permissionTag) && permissionTag is JObject)
						{
							string prefix = permissionTag["prefix"]?.ToString() ?? "";
							string? nickColor = !string.IsNullOrEmpty(permissionTag?["nick_color"]?.ToString()) ? permissionTag?["nick_color"]?.ToString() : $"{ChatColors.Default}";
							string messageColor = permissionTag?["message_color"]?.ToString() ?? $"{ChatColors.Default}";

							Server.PrintToChatAll(ReplaceTags($" {prefix}{nickColor}{player.PlayerName}{ChatColors.Default}: {messageColor}{info.GetArg(1)}", player.TeamNum));

							return HookResult.Handled;
						}
					}
				}
			}

			if (tagsObject.TryGetValue("everyone", out var everyoneTag) && everyoneTag is JObject)
			{
				string prefix = everyoneTag["prefix"]?.ToString() ?? "";
				string? nickColor = !string.IsNullOrEmpty(everyoneTag?["nick_color"]?.ToString()) ? everyoneTag?["nick_color"]?.ToString() : $"{ChatColors.Default}";
				string messageColor = everyoneTag?["message_color"]?.ToString() ?? $"{ChatColors.Default}";

				Server.PrintToChatAll(ReplaceTags($" {prefix}{nickColor}{player.PlayerName}{ChatColors.Default}: {messageColor}{info.GetArg(1)}", player.TeamNum));

				return HookResult.Handled;
			}
		}

		return HookResult.Continue;
	}

	private HookResult OnPlayerChatTeam(CCSPlayerController? player, CommandInfo info)
	{
		if (player == null || !player.IsValid) return HookResult.Continue;
		string steamid = new SteamID(player.SteamID).SteamId64.ToString();

		if (info.GetArg(1).StartsWith("!") || info.GetArg(1).StartsWith("/") || info.GetArg(1) == "rtv") return HookResult.Continue;

		if (JsonTags != null && JsonTags.TryGetValue("tags", out var tags) && tags is JObject tagsObject)
		{
			string deadIcon = !player.PawnIsAlive ? $"{ChatColors.White}☠ {ChatColors.Default}" : "";
			if (tagsObject.TryGetValue(steamid, out var playerTag) && playerTag is JObject)
			{
				string prefix = playerTag["prefix"]?.ToString() ?? "";
				string? nickColor = !string.IsNullOrEmpty(playerTag?["nick_color"]?.ToString()) ? playerTag?["nick_color"]?.ToString() : $"{ChatColors.Default}";
				string messageColor = playerTag?["message_color"]?.ToString() ?? $"{ChatColors.Default}";

				for (int i = 1; i <= Server.MaxPlayers; i++)
				{
					CCSPlayerController? p = Utilities.GetPlayerFromIndex(i);
					if (p == null || !p.IsValid || p.IsBot || p.TeamNum != player.TeamNum) continue;

					p.PrintToChat(ReplaceTags($" {deadIcon}{TeamName(player.TeamNum)} {ChatColors.Default}{prefix}{nickColor}{player.PlayerName}{ChatColors.Default}: {messageColor}{info.GetArg(1)}", p.TeamNum));
				}

				return HookResult.Handled;
			}

			foreach (var tagKey in tagsObject.Properties())
			{
				if (tagKey.Name.StartsWith("@"))
				{
					string permission = tagKey.Name;
					bool hasPermission = AdminManager.PlayerHasPermissions(player, permission);

					if (hasPermission)
					{
						if (tagsObject.TryGetValue(permission, out var permissionTag) && permissionTag is JObject)
						{
							string prefix = permissionTag["prefix"]?.ToString() ?? "";
							string? nickColor = !string.IsNullOrEmpty(permissionTag?["nick_color"]?.ToString()) ? permissionTag?["nick_color"]?.ToString() : $"{ChatColors.Default}";
							string messageColor = permissionTag["message_color"]?.ToString() ?? $"{ChatColors.Default}"; ;

							for (int i = 1; i <= Server.MaxPlayers; i++)
							{
								CCSPlayerController? p = Utilities.GetPlayerFromIndex(i);
								if (p == null || !p.IsValid || p.IsBot || p.TeamNum != player.TeamNum) continue;

								p.PrintToChat(ReplaceTags($" {deadIcon}{TeamName(player.TeamNum)} {ChatColors.Default}{prefix}{nickColor}{player.PlayerName}{ChatColors.Default}: {messageColor}{info.GetArg(1)}", p.TeamNum));
							}

							return HookResult.Handled;
						}
					}
				}
			}

			if (tagsObject.TryGetValue("everyone", out var everyoneTag) && everyoneTag is JObject)
			{
				string prefix = everyoneTag["prefix"]?.ToString() ?? "";
				string? nickColor = !string.IsNullOrEmpty(everyoneTag?["nick_color"]?.ToString()) ? everyoneTag?["nick_color"]?.ToString() : $"{ChatColors.Default}";
				string messageColor = everyoneTag?["message_color"]?.ToString() ?? $"{ChatColors.Default}";

				for (int i = 1; i <= Server.MaxPlayers; i++)
				{
					CCSPlayerController? p = Utilities.GetPlayerFromIndex(i);
					if (p == null || !p.IsValid || p.IsBot || p.TeamNum != player.TeamNum) continue;

					p.PrintToChat(ReplaceTags($" {TeamName(player.TeamNum)} {ChatColors.Default}{prefix}{nickColor}{player.PlayerName}{ChatColors.Default}: {messageColor}{info.GetArg(1)}", p.TeamNum));
				}

				return HookResult.Handled;
			}
		}

		for (int i = 1; i <= Server.MaxPlayers; i++)
		{
			CCSPlayerController? p = Utilities.GetPlayerFromIndex(i);
			if (p == null || !p.IsValid || p.IsBot || p.TeamNum != player.TeamNum) continue;

			p.PrintToChat(ReplaceTags($" {TeamName(player.TeamNum)} {ChatColors.Default}{player.PlayerName}: {info.GetArg(1)}", p.TeamNum));
		}

		return HookResult.Handled;
	}

	private void SetPlayerClanTag(CCSPlayerController? player)
	{
		if (player == null || !player.IsValid || player.IsBot) return;

		string steamid = new SteamID(player.SteamID).SteamId64.ToString();

		if (JsonTags != null && JsonTags.TryGetValue("tags", out var tags) && tags is JObject tagsObject)
		{
			if (tagsObject.TryGetValue(steamid, out var playerTag) && playerTag is JObject)
			{
				player.Clan = playerTag["scoreboard"]?.ToString() ?? "";
				return;
			}

			foreach (var tagKey in tagsObject.Properties())
			{
				if (tagKey.Name.StartsWith("@"))
				{
					string permission = tagKey.Name;
					bool hasPermission = AdminManager.PlayerHasPermissions(player, permission);

					if (hasPermission)
					{
						if (tagsObject.TryGetValue(permission, out var permissionTag) && permissionTag is JObject)
						{
							player.Clan = permissionTag["scoreboard"]?.ToString() ?? "";
							return;
						}
					}
				}
			}

			if (JsonTags != null && JsonTags["tags"]?["everyone"]?["scoreboard"] != null)
			{
				player.Clan = JsonTags?["tags"]?["everyone"]?["scoreboard"]?.ToString() ?? "";
			}
		}
	}

	private string TeamName(int teamNum)
	{
		string teamName = "";

		switch (teamNum)
		{
			case 0:
				teamName = $"(NONE)";
				break;
			case 1:
				teamName = $"(SPEC)";
				break;
			case 2:
				teamName = $"{ChatColors.Gold}(T)";
				break;
			case 3:
				teamName = $"{ChatColors.Blue}(CT)";
				break;
		}

		return teamName;
	}

	private string TeamColor(int teamNum)
	{
		string teamColor = "";

		switch (teamNum)
		{
			case 2:
				teamColor = $"{ChatColors.Gold}";
				break;
			case 3:
				teamColor = $"{ChatColors.Blue}";
				break;
			default:
				teamColor = "";
				break;
		}

		return teamColor;
	}

	private string ReplaceTags(string message, int teamNum = 0)
	{
		if (message.Contains('{'))
		{
			string modifiedValue = message;
			foreach (FieldInfo field in typeof(ChatColors).GetFields())
			{
				string pattern = $"{{{field.Name}}}";
				if (message.Contains(pattern, StringComparison.OrdinalIgnoreCase))
				{
					modifiedValue = modifiedValue.Replace(pattern, field.GetValue(null)!.ToString(), StringComparison.OrdinalIgnoreCase);
				}
			}
			return modifiedValue.Replace("{TEAMCOLOR}", TeamColor(teamNum));
		}

		return message;
	}

}
