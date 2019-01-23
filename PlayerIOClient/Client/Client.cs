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
        public string Token { get; internal set; }

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

        internal Client(PlayerIOChannel channel)
        {
            this.Channel = channel;
            this.Token = channel.Token;

            this.Multiplayer = new Multiplayer(channel);
            this.BigDB = new BigDB(channel);
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
