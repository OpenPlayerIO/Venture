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
        private static string LibraryVersion => "1.1.2"; // Venture version (major.minor.patch)

        /// <summary>
        /// Authenticate with SimpleUsers.
        /// </summary>
        public static SimpleConnect SimpleConnect => new SimpleConnect();

        /// <summary>
        /// Authenticate with Facebook.
        /// </summary>
        public static FacebookConnect FacebookConnect => new FacebookConnect();

        /// <summary>
        /// Authenticate with Kongregate.
        /// </summary>
        public static KongregateConnect KongregateConnect => new KongregateConnect();

        /// <summary>
        /// Authenticate with Armor Games.
        /// </summary>
        public static ArmorGameConnect ArmorGameConnect => new ArmorGameConnect();

        /// <summary>
        /// Authenticate with Steam.
        /// </summary>
        public static SteamConnect SteamConnect => new SteamConnect();

        public static Client Authenticate(string gameId, string connectionId, Dictionary<string, string> authenticationArguments, string[] playerInsightSegments = null)
        {
            const string securePasswords = "secureSimpleUserPasswordsOverHttp";

            if (authenticationArguments.TryGetValue(securePasswords, out var useSecure) &&
                useSecure == "true")
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

    public abstract class BaseConnect
    {
        public abstract Client Connect();

        protected virtual void PerformConnectChecks()
        {
            if (string.IsNullOrEmpty(this.GameId))
                throw new ArgumentException(this.Needs(nameof(this.GameId)));

            if (string.IsNullOrEmpty(this.ConnectionId))
                throw new ArgumentException(this.Needs(nameof(this.ConnectionId)));
        }

        protected string Needs(string needs) => $"A {needs} must be specified in order to use this method.";

        public string ConnectionId { get; set; }
        public string GameId { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();
    }

    public abstract class TokenConnect : BaseConnect
    {
        protected override void PerformConnectChecks()
        {
            base.PerformConnectChecks();

            if (string.IsNullOrEmpty(this.Token))
                throw new ArgumentException(this.Needs(nameof(this.Token)));
        }

        public string Token { get; set; }
    }

    public abstract class UserIdConnect : TokenConnect
    {
        protected override void PerformConnectChecks()
        {
            base.PerformConnectChecks();

            if (string.IsNullOrEmpty(this.UserId))
                throw new ArgumentException(this.Needs(nameof(this.UserId)));
        }

        public string UserId { get; set; }
    }

    public class KongregateConnect : UserIdConnect
    {
        /// <summary>
        /// Authenticate with the Kongregate userId and token provided.
        /// </summary>
        public override Client Connect()
        {
            this.PerformConnectChecks();

            return PlayerIO.Authenticate(this.GameId, this.ConnectionId, DictionaryEx.Create(("userId", this.UserId), ("gameAuthToken", this.Token)));
        }
    }

    public class ArmorGameConnect : UserIdConnect
    {
        /// <summary>
        /// Authenticate with the ArmorGams userId and auth token provided.
        /// </summary>
        public override Client Connect()
        {
            this.PerformConnectChecks();

            return PlayerIO.Authenticate(this.GameId, this.ConnectionId, DictionaryEx.Create(("userId", this.UserId), ("authToken", this.Token)));
        }
    }

    public class SteamConnect : BaseConnect
    {
        /// <summary>
        /// Authenticate with the Steam App ID and Session Ticket provided.
        /// </summary>
        public override Client Connect()
        {
            base.PerformConnectChecks();

            if (string.IsNullOrEmpty(this.SteamAppId))
                throw new ArgumentException(this.Needs("Steam App ID"));

            if (string.IsNullOrEmpty(this.SteamSessionTicket))
                throw new ArgumentException(this.Needs("Steam Session Ticket"));

            return PlayerIO.Authenticate(this.GameId, this.ConnectionId, DictionaryEx.Create(("steamSessionTicket", this.SteamSessionTicket), ("steamAppId", this.SteamAppId)));
        }

        public string SteamAppId { get; set; }
        public string SteamSessionTicket { get; set; }
    }

    public class FacebookConnect : TokenConnect
    {
        /// <summary>
        /// Authenticate with the Facebook token provided.
        /// </summary>
        public override Client Connect()
        {
            base.PerformConnectChecks();

            return PlayerIO.Authenticate(this.GameId, this.ConnectionId, DictionaryEx.Create(("accessToken", this.Token)));
        }
    }

    public class SimpleConnect : BaseConnect
    {
        public SimpleConnect() => this.ConnectionId = "simpleUsers";

        /// <summary>
        /// Authenticate with the password plus the username and/or email provided.
        /// </summary>
        public override Client Connect()
        {
            base.PerformConnectChecks();

            if (string.IsNullOrEmpty(this.Username) && string.IsNullOrEmpty(this.Email))
                throw new ArgumentException(this.Needs("username or email"));

            if (string.IsNullOrEmpty(this.Password))
                throw new ArgumentException(this.Needs("password"));

            return PlayerIO.Authenticate(this.GameId, this.ConnectionId, DictionaryEx.Create(
                ("username", this.Username),
                ("email", this.Email),
                ("password", this.Password)
                ));
        }

        /// <summary>
        /// Create a captcha image and key, to be used for registrations where the added security of captcha is required.
        /// </summary>
        /// <param name="width"> The width of the captcha image. </param>
        /// <param name="height"> The height of the captcha image. </param>
        public SimpleCaptcha GetSimpleCaptcha(int width, int height)
        {
            if (string.IsNullOrEmpty(this.GameId))
                throw new ArgumentException("A game ID must be specified in order to use this method.");

            var (success, response, error) = new PlayerIOChannel().Request<SimpleGetCaptchaArgs, SimpleGetCaptchaOutput>(415, new SimpleGetCaptchaArgs
            {
                GameId = this.GameId,
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
            if (string.IsNullOrEmpty(this.GameId))
                throw new ArgumentException("A game ID must be specified in order to use this method.");

            if (string.IsNullOrEmpty(this.Username) && string.IsNullOrEmpty(this.Email))
                throw new ArgumentException("A username or email must be specified in order to use this method.");

            var response = true;

            try
            {
                PlayerIO.Authenticate(this.GameId, this.ConnectionId, DictionaryEx.Create(
                    ("checkusername", "true"),
                    ("username", this.Username),
                    ("email", this.Email)
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
            if (string.IsNullOrEmpty(this.GameId))
                throw new ArgumentException("A game ID must be specified in order to use this method.");

            if (string.IsNullOrEmpty(this.Username) && string.IsNullOrEmpty(this.Email))
                throw new ArgumentException("A username or email must be specified in order to use this method.");

            if (string.IsNullOrEmpty(newPassword))
                throw new ArgumentException("You must specify a new password to use for this method.");

            var response = false;

            try
            {
                PlayerIO.Authenticate(this.GameId, this.ConnectionId, DictionaryEx.Create(
                    ("changepassword", "true"),
                    ("username", this.Username),
                    ("email", this.Email),
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
            if (string.IsNullOrEmpty(this.GameId))
                throw new ArgumentException("A game ID must be specified in order to use this method.");

            if (string.IsNullOrEmpty(this.Username) && string.IsNullOrEmpty(this.Email))
                throw new ArgumentException("A username or email must be specified in order to use this method.");

            if (string.IsNullOrEmpty(newEmail))
                throw new ArgumentException("You must specify a new email to use for this method.");

            var response = false;

            try
            {
                PlayerIO.Authenticate(this.GameId, this.ConnectionId, DictionaryEx.Create(
                    ("changeemail", "true"),
                    ("username", this.Username),
                    ("email", this.Email),
                    ("password", this.Password),
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

        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public static class ConnectionExtensions
    {
        public static T WithGameId<T>(this T baseConnect, string gameId) where T : BaseConnect
        {
            baseConnect.GameId = gameId;
            return baseConnect;
        }

        /// <summary>
        /// If you are using a distinct connection type, specify it using this method. By default, the connection type will be set to 'simpleUsers'
        /// </summary>
        public static T WithConnectionId<T>(this T baseConnect, string connectionId) where T : BaseConnect
        {
            baseConnect.ConnectionId = connectionId;
            return baseConnect;
        }

        public static T WithToken<T>(this T tokenConnect, string token) where T : TokenConnect
        {
            tokenConnect.Token = token;
            return tokenConnect;
        }

        public static T WithUserId<T>(this T useridConnect, string userid) where T : UserIdConnect
        {
            useridConnect.UserId = userid;
            return useridConnect;
        }

        public static SimpleConnect WithUsername(this SimpleConnect simpleConnect, string username)
        {
            simpleConnect.Username = username;
            return simpleConnect;
        }

        public static SimpleConnect WithEmail(this SimpleConnect simpleConnect, string email)
        {
            simpleConnect.Email = email;
            return simpleConnect;
        }

        public static SimpleConnect WithPassword(this SimpleConnect simpleConnect, string password)
        {
            simpleConnect.Password = password;
            return simpleConnect;
        }

        public static SteamConnect SteamSessionTicket(this SteamConnect steamConnect, string steamSessionTicket)
        {
            steamConnect.SteamSessionTicket = steamSessionTicket;
            return steamConnect;
        }

        public static SteamConnect SteamAppId(this SteamConnect steamConnect, string steamAppId)
        {
            steamConnect.SteamAppId = steamAppId;
            return steamConnect;
        }
    }
}
