# Venture
An open-source alternative to PlayerIOClient primarily for .NET Core.

## Getting Started
The usage of this library is slightly different than the client [Player.IO](https://playerio.com) offers,
although the functionality is essentially the same with several additional benefits and features which will be further detailed below.

### Authentication
```csharp
var basic = PlayerIO.Connect("game-id", "connection-id", "user-id", "auth");
var simple = PlayerIO.SimpleConnect("game-id", "username-or-email", "password");
var kong = PlayerIO.KongregateConnect("game-id", "user-id", "token");
var facebook = PlayerIO.FacebookConnect("game-id", "access-token");
var armor = PlayerIO.ArmorGamesConnect("game-id", "user-id", "token");
var steam= PlayerIO.SteamConnect("game-id", "app-id", "session-ticket");
```

### Multiplayer
```csharp
var client = PlayerIO.Connect("game-id", "connection-id", "user-id", "auth").Connect();
var connection = client.Multiplayer.CreateJoinRoom("room-id", "room-type");

connection.OnMessage += (con, message) => Console.WriteLine(message);
connection.Send("init");
``` 

### Account Management
```csharp
if (PlayerIO.SimpleConnect("game-id", "current-email", "password").ChangeEmail("new-email"))
    Console.WriteLine("Email changed!");

if (PlayerIO.SimpleConnect("game-id", "current-email", "password").ChangePassword("new-password"))
    Console.WriteLine("Password changed!");
```

### Account Registration
```csharp
var captcha = SimpleConnect.GetSimpleCaptcha("game-id", 64, 64);
var key = captcha.CaptchaKey; // the required key of the captcha
var url = captcha.CaptchaImageUrl; // an image containing the captcha text

PlayerIO.SimpleRegister("game-id", "username", "password", "email", key, "captcha-text");
```

### BigDB

#### You can create, load and delete database objects from BigDB as well as serialize and deserialize them for local storage.
#### The serializtion uses [TSON](https://githb.com/atillabyte/tson) and can be restored back into a database object with `DatabaseObject.LoadFromString()`
```csharp
client.BigDB.LoadMyPlayerObject()
            .Set("Username", "Alice")
            .Set("Age", 17)
            .Set("IsOnline", true)
            .Set("Birthday", new DateTime(2002, 05, 11))
            .Set("Backpack", new DatabaseArray().Add("Book").Add("Plushie"))
            .Set("Friends",  new DatabaseArray().Add(new DatabaseObject().Set("Username", "Joey"))).Save();
```
```csharp
// serialized with .ToString()
{
    "Username": string("Alice"),
    "Age": int(17),
    "IsOnline": bool(true),
    "Birthday": datetime("2002-05-11T00:00:00.0000000"),
    "Backpack": [
        string("Book"),
        string("Plushie")
    ],
    "Friends": [
        {
            "Username": string("Joey")
        }
    ]
}
```

