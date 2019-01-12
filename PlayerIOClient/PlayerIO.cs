using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PlayerIOClient
{
    /// <summary>
    /// The entry class for the initial connection to Player.IO
    /// </summary>
    public static class PlayerIO
    {
        public static string LibraryVersion => "1.0.1"; // Venture version (major.minor.patch)

        /// <summary>
        /// Authenticate with SimpleUsers.
        /// </summary>
        public static SimpleConnect SimpleConnect
            => new Lazy<SimpleConnect>(() => new SimpleConnect()).Value;

        /// <summary>
        /// Authenticate with Facebook.
        /// </summary>
        public static FacebookConnect FacebookConnect
            => new Lazy<FacebookConnect>(() => new FacebookConnect()).Value;

        /// <summary>
        /// Authenticate with Kongregate.
        /// </summary>
        public static KongregateConnect KongregateConnect
            => new Lazy<KongregateConnect>(() => new KongregateConnect()).Value;

        /// <summary>
        /// Authenticate with Armor Games.
        /// </summary>
        public static ArmorGameConnect ArmorGameConnect
            => new Lazy<ArmorGameConnect>(() => new ArmorGameConnect()).Value;

        /// <summary>
        /// Authenticate with Steam.
        /// </summary>
        public static SteamConnect SteamConnect =>
            new Lazy<SteamConnect>(() => new SteamConnect()).Value;

        public static Client Authenticate(string gameId, string connectionId, Dictionary<string, string> authenticationArguments, string[] playerInsightSegments = null)
        {
            if (authenticationArguments?.ContainsKey("secureSimpleUserPasswordsOverHttp") == true && authenticationArguments["secureSimpleUserPasswordsOverHttp"] == "true")
            {
                var (publicKey, nonce) = PlayerIOAuth.SimpleUserGetSecureLoginInfo();
                authenticationArguments["password"] = PlayerIOAuth.SimpleUserPasswordEncrypt(publicKey, authenticationArguments["password"]);
                authenticationArguments["nonce"] = nonce;
            }

            var (success, response, error) = new PlayerIOChannel().Request<AuthenticateArgs, AuthenticateOutput>(13, new AuthenticateArgs
            {
                GameId = gameId,
                ConnectionId = connectionId,
                AuthenticationArguments = DictionaryEx.Convert(authenticationArguments ?? new Dictionary<string, string>()),
                PlayerInsightSegments = playerInsightSegments?.ToList() ?? new List<string>(),
                ClientAPI = $"csharp",
                ClientInfo = DictionaryEx.Convert(PlayerIOAuth.GetClientInfo()),
                PlayCodes = new List<string>()
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);

            return new Client(new PlayerIOChannel() { Token = response.Token })
            {
                PlayerInsight = new PlayerInsight(response.PlayerInsightState),
                ConnectUserId = response.UserId,
            };
        }

        /// <summary> Connects to a game based on Player.IO as the given user. </summary>
        /// <param name="gameId">
        /// The ID of the game you wish to connect to. This value can be found in the admin panel.
        /// </param>
        /// <param name="connectionId">
        /// The ID of the connection, as given in the settings section of the admin panel. 'public'
        /// should be used as the default.
        /// </param>
        /// <param name="userId"> The ID of the user you wish to authenticate. </param>
        /// <param name="auth">
        /// If the connection identified by ConnectionIdentifier only accepts authenticated requests:
        /// The auth value generated based on 'userId'. You can generate an auth value using the
        /// CalcAuth() method.
        /// </param>
        public static Client Connect(string gameId, string connectionId, string userId, string auth)
        {
            var (success, response, error) = new PlayerIOChannel().Request<ConnectArgs, ConnectOutput>(10, new ConnectArgs()
            {
                GameId = gameId,
                ConnectionId = connectionId,
                UserId = userId,
                Auth = auth
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);

            return new Client(new PlayerIOChannel() { Token = response.Token })
            {
                PlayerInsight = new PlayerInsight(response.PlayerInsightState),
                ConnectUserId = response.UserId
            };
        }

        /// <summary> Calculate an auth hash for use in the Connect method. </summary>
        /// <param name="userId"> The UserID to use when generating the hash. </param>
        /// <param name="sharedSecret">
        /// The shared secret to use when generating the hash. This must be the same value as the one
        /// given to a connection in the admin panel.
        /// </param>
        /// <returns> The generated auth hash (based on SHA-1) </returns>
        public static string CalcAuth(string userId, string sharedSecret)
        {
            var unixTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

            using (var hmacInstance = new HMACSHA1(Encoding.UTF8.GetBytes(sharedSecret)))
            {
                var hmacHash = hmacInstance.ComputeHash(Encoding.UTF8.GetBytes(unixTime + ":" + userId));

                var strBld = new StringBuilder(unixTime + ":" + BitConverter.ToString(hmacHash));
                return strBld.Replace("-", "").ToString().ToLower(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        /// <summary> Calculate an auth hash for use in the Connect method. </summary>
        /// <param name="userId"> The UserID to use when generating the hash. </param>
        /// <param name="sharedSecret"> The shared secret to use when generating the hash. This must be the same value as the one
        /// given to a connection in the admin panel. </param>
        /// <returns> The generated auth hash (based on SHA-256) </returns>
        public static string CalcAuth256(string userId, string sharedSecret)
        {
            string ToHexString(byte[] bytes)
            {
                return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
            }

            var unixTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            return unixTime + ":" + ToHexString(new HMACSHA256(Encoding.UTF8.GetBytes(sharedSecret)).ComputeHash(Encoding.UTF8.GetBytes(unixTime + ":" + userId)));
        }
    }
    
    public class KongregateConnect
    {
        /// <summary>
        /// Authenticate with the Kongregate userId and token provided.
        /// </summary>
        public Client Connect()
        {
            if (string.IsNullOrEmpty(this.gameId))
                throw new ArgumentException("A game ID must be specified in order to use this method.");

            if (string.IsNullOrEmpty(this.token))
                throw new ArgumentException("A Kongregate token must be specified in order to use this method.");

            if (string.IsNullOrEmpty(this.userId))
                throw new ArgumentException("A Kongregate userId must be specified in order to use this method.");

            return PlayerIO.Authenticate(this.gameId, this.connectionId, DictionaryEx.Create(("userId", this.userId), ("gameAuthToken", this.token)));
        }

        public KongregateConnect GameId(string gameId)
        {
            this.gameId = gameId;
            return this;
        }

        public KongregateConnect Token(string token)
        {
            this.token = token;
            return this;
        }

        public KongregateConnect UserId(string userId)
        {
            this.userId = userId;
            return this;
        }

        /// <summary>
        /// If you are using a distinct connection type, specify it using this method. By default, the connection type will be set to 'public'
        /// </summary>
        public KongregateConnect ConnectionId(string connectionId)
        {
            this.connectionId = connectionId;
            return this;
        }

        private string connectionId { get; set; } = "public";
        private string gameId { get; set; }
        private string userId { get; set; }
        private string token { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();
    }

    public class ArmorGameConnect
    {
        /// <summary>
        /// Authenticate with the ArmorGams userId and auth token provided.
        /// </summary>
        public Client Connect()
        {
            if (string.IsNullOrEmpty(this.gameId))
                throw new ArgumentException("A game ID must be specified in order to use this method.");

            if (string.IsNullOrEmpty(this.token))
                throw new ArgumentException("An ArmorGames auth token must be specified in order to use this method.");

            if (string.IsNullOrEmpty(this.userId))
                throw new ArgumentException("An ArmorGames userId must be specified in order to use this method.");

            return PlayerIO.Authenticate(this.gameId, this.connectionId, DictionaryEx.Create(("userId", this.userId), ("authToken", this.token)));
        }

        public ArmorGameConnect GameId(string gameId)
        {
            this.gameId = gameId;
            return this;
        }

        public ArmorGameConnect Token(string token)
        {
            this.token = token;
            return this;
        }

        public ArmorGameConnect UserId(string userId)
        {
            this.userId = userId;
            return this;
        }

        /// <summary>
        /// If you are using a distinct connection type, specify it using this method. By default, the connection type will be set to 'public'
        /// </summary>
        public ArmorGameConnect ConnectionId(string connectionId)
        {
            this.connectionId = connectionId;
            return this;
        }

        private string connectionId { get; set; } = "public";
        private string gameId { get; set; }
        private string userId { get; set; }
        private string token { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();
    }

    public class SteamConnect
    {
        /// <summary>
        /// Authenticate with the Steam App ID and Session Ticket provided.
        /// </summary>
        public Client Connect()
        {
            if (string.IsNullOrEmpty(this.gameId))
                throw new ArgumentException("A game ID must be specified in order to use this method.");

            if (string.IsNullOrEmpty(this.steamAppId))
                throw new ArgumentException("A Steam App ID must be specified in order to use this method.");

            if (string.IsNullOrEmpty(this.steamSessionTicket))
                throw new ArgumentException("A Steam Session Ticket must be specified in order to use this method.");

            return PlayerIO.Authenticate(this.gameId, this.connectionId, DictionaryEx.Create(("steamSessionTicket", this.steamSessionTicket), ("steamAppId", this.steamAppId)));
        }

        public SteamConnect GameId(string gameId)
        {
            this.gameId = gameId;
            return this;
        }

        public SteamConnect SteamSessionTicket(string steamSessionTicket)
        {
            this.steamSessionTicket = steamSessionTicket;
            return this;
        }

        public SteamConnect SteamAppId(string steamAppId)
        {
            this.steamAppId = steamAppId;
            return this;
        }

        /// <summary>
        /// If you are using a distinct connection type, specify it using this method. By default, the connection type will be set to 'public'
        /// </summary>
        public SteamConnect ConnectionId(string connectionId)
        {
            this.connectionId = connectionId;
            return this;
        }

        private string connectionId { get; set; } = "public";
        private string gameId { get; set; }
        private string steamAppId { get; set; }
        private string steamSessionTicket { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();
    }

    public class FacebookConnect
    {
        /// <summary>
        /// Authenticate with the Facebook token provided.
        /// </summary>
        public Client Connect()
        {
            if (string.IsNullOrEmpty(this.gameId))
                throw new ArgumentException("A game ID must be specified in order to use this method.");

            if (string.IsNullOrEmpty(this.token))
                throw new ArgumentException("A Facebook token must be specified in order to use this method.");

            return PlayerIO.Authenticate(this.gameId, this.connectionId, DictionaryEx.Create(("accessToken", this.token)));
        }

        public FacebookConnect GameId(string gameId)
        {
            this.gameId = gameId;
            return this;
        }

        public FacebookConnect Token(string token)
        {
            this.token = token;
            return this;
        }

        /// <summary>
        /// If you are using a distinct connection type, specify it using this method. By default, the connection type will be set to 'public'
        /// </summary>
        public FacebookConnect ConnectionId(string connectionId)
        {
            this.connectionId = connectionId;
            return this;
        }

        private string connectionId { get; set; } = "public";
        private string gameId { get; set; }
        private string token { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();
    }

    public class SimpleConnect
    {
        /// <summary>
        /// Authenticate with the password plus the username and/or email provided.
        /// </summary>
        public Client Connect()
        {
            if (string.IsNullOrEmpty(this.gameId))
                throw new ArgumentException("A game ID must be specified in order to use this method.");

            if (string.IsNullOrEmpty(this.username) && string.IsNullOrEmpty(this.email))
                throw new ArgumentException("A username or email must be specified in order to use this method.");

            if (string.IsNullOrEmpty(this.password))
                throw new ArgumentException("A password must be specified in order to use this method.");

            return PlayerIO.Authenticate(this.gameId, this.connectionId, DictionaryEx.Create(
                ("username", this.username),
                ("email", this.email),
                ("password", this.password)
                ));
        }

        /// <summary>
        /// Create a captcha image and key, to be used for registrations where the added security of captcha is required.
        /// </summary>
        /// <param name="width"> The width of the captcha image. </param>
        /// <param name="height"> The height of the captcha image. </param>
        public SimpleCaptcha GetSimpleCaptcha(int width, int height)
        {
            if (string.IsNullOrEmpty(this.gameId))
                throw new ArgumentException("A game ID must be specified in order to use this method.");

            var (success, response, error) = new PlayerIOChannel().Request<SimpleGetCaptchaArgs, SimpleGetCaptchaOutput>(415, new SimpleGetCaptchaArgs
            {
                GameId = this.gameId,
                Width = width,
                Height = height
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);

            return new SimpleCaptcha(response.CaptchaKey, response.CaptchaImageUrl);
        }

        /// <summary>
        /// Check whether a simple user currently exists with the details provided.
        /// </summary>
        /// <returns> If the simple user exists already, this method returns <see langword="true"/> </returns>
        public bool RegistrationCheck()
        {
            if (string.IsNullOrEmpty(this.gameId))
                throw new ArgumentException("A game ID must be specified in order to use this method.");

            if (string.IsNullOrEmpty(this.username) && string.IsNullOrEmpty(this.email))
                throw new ArgumentException("A username or email must be specified in order to use this method.");

            var response = true;

            try
            {
                PlayerIO.Authenticate(this.gameId, this.connectionId, DictionaryEx.Create(
                    ("checkusername", "true"),
                    ("username", this.username),
                    ("email", this.email)
                ));
            }
            catch (PlayerIOError error)
            {
                if (error.ErrorCode == ErrorCode.UnknownUser)
                    response = false;
            }

            return response;
        }

        /// <summary>
        /// Change the password for a simple user with the provided username or email address, and valid password.
        /// </summary>
        /// <returns> If the change was successful, returns <see langword="true"/>. </returns>
        public bool ChangePassword(string newPassword)
        {
            if (string.IsNullOrEmpty(this.gameId))
                throw new ArgumentException("A game ID must be specified in order to use this method.");

            if (string.IsNullOrEmpty(this.username) && string.IsNullOrEmpty(this.email))
                throw new ArgumentException("A username or email must be specified in order to use this method.");

            if (string.IsNullOrEmpty(newPassword))
                throw new ArgumentException("You must specify a new password to use for this method.");

            var response = false;

            try
            {
                PlayerIO.Authenticate(this.gameId, this.connectionId, DictionaryEx.Create(
                    ("changepassword", "true"),
                    ("username", this.username),
                    ("email", this.email),
                    ("newpassword", newPassword)
                ));
            }
            catch (PlayerIOError error)
            {
                if (error.ErrorCode == ErrorCode.GeneralError && error.Message.ToLower().Contains("password changed"))
                    response = true;
            }

            return response;
        }

        public bool ChangeEmail(string newEmail)
        {
            if (string.IsNullOrEmpty(this.gameId))
                throw new ArgumentException("A game ID must be specified in order to use this method.");

            if (string.IsNullOrEmpty(this.username) && string.IsNullOrEmpty(this.email))
                throw new ArgumentException("A username or email must be specified in order to use this method.");

            if (string.IsNullOrEmpty(newEmail))
                throw new ArgumentException("You must specify a new email to use for this method.");

            var response = false;

            try
            {
                PlayerIO.Authenticate(this.gameId, this.connectionId, DictionaryEx.Create(
                    ("changeemail", "true"),
                    ("username", this.username),
                    ("email", this.email),
                    ("password", this.password),
                    ("newemail", newEmail)
                ));
            }
            catch (PlayerIOError error)
            {
                if (error.ErrorCode == ErrorCode.GeneralError && error.Message.ToLower().Contains("email address changed"))
                    response = true;
            }

            return response;
        }

        /// <summary>
        /// If you are using a distinct connection type, specify it using this method. By default, the connection type will be set to 'simpleUsers'
        /// </summary>
        public SimpleConnect ConnectionId(string connectionId)
        {
            this.connectionId = connectionId;
            return this;
        }

        public SimpleConnect Password(string password)
        {
            this.password = password;
            return this;
        }

        public SimpleConnect GameId(string gameId)
        {
            this.gameId = gameId;
            return this;
        }
        
        public SimpleConnect Username(string username)
        {
            this.username = username;
            return this;
        }

        public SimpleConnect Email(string email)
        {
            this.email = email;
            return this;
        }

        private string gameId { get; set; }
        private string connectionId { get; set; } = "simpleUsers";
        private string username { get; set; }
        private string email { get; set; }
        private string password { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();
    }
}
