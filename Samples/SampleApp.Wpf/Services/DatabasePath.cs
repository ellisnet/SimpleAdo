using System;
using System.IO;

namespace SampleApp.Services
{
    public class DatabasePath : IDatabasePath
    {
        public string GetPath(string databaseName)
        {
            databaseName = databaseName?.Trim() ?? throw new ArgumentNullException(nameof(databaseName));
            if (databaseName == "") { throw new ArgumentOutOfRangeException(nameof(databaseName));}
            //TODO: Puts the SQLite database in the user's temp folder - probably not the most permanent location
            return Path.Combine(Path.GetTempPath(), databaseName);
        }
    }
}
