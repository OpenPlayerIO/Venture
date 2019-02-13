using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;

namespace PlayerIOClient
{
    public delegate List<ServerEndPoint> GameServerEndpointFilterDelegate(List<ServerEndPoint> endpoints);

    public partial class Multiplayer
    {
        /// <summary> Join a running multiplayer room. </summary>
        /// <param name="roomId"> The ID of the room you wish to join. </param>
        /// <param name="joinData"> Data to send to the room with additional information about the join. </param>
        /// <param name="successCallback"> A callback called when you successfully created and joined the room. </param>
        /// <param name="errorCallback"> A callback called instead of <paramref name="successCallback"/> if an error occurs when joining the room. </param>
        public void JoinRoom(string roomId, Dictionary<string, string> joinData = null, Callback<Connection> successCallback = null, Callback<PlayerIOError> errorCallback = null)
            => CallbackHandler.CreateHandler(() => this.JoinRoom(roomId, joinData), ref successCallback, ref errorCallback);

        /// <summary> Creates a multiplayer room (if it doesn't exists already), and joins it. </summary>
        /// <param name="roomId"> The ID of the room you wish to (create and then) join. </param>
        /// <param name="roomType">
        /// If the room doesn't exists: The name of the room type you wish to run the room as. This
        /// value should match one of the 'RoomType(...)' attributes of your uploaded code. A room
        /// type of 'bounce' is always available.
        /// </param>
        /// <param name="visible">
        /// If the room doesn't exists: Determines (upon creation) if the room should be visible when
        /// listing rooms with ListRooms.
        /// </param>
        /// <param name="roomData">
        /// If the room doesn't exists: The data to initialize the room with (upon creation).
        /// </param>
        /// <param name="joinData">
        /// Data to send to the room with additional information about the join.
        /// </param>
        /// <param name="successCallback"> A callback called when you successfully created and/or joined the room. </param>
        /// <param name="errorCallback"> A callback called instead of <paramref name="successCallback"/> if an error occurs when creating or joining the room. </param>
        public void CreateJoinRoom(string roomId, string roomType, bool visible = true, Dictionary<string, string> roomData = null, Dictionary<string, string> joinData = null, Callback<Connection> successCallback = null, Callback<PlayerIOError> errorCallback = null)
            => CallbackHandler.CreateHandler(() => this.CreateJoinRoom(roomId, roomType, visible, roomData, joinData), ref successCallback, ref errorCallback);
    }

    /// <summary>
    /// The Player.IO Multiplayer service.
    /// </summary>
    public partial class Multiplayer
    {
        /// <summary>
        /// If set, rooms will be created on the development server at the address defined by the endpoint specified, instead of using the live Player.IO servers.
        /// </summary>
        public DevelopmentServer DevelopmentServer = null;

        /// <summary>
        /// If set, multiplayer connections will be created explicitly using the proxy specified.
        /// <para>
        /// NOTE: API requests will not be proxied using this property.
        /// </para>
        /// </summary>
        public ProxyOptions ProxyOptions = null;

        /// <summary>
        /// If true, the multiplayer connections will be encrypted using TLS/SSL. Beaware that this will cause a performance degredation by introducting secure connection negotiation latency.
        /// </summary>
        public bool UseSecureConnections { get; set; } // TODO: Implement this.

        /// <summary>
        /// If not null, allows you to filter, rearrange, or completely redefine the game server endpoints. The endpoints are tried consecutively until there is one successful connection to a game server.
        /// If the <see cref="DevelopmentServer"/> property is set, this property is ignored.
        /// </summary>
        public GameServerEndpointFilterDelegate GameServerEndPointFilter { get; set; }

        internal Multiplayer(PlayerIOChannel channel)
        {
            this.Channel = channel;
        }

        /// <summary> Lists the currently running multiplayer rooms. </summary>
        /// <param name="roomType"> The type of the rooms you wish to list. </param>
        /// <param name="searchCriteria">
        /// Only rooms with the same values in their roomData will be returned.
        /// </param>
        /// <param name="resultLimit">
        /// The maximum amount of rooms you want to receive. Use 0 for 'as many as possible'.
        /// </param>
        /// <param name="resultOffset"> Defines the index to show results from. </param>
        /// <param name="onlyDevRooms">
        /// Set to 'true' to list rooms from the development room list, rather than from the game's
        /// global room list.
        /// </param>
        public RoomInfo[] ListRooms(string roomType, Dictionary<string, string> searchCriteria, int resultLimit, int resultOffset, bool onlyDevRooms = false)
        {
            var (success, response, error) = this.Channel.Request<ListRoomsArgs, ListRoomsOutput>(30, new ListRoomsArgs
            {
                RoomType = roomType,
                SearchCriteria = DictionaryEx.Convert(searchCriteria),
                ResultLimit = resultLimit,
                ResultOffset = resultOffset,
                OnlyDevRooms = onlyDevRooms
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);

            if (response.RoomInfo == null)
                return new RoomInfo[0];

            return response.RoomInfo;
        }

        /// <summary> Create a multiplayer room on the Player.IO infrastructure. </summary>
        /// <param name="roomId"> The ID you wish to assign to your new room - You can use this to connect to the specific room later as long as it still exists. </param>
        /// <param name="roomType"> The name of the room type you wish to run the room as. This value should match one of the [RoomType(...)] attributes of your uploaded code. A room type of 'bounce' is always available. </param>
        /// <param name="visible"> Whether the room should be visible when listing rooms with ListRooms. </param>
        /// <param name="roomData"> The data to initialize the room with, this can be read with ListRooms and changed from the serverside. </param>
        /// <returns> The ID of the room that was created. </returns>
        public string CreateRoom(string roomId, string roomType, bool visible, Dictionary<string, string> roomData)
        {
            var (success, response, error) = this.Channel.Request<CreateRoomArgs, CreateRoomOutput>(21, new CreateRoomArgs
            {
                RoomId = roomId,
                RoomType = roomType,
                Visible = visible,
                RoomData = DictionaryEx.Convert(roomData),
                IsDevRoom = this.DevelopmentServer != null
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);

            return response.RoomId;
        }

        /// <summary> Join a running multiplayer room. </summary>
        /// <param name="roomId"> The ID of the room you wish to join. </param>
        /// <param name="joinData"> Data to send to the room with additional information about the join. </param>
        public Connection JoinRoom(string roomId, Dictionary<string, string> joinData = null)
        {
            var (success, response, error) = this.Channel.Request<JoinRoomArgs, JoinRoomOutput>(24, new JoinRoomArgs
            {
                RoomId = roomId,
                JoinData = DictionaryEx.Convert(joinData),
                IsDevRoom = this.DevelopmentServer != null
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);

            var endpoints = this.FilterGameEndPoints(response.Endpoints.ToList());

            foreach (var endpoint in endpoints)
            {
                if (PortCheck.IsPortOpen(endpoint.Address, endpoint.Port, 1000, 3))
                {
                    var resolution = Dns.GetHostAddresses(endpoint.Address).First();

                    return new Connection(new IPEndPoint(resolution, endpoint.Port), response.JoinKey, joinData, this.ProxyOptions);
                }
            }

            throw new PlayerIOError(ErrorCode.GeneralError, "[Venture] Unable to join room - unable to establish connection from any endpoint(s) returned by API.");
        }

        /// <summary> Creates a multiplayer room (if it doesn't exists already), and joins it. </summary>
        /// <param name="roomId"> The ID of the room you wish to (create and then) join. </param>
        /// <param name="roomType">
        /// If the room doesn't exists: The name of the room type you wish to run the room as. This
        /// value should match one of the 'RoomType(...)' attributes of your uploaded code. A room
        /// type of 'bounce' is always available.
        /// </param>
        /// <param name="visible">
        /// If the room doesn't exists: Determines (upon creation) if the room should be visible when
        /// listing rooms with ListRooms.
        /// </param>
        /// <param name="roomData">
        /// If the room doesn't exists: The data to initialize the room with (upon creation).
        /// </param>
        /// <param name="joinData">
        /// Data to send to the room with additional information about the join.
        /// </param>
        public Connection CreateJoinRoom(string roomId, string roomType, bool visible = true, Dictionary<string, string> roomData = null, Dictionary<string, string> joinData = null)
        {
            var (success, response, error) = this.Channel.Request<CreateJoinRoomArgs, CreateJoinRoomOutput>(27, new CreateJoinRoomArgs
            {
                RoomId = roomId,
                RoomType = roomType,
                Visible = visible,
                RoomData = roomData,
                JoinData = joinData,
                IsDevRoom = this.DevelopmentServer != null
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);

            var endpoints = this.FilterGameEndPoints(response.Endpoints.ToList());

            foreach (var endpoint in endpoints)
            {
                if (PortCheck.IsPortOpen(endpoint.Address, endpoint.Port, 1000, 3))
                {
                    var resolution = Dns.GetHostAddresses(endpoint.Address).First();

                    return new Connection(new IPEndPoint(resolution, endpoint.Port), response.JoinKey, joinData, this.ProxyOptions);
                }
            }

            throw new PlayerIOError(ErrorCode.GeneralError, "[Venture] Unable to join room - unable to establish connection from any endpoint(s) returned by API.");
        }

        private List<ServerEndPoint> FilterGameEndPoints(List<ServerEndPoint> endpoints)
        {
            if (endpoints == null)
                endpoints = new List<ServerEndPoint>();

            if (this.DevelopmentServer != null)
            {
                endpoints.Clear();
                endpoints.Add(new ServerEndPoint(DevelopmentServer.Address, DevelopmentServer.Port));

                return endpoints;
            }

            if (this.GameServerEndPointFilter != null)
            {
                endpoints.Clear();
                endpoints.AddRange(this.GameServerEndPointFilter(endpoints));

                return endpoints;
            }

            return endpoints;
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
