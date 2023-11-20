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

### Colors
```
        public static char Default = '\x01';
        public static char White = '\x01';
        public static char Darkred = '\x02';
        public static char Green = '\x04';
        public static char LightYellow = '\x03';
        public static char LightBlue = '\x03';
        public static char Olive = '\x05';
        public static char Lime = '\x06';
        public static char Red = '\x07';
        public static char Purple = '\x03';
        public static char Grey = '\x08';
        public static char Yellow = '\x09';
        public static char Gold = '\x10';
        public static char Silver = '\x0A';
        public static char Blue = '\x0B';
        public static char DarkBlue = '\x0C';
        public static char BlueGrey = '\x0D';
        public static char Magenta = '\x0E';
        public static char LightRed = '\x0F';
```
Use color name for e.g. {LightRed}

### TODO
- Apply tags in team chat
