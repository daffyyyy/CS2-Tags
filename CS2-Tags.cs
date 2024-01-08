using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace CS2_Tags;

[MinimumApiVersion(142)]
public class CS2_Tags : BasePlugin
{
	private List<int> GaggedIds = new List<int>();
	public static JObject? JsonTags { get; private set; }
	public override string ModuleName => "CS2-Tags";
	public override string ModuleDescription => "Add player tags easily in cs2 game";
	public override string ModuleAuthor => "daffyy";
	public override string ModuleVersion => "1.0.3a";

	public override void Load(bool hotReload)
	{
		CreateOrLoadJsonFile(ModuleDirectory + "/tags.json");

		RegisterListener<Listeners.OnClientAuthorized>(OnClientAuthorized);
		RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
		RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
		RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
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
					["#css/admin"] = new JObject
					{
						["prefix"] = "{RED}[ADMIN]",
						["nick_color"] = "{RED}",
						["message_color"] = "{GOLD}",
						["scoreboard"] = "[ADMIN]"
					},
					["@css/chat"] = new JObject
					{
						["prefix"] = "{GREEN}[CHAT]",
						["nick_color"] = "{RED}",
						["message_color"] = "{GOLD}",
						["scoreboard"] = "[CHAT]"
					},
					["76561197961430531"] = new JObject
					{
						["prefix"] = "{RED}[ADMIN]",
						["nick_color"] = "{RED}",
						["message_color"] = "{GOLD}",
						["scoreboard"] = "[ADMIN]"
					},
					["everyone"] = new JObject
					{
						["team_chat"] = false,
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

		Server.PrintToConsole("[CS2-Tags] Config reloaded!");
	}

	[ConsoleCommand("css_tag_mute")]
	[CommandHelper(minArgs: 1, usage: "<Index>", whoCanExecute: CommandUsage.SERVER_ONLY)]
	public void OnTagMuteCommand(CCSPlayerController? caller, CommandInfo command)
	{
		int index = 0;
		int.TryParse(command.GetArg(1), out index);

		if (index == 0)
			return;

		if (!GaggedIds.Contains(index))
			GaggedIds.Add(index);
	}

	[ConsoleCommand("css_tag_unmute")]
	[CommandHelper(minArgs: 1, usage: "<Index>", whoCanExecute: CommandUsage.SERVER_ONLY)]
	public void OnTagUnMuteCommand(CCSPlayerController? caller, CommandInfo command)
	{
		int index = 0;
		int.TryParse(command.GetArg(1), out index);

		if (index == 0)
			return;

		if (GaggedIds.Contains(index))
			GaggedIds.Remove(index);
	}

	private void OnClientAuthorized(int playerSlot, SteamID steamId)
	{
		CCSPlayerController? player = Utilities.GetPlayerFromSlot(playerSlot);

		if (player == null || !player.IsValid || player.IsBot) return;

		AddTimer(2.0f, () => SetPlayerClanTag(player));
	}

	private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
	{
		CCSPlayerController? player = @event.Userid;

		if (player == null || !player.IsValid || player.IsBot) return HookResult.Continue;

		AddTimer(2.0f, () => SetPlayerClanTag(player));

		return HookResult.Continue;
	}

	private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
	{
		CCSPlayerController? player = @event.Userid;
		if (player == null || !player.IsValid || player.IsBot) return HookResult.Continue;

		AddTimer(1.5f, () => SetPlayerClanTag(player));

		return HookResult.Continue;
	}

	private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
	{
		CCSPlayerController? player = @event.Userid;
		if (player == null || !player.IsValid || player.IsBot) return HookResult.Continue;

		AddTimer(1.5f, () => SetPlayerClanTag(player));

		return HookResult.Continue;
	}

	private HookResult OnPlayerChat(CCSPlayerController? player, CommandInfo info)
	{
		if (player == null || !player.IsValid || info.GetArg(1).Length == 0 || player.AuthorizedSteamID == null) return HookResult.Continue;
		string steamid = player.AuthorizedSteamID.SteamId64.ToString();

		if (GaggedIds.Contains((int)player.Index)) return HookResult.Handled;

		if (info.GetArg(1).StartsWith("!") || info.GetArg(1).StartsWith("@") || info.GetArg(1).StartsWith("/") || info.GetArg(1).StartsWith(".") || info.GetArg(1) == "rtv") return HookResult.Continue;

		if (JsonTags != null && JsonTags.TryGetValue("tags", out var tags) && tags is JObject tagsObject)
		{
			string deadIcon = !player.PawnIsAlive ? $"{ChatColors.White}☠ {ChatColors.Default}" : "";

			if (tagsObject.TryGetValue(steamid, out var playerTag) && playerTag is JObject)
			{
				string prefix = playerTag["prefix"]?.ToString() ?? "";
				string nickColor = playerTag?["nick_color"]?.ToString() ?? ChatColors.Default.ToString();
				string messageColor = playerTag?["message_color"]?.ToString() ?? ChatColors.Default.ToString();

				Server.PrintToChatAll(ReplaceTags($" {deadIcon}{prefix}{nickColor}{player.PlayerName}{ChatColors.Default}: {messageColor}{info.GetArg(1)}", player.TeamNum));

				return HookResult.Handled;
			}

			foreach (var tagKey in tagsObject.Properties())
			{
				if (tagKey.Name.StartsWith("#"))
				{
					string group = tagKey.Name;
					bool inGroup = AdminManager.PlayerInGroup(player, group);

					if (inGroup)
					{
						if (tagsObject.TryGetValue(group, out var groupTag) && groupTag is JObject)
						{
							string prefix = groupTag["prefix"]?.ToString() ?? "";
							string nickColor = groupTag?["nick_color"]?.ToString() ?? ChatColors.Default.ToString();
							string messageColor = groupTag?["message_color"]?.ToString() ?? ChatColors.Default.ToString();

							Server.PrintToChatAll(ReplaceTags($" {deadIcon}{prefix}{nickColor}{player.PlayerName}{ChatColors.Default}: {messageColor}{info.GetArg(1)}", player.TeamNum));

							return HookResult.Handled;
						}
					}
				}

				if (tagKey.Name.StartsWith("@"))
				{
					string permission = tagKey.Name;
					bool hasPermission = AdminManager.PlayerHasPermissions(player, permission);

					if (hasPermission)
					{
						if (tagsObject.TryGetValue(permission, out var permissionTag) && permissionTag is JObject)
						{
							string prefix = permissionTag["prefix"]?.ToString() ?? "";
							string nickColor = permissionTag?["nick_color"]?.ToString() ?? ChatColors.Default.ToString();
							string messageColor = permissionTag?["message_color"]?.ToString() ?? ChatColors.Default.ToString();

							Server.PrintToChatAll(ReplaceTags($" {deadIcon}{prefix}{nickColor}{player.PlayerName}{ChatColors.Default}: {messageColor}{info.GetArg(1)}", player.TeamNum));

							return HookResult.Handled;
						}
					}
				}
			}

			if (tagsObject.TryGetValue("everyone", out var everyoneTag) && everyoneTag is JObject && everyoneTag?["team_chat"]?.Value<bool>() == true)
			{
				string prefix = everyoneTag["prefix"]?.ToString() ?? "";
				string nickColor = everyoneTag?["nick_color"]?.ToString() ?? ChatColors.Default.ToString();
				string messageColor = everyoneTag?["message_color"]?.ToString() ?? ChatColors.Default.ToString();

				Server.PrintToChatAll(ReplaceTags($" {deadIcon}{prefix}{nickColor}{player.PlayerName}{ChatColors.Default}: {messageColor}{info.GetArg(1)}", player.TeamNum));

				return HookResult.Handled;
			}
		}

		return HookResult.Continue;
	}

	private HookResult OnPlayerChatTeam(CCSPlayerController? player, CommandInfo info)
	{
		if (player == null || !player.IsValid || info.GetArg(1).Length == 0 || player.AuthorizedSteamID == null) return HookResult.Continue;
		string steamid = player.AuthorizedSteamID.SteamId64.ToString();

		if (GaggedIds.Contains((int)player.Index)) return HookResult.Handled;

		if (info.GetArg(1).StartsWith("@") && AdminManager.PlayerHasPermissions(player, "@css/chat"))
		{
			foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && !p.IsBot && !p.IsHLTV && AdminManager.PlayerHasPermissions(p, "@css/chat")))
			{
				p.PrintToChat($" {ChatColors.Lime}(ADMIN) {ChatColors.Default}{player.PlayerName}: {info.GetArg(1).Remove(0, 1)}");
			}

			return HookResult.Handled;
		}

		if (info.GetArg(1).StartsWith("!") || info.GetArg(1).StartsWith("@") || info.GetArg(1).StartsWith("/") || info.GetArg(1).StartsWith(".") || info.GetArg(1) == "rtv") return HookResult.Continue;

		if (JsonTags != null && JsonTags.TryGetValue("tags", out var tags) && tags is JObject tagsObject)
		{
			string deadIcon = !player.PawnIsAlive ? $"{ChatColors.White}☠ {ChatColors.Default}" : "";
			if (tagsObject.TryGetValue(steamid, out var playerTag) && playerTag is JObject)
			{
				string prefix = playerTag["prefix"]?.ToString() ?? "";
				string nickColor = playerTag?["nick_color"]?.ToString() ?? ChatColors.Default.ToString();
				string messageColor = playerTag?["message_color"]?.ToString() ?? ChatColors.Default.ToString();

				foreach (var p in Utilities.GetPlayers().Where(p => p.TeamNum == player.TeamNum && p.IsValid && !p.IsBot))
				{
					string messageToSend = $"{deadIcon}{TeamName(player.TeamNum)} {ChatColors.Default}{prefix}{nickColor}{player.PlayerName}{ChatColors.Default}: {messageColor}{info.GetArg(1)}";
					p.PrintToChat($" {ReplaceTags(messageToSend, p.TeamNum)}");
				}

				return HookResult.Handled;
			}

			foreach (var tagKey in tagsObject.Properties())
			{
				if (tagKey.Name.StartsWith("#"))
				{
					string group = tagKey.Name;
					bool inGroup = AdminManager.PlayerInGroup(player, group);

					if (inGroup && tagsObject.TryGetValue(group, out var groupTag) && groupTag is JObject)
					{
						string prefix = groupTag["prefix"]?.ToString() ?? "";
						string nickColor = groupTag["nick_color"]?.ToString() ?? ChatColors.Default.ToString();
						string messageColor = groupTag["message_color"]?.ToString() ?? ChatColors.Default.ToString();

						foreach (var p in Utilities.GetPlayers().Where(p => p.TeamNum == player.TeamNum && p.IsValid && !p.IsBot))
						{
							string messageToSend = $"{deadIcon}{TeamName(player.TeamNum)} {ChatColors.Default}{prefix}{nickColor}{player.PlayerName}{ChatColors.Default}: {messageColor}{info.GetArg(1)}";
							p.PrintToChat($" {ReplaceTags(messageToSend, p.TeamNum)}");
						}

						return HookResult.Handled;
					}
				}

				if (tagKey.Name.StartsWith("@"))
				{
					string permission = tagKey.Name;
					bool hasPermission = AdminManager.PlayerHasPermissions(player, permission);

					if (hasPermission && tagsObject.TryGetValue(permission, out var permissionTag) && permissionTag is JObject)
					{
						string prefix = permissionTag["prefix"]?.ToString() ?? "";
						string nickColor = permissionTag["nick_color"]?.ToString() ?? ChatColors.Default.ToString();
						string messageColor = permissionTag["message_color"]?.ToString() ?? ChatColors.Default.ToString();

						foreach (var p in Utilities.GetPlayers().Where(p => p.TeamNum == player.TeamNum && p.IsValid && !p.IsBot))
						{
							string messageToSend = $"{deadIcon}{TeamName(player.TeamNum)} {ChatColors.Default}{prefix}{nickColor}{player.PlayerName}{ChatColors.Default}: {messageColor}{info.GetArg(1)}";
							p.PrintToChat($" {ReplaceTags(messageToSend, p.TeamNum)}");
						}

						return HookResult.Handled;
					}
				}
			}

			if (tagsObject.TryGetValue("everyone", out var everyoneTag) && everyoneTag is JObject)
			{
				string prefix = everyoneTag["prefix"]?.ToString() ?? "";
				string nickColor = everyoneTag["nick_color"]?.ToString() ?? ChatColors.Default.ToString();
				string messageColor = everyoneTag["message_color"]?.ToString() ?? ChatColors.Default.ToString();

				foreach (var p in Utilities.GetPlayers().Where(p => p.TeamNum == player.TeamNum && p.IsValid && !p.IsBot))
				{
					string messageToSend = $"{deadIcon}{TeamName(player.TeamNum)} {ChatColors.Default}{prefix}{nickColor}{player.PlayerName}{ChatColors.Default}: {messageColor}{info.GetArg(1)}";
					p.PrintToChat($" {ReplaceTags(messageToSend, p.TeamNum)}");
				}
				//p.PrintToChat(ReplaceTags($" {TeamName(player.TeamNum)} {ChatColors.Default}{prefix}{nickColor}{player.PlayerName}{ChatColors.Default}: {messageColor}{info.GetArg(1)}", p.TeamNum));

				return HookResult.Handled;
			}
		}
		return HookResult.Continue;
	}

	private void SetPlayerClanTag(CCSPlayerController? player)
	{
		if (player == null || !player.IsValid || player.IsBot || player.IsHLTV || player.AuthorizedSteamID == null) return;

		string steamid = player.AuthorizedSteamID!.SteamId64.ToString();

		if (JsonTags != null && JsonTags.TryGetValue("tags", out var tags) && tags is JObject tagsObject)
		{
			if (tagsObject.TryGetValue(steamid, out var playerTag) && playerTag is JObject)
			{
				var scoreboardValue = playerTag["scoreboard"]?.ToString();
				if (!string.IsNullOrEmpty(scoreboardValue))
				{
					player.Clan = scoreboardValue;
					return;
				}
			}

			foreach (var tagKey in tagsObject.Properties())
			{
				if (tagKey.Name.StartsWith("#"))
				{
					string group = tagKey.Name;
					bool inGroup = AdminManager.PlayerInGroup(player, group);

					if (inGroup)
					{
						if (tagsObject.TryGetValue(group, out var groupTag) && groupTag is JObject)
						{
							var scoreboardValue = groupTag["scoreboard"]?.ToString();
							if (!string.IsNullOrEmpty(scoreboardValue))
							{
								player.Clan = scoreboardValue;
								return;
							}
						}
					}
				}

				if (tagKey.Name.StartsWith("@"))
				{
					string permission = tagKey.Name;
					bool hasPermission = AdminManager.PlayerHasPermissions(player, permission);

					if (hasPermission)
					{
						if (tagsObject.TryGetValue(permission, out var permissionTag) && permissionTag is JObject)
						{
							var scoreboardValue = permissionTag["scoreboard"]?.ToString();
							if (!string.IsNullOrEmpty(scoreboardValue))
							{
								player.Clan = scoreboardValue;
								return;
							}
						}
					}
				}
			}

			if (tagsObject.TryGetValue("everyone", out var everyone) && everyone is JObject everyoneObject)
			{
				var scoreboardValue = everyoneObject["scoreboard"]?.ToString();
				if (!string.IsNullOrEmpty(scoreboardValue))
				{
					player.Clan = scoreboardValue;
				}
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
				teamName = $"{ChatColors.Yellow}(T)";
				break;

			case 3:
				teamName = $"{ChatColors.Blue}(CT)";
				break;
		}

		return teamName;
	}

	private string TeamColor(int teamNum)
	{
		string teamColor;

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