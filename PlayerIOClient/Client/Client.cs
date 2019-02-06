using System.ComponentModel;

namespace PlayerIOClient
{
    /// <summary>
    /// A Player.IO client with access to various Player.IO services.
    /// </summary>
    public class Client
    {
        /// <summary>
        /// The Player.IO token for this client.
        /// </summary>
        public string Token => this.Channel.Token;

        /// <summary>
        /// The ConnectUserId of this client.
        /// </summary>
        public string ConnectUserId { get; internal set; }

        /// <summary>
        /// The property used to access the PlayerInsight service.
        /// </summary>
        public PlayerInsight PlayerInsight { get; internal set; }

        /// <summary>
        /// The property used to access the Multiplayer service.
        /// </summary>
        public Multiplayer Multiplayer { get; }

        /// <summary>
        /// The property used to access the BigDB service.
        /// </summary>
        public BigDB BigDB { get; }

        /// <summary>
        /// The property used to access the PayVault service.
        /// </summary>
        public PayVault PayVault { get; }

        /// <summary>
        /// The property used to access the ErrorLog service.
        /// </summary>
        public ErrorLog ErrorLog { get; }

        /// <summary>
        /// The ID of the game the client is connected to.
        /// </summary>
        public string GameId { get; }

        internal Client(PlayerIOChannel channel, string gameId)
        {
            this.Channel = channel;
            this.GameId = gameId;

            this.Multiplayer = new Multiplayer(channel);
            this.BigDB = new BigDB(channel);
            this.ErrorLog = new ErrorLog(channel);
            this.PayVault = new PayVault(channel, this.ConnectUserId);
        }

        /// <summary>
        /// An optional means of instantiating a client from a Player.IO Player Token, for advanced usage purposes.
        /// A typical game should most likely use the connection methods in the static PlayerIO class instead.
        /// </summary>
        /// <param name="playerToken"> (required) </param>
        /// <param name="gameId"> (optional) </param>
        public static Client Create(string playerToken, string gameId)
        {
            return new Client(new PlayerIOChannel() { Token = playerToken }, gameId);
        }

        private PlayerIOChannel Channel { get; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();
    }
}
