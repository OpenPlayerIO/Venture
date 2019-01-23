using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

                    if (match != null)
                        yield return new DatabaseObject(this, table, match.Key, match.Version, match.Properties);
                }
            }
        }

        internal PlayerIOChannel Channel { get; }
    }
}
