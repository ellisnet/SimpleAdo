using System;
using System.Security.Cryptography;
using NUnit.Framework;

namespace SimpleAdo.Sqlite.UnitTests
{
    [TestFixture]
    public class SqliteDataReaderTests
    {
        //SQL statements used for testing
        private readonly string tableName = "TestTable";
        private readonly string tableCreateSql = "CREATE TABLE IF NOT EXISTS {0} (IdColumn INTEGER PRIMARY KEY AUTOINCREMENT, DateTimeColumn DATETIME NOT NULL, TextColumn TEXT);";
        private readonly string addColumnSql = "ALTER TABLE {0} ADD COLUMN EncryptedColumn ENCRYPTED;";
        private readonly string insertSql1 = "INSERT INTO {0} (DateTimeColumn, TextColumn) VALUES (@date, @text);";
        private readonly string insertSql2 = "INSERT INTO {0} (DateTimeColumn, TextColumn, EncryptedColumn) VALUES (@date, @text, @encrypted);";
        private readonly string selectSql1 = "SELECT {0} FROM {1} ORDER BY {2};";
        private readonly string selectSql2 = "SELECT {0} FROM {1} ORDER BY {2} LIMIT 1;";

        private readonly Tuple<DateTime, string>[] _sampleData =
        {
            new Tuple<DateTime, string>(DateTime.Now, "Current date & time"),
            new Tuple<DateTime, string>(DateTime.MinValue, "Earliest possible date & time"),
            new Tuple<DateTime, string>(DateTime.MaxValue, "Latest possible date & time"),
        };

        #region Write and then read data tests

        [Test]
        public void WriteAndReadData_BasicColumns_IsSuccessful()
        {
            //Arrange
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);
            int dataItemIndex = 0;

            using (var db = new SqliteConnection(dbPath, true))
            {
                //Creating my table
                using (var cmd = new SqliteCommand(String.Format(tableCreateSql, tableName), db))
                {
                    db.SafeOpen();
                    cmd.ExecuteNonQuery();
                }

                //Act
                using (var cmd = new SqliteCommand(String.Format(insertSql1, tableName), db))
                {
                    //Adding my records
                    foreach (Tuple<DateTime, string> dataItem in _sampleData)
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@date", dataItem.Item1);
                        cmd.Parameters.Add("@text", dataItem.Item2);
                        db.SafeOpen();
                        cmd.ExecuteNonQuery();
                    }
                }

                db.SafeClose(); //Closing the database to confirm that readers automatically open the underlying database connection, when necessary

                //Assert
                using (var cmd = new SqliteCommand(String.Format(selectSql1, "[IdColumn], [DateTimeColumn], [TextColumn]", tableName, "[IdColumn]"), db))
                using (var reader = new SqliteDataReader(cmd))
                {
                    while (reader.Read())
                    {
                        Assert.AreEqual((dataItemIndex + 1), reader.GetInt64("IdColumn"));
                        Assert.AreEqual(_sampleData[dataItemIndex].Item1, reader.GetDateTime("DateTimeColumn"));
                        Assert.AreEqual(_sampleData[dataItemIndex].Item2, reader.GetString("TextColumn"));

                        dataItemIndex++;
                    }
                    Assert.AreEqual(3, dataItemIndex); //Should have read three records
                }
                Assert.AreEqual(ConnectionState.Open, db.State); //It is expected that a database connection will be open after being used with a DataReader

                db.SafeOpen(); //Setting the database to open to confirm that readers do not have problems with an already-open database

                dataItemIndex = 0;
                using (var cmd = new SqliteCommand(String.Format(selectSql2, "[DateTimeColumn], [TextColumn]", tableName, "[DateTimeColumn] ASC"), db))
                using (var reader = new SqliteDataReader(cmd))
                {
                    while (reader.Read())
                    {
                        Assert.AreEqual(DateTime.MinValue, reader.GetDateTime("DateTimeColumn")); //Should have found the MinValue based on the ORDER BY clause
                        dataItemIndex++;
                    }
                    Assert.AreEqual(1, dataItemIndex); //Should have only read one record based on the LIMIT clause
                }

                dataItemIndex = 0;
                using (var cmd = new SqliteCommand(String.Format(selectSql2, "[DateTimeColumn], [TextColumn]", tableName, "[DateTimeColumn] DESC"), db))
                using (var reader = new SqliteDataReader(cmd))
                {
                    while (reader.Read())
                    {
                        Assert.AreEqual(DateTime.MaxValue, reader.GetDateTime("DateTimeColumn")); //Should have found the MaxValue based on the ORDER BY clause
                        dataItemIndex++;
                    }
                    Assert.AreEqual(1, dataItemIndex); //Should have only read one record based on the LIMIT clause
                }
            }
        }


        [Test]
        public void WriteAndReadData_EncryptedColumn_IsSuccessful()
        {
            //Arrange
            var cryptEngine = new SampleAesCryptEngine("myTestPassword");
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);
            int dataItemIndex = 0;

            using (var db = new SqliteConnection(dbPath,cryptEngine, true))
            {
                //Creating my table & adding encrypted column
                using (var cmd = new SqliteCommand(String.Format(tableCreateSql, tableName), db))
                {
                    db.SafeOpen();
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = String.Format(addColumnSql, tableName);
                    cmd.ExecuteNonQuery();
                }

                //Act
                using (var cmd = new SqliteCommand(String.Format(insertSql2, tableName), db))
                {
                    //Adding my records
                    foreach (Tuple<DateTime, string> dataItem in _sampleData)
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@date", dataItem.Item1);
                        cmd.Parameters.Add("@text", dataItem.Item2);
                        cmd.AddEncryptedParameter(new SqliteParameter("@encrypted", dataItem.Item2)); //Adds the string value (Tuple Item2) but with encryption
                        db.SafeOpen();
                        cmd.ExecuteNonQuery();
                    }
                }

                //Assert
                using (var cmd = new SqliteCommand(String.Format(selectSql1, "[IdColumn], [DateTimeColumn], [TextColumn], [EncryptedColumn]", tableName, "[IdColumn]"), db))
                using (var reader = new SqliteDataReader(cmd))
                {
                    while (reader.Read())
                    {
                        Assert.AreEqual(_sampleData[dataItemIndex].Item2, reader.GetString("TextColumn"));
                        Assert.AreNotEqual(_sampleData[dataItemIndex].Item2, reader.GetString("EncryptedColumn")); //The encrypted text should not match the unencrypted text
                        Assert.AreEqual(_sampleData[dataItemIndex].Item2, reader.GetDecrypted<string>("EncryptedColumn")); //The decrypted text should match the unencrypted text

                        dataItemIndex++;
                    }
                    Assert.AreEqual(3, dataItemIndex); //Should have read three records
                }
            }
        }

        [Test]
        public void WriteAndReadData_EncryptedColumnWithWrongPassword_CausesException()
        {
            //Arrange
            string rightPassword = "myTestPassword";
            string wrongPassword = "wrongPassword";
            var cryptEngine = new SampleAesCryptEngine(rightPassword);
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);
            Exception decryptException = null;

            using (var db = new SqliteConnection(dbPath, cryptEngine, true))
            {
                //Creating my table & adding encrypted column
                using (var cmd = new SqliteCommand(String.Format(tableCreateSql, tableName), db))
                {
                    db.SafeOpen();
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = String.Format(addColumnSql, tableName);
                    cmd.ExecuteNonQuery();
                }

                //Act
                using (var cmd = new SqliteCommand(String.Format(insertSql2, tableName), db))
                {
                    //Adding my records
                    foreach (Tuple<DateTime, string> dataItem in _sampleData)
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@date", dataItem.Item1);
                        cmd.Parameters.Add("@text", dataItem.Item2);
                        cmd.AddEncryptedParameter(new SqliteParameter("@encrypted", dataItem.Item2)); //Adds the string value (Tuple Item2) but with encryption
                        db.SafeOpen();
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            //Assert
            cryptEngine = new SampleAesCryptEngine(wrongPassword); //Testing first with the wrong password
            using (var db = new SqliteConnection(dbPath, cryptEngine, true))
            using (var cmd = new SqliteCommand(String.Format(selectSql2, "[IdColumn], [DateTimeColumn], [TextColumn], [EncryptedColumn]", tableName, "[IdColumn]"), db))
            using (var reader = new SqliteDataReader(cmd))
            {
                while (reader.Read())
                {
                    try
                    {
                        //This should fail, because the value that is read from the column cannot be properly decrypted without the correct password
                        Assert.AreEqual(_sampleData[0].Item2, reader.GetDecrypted<string>("EncryptedColumn"));
                    }
                    catch (Exception e)
                    {
                        decryptException = e;
                    }                    
                }
                Assert.IsNotNull(decryptException as CryptographicException);
            }

            //Just checking to make sure we can still decrypt the value with the correct password
            cryptEngine = new SampleAesCryptEngine(rightPassword); //Should work correctly with the right password
            using (var db = new SqliteConnection(dbPath, cryptEngine, true))
            using (var cmd = new SqliteCommand(String.Format(selectSql2, "[IdColumn], [DateTimeColumn], [TextColumn], [EncryptedColumn]", tableName, "[IdColumn]"), db))
            using (var reader = new SqliteDataReader(cmd))
            {
                while (reader.Read())
                {
                    Assert.AreEqual(_sampleData[0].Item2, reader.GetDecrypted<string>("EncryptedColumn")); //The decrypted text should match the unencrypted text
                }
            }
        }

        #endregion

        [OneTimeTearDown]
        public void Cleanup() => TempDatabaseFiles.CleanUp(this.GetType());
    }
}
