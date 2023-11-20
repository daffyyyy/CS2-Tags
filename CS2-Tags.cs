using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using Newtonsoft.Json.Linq;
using System.Reflection;
using static CounterStrikeSharp.API.Core.Listeners;

namespace CS2_Tags;
public class CS2_Tags : BasePlugin
{
	public static JObject? JsonTags { get; private set; }
	public override string ModuleName => "CS2-Tags";
	public override string ModuleDescription => "Add player tags easily in cs2 game";
	public override string ModuleAuthor => "daffyy";
	public override string ModuleVersion => "1.0.0";

	public override void Load(bool hotReload)
	{
		CreateOrLoadJsonFile(ModuleDirectory + "/tags.json");

		RegisterListener<Listeners.OnClientPutInServer>(OnClientPutInServer);
		AddCommandListener("say", OnPlayerChat);
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

	private void OnClientPutInServer(int playerSlot)
	{
		CCSPlayerController? player = Utilities.GetPlayerFromSlot(playerSlot);

		if (player == null || !player.IsValid) return;

		string steamid = new SteamID(player.SteamID).SteamId64.ToString();

		if (JsonTags != null && JsonTags.TryGetValue("tags", out var tags) && tags is JObject tagsObject)
		{
			if (tagsObject.TryGetValue(steamid, out var playerTag) && playerTag is JObject)
			{
				player.Clan = playerTag["scoreboard"]?.ToString() ?? "";
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
							player.Clan = permissionTag["prefix"]?.ToString() ?? "";
							break;
						}
					}
				}
			}
		}
	}

	private HookResult OnPlayerChat(CCSPlayerController? player, CommandInfo info)
	{
		if (player == null || !player.IsValid) return HookResult.Continue;
		string steamid = new SteamID(player.SteamID).SteamId64.ToString();

		if (JsonTags != null && JsonTags.TryGetValue("tags", out var tags) && tags is JObject tagsObject)
		{
			if (tagsObject.TryGetValue(steamid, out var playerTag) && playerTag is JObject)
			{
				string prefix = playerTag["prefix"]?.ToString() ?? "";
				string nickColor = playerTag["nick_color"]?.ToString() ?? "";
				string messageColor = playerTag["message_color"]?.ToString() ?? "";

				Server.PrintToChatAll(ReplaceTags($" {prefix}{nickColor}{player.PlayerName}{ChatColors.Default}: {messageColor}{info.GetArg(1)}"));

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
							string nickColor = permissionTag["nick_color"]?.ToString() ?? "";
							string messageColor = permissionTag["message_color"]?.ToString() ?? "";
							Server.PrintToChatAll(ReplaceTags($" {prefix}{nickColor}{player.PlayerName}{ChatColors.Default}: {messageColor}{info.GetArg(1)}"));

							return HookResult.Handled;
						}
					}
				}
			}
		}

		return HookResult.Continue;
	}

	private string ReplaceTags(string message)
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
			return modifiedValue;
		}

		return message;
	}

}
