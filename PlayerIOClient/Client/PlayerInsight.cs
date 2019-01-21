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

        /// <summary>
        /// The amount of players online in the entire game.
        /// </summary>
        public int PlayersOnline { get; }

        /// <summary>
        /// A dictionary containing the segments for the user.
        /// </summary>
        public Dictionary<string, string> Segments { get; }
    }
}
