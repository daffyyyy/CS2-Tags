# CS2-Tags

### Do you appreciate what I do? Buy me a cup of tea ❤️
[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/Y8Y4THKXG)

![image](https://github.com/daffyyyy/CS2-Tags/assets/41084667/25dd3f2b-0604-41a2-b2bd-9be230db71e1)
![image](https://github.com/daffyyyy/CS2-Tags/assets/41084667/663a0de1-b875-48fc-bda5-56add5a4833b)

### Description
Adds tags to the server that can be easily edited, tags can be assigned via permission or steamid64

### Commands
- css_tags_reload - Reload tags config

### Configuration
```
{
  "tags": {
    "#css/admin": { // Group 
      "prefix": "{GREEN}[ADMIN]", // Chat prefix
      "nick_color": "{RED}", // Nick color
      "message_color": "{GOLD}", // Message nick color
      "scoreboard": "[ADMIN]" // Scoreboard tag
    },
    "@css/chat": { // Permission 
      "prefix": "{GREEN}[ADMIN]", // Chat prefix
      "nick_color": "{RED}", // Nick color
      "message_color": "{GOLD}", // Message nick color
      "scoreboard": "[ADMIN]" // Scoreboard tag
    },
    "76561198202892670": { // SteamID64
      "prefix": "{GREEN}[ADMIN]",
      "nick_color": "{RED}",
      "message_color": "{GOLD}",
      "scoreboard": "[ADMIN]"
    },
    "everyone": { // Tag for everyone, bots excluded
      "prefix": "",
      "nick_color": "",
      "message_color": "",
      "scoreboard": "[Player]"
    }
  }
}
```
In addons/counterstrikesharp/plugins/CS2-Tags/tags.json

### Requirments
[CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp/) **tested on v142**

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

```{TEAMCOLOR} - Team color```
Use color name for e.g. {LightRed}

### TODO
- Thinking about better fix for commands handling
