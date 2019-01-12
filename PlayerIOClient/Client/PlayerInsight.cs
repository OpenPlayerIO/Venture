using System.Collections.Generic;

namespace PlayerIOClient
{
    public class PlayerInsight
    {
        internal PlayerInsight(PlayerInsightState state)
        {
            this.PlayersOnline = state.PlayersOnline;
            this.Segments = DictionaryEx.Convert(state.Segments);
        }

        public int PlayersOnline { get; }
        public Dictionary<string, string> Segments { get; }
    }
}
