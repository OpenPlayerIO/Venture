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
    public class BigDB
    {
        internal BigDB(PlayerIOChannel channel)
        {
            this.Channel = channel;
        }

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

        /// <summary> Creates a new database object in the given table with the specified key. 
        /// If no key is specified (null), the Database Object will receive an automatically generated key. 
        /// </summary>
        /// <param name="table"> The name of the table in which to create the database object. </param>
        /// <param name="key"> The key to assign to the database object. </param>
        /// <param name="dbo"> The database object to create in the table. </param>
        /// <returns> A new instance of the DatabaseObject from which .Save() can be called for future modifications. </returns>
        public void CreateObject(string table, string key, DatabaseObject dbo)
        {
            this.CreateObjects(new[] { new DatabaseObjectPushModel
            {
                Table = table,
                Key = key,
                Properties = DatabaseEx.FromDatabaseObject(dbo)
            } }, false);
        }

        internal void CreateObjects(DatabaseObjectPushModel[] objects, bool loadExisting)
        {
            var (success, response, error) = this.Channel.Request<CreateObjectsArgs, CreateObjectsOutput>(82, new CreateObjectsArgs
            {
                LoadExisting = loadExisting,
                Objects = objects
            });

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);
        }


        internal PlayerIOChannel Channel { get; }
    }
}
