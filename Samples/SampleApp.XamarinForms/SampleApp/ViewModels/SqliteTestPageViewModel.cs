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

using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

using SimpleAdo;
using SimpleAdo.Sqlite;

namespace SampleApp.ViewModels
{
    public class SqliteTestPageViewModel : SimpleViewModel
    {
        public SqliteTestPageViewModel(Page view) : base(view) { }

        public async Task<bool> RunTestsCommand(object sender, EventArgs e)
        {

            #region ADDED TO SAMPLE TO DEMONSTRATE SimpleAdo.Sqlite

            //using SimpleAdo;
            //using SimpleAdo.Sqlite;

            string databasePath = App.AppDatabasePath.GetPath("sample.sqlite");

            await _view.DisplayAlert("Running tests...", $"Running tests in the SQLite database:\n{databasePath}", "OK");

            try
            {
                using (var dbConn = new SqliteConnection(databasePath, App.AppCryptEngine, true))
                {
                    string myTableName = "TestTable1";

                    Debug.WriteLine("Doing ADO stuff");

                    //Create the table if it doesn't exist
                    string sql = "CREATE TABLE IF NOT EXISTS " + myTableName + " (IdColumn INTEGER PRIMARY KEY AUTOINCREMENT, DateTimeColumn DATETIME, TextColumn TEXT);";
                    using (var cmd = new SqliteCommand(sql, dbConn))
                    {
                        dbConn.SafeOpen();
                        cmd.ExecuteNonQuery();
                        Debug.WriteLine("Table [" + myTableName + "] created (if it didn't exist).");
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
                        Debug.WriteLine("A record with ID " + newRowId + " was created in table [" + myTableName + "].");
                    }

                    //Read the datetime column on the oldest record
                    sql = "SELECT [DateTimeColumn] FROM " + myTableName + " ORDER BY [DateTimeColumn] LIMIT 1;";
                    using (var cmd = new SqliteCommand(sql, dbConn))
                    {
                        dbConn.SafeOpen();
                        DateTime oldest = Convert.ToDateTime(cmd.ExecuteScalar());
                        Debug.WriteLine("The oldest record in table [" + myTableName + "] has timestamp: " + oldest);
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
                        Debug.WriteLine("The [EncryptedColumn] column already exists.");
                        columnAlreadyExists = true;
                    }

                    #endregion

                    if (!columnAlreadyExists)
                    {
                        using (var cmd = new SqliteCommand(sql, dbConn))
                        {
                            dbConn.SafeOpen();
                            cmd.ExecuteNonQuery();
                            Debug.WriteLine("The [EncryptedColumn] column was created in table [" + myTableName + "].");
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
                        Debug.WriteLine("A record featuring encrypted data with ID " + newRowId + " was created in table [" + myTableName + "].");
                    }

                    //Get the value of the encrypted column
                    sql = "SELECT [EncryptedColumn] FROM " + myTableName + " WHERE [IdColumn] = @id;";
                    using (var cmd = new SqliteCommand(sql, dbConn))
                    {
                        cmd.Parameters.Add(new SqliteParameter("@id", newRowId));
                        dbConn.SafeOpen();
                        string encryptedColumnValue = cmd.ExecuteScalar().ToString();
                        var decryptedValue = App.AppCryptEngine.DecryptObject<Tuple<string, string, string>>(encryptedColumnValue);
                        Debug.WriteLine("The actual (encrypted) value of the [EncryptedColumn] column of record ID " + newRowId + " is: " + encryptedColumnValue);
                        Debug.WriteLine("The decrypted value of the [EncryptedColumn] column of record ID " + newRowId + " is: " +
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
                                Debug.WriteLine(sb.ToString());
                            }
                        }
                    }
                }

                Debug.WriteLine("Done.");

                await _view.DisplayAlert("All done!",
                    "The SQLite tests are all completed - check the IDE Output log messages and compare with the code in the 'SqliteTestPageViewModel.cs' code file.",
                    "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error - unhandled exception: " + ex.Message);
                Debug.WriteLine("Details:");
                Debug.WriteLine(ex.ToString());
                Exception innerEx = ex.InnerException;
                int innerIndex = 0;
                while (innerEx != null)
                {
                    innerIndex++;
                    Debug.WriteLine($"Inner Exception #{innerIndex}:");
                    Debug.WriteLine(innerEx.ToString());
                    innerEx = innerEx.InnerException;
                }
                await _view.DisplayAlert("!!Error!!",
                    "An error occurred - check the IDE Output log window for details.",
                    "OK");
            }

            #endregion

            return true;
        }

    }
}
