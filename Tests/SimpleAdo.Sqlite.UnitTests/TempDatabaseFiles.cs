using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SimpleAdo.Sqlite.UnitTests
{
    public static class TempDatabaseFiles
    {
        private static readonly List<Tuple<string,string>> filesList = new List<Tuple<string, string>>();
        private static readonly object locker = new object();

        public static string GetNewDatabasePath(object sender = null)
        {
            string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid().ToString("D").ToUpper()}.sqlite");
            Add(sender?.GetType(), path);
            return path;
        }

        public static void Add(Type fixtureType, string databaseFilePath)
        {
            string fixtureTypeName = fixtureType?.AssemblyQualifiedName ?? "";
            if (!String.IsNullOrWhiteSpace(databaseFilePath))
            {
                lock (locker)
                {
                    if (!filesList.Any(a => a.Item1 == fixtureTypeName && a.Item2 == databaseFilePath))
                    {
                        filesList.Add(Tuple.Create(fixtureTypeName, databaseFilePath));
                    }
                }
            }
        }

        public static void CleanUp(Type fixtureType)
        {
            lock (locker)
            {
                string fixtureTypeName = fixtureType?.AssemblyQualifiedName ?? "";

                //TODO: Still haven't found a good way to the release the lock that SQLite has on its files -
                // calling sqlite3_shutdown() does not seem to work - so all file deletions below are currently failing.

                // ReSharper disable once UnusedVariable
                var result = SqliteConnection.ShutdownSqlite3Provider();

                var deletedFiles = new List<Tuple<string, string>>();

                foreach (Tuple<string, string> databaseFile in filesList.Where(w => w.Item1 == fixtureTypeName))
                {
                    if (File.Exists(databaseFile.Item2))
                    {
                        try
                        {
                            File.Delete(databaseFile.Item2);
                            deletedFiles.Add(databaseFile);
                        }
                        catch (Exception e)
                        {
                            //Temp database file deletion isn't working on the AppVeyor CI build server either; so decided not to warn about
                            // it for now:
                            //Console.WriteLine($"Warning - error encountered while deleting temporary database file after test completion: {e}");
                            //Debugger.Break();
                        }
                    }
                }

                foreach (Tuple<string, string> databaseFile in deletedFiles)
                {
                    if (!File.Exists(databaseFile.Item2))
                    {
                        filesList.Remove(databaseFile);
                    }
                }
            }
        }
    }
}
