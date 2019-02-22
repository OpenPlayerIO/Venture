using System;
using System.Collections.Generic;
using ProtoBuf;
using Tson.NET;

namespace PlayerIOClient
{
    /// <summary>
    /// An entry in a user's PayVault history
    /// </summary>
    [ProtoContract]
    public class PayVaultHistoryEntry
    {
        /// <summary> The coin amount of this entry. </summary>
        [ProtoMember(1)]
        public int Amount { get; }

        /// <summary> The type of this entry, for example 'buy','credit','debit' ... </summary>
        [ProtoMember(2)]
        public string Type { get; }

        [ProtoMember(3)]
        public long Created { get; }

        /// <summary> When this entry was created. </summary>
        [ProtoIgnore]
        public DateTime Timestamp => this.Created.FromUnixTime();

        /// <summary> The item keys related to this entry (entries with type 'buy'). </summary>
        [ProtoMember(4)]
        public List<string> ItemKeys { get; }

        /// <summary> The developer supplied reason for entries of type 'credit' and 'debit'. </summary>
        [ProtoMember(5)]
        public string Reason { get; }

        /// <summary> The transaction id from the PayVault provider corresponding to this entry. </summary>
        [ProtoMember(6)]
        public string ProviderTransactionId { get; }

        /// <summary> The price in real currency of this entry formatted as a human readable currency string (e.g. $10.00 USD) </summary>
        [ProtoMember(7)]
        public string ProviderPrice { get; }

        public override string ToString()
        {
            return TsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
