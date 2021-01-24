using System;
using System.IO;
//using LocalCommons.Data;
//using LocalCommons.Database;
using LocalCommons.Logging;
using LocalCommons.Utilities;
//using LocalCommons.Utilities.Configuration;

namespace LocalCommons
{
    /// <summary>
    /// Base class for server applications.
    /// </summary>
    public abstract class Server
    {

        /// <summary>
        /// Initializes database connection with data from Conf.
        /// </summary>
        /*protected void LoadDBData(ArcheageDb db, Conf conf)
        {
            try
            {
                Log.Info("Initializing database...");
                db.Init(conf.Database.Host, conf.Database.User, conf.Database.Pass, conf.Database.Db);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to initialize database: {0}", ex.Message);
                CliUtil.Exit(1, true);
            }
        }*/

    }

    [Flags]
    public enum DataToLoad
    {
        Items = 0x01,
        Maps = 0x02,
        Jobs = 0x04,
        Servers = 0x08,
        Barracks = 0x10,
        Monsters = 0x20,
        Skills = 0x40,
        Exp = 0x80,
        Dialogues = 0x100,
        Shops = 0x200,
        Help = 0x400,
        CustomCommands = 0x800,
        ChatMacros = 0x1000,

        All = 0x7FFFFFFF,
    }
}
