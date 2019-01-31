using System.Linq;

namespace PlayerIOClient
{
    public partial class PayVault
    {
        private string CurrentVersion { get; set; }

        private string ConnectUserId { get; }
        private long InternalCoins { get; set; }
        private VaultItem[] InternalItems { get; set; }

        private void ReadContent(PayVaultContents contents)
        {
            if (contents != null)
            {
                this.CurrentVersion = contents.Version;
                this.InternalCoins = (long)contents.Coins;
                this.InternalItems = contents.Items.Select(i => new VaultItem(i.Id, i.ItemKey, i.PurchaseDate.FromUnixTime(), i.Properties)).ToArray();
            }
        }

        internal PayVaultContents PayVaultRefresh(string lastVersion, string targetUserId)
        {
            var (success, response, error) = this.Channel.Request<PayVaultRefreshArgs, PayVaultRefreshOutput>(163, new PayVaultRefreshArgs
            {
                LastVersion = lastVersion,
                TargetUserId = targetUserId
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);

            return response.VaultContents;
        }
    }

    /// <summary>
    /// The Player.IO PayVault service.
    /// </summary>
    public partial class PayVault
    {
        internal PayVault(PlayerIOChannel channel, string connectUserId)
        {
            this.Channel = channel;
            this.ConnectUserId = connectUserId;
        }

        /// <summary>
        /// This property contains the current coin balance of this Vault. This property can only be read on an up-to-date vault.
        /// </summary>
        public int Coins
        {
            get
            {
                if (this.CurrentVersion == null)
                {
                    throw new PlayerIOError(ErrorCode.VaultNotLoaded, "Cannot access coins before vault has been loaded. Please refresh the vault first");
                }
                return (int)this.InternalCoins;
            }
        }

        /// <summary>
        /// This property contains the list of items in this Vault. This property can only be read on an up-to-date vault.
        /// </summary>
        public VaultItem[] Items
        {
            get
            {
                if (this.CurrentVersion == null)
                {
                    throw new PlayerIOError(ErrorCode.VaultNotLoaded, "Cannot access items before vault has been loaded. Please refresh the vault first");
                }
                return this.InternalItems;
            }
        }

        public override string ToString()
        {
            if (this.CurrentVersion != null)
            {
                return string.Concat(new object[]
                {
                    this.Coins,
                    " coins, ",
                    this.Items.Length,
                    " items"
                });
            }
            return "not loaded";
        }

        /// <summary> A method to refresh this PayVault, making sure the Items and Coins are up-to-date.</summary>
        public void Refresh()
        {
            this.ReadContent(this.PayVaultRefresh(this.CurrentVersion, this.ConnectUserId));
        }

        /// <summary>
        /// This method check whether the Vault contain at least one item with the key specified. This method can only be called on an up-to-date vault.
        /// </summary>
        /// <param name="itemKey"> The item key to check for. </param>
        /// <returns> If the user has at least one of the given key. </returns>
        public bool Has(string itemKey)
        {
            for (var num = 0; num != this.Items.Length; num++)
            {
                if (this.Items[num].ItemKey == itemKey)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// This method reetrns sthe first item of the given item key from this vault. This method can only be called on an up-to-date vault.
        /// </summary>
        /// <param name="itemKey"> The item key of the item to get. </param>
        /// <returns> A VaultItem if one was found, otherwise null. </returns>
        public VaultItem First(string itemKey)
        {
            for (var i = 0; i != this.Items.Length; i++)
            {
                if (this.Items[i].ItemKey == itemKey)
                    return this.Items[i];
            }

            return null;
        }

        /// <summary>
        /// This method returns the number of items of a given item key in this vault. This method can only be called on an up-to-date vault.
        /// </summary>
        /// <param name="itemKey">The key of the items to count. </param>
        /// <returns> The amount of items of the given key that the user has in the vault. </returns>
        public int Count(string itemKey)
        {
            var count = 0;

            for (var i = 0; i != this.Items.Length; i++)
            {
                if (this.Items[i].ItemKey == itemKey)
                    count++;
            }

            return count;
        }

        internal PlayerIOChannel Channel { get; }
    }
}
