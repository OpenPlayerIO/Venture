using System.Collections.Generic;
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
                ConnectUserId = targetUserId
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);

            return response.PayVaultContents;
        }

        internal Dictionary<string, string> PayVaultPaymentInfo(string provider, Dictionary<string, string> purchaseArguments, List<PayVaultBuyItemInfo> items)
        {
            var (success, response, error) = this.Channel.Request<PayVaultPaymentInfoArgs, PayVaultPaymentInfoOutput>(181, new PayVaultPaymentInfoArgs
            {
                Provider = provider,
                PurchaseArguments = DictionaryEx.Convert(purchaseArguments).ToList(),
                Items = items
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);

            return DictionaryEx.Convert(response.ProviderArguments);
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
                    throw new PlayerIOError(ErrorCode.VaultNotLoaded, "Cannot access coins before vault has been loaded. Please refresh the vault first.");

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
                    throw new PlayerIOError(ErrorCode.VaultNotLoaded, "Cannot access items before vault has been loaded. Please refresh the vault first.");

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

        /// <summary>
        /// A method to refresh this PayVault, making sure the Items and Coins are up-to-date.
        /// </summary>
        public void Refresh() => this.ReadContent(this.PayVaultRefresh(this.CurrentVersion, this.ConnectUserId));

        /// <summary>
        /// This method check whether the Vault contain at least one item with the key specified. This method can only be called on an up-to-date vault.
        /// </summary>
        /// <param name="itemKey"> The key of the item to check for. </param>
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
        /// This method reetrns the first item of the given item key from this vault. This method can only be called on an up-to-date vault.
        /// </summary>
        /// <param name="itemKey"> The key of the item to get. </param>
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

        /// <summary> This method purchases items with Coins. </summary>
        /// <param name="storeItems"> If true, the items will be stored in the Vault. If false, the items will be consumed immediately after purchase. </param>
        /// <param name="items"> A list of items to buy, together with any additional payload. </param>
        public void Buy(bool storeItems, params BuyItemInfo[] items)
        {
            var (success, response, error) = this.Channel.Request<PayVaultBuyArgs, PayVaultBuyOutput>(175, new PayVaultBuyArgs()
            {
                ConnectUserId = ConnectUserId,
                KeepItems = storeItems,
                Items = items.Select(i => new PayVaultBuyItemInfo() { ItemKey = i.ItemKey, Payload = DatabaseEx.FromDatabaseObject(i as DatabaseObject).ToList() }).ToList()
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);

            this.ReadContent(response.PayVaultContents);
        }

        /// <summary> 
        /// This method consumes the items specified in this Vault. This will cause them to be removed, but this action will not show up in the vault history.
        /// </summary>
        /// <param name="items"> The VaultItems to use from the users vault - this should be instances from the Items array property. </param>
        public void Consume(params VaultItem[] items)
        {
            var (success, response, error) = this.Channel.Request<PayVaultConsumeArgs, PayVaultConsumeOutput>(166, new PayVaultConsumeArgs()
            {
                ConnectUserId = ConnectUserId,
                ItemIds = items.Select(i => i.Id).ToList()
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);

            this.ReadContent(response.PayVaultContents);
        }

        /// <summary>
        /// This method gives coins to this Vault.
        /// </summary>
        /// <param name="coinAmount"> The amount of coins to give. </param>
        /// <param name="reason"> Your reason for giving the coins to this user. This will show up in the vault history, and in the PayVault admin panel for this user. </param>
        public void Credit(uint coinAmount, string reason)
        {
            var (success, response, error) = this.Channel.Request<PayVaultCreditArgs, PayVaultCreditOutput>(169, new PayVaultCreditArgs()
            {
                ConnectUserId = ConnectUserId,
                Amount = coinAmount,
                Reason = reason
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);

            this.ReadContent(response.PayVaultContents);
        }

        /// <summary>
        /// This method takes coins from this Vault.
        /// </summary>
        /// <param name="coinAmount"> The amount of coins to take. </param>
        /// <param name="reason"> Your reason for taking the coins from this user. This will show up in the vault history, and in the PayVault admin panel for this user. </param>
        public void Debit(uint coinAmount, string reason)
        {
            var (success, response, error) = this.Channel.Request<PayVaultCreditArgs, PayVaultCreditOutput>(172, new PayVaultCreditArgs()
            {
                ConnectUserId = ConnectUserId,
                Amount = coinAmount,
                Reason = reason
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);

            this.ReadContent(response.PayVaultContents);
        }

        /// <summary>
        /// This method gives the user items without taking any of their coins from the vault.
        /// </summary>
        /// <param name="items"> Each BuyItemInfo instance contains the key of the item to give in the PayVaultItems BigDB table and any additional payload. </param>
        public void Give(params BuyItemInfo[] items)
        {
            var (success, response, error) = this.Channel.Request<PayVaultGiveArgs, PayVaultGiveOutput>(178, new PayVaultGiveArgs()
            {
                ConnectUserId = ConnectUserId,
                Items = items.Select(i => new PayVaultBuyItemInfo() { ItemKey = i.ItemKey, Payload = DatabaseEx.FromDatabaseObject(i) }).ToList()
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);

            this.ReadContent(response.PayVaultContents);
        }

        /// <summary>
        /// This method loads a page of entries from this Vaults history, in reverse chronological order (i.e. newest first.)
        /// </summary>
        /// <param name="page"> The page of entries to load. The first page has number 0. </param>
        /// <param name="pageSize"> The number of entries per page. </param>
        /// <returns> The loaded history entries, or an empty array if none were found on the given page. </returns>
        public PayVaultHistoryEntry[] ReadHistory(uint page, uint pageSize)
        {
            if (this.CurrentVersion == null)
                throw new PlayerIOError(ErrorCode.VaultNotLoaded, "Cannot access vault history before vault has been loaded. Please refresh the vault first.");

            var (success, response, error) = this.Channel.Request<PayVaultReadHistoryArgs, PayVaultReadHistoryOutput>(160, new PayVaultReadHistoryArgs()
            {
                Page = page,
                PageSize = pageSize,
                ConnectUserId = ConnectUserId
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);

            if (response.Entries == null)
                response.Entries = new List<PayVaultHistoryEntry>();

            return response.Entries.ToArray();
        }
        
        /// <summary>
        /// This method retrieves information about how to make a coin purchase with the specified PayVault provider. 
        /// </summary>
        /// <param name="provider"> The name of the PayVault provider to use for the coin purchase. </param>
        /// <param name="purchaseArguments"> Any additional information that will be given to the PayVault provider to configure this purchase. </param> 
        /// <returns>
        /// A dictionary with PayVault provider-specific information about how to proceed with the purchase.
        /// </returns>
        public Dictionary<string, string> GetBuyCoinsInfo(string provider, Dictionary<string, string> purchaseArgument)
        {
            if (this.CurrentVersion == null)
                throw new PlayerIOError(ErrorCode.VaultNotLoaded, "Cannot access this method before vault has been loaded. Please refresh the vault first.");

            return this.PayVaultPaymentInfo(provider, purchaseArgument, null);
        }

        /// <summary>
        /// This method retrieves information about how to make a direct item purchase with the specified PayVault provider.
        /// </summary>
        /// <param name="provider"> The name of the PayVault provider to use for the coin purchase. </param>
        /// <param name="purchaseArguments"> Any additional information that will be given to the PayVault provider to configure this purchase. </param> 
        /// <param name="items" >Each BuyItemInfo instance contains the key of the item to buy and any additional payload. </param>
        /// <returns>
        /// A dictionary with PayVault provider-specific information about how to proceed with the purchase.
        /// </returns>
        public Dictionary<string, string> GetBuyDirectInfo(string provider, Dictionary<string, string> purchaseArguments, params BuyItemInfo[] items)
        {
            if (this.CurrentVersion == null)
                throw new PlayerIOError(ErrorCode.VaultNotLoaded, "Cannot access this method before vault has been loaded. Please refresh the vault first.");

            return this.PayVaultPaymentInfo(provider, purchaseArguments, Items.Select(i => new PayVaultBuyItemInfo() { ItemKey = i.ItemKey, Payload = DatabaseEx.FromDatabaseObject(i) }).ToList());
        }

        /// <summary> 
        /// This method uses information from a provider to finalize a purchase with the specified PayVault provider.
        /// </summary>
        /// <param name="provider"> The name of the PayVault provider to use. </param>
        /// <param name="providerArguments"> The information needed to finalize this purchase. </param> 
        /// <returns>
        /// A dictionary with PayVault provider-specific information about the purchase
        /// </returns>
        public Dictionary<string, string> UseBuyInfo(string provider, Dictionary<string, string> providerArguments)
        {
            if (this.CurrentVersion == null)
                throw new PlayerIOError(ErrorCode.VaultNotLoaded, "Cannot access this method before vault has been loaded. Please refresh the vault first.");

            var (success, response, error) = this.Channel.Request<PayVaultUsePaymentInfoArgs, PayVaultUsePaymentInfoOutput>(184, new PayVaultUsePaymentInfoArgs
            {
                Provider = provider,
                ProviderArguments = DictionaryEx.Convert(providerArguments).ToList()
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);

            this.ReadContent(response.VaultContents);

            return DictionaryEx.Convert(response.ProviderResults);
        }

        internal PlayerIOChannel Channel { get; }
    }
}
