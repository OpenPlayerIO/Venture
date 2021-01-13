using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Text;

namespace PlayerIOClient
{
    /// <summary>
    /// The entry class for the initial connection to Player.IO
    /// </summary>
    public static class PlayerIO
    {
        /// <summary>
        /// The seconds to wait for every API request before timing out. (-1 for never)
        /// </summary>
        public static int APIRequestTimeout { get; set; } = -1;

        /// <summary>
        /// The API endpoint. (default: https://api.playerio.com)
        /// </summary>
        public static string APIEndPoint { get; set; } = "https://api.playerio.com";

        /// <summary>
        /// Authenticate with SimpleUser connection type using the username or email address and password provided.
        /// </summary>
        public static Client SimpleConnect(string gameId, string usernameOrEmail, string password, string connectionId = "public")
        {
            if (string.IsNullOrEmpty(gameId))
                throw new ArgumentException("A game ID must be specified in order to use this method.");

            if (string.IsNullOrEmpty(usernameOrEmail))
                throw new ArgumentException("A username or email must be specified in order to use this method.");

            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("A password must be specified in order to use this method.");

            return PlayerIO.Authenticate(gameId, connectionId, DictionaryEx.Create(
                ("username", usernameOrEmail.Contains("@") ? null : usernameOrEmail),
                ("email", usernameOrEmail.Contains("@") ? usernameOrEmail : null),
                ("password", password)));
        }

        /// <summary>
        /// Change the password for a simple user with the provided username or email address, and valid password.
        /// </summary>
        /// <returns> If the change was successful, returns <see langword="true"/>. </returns>
        public static bool ChangePassword(string gameId, string usernameOrEmail, string password, string newPassword, string connectionId = "public")
        {
            if (string.IsNullOrEmpty(gameId))
                throw new ArgumentException("A game ID must be specified in order to use this method.");

            if (string.IsNullOrEmpty(usernameOrEmail))
                throw new ArgumentException("A username or email must be specified in order to use this method.");

            if (string.IsNullOrEmpty(newPassword))
                throw new ArgumentException("You must specify a new password to use for this method.");

            var response = false;

            try
            {
                PlayerIO.Authenticate(gameId, connectionId, DictionaryEx.Create(
                    ("changepassword", "true"),
                    ("username", usernameOrEmail.Contains("@") ? null : usernameOrEmail),
                    ("email", usernameOrEmail.Contains("@") ? usernameOrEmail : null),
                    ("password", password),
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

        /// <summary>
        /// Change the email for a simple user with the provided username or email address, and valid password.
        /// </summary>
        /// <returns> If the change was successful, returns <see langword="true"/>. </returns>
        public static bool ChangeEmail(string gameId, string usernameOrEmail, string password, string newEmail, string connectionId = "public")
        {
            if (string.IsNullOrEmpty(gameId))
                throw new ArgumentException("A game ID must be specified in order to use this method.");

            if (string.IsNullOrEmpty(usernameOrEmail))
                throw new ArgumentException("A username or email must be specified in order to use this method.");

            if (string.IsNullOrEmpty(newEmail))
                throw new ArgumentException("You must specify a new email to use for this method.");

            var response = false;

            try
            {
                PlayerIO.Authenticate(gameId, connectionId, DictionaryEx.Create(
                    ("changeemail", "true"),
                    ("username", usernameOrEmail.Contains("@") ? null : usernameOrEmail),
                    ("email", usernameOrEmail.Contains("@") ? usernameOrEmail : null),
                    ("password", password),
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
        /// Create a captcha image and key from the specified game, to be used for registrations where the added security of captcha is required.
        /// </summary>
        /// <param name="width"> The width of the captcha image. </param>
        /// <param name="height"> The height of the captcha image. </param>
        public static SimpleCaptcha CreateCaptcha(string gameId, int width, int height)
        {
            if (string.IsNullOrEmpty(gameId))
                throw new ArgumentException("A game ID must be specified in order to use this method.");

            var (success, response, error) = new PlayerIOChannel().Request<SimpleGetCaptchaArgs, SimpleGetCaptchaOutput>(415, new SimpleGetCaptchaArgs
            {
                GameId = gameId,
                Width = width,
                Height = height
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);

            return new SimpleCaptcha(response.CaptchaKey, response.CaptchaImageUrl);
        }

        /// <summary>
        /// Create a captcha image and key for the game the client is connected to, for use in registrations where the added security of captcha is required.
        /// </summary>
        /// <param name="width"> The width of the captcha image. </param>
        /// <param name="height"> The height of the captcha image. </param>
        public static SimpleCaptcha CreateCaptcha(this Client client, int width, int height)
            => PlayerIO.CreateCaptcha(client.GameId, width, height);

        /// <summary>
        /// Check whether a simple user currently exists with the details provided.
        /// </summary>
        /// <returns> If the simple user exists already, this method returns <see langword="true"/> </returns>
        public static bool SimpleUserIsRegistered(string gameId, string username, string email, string connectionId = "public")
        {
            if (string.IsNullOrEmpty(gameId))
                throw new ArgumentException("A game ID must be specified in order to use this method.");

            if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(email))
                throw new ArgumentException("A username or email must be specified in order to use this method.");

            var response = true;

            try
            {
                PlayerIO.Authenticate(gameId, connectionId, DictionaryEx.Create(
                    ("checkusername", "true"),
                    ("username", username),
                    ("email", email)
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
        /// Authenticate with Facebook using the Access Token provided.
        /// </summary>
        public static Client FacebookConnect(string gameId, string accessToken, string connectionId = "public")
        {
            if (string.IsNullOrEmpty(gameId))
                throw new ArgumentException("A game ID must be specified in order to use this method.");

            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentException("A Facebook access token must be specified in order to use this method.");

            return PlayerIO.Authenticate(gameId, connectionId, DictionaryEx.Create(("accessToken", accessToken)));
        }

        /// <summary>
        /// Authenticate with Steam using the Steam App ID and Steam Session Ticket provided.
        /// </summary>
        public static Client SteamConnect(string gameId, string appId, string sessionTicket, string connectionId = "public")
        {
            if (string.IsNullOrEmpty(gameId))
                throw new ArgumentException("A game ID must be specified in order to use this method.");

            if (string.IsNullOrEmpty(appId))
                throw new ArgumentException("A Steam App ID must be specified in order to use this method.");

            if (string.IsNullOrEmpty(sessionTicket))
                throw new ArgumentException("A Steam Session Ticket must be specified in order to use this method.");

            return PlayerIO.Authenticate(gameId, connectionId, DictionaryEx.Create(("steamSessionTicket", sessionTicket), ("steamAppId", appId)));
        }

        /// <summary>
        /// Authenticate with Kongregate using the userId and token provided.
        /// </summary>
        public static Client KongregateConnect(string gameId, string userId, string token, string connectionId = "public")
        {
            if (string.IsNullOrEmpty(gameId))
                throw new ArgumentException("A game ID must be specified in order to use this method.");

            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("A Kongregate userId must be specified in order to use this method.");

            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("A Kongregate token must be specified in order to use this method.");

            return PlayerIO.Authenticate(gameId, connectionId, DictionaryEx.Create(("userId", userId), ("gameAuthToken", token)));
        }

        /// <summary>
        /// Authenticate with ArmorGames using the userId and token provided.
        /// </summary>
        public static Client ArmorGamesConnect(string gameId, string userId, string token, string connectionId = "public")
        {
            if (string.IsNullOrEmpty(gameId))
                throw new ArgumentException("A game ID must be specified in order to use this method.");

            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("An ArmorGames userId must be specified in order to use this method.");

            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("An ArmorGames auth token must be specified in order to use this method.");

            return PlayerIO.Authenticate(gameId, connectionId, DictionaryEx.Create(("userId", userId), ("authToken", token)));
        }

        /// <summary> 
        /// Connect to Player.IO using as the given user.
        /// </summary>
        /// <param name="gameId"> The ID of the game you wish to connect to. This value can be found in the admin panel. </param>
        /// <param name="connectionId">The ID of the connection, as given in the settings section of the admin panel. 'public' should be used as the default. </param>
        /// <param name="authenticationArguments"> A dictionary of arguments for the given connection. </param>
        /// <param name="playerInsightSegments"> Custom segments for the user in PlayerInsight. </param>
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
                ClientAPI = "csharp",
                ClientInfo = DictionaryEx.Convert(PlayerIOAuth.GetClientInfo()),
                PlayCodes = new List<string>()
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);

            return new Client(new PlayerIOChannel() { Token = response.Token }, gameId)
            {
                PlayerInsight = new PlayerInsight(response.PlayerInsightState),
                ConnectUserId = response.UserId,
            };
        }

        /// <summary> 
        /// Connects to a game based on Player.IO as the given user using basic authentiation.
        /// </summary>
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
        /// CalcAuth256() method.
        /// </param>
        public static Client Connect(string gameId, string connectionId, string userId, string auth) =>
            PlayerIO.Authenticate(gameId, connectionId, DictionaryEx.Create(("userId", userId), ("auth", auth)));

        /// <summary>
        /// Calculate an auth hash for use in basic authenticcation.
        /// </summary>
        /// <param name="userId"> The UserID to use when generating the hash. </param>
        /// <param name="sharedSecret">
        /// The shared secret to use when generating the hash. This must be the same value as the one
        /// given to a connection in the admin panel.
        /// </param>
        /// <returns> The generated auth hash (based on SHA-1) </returns>
        [Obsolete("Player.IO now uses SHA-256 by default. If your connection type uses SHA-256 use the CalcAuth256() method instead.", false)]
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

        /// <summary> Calculate an auth hash for use in basic authentication. </summary>
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

        /// <summary> 
        /// Register a new Simple User in the specified game.
        /// </summary>
        /// <param name="gameId"> The ID of the game you wish to connect to. This value can be found in the admin panel. </param>
        /// <param name="username"> The username of the new user. </param>
        /// <param name="password"> The password of the new user. </param>
        /// <param name="email"> The email of the new user. (optional unless required) </param>
        /// <param name="captchaKey"> (only if captcha is required) The key of the Captcha image used to get the user to write in the Captcha value </param>
        /// <param name="captchaValue"> (only if captcha is required) The string the user entered in response to the captcha image </param>
        /// <param name="extraData"> Any extra data that you wish to store with the user such as gender, birthdate, etc. (optional) </param>
        /// <param name="partnerId"> The PartnerPay partner id this user should be tagged with, if you are using the PartnerPay system. </param>
        /// <param name="playerInsightSegments"> Custom segments for the user in PlayerInsight. </param>
        /// <returns> A new instance of a connected client. </returns>
        public static Client SimpleRegister(string gameId, string username, string password, string email = null, string captchaKey = null, string captchaValue = null,
            Dictionary<string, string> extraData = null, string partnerId = null, string[] playerInsightSegments = null)
        {
            var (success, response, error) = new PlayerIOChannel().Request<SimpleRegisterArgs, SimpleRegisterOutput>(403, new SimpleRegisterArgs()
            {
                GameId = gameId,
                Username = username,
                Password = password,
                Email = email,
                CaptchaKey = captchaKey,
                CaptchaValue = captchaValue,
                ExtraData = DictionaryEx.Convert(extraData).ToList(),
                PartnerId = partnerId,
                PlayerInsightSegments = playerInsightSegments,
                ClientAPI = "csharp",
                ClientInfo = DictionaryEx.Convert(PlayerIOAuth.GetClientInfo()).ToList(),
            });

            if (!success)
                throw error as PlayerIORegistrationError;

            return new Client(new PlayerIOChannel() { Token = response.Token }, gameId)
            {
                PlayerInsight = new PlayerInsight(response.PlayerInsightState),
                ConnectUserId = response.UserId,
            };
        }
    }
}
