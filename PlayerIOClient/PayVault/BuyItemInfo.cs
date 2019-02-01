namespace PlayerIOClient
{
    /// <summary>
    /// Represents information about the purchase of a PayVault item.
    /// <para>
    /// The minimum information necessary to create an instance of this class is an itemKey representing the PayVaultItem that should be purchased.
    /// </para>
    /// <para>
    /// It is also possible to add a custom payload, and this data will always be present when reading the item from the user's Vault.
    /// For example: Imagine that we have a racing game and users should be able to buy different cars of different colors, but that color doesn't affect the price
    /// of the car. Instead of creating one PayVaultItem for each combination of type and color, we need only create one item for each type and give them
    /// a different price, and color can then be added in the payload when buying a car.
    /// </para>
    /// <para>
    /// Setting the payload works exactly like manipulating a DatabaseObject from BigDB.
    /// </para>
    /// </summary>
    public class BuyItemInfo : DatabaseObject
    {
        public string ItemKey { get; private set; }

        /// <summary>
        /// Creates a new BuyItemInfo to describe an item to purchase.
        /// </summary>
        /// <param name="itemKey"> They key of the underlying item in the PayVaultItems table. </param>
        public BuyItemInfo(string itemKey)
        {
            this.ItemKey = itemKey;
        }
    }
}
