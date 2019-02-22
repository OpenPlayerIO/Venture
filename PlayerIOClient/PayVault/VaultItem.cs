using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PlayerIOClient
{
    public class VaultItem : DatabaseObject
    {
        /// <summary> The unique ID of this particular vault item in the vault of the user. </summary>
        public string Id { get; private set; }

        /// <summary> The key of the underlying item in the PayVaultItems BigDB table. </summary>
        public string ItemKey { get; private set; }

        /// <summary> The date and time when the vault item was originally purchased. </summary>
        public DateTime PurchaseDate { get; private set; }

        internal VaultItem(string id, string itemKey, DateTime purchaseDate, List<ObjectProperty> properties)
        {
            this.Id = id;
            this.ItemKey = itemKey;
            this.PurchaseDate = purchaseDate;
            this.ExistsInDatabase = true;
            this.Properties = new Dictionary<string, object>();

            if (properties != null)
                this.Properties = (DatabaseEx.FromDictionary(DatabaseEx.ToDictionary(properties)) as DatabaseObject).Properties;
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "Id:",
                this.Id,
                ", Key:",
                this.ItemKey,
                " ",
                base.ToString()
            });
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public new string Key { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override DatabaseObject SetProperty(string property, object value) =>
            throw new InvalidOperationException("You cannot set properties on a Vault Item.");
    }
}
