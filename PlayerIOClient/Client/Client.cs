using System.ComponentModel;

namespace PlayerIOClient
{
    public class Client
    {
        public string Token { get; internal set; }
        public string ConnectUserId { get; internal set; }

        public PlayerInsight PlayerInsight { get; internal set; }
        public Multiplayer Multiplayer { get; }

        internal Client(PlayerIOChannel channel)
        {
            this.Channel = channel;
            this.Token = channel.Token;

            this.Multiplayer = new Multiplayer(channel);
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
