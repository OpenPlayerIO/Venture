using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayerIOClient
{
    /// <summary>
    /// The Player.IO BigDB service.
    /// 
    /// <para> 
    /// This class is used to create, load, and delete database objects. All database objects are stored in tables and have a unique key. 
    /// </para>
    /// <para>
    /// You can set up tables in your admin panel, and you can also set up indexes there for when you want to load objects by properties
    /// </para>
    /// </summary>
    public partial class BigDB
    {
        internal BigDB(PlayerIOChannel channel)
        {
            this.Channel = channel;
        }

        /// <summary> Loads the database object with the given key from the specified table. </summary>
        /// <param name="table"> The name of the table in which to load the database object from. </param>
        /// <param name="key"> The key of the database object to load. </param>
        /// <returns> A database object from BigDB with the specified key and table. </returns>
        public void Load(string table, string key, Callback<DatabaseObject> successCallback, Callback<PlayerIOError> errorCallback = null)
            => CallbackHandler.CreateHandler(() => this.Load(table, key), ref successCallback, ref errorCallback);

        /// <summary> Loads the database objects with the given keys from the specified table. </summary>
        /// <param name="table"> The name of the table in which to load the database objects from. </param>
        /// <param name="keys"> The keys of the database objects to load. </param>
        /// <returns> An array of database objects in the same order as the keys array, with null values for non-existant database objects. </returns>
        public void LoadKeys(string table, string[] keys, Callback<IEnumerable<DatabaseObject>> successCallback, Callback<PlayerIOError> errorCallback = null)
            => CallbackHandler.CreateHandler(() => this.LoadKeys(table, keys), ref successCallback, ref errorCallback);

        /// <summary> Loads or creates the database objects with the given keys from the specified table. If the given key doesn't exist, it will be created with a new database object. </summary>
        /// <param name="table"> The name of the table in which to load the database objects from. </param>
        /// <param name="keys"> The keys of the database objects to load. </param>
        /// <returns> An array of database objects in the same order as the keys array. </returns>
        public void LoadKeysOrCreate(string table, string[] keys, Callback<IEnumerable<DatabaseObject>> successCallback, Callback<PlayerIOError> errorCallback = null)
            => CallbackHandler.CreateHandler(() => this.LoadKeysOrCreate(table, keys), ref successCallback, ref errorCallback);

        /// <summary>
        /// Load the database object corresponding to the ConnectUserId of the client from the PlayerObjects table.
        /// </summary>
        /// <returns> The database object for the client. </returns>
        public void LoadMyPlayerObject(Callback<DatabaseObject> successCallback, Callback<PlayerIOError> errorCallback = null)
            => CallbackHandler.CreateHandler(() => this.LoadMyPlayerObject(), ref successCallback, ref errorCallback);

        /// <summary> Load a range of database objects from a table using the specified index. </summary>
        /// <param name="table"> The table to load the database object from. </param>
        /// <param name="index"> The name of the index to query for the database object. </param>
        /// <param name="indexPath">
        /// Where in the index to start the range search: An array of objects of the same types as the index properties,specifying where in the index to start loading database objects from. 
        /// For instance, in the index [Mode,Map,Score] you might use new object[]{"expert","skyland"} as the indexPath and use the start and stop arguments to determine the range of scores 
        /// you wish to return. IndexPath can be set to null if there is only one property in the index.
        /// </param>
        /// <param name="start">
        /// Where to start the range search. For instance, if the index is [Mode,Map,Score] and indexPath is ["expert","skyland"], then start defines the minimum score to include in the results. </param>
        /// <param name="stop">
        /// Where to stop the range search. For instance, if the index is [Mode,Map,Score] and indexPath is ["expert","skyland"], then stop defines the maximum score to include in the results. </param>
        /// <param name="limit"> The maximum amount of objects to return. </param>
        /// <returns> An array containing the database objects that were found in the search. </returns>
        public void LoadRange(string table, string index, object[] indexPath, object start, object stop, int limit, Callback<DatabaseObject[]> successCallback, Callback<PlayerIOError> errorCallback = null)
            => CallbackHandler.CreateHandler(() => this.LoadRange(table, index, indexPath, start, stop, limit), ref successCallback, ref errorCallback);

        /// <summary>
        /// Load a database object from a table using the specified index.
        /// </summary>
        /// <param name="table"> The table to load the database object from. </param>
        /// <param name="index"> The name of the index to query for the database object. </param>
        /// <param name="indexValue"> An array of objects of the same types as the index properties, specifying which object to load. </param>
        /// <returns> The database object found, or null if no object was found. </returns>
        public void LoadSingle(string table, string index, object[] indexValue, Callback<DatabaseObject> successCallback, Callback<PlayerIOError> errorCallback = null)
            => CallbackHandler.CreateHandler(() => this.LoadSingle(table, index, indexValue), ref successCallback, ref errorCallback);

        /// <summary>
        /// Delete a range of database objects from a table using an index.
        /// </summary>
        /// <param name="table"> The table to delete the database object from</param>
        /// <param name="index"> The name of the index to query for the database objects to delete</param>
        /// <param name="indexPath">
        /// Where in the index to start the range delete: An array of objects of the same types as the index properties, specifying where in the index to start loading database objects from. 
        /// For instance, in the index [Mode,Map,Score] you might use new object[]{"expert","skyland"} as the indexPath and use the start and stop arguments to determine the range of scores 
        /// you wish to delete. IndexPath can be set to null instead of an empty array.
        /// </param>
        /// <param name="start">
        /// Where to start the range delete. For instance, if the index is [Mode,Map,Score] and indexPath is ["expert","skyland"], then start defines the minimum score to delete.
        /// </param>
        /// <param name="stop">
        /// Where to stop the range delete. For instance, if the index is [Mode,Map,Score] and indexPath is ["expert","skyland"], then stop defines the maximum score to delete.
        /// </param>
        public void DeleteRange(string table, string index, object[] indexPath, object start, object stop, Callback successCallback, Callback<PlayerIOError> errorCallback = null)
            => CallbackHandler.CreateHandler(() => this.DeleteRange(table, index, indexPath, start, stop), ref successCallback, ref errorCallback);

        /// <summary>
        /// Delete the database objects with the given keys from the specified table.
        /// </summary>
        /// <param name="table"> The name of the table. </param>
        /// <param name="keys"> The database object keys. </param>
        public void DeleteKeys(string table, string[] keys, Callback successCallback, Callback<PlayerIOError> errorCallback = null)
            => CallbackHandler.CreateHandler(() => this.DeleteKeys(table, keys), ref successCallback, ref errorCallback);

        /// <summary> Create a new database object in the given table with the specified key. 
        /// If no key is specified (null), the Database Object will receive an automatically generated key. 
        /// </summary>
        /// <param name="table"> The name of the table in which to create the database object. </param>
        /// <param name="key"> The key to assign to the database object. </param>
        /// <param name="dbo"> The database object to create in the table. </param>
        /// <returns> A new instance of the DatabaseObject from which .Save() can be called for future modifications. </returns>
        public void CreateObject(string table, string key, DatabaseObject dbo, Callback<DatabaseObject> successCallback, Callback<PlayerIOError> errorCallback = null)
            => CallbackHandler.CreateHandler(() => this.CreateObject(table, key, dbo), ref successCallback, ref errorCallback);
    }

    public partial class BigDB
    {
        /// <summary> Loads the database object with the given key from the specified table. </summary>
        /// <param name="table"> The name of the table in which to load the database object from. </param>
        /// <param name="key"> The key of the database object to load. </param>
        /// <returns> A database object from BigDB with the specified key and table. </returns>
        public DatabaseObject Load(string table, string key)
        {
            if (string.IsNullOrEmpty(table) || string.IsNullOrEmpty(key))
                throw new PlayerIOError(ErrorCode.GeneralError, "You must specify a table and a key to load a Database Object from BigDB.");

            var (success, response, error) = this.Channel.Request<LoadObjectsArgs, LoadObjectsOutput>(85, new LoadObjectsArgs()
            {
                ObjectIds = new BigDBObjectId() { Keys = new List<string>() { key }, Table = table }
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);

            if (response.Objects == null || response.Objects.Count == 0)
                return null;

            return new DatabaseObject(this, table, response.Objects.First().Key, response.Objects.First().Version, response.Objects.First().Properties);
        }

        /// <summary> Loads the database objects with the given keys from the specified table. </summary>
        /// <param name="table"> The name of the table in which to load the database objects from. </param>
        /// <param name="keys"> The keys of the database objects to load. </param>
        /// <returns> An array of database objects in the same order as the keys array, with null values for non-existant database objects. </returns>
        public IEnumerable<DatabaseObject> LoadKeys(string table, params string[] keys)
        {
            if (string.IsNullOrEmpty(table))
                throw new PlayerIOError(ErrorCode.GeneralError, "You must specify a table in order to load keys from BigDB.");

            if (keys.Length == 0)
                throw new PlayerIOError(ErrorCode.GeneralError, "You must specify at least one key to load from BigDB.");

            var (success, response, error) = this.Channel.Request<LoadObjectsArgs, LoadObjectsOutput>(85, new LoadObjectsArgs()
            {
                ObjectIds = new BigDBObjectId() { Keys = keys.ToList(), Table = table }
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);

            if (response.Objects == null || response.Objects.Count == 0)
            {
                yield return null;
            }
            else
            {
                foreach (var key in keys)
                {
                    var match = response.Objects.Find(o => o.Key == key);

                    yield return match != null ? new DatabaseObject(this, table, match.Key, match.Version, match.Properties) : null;
                }
            }
        }

        /// <summary> Loads or creates the database objects with the given keys from the specified table. If the given key doesn't exist, it will be created with a new database object. </summary>
        /// <param name="table"> The name of the table in which to load the database objects from. </param>
        /// <param name="keys"> The keys of the database objects to load. </param>
        /// <returns> An array of database objects in the same order as the keys array. </returns>
        public IEnumerable<DatabaseObject> LoadKeysOrCreate(string table, params string[] keys)
        {
            if (string.IsNullOrEmpty(table))
                throw new PlayerIOError(ErrorCode.GeneralError, "You must specify a table in order to load keys from BigDB.");

            if (keys.Length == 0)
                throw new PlayerIOError(ErrorCode.GeneralError, "You must specify at least one key to load from BigDB.");

            var (success, response, error) = this.Channel.Request<LoadObjectsArgs, LoadObjectsOutput>(85, new LoadObjectsArgs()
            {
                ObjectIds = new BigDBObjectId() { Keys = keys.ToList(), Table = table }
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);

            if (response.Objects == null || response.Objects.Count == 0)
            {
                yield return null;
            }
            else
            {
                foreach (var key in keys)
                {
                    var match = response.Objects.Find(o => o.Key == key);

                    if (match == null)
                    {
                        this.CreateObject(table, key, new DatabaseObject());
                        yield return this.Load(table, key);
                    }

                    yield return new DatabaseObject(this, table, match.Key, match.Version, match.Properties);
                }
            }
        }

        /// <summary> Load a range of database objects from a table using the specified index. </summary>
        /// <param name="table"> The table to load the database object from. </param>
        /// <param name="index"> The name of the index to query for the database object. </param>
        /// <param name="indexPath">
        /// Where in the index to start the range search: An array of objects of the same types as the index properties,specifying where in the index to start loading database objects from. 
        /// For instance, in the index [Mode,Map,Score] you might use new object[]{"expert","skyland"} as the indexPath and use the start and stop arguments to determine the range of scores 
        /// you wish to return. IndexPath can be set to null if there is only one property in the index.
        /// </param>
        /// <param name="start">
        /// Where to start the range search. For instance, if the index is [Mode,Map,Score] and indexPath is ["expert","skyland"], then start defines the minimum score to include in the results. </param>
        /// <param name="stop">
        /// Where to stop the range search. For instance, if the index is [Mode,Map,Score] and indexPath is ["expert","skyland"], then stop defines the maximum score to include in the results. </param>
        /// <param name="limit"> The maximum amount of objects to return. </param>
        /// <returns> An array containing the database objects that were found in the search. </returns>
        public DatabaseObject[] LoadRange(string table, string index, object[] indexPath, object start, object stop, int limit)
        {
            var (success, response, error) = this.Channel.Request<LoadIndexRangeArgs, LoadIndexRangeOutput>(97, new LoadIndexRangeArgs()
            {
                Table = table,
                Index = index,
                StartIndexValue = DatabaseEx.MakeRange(indexPath, start),
                StopIndexValue = DatabaseEx.MakeRange(indexPath, stop),
                Limit = limit
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);

            if (response.Objects == null || !response.Objects.Any())
                return new DatabaseObject[] { };

            return response.Objects.Select(o => new DatabaseObject(this, table, o.Key, o.Version, o.Properties)).ToArray();
        }

        /// <summary>
        /// Load the database object corresponding to the ConnectUserId of the client from the PlayerObjects table.
        /// </summary>
        /// <returns> The database object for the client. </returns>
        public DatabaseObject LoadMyPlayerObject()
        {
            var (success, response, error) = this.Channel.Request<LoadMyPlayerObjectArgs, LoadMyPlayerObjectOutput>(103, new LoadMyPlayerObjectArgs());

            if (!success)
                throw new PlayerIOError(ErrorCode.GeneralError, error.Message);

            return new DatabaseObject(this, "PlayerObjects", response.PlayerObject.Key, response.PlayerObject.Version, response.PlayerObject.Properties);
        }

        /// <summary>
        /// Load a database object from a table using the specified index.
        /// </summary>
        /// <param name="table"> The table to load the database object from. </param>
        /// <param name="index"> The name of the index to query for the database object. </param>
        /// <param name="indexValue"> An array of objects of the same types as the index properties, specifying which object to load. </param>
        /// <returns> The database object found, or null if no object was found. </returns>
        public DatabaseObject LoadSingle(string table, string index, params object[] indexValue)
        {
            var (success, response, error) = this.Channel.Request<LoadMatchingObjectsArgs, LoadMatchingObjectsOutput>(94, new LoadMatchingObjectsArgs()
            {
                Table = table,
                Index = index,
                IndexValue = indexValue.Select(o => DatabaseEx.Create(o)).ToList(),
                Limit = 1
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);

            if (response.Objects == null || !response.Objects.Any())
                return null;

            return new DatabaseObject(this, table, response.Objects.First().Key, response.Objects.First().Version, response.Objects.First().Properties);
        }
        
        /// <summary>
        /// Delete a range of database objects from a table using an index.
        /// </summary>
        /// <param name="table"> The table to delete the database object from</param>
        /// <param name="index"> The name of the index to query for the database objects to delete</param>
        /// <param name="indexPath">
        /// Where in the index to start the range delete: An array of objects of the same types as the index properties, specifying where in the index to start loading database objects from. 
        /// For instance, in the index [Mode,Map,Score] you might use new object[]{"expert","skyland"} as the indexPath and use the start and stop arguments to determine the range of scores 
        /// you wish to delete. IndexPath can be set to null instead of an empty array.
        /// </param>
        /// <param name="start">
        /// Where to start the range delete. For instance, if the index is [Mode,Map,Score] and indexPath is ["expert","skyland"], then start defines the minimum score to delete.
        /// </param>
        /// <param name="stop">
        /// Where to stop the range delete. For instance, if the index is [Mode,Map,Score] and indexPath is ["expert","skyland"], then stop defines the maximum score to delete.
        /// </param>
        public void DeleteRange(string table, string index, object[] indexPath, object start, object stop)
        {
            var (sucecss, response, error) = this.Channel.Request<DeleteIndexRangeArgs, DeleteIndexRangeOutput>(100, new DeleteIndexRangeArgs()
            {
                Table = table,
                Index = index,
                StartIndexValue = DatabaseEx.MakeRange(indexPath, start),
                StopIndexValue = DatabaseEx.MakeRange(indexPath, stop)
            });

            if (!sucecss)
                throw new PlayerIOError(error.ErrorCode, error.Message);
        }

        /// <summary> Create a new database object in the given table with the specified key. 
        /// If no key is specified (null), the Database Object will receive an automatically generated key. 
        /// </summary>
        /// <param name="table"> The name of the table in which to create the database object. </param>
        /// <param name="key"> The key to assign to the database object. </param>
        /// <param name="dbo"> The database object to create in the table. </param>
        /// <returns> A new instance of the DatabaseObject from which .Save() can be called for future modifications. </returns>
        public DatabaseObject CreateObject(string table, string key, DatabaseObject dbo)
        {
            try
            {
                var instance = this.CreateObjects(new[]
                {
                    new DatabaseObjectPushModel()
                    {
                        Table = table,
                        Key = key,
                        Properties = DatabaseEx.FromDatabaseObject(dbo)
                    }
                }, false).FirstOrDefault();

                if (instance == null)
                {
                    throw new PlayerIOError(ErrorCode.InternalError, "The API responded without any database objects post creation call.");
                }

                instance.Table = table;

                return instance;
            }
            catch (PlayerIOError error)
            {
                throw error;
            }
            catch (Exception error)
            {
                throw error;
            }

            throw null;
        }

        /// <summary>
        /// Delete the database objects with the given keys from the specified table.
        /// </summary>
        /// <param name="table"> The name of the table. </param>
        /// <param name="keys"> The database object keys. </param>
        public void DeleteKeys(string table, params string[] keys)
        {
            var (success, response, error) = this.Channel.Request<DeleteObjectsArgs, DeleteObjectsOutput>(91, new DeleteObjectsArgs()
            {
                ObjectIds = new List<BigDBObjectId>()
                {
                    new BigDBObjectId()
                    {
                        Table = table,
                        Keys = keys.ToList()
                    }
                }
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);
        }

        internal List<DatabaseObject> CreateObjects(DatabaseObjectPushModel[] objects, bool loadExisting)
        {
            var (success, response, error) = this.Channel.Request<CreateObjectsArgs, CreateObjectsOutput>(82, new CreateObjectsArgs
            {
                LoadExisting = loadExisting,
                Objects = objects
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);

            return response.Objects.Select(o => new DatabaseObject(this, "", o.Key, o.Version, o.Properties)).ToList();
        }

        internal List<string> SaveChanges(LockType lockType, List<BigDBChangeSet> changeSets, bool createIfMissing)
        {
            var (success, response, error) = this.Channel.Request<SaveObjectChangesArgs, SaveObjectChangesOutput>(88, new SaveObjectChangesArgs()
            {
                LockType = lockType,
                ChangeSets = changeSets,
                CreateIfMissing = createIfMissing
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);

            return response.Versions;
        }

        internal PlayerIOChannel Channel { get; }
    }
}