namespace PlayerIOClient
{
    using System;

    public class PlayerIOError : Exception
    {
        public ErrorCode ErrorCode { get; set; }
        public override string Message { get; }

        internal PlayerIOError(ErrorCode errorCode, string message)
        {
            this.ErrorCode = errorCode;
            this.Message = message;
        }
    }

    public enum ErrorCode
    {
        /// <summary>The method requested is not supported</summary>
        UnsupportedMethod,

        /// <summary>A general error occurred</summary>
        GeneralError,

        /// <summary>An unexpected error occurred inside the Player.IO webservice. Please try again.</summary>
        InternalError,

        /// <summary>Access is denied</summary>
        AccessDenied,

        /// <summary>The message is malformatted</summary>
        InvalidMessageFormat,

        /// <summary>A value is missing</summary>
        MissingValue,

        /// <summary>A game is required to do this action</summary>
        GameRequired,

        /// <summary>An error occurred while contacting an external service</summary>
        ExternalError,

        /// <summary>The given argument value is outside the range of allowed values.</summary>
        ArgumentOutOfRange,

        /// <summary>The game has been disabled, most likely because of missing payment.</summary>
        GameDisabled,

        /// <summary>The game requested is not known by the server</summary>
        UnknownGame,

        /// <summary>The connection requested is not known by the server</summary>
        UnknownConnection,

        /// <summary>The auth given is invalid or malformatted</summary>
        InvalidAuth,

        /// <summary>There is no server in any of the selected server clusters for the game that are eligible to start a new room in (they're all at full capacity or there are no servers in any of the clusters). Either change the selected clusters for your game in the admin panel, try again later or start some more servers for one of your clusters.</summary>
        NoServersAvailable,

        /// <summary>The room data for the room was over the allowed size limit</summary>
        RoomDataTooLarge,

        /// <summary>You are unable to create room because there is already a room with the specified id</summary>
        RoomAlreadyExists,

        /// <summary>The game you're connected to does not have a room type with the specified name</summary>
        UnknownRoomType,

        /// <summary>There is no room running with that id</summary>
        UnknownRoom,
        /// <summary>You can't join the room when the RoomID is null or the empty string</summary>
        MissingRoomId,

        /// <summary>The room already has the maxmium amount of users in it.</summary>
        RoomIsFull,

        /// <summary>The key you specified is not set as searchable. You can change the searchable keys in the admin panel for the server type</summary>
        NotASearchColumn,

        /// <summary>The QuickConnect method (simple, facebook, kongregate...) is not enabled for the game. You can enable the various methods in the admin panel for the game</summary>
        QuickConnectMethodNotEnabled,

        /// <summary>The user is unknown</summary>
        UnknownUser,

        /// <summary>The password supplied is incorrect</summary>
        InvalidPassword,

        /// <summary>The supplied data is incorrect</summary>
        InvalidRegistrationData,

        /// <summary>The key given for the BigDB object is not a valid BigDB key. Keys must be between 1 and 50 characters. Only letters, numbers, hyphens, underbars, and spaces are allowed.</summary>
        InvalidBigDBKey,

        /// <summary>The object exceeds the maximum allowed size for BigDB objects.</summary>
        BigDBObjectTooLarge,

        /// <summary>Could not locate the database object.</summary>
        BigDBObjectDoesNotExist,

        /// <summary>The specified table does not exist.</summary>
        UnknownTable,

        /// <summary>The specified index does not exist.</summary>
        UnknownIndex,

        /// <summary>The value given for the index, does not match the expected type.</summary>
        InvalidIndexValue,

        /// <summary>The operation was aborted because the user attempting the operation was not the original creator of the object accessed.</summary>
        NotObjectCreator,

        /// <summary>The key is in use by another database object</summary>
        KeyAlreadyUsed,

        /// <summary>BigDB object could not be saved using optimistic locks as it's out of date.</summary>
        StaleVersion,

        /// <summary>Cannot create circular references inside database objects</summary>
        CircularReference,

        /// <summary>The server could not complete the heartbeat</summary>
        HeartbeatFailed = 40,

        /// <summary>The game code is invalid</summary>
        InvalidGameCode,

        /// <summary>Cannot access coins or items before vault has been loaded. Please refresh the vault first.</summary>
        VaultNotLoaded = 50,

        /// <summary>There is no PayVault provider with the specified id</summary>
        UnknownPayVaultProvider,

        /// <summary>The specified PayVault provider does not support direct purchase</summary>
        DirectPurchaseNotSupportedByProvider,

        /// <summary>The specified PayVault provider does not support buying coins</summary>
        BuyingCoinsNotSupportedByProvider = 54,

        /// <summary>The user does not have enough coins in the PayVault to complete the purchase or debit.</summary>
        NotEnoughCoins,

        /// <summary>The item does not exist in the vault.</summary>
        ItemNotInVault,

        /// <summary>The chosen provider rejected one or more of the purchase arguments</summary>
        InvalidPurchaseArguments,

        /// <summary>The chosen provider is not configured correctly in the admin panel</summary>
        InvalidPayVaultProviderSetup,

        /// <summary>Unable to locate the custom PartnerPay action with the given key</summary>
        UnknownPartnerPayAction = 70,

        /// <summary>The given type was invalid</summary>
        InvalidType = 80,

        /// <summary>The index was out of bounds from the range of acceptable values</summary>
        IndexOutOfBounds,

        /// <summary>The given identifier does not match the expected format</summary>
        InvalidIdentifier,

        /// <summary>The given argument did not have the expected value</summary>
        InvalidArgument,

        /// <summary>This client has been logged out</summary>
        LoggedOut,

        /// <summary>The given segment was invalid.</summary>
        InvalidSegment = 90,

        /// <summary>Cannot access requests before Refresh() has been called.</summary>
        GameRequestsNotLoaded = 100,

        /// <summary>Cannot access achievements before Refresh() has been called.</summary>
        AchievementsNotLoaded = 110,

        /// <summary>Cannot find the achievement with the specified id.</summary>
        UnknownAchievement,

        /// <summary>Cannot access notification endpoints before Refresh() has been called.</summary>
        NotificationsNotLoaded = 120,

        /// <summary>The given notifications endpoint is invalid</summary>
        InvalidNotificationsEndpoint,

        /// <summary>There is an issue with the network</summary>
        NetworkIssue = 130,

        /// <summary>Cannot access OneScore before Refresh() has been called.</summary>
        OneScoreNotLoaded,

        /// <summary>The Publishing Network features are only avaliable when authenticated to PlayerIO using Publishing Network authentication. Authentication methods are managed in the connections setting of your game in the admin panel on PlayerIO.</summary>
        PublishingNetworkNotAvailable = 200,

        /// <summary>Cannot access profile, friends, ignored before Publishing Network has been loaded. Please refresh Publishing Network first.</summary>
        PublishingNetworkNotLoaded,

        /// <summary>The dialog was closed by the user</summary>
        DialogClosed = 301,

        /// <summary>Check cookie required.</summary>
        AdTrackCheckCookie
    }
}
