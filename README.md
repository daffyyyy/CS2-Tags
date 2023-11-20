# CS2-Tags

### Description
Adds tags to the server that can be easily edited, tags can be assigned via permission or steamid64

### Confiugration
```
{
  "tags": {
    "@css/chat": { // Permission 
      "prefix": "{GREEN}[ADMIN]", // Chat prefix
      "nick_color": "{RED}", // Nick color
      "message_color": "{GOLD}", // Message nick color
      "scoreboard": "[ADMIN]" // Scoreboard tag
    },
    "@css/ban": {
      "prefix": "{GREEN}[ADMIN]",
      "nick_color": "{RED}",
      "message_color": "{GOLD}",
      "scoreboard": "[ADMIN]"
    },
    "76561198202892670": { // SteamID64
      "prefix": "{GREEN}[ADMIN]",
      "nick_color": "{RED}",
      "message_color": "{GOLD}",
      "scoreboard": "[ADMIN]"
    }
  }
}
```
In addons/counterstrikesharp/plugins/CS2-Tags/tags.json
