/* Copyright 2017 Ellisnet - Jeremy Ellis (jeremy@ellisnet.com)

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

//This file - MainViewModelForWindows.cs - was ADDED TO SAMPLE TO DEMONSTRATE SimpleAdo.Sqlite

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if NETFX_CORE
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

using SimpleAdo;
using SimpleAdo.Sqlite;

namespace SampleApp.ViewModels
{
    public class MainViewModel : SimpleViewModel
    {

        private readonly App _myApp;

        public string DatabasePath => _myApp.DatabasePath.GetPath("sample.sqlite");

        private string _message = "";
        public string Message
        {
            get => _message;
            set
            {
                _message = value ?? "";
                ThisPropertyChanged();
            }
        }

        private SimpleCommand _runTestsCommand;
        public SimpleCommand RunTestsCommand
        {
            get
            {
                return _runTestsCommand 
                    ?? (_runTestsCommand = new SimpleCommand(() => !String.IsNullOrWhiteSpace(DatabasePath), async () => await DoSqliteTests()));
            }
        }

        public MainViewModel()
        {
            _myApp = ((App)Application.Current);
        }

        private void AddMessageLine(string line)
        {
            if (!String.IsNullOrWhiteSpace(line))
            {
                Message += "\n" + line.Trim();
            }
        }

        private void AddMessageLine(string format, params object[] args)
        {
            if (!String.IsNullOrWhiteSpace(format))
            {
                Message += "\n" + String.Format(format, args);
            }
        }

        private async Task DoSqliteTests()
        {
            #region ADDED TO SAMPLE TO DEMONSTRATE SimpleAdo.Sqlite

            //using SimpleAdo;
            //using SimpleAdo.Sqlite;

            try
            {
                #region Create a table, add a record, add a column, add encrypted data, read back data

                using (var dbConn = new SqliteConnection(DatabasePath, _myApp.CryptEngine, true))
                {
                    string myTableName = "TestTable1";

                    AddMessageLine("Doing ADO stuff");

                    //Create the table if it doesn't exist
                    string sql = "CREATE TABLE IF NOT EXISTS " + myTableName + " (IdColumn INTEGER PRIMARY KEY AUTOINCREMENT, DateTimeColumn DATETIME, TextColumn TEXT);";
                    using (var cmd = new SqliteCommand(sql, dbConn))
                    {
                        dbConn.SafeOpen();
                        cmd.ExecuteNonQuery();
                        AddMessageLine("Table [" + myTableName + "] created (if it didn't exist).");
                    }

                    //Add a record
                    sql = "INSERT INTO " + myTableName + " (DateTimeColumn, TextColumn) VALUES (@date, @text);";
                    int newRowId;
                    using (var cmd = new SqliteCommand(sql, dbConn))
                    {
                        cmd.Parameters.Add(new SqliteParameter("@date", DateTime.Now)); //You can create the parameter yourself
                        cmd.Parameters.Add("@text", "Hello SQLite."); //Or you can have the add operation create it
                        dbConn.SafeOpen();
                        newRowId = Convert.ToInt32(cmd.ExecuteReturnRowId());  //Note: INTEGER columns in SQLite are always long/Int64 - including ID columns, so converting to int
                        AddMessageLine("A record with ID " + newRowId + " was created in table [" + myTableName + "].");
                    }

                    //Read the datetime column on the oldest record
                    sql = "SELECT [DateTimeColumn] FROM " + myTableName + " ORDER BY [DateTimeColumn] LIMIT 1;";
                    using (var cmd = new SqliteCommand(sql, dbConn))
                    {
                        dbConn.SafeOpen();
                        DateTime oldest = Convert.ToDateTime(cmd.ExecuteScalar());
                        AddMessageLine("The oldest record in table [" + myTableName + "] has timestamp: " + oldest);
                    }

                    //Add an encrypted column to the table
                    //NOTE: There is no benefit to creating the column as SQLite datatype ENCRYPTED vs. TEXT
                    //  It is actually a TEXT column - but I think it is nice to set it to type ENCRYPTED for identification purposes.
                    sql = "ALTER TABLE " + myTableName + " ADD COLUMN EncryptedColumn ENCRYPTED;";
                    //Note: This column shouldn't exist until the above sql is run, since I just created the table above.  But if this application has been run multiple times, 
                    //  the column may already exist in the table - so I need to check for it.
                    bool columnAlreadyExists = false;

                    #region Check for column

                    IColumnInfo[] tableColumns = dbConn.GetTableColumnList(myTableName);
                    if (tableColumns.Any(a => a.Name == "EncryptedColumn" && a.DataType == DbType.Encrypted))
                    {
                        AddMessageLine("The [EncryptedColumn] column already exists.");
                        columnAlreadyExists = true;
                    }

                    #endregion

                    if (!columnAlreadyExists)
                    {
                        using (var cmd = new SqliteCommand(sql, dbConn))
                        {
                            dbConn.SafeOpen();
                            cmd.ExecuteNonQuery();
                            AddMessageLine("The [EncryptedColumn] column was created in table [" + myTableName + "].");
                        }
                    }

                    //Add a record with an encrypted column value
                    sql = "INSERT INTO " + myTableName + " (DateTimeColumn, TextColumn, EncryptedColumn) VALUES (@date, @text, @encrypted);";
                    using (var cmd = new SqliteCommand(sql, dbConn))
                    {
                        cmd.Parameters.Add(new SqliteParameter("@date", DateTime.Now)); //You can create the parameter yourself
                        cmd.Parameters.Add("@text", "Hello data."); //Or you can have the add operation create it
                        cmd.AddEncryptedParameter(new SqliteParameter("@encrypted",
                            Tuple.Create("Hello", "encrypted", "data")));
                        dbConn.SafeOpen();
                        newRowId = Convert.ToInt32(cmd.ExecuteReturnRowId());  //Note: INTEGER columns in SQLite are always long/Int64 - including ID columns, so converting to int
                        AddMessageLine("A record featuring encrypted data with ID " + newRowId + " was created in table [" + myTableName + "].");
                    }

                    //Get the value of the encrypted column
                    sql = "SELECT [EncryptedColumn] FROM " + myTableName + " WHERE [IdColumn] = @id;";
                    using (var cmd = new SqliteCommand(sql, dbConn))
                    {
                        cmd.Parameters.Add(new SqliteParameter("@id", newRowId));
                        dbConn.SafeOpen();
                        string encryptedColumnValue = cmd.ExecuteScalar().ToString();
                        var decryptedValue = _myApp.CryptEngine.DecryptObject<Tuple<string, string, string>>(encryptedColumnValue);
                        AddMessageLine("The actual (encrypted) value of the [EncryptedColumn] column of record ID " + newRowId + " is: " + encryptedColumnValue);
                        AddMessageLine("The decrypted value of the [EncryptedColumn] column of record ID " + newRowId + " is: " +
                            decryptedValue.Item1 + " " + decryptedValue.Item2 + " " + decryptedValue.Item3);
                    }

                    //Using a SqliteDataReader and GetDecrypted<T> to get all of the encrypted values
                    sql = "SELECT [IdColumn], [DateTimeColumn], [EncryptedColumn] FROM " + myTableName + ";";
                    using (var cmd = new SqliteCommand(sql, dbConn))
                    {
                        dbConn.SafeOpen();
                        using (var dr = new SqliteDataReader(cmd))
                        {
                            while (dr.Read())
                            {
                                var sb = new StringBuilder();
                                sb.Append("ID: " + dr.GetInt32("IdColumn"));
                                sb.Append(" - Timestamp: " + dr.GetDateTime("DateTimeColumn"));
                                //IMPORTANT: By default, GetDecrypted<T> will throw an exception on a NULL column value.  You can specify DbNullHandling.ReturnTypeDefaultValue
                                // to return the default value of the specified type - as in default(T) - when a NULL column value is encountered, if you choose.
                                var decryptedValue = dr.GetDecrypted<Tuple<string, string, string>>("EncryptedColumn", DbNullHandling.ReturnTypeDefaultValue);
                                sb.Append(" - Value: " + ((decryptedValue == null) ? "NULL" :
                                    decryptedValue.Item1 + " " + decryptedValue.Item2 + " " + decryptedValue.Item3));
                                AddMessageLine(sb.ToString());
                            }
                        }
                    }

                    //NOTE:
                    // Was previously: Testing out re-using a SqliteCommand instance with various SQL statements - and the ability to send multiple statements via one ExecuteNonQuery()
                    // It is *NOT* possible to send multiple SQL commands via the same SqliteCommand.CommandText; only the first one will be executed.
                    // So, instead this is demonstrating re-using a SqliteCommand instance with various SQL statements - and calling ExecuteNonQuery() on each of them.
                    myTableName = "TestTable2";

                    using (var cmd = new SqliteCommand(dbConn))
                    {
                        dbConn.SafeOpen();
                        cmd.CommandText = "CREATE TABLE IF NOT EXISTS " + myTableName + " (IdColumn INTEGER PRIMARY KEY AUTOINCREMENT, TextColumn TEXT);";
                        cmd.ExecuteNonQuery();
                        AddMessageLine("Table [" + myTableName + "] created (if it didn't exist).");

                        //OLD Logic:
                        /*
                        cmd.CommandText = "INSERT INTO " + myTableName + " (TextColumn) VALUES ('First value');";
                        cmd.CommandText += "INSERT INTO " + myTableName + " (TextColumn) VALUES ('Second value');";
                        cmd.CommandText += "INSERT INTO " + myTableName + " (TextColumn) VALUES ('Third value');";
                        cmd.ExecuteNonQuery();
                        */
                        //Now using:
                        cmd.CommandText = "INSERT INTO " + myTableName + " (TextColumn) VALUES ('First value');";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "INSERT INTO " + myTableName + " (TextColumn) VALUES ('Second value');";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "INSERT INTO " + myTableName + " (TextColumn) VALUES ('Third value');";
                        cmd.ExecuteNonQuery();
                        AddMessageLine("Should have just added three records to the " + myTableName + " table.");

                        cmd.CommandText = "SELECT IdColumn, TextColumn FROM " + myTableName + ";";
                        using (var dr = new SqliteDataReader(cmd))
                        {
                            while (dr.Read())
                            {
                                AddMessageLine($"ID: {dr.GetInt32("IdColumn")} - Text: {dr.GetString("TextColumn")}");
                            }
                        }
                    }

                }

                #endregion

                AddMessageLine("Done.");

                string linebreak = "";
#if NETFX_CORE
                linebreak = "\n";
#endif

                //Pop up a message saying we are done
                await new SimpleDialog(
                    $"If you can see this message, then the sample SimpleAdo.Sqlite code {linebreak}ran correctly. " +
                    $"Take a look at the code in the 'MainViewModel.cs' file, {linebreak}and compare it to what you are seeing in the Output window. " +
                    $"{linebreak}And have a nice day...",
                    "All Done! - Check the Output window").ShowAsync();
            }
            catch (Exception ex)
            {
                await new SimpleDialog(
                    "An error occurred: " + ex.Message + "\n\nDetails:\n" + ex,
                    "ERROR").ShowAsync();
            }

#endregion
        }

    }
}
