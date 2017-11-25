using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SimpleAdo.Sqlite.UnitTests
{
    [TestFixture]
    public class SqliteCommandTests
    {
        //SQL statements used for testing
        private readonly string tableName = "TestTable";
        private readonly string tableCreateSql1 = "CREATE TABLE IF NOT EXISTS {0} (IdColumn INTEGER PRIMARY KEY AUTOINCREMENT, DateTimeColumn DATETIME NOT NULL, TextColumn TEXT);";
        private readonly string tableCreateSql2 = "CREATE TABLE IF NOT EXISTS {0} (IdColumn INTEGER PRIMARY KEY AUTOINCREMENT, DateTimeOffsetColumn DATETIMEOFFSET NOT NULL, TextColumn TEXT);";
        private readonly string tableDropSql = "DROP TABLE {0};";
        private readonly string addColumnSql1 = "ALTER TABLE {0} ADD COLUMN EncryptedColumn ENCRYPTED;";
        private readonly string addColumnSql2 = "ALTER TABLE {0} ADD COLUMN DateTimeOffsetColumn DATETIMEOFFSET;";
        private readonly string insertSql = "INSERT INTO {0} ({1}, TextColumn) VALUES (@date, @text);";
        private readonly string selectSql1 = "SELECT [{0}] FROM {1} ORDER BY [{2}] LIMIT 1;";
        private readonly string selectSql2 = "SELECT [{0}] FROM {1} WHERE [{2}] = {3};";

        #region Table schema tests

        [Test]
        public void TableSchema_CreateTableWithOpen_IsSuccessful()
        {
            //Arrange
            bool tableExists;
            bool fakeTableExists;
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);

            //Act
            using (var db = new SqliteConnection(dbPath, true))
            {
                using (var cmd = new SqliteCommand(String.Format(tableCreateSql1, tableName), db))
                {
                    db.Open();
                    cmd.ExecuteNonQuery();
                    tableExists = db.TableExists(tableName);
                    //Testing calling TableExists() on a table that doesn't exist
                    fakeTableExists = db.TableExists("fake.table.name_123456");
                }
            }

            using (var db = new SqliteConnection(dbPath, true))
            {
                //Make sure table still exists
                tableExists &= db.TableExists(tableName);
            }

            //Assert
            Assert.IsTrue(tableExists);
            Assert.IsFalse(fakeTableExists);
        }

        [Test]
        public void TableSchema_CreateTableThenDrop_IsSuccessful()
        {
            //Arrange
            bool tableExistsBeforeDrop;
            bool tableExistsAfterDrop;
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);

            //Act
            using (var db = new SqliteConnection(dbPath, true))
            {
                using (var cmd = new SqliteCommand(String.Format(tableCreateSql1, tableName), db))
                {
                    db.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            using (var db = new SqliteConnection(dbPath, true))
            {
                using (var cmd = new SqliteCommand(String.Format(tableDropSql, tableName), db))
                {
                    db.Open();
                    tableExistsBeforeDrop = db.TableExists(tableName);
                    cmd.ExecuteNonQuery();
                }
            }

            using (var db = new SqliteConnection(dbPath, true))
            {
                tableExistsAfterDrop = db.TableExists(tableName);
            }

            //Assert
            Assert.IsTrue(tableExistsBeforeDrop);
            Assert.IsFalse(tableExistsAfterDrop);
        }

        [Test]
        public void TableSchema_CreateTableWithoutOpen_CausesException()
        {
            //Arrange
            bool tableExists = false;
            Exception openException = null;

            //Act
            try
            {
                using (var db = new SqliteConnection(TempDatabaseFiles.GetNewDatabasePath(this), true))
                {
                    using (var cmd = new SqliteCommand(String.Format(tableCreateSql1, tableName), db))
                    {
                        //db.Open(); - testing executing query without opening first
                        cmd.ExecuteNonQuery();
                        //The following line should never be hit
                        tableExists = db.TableExists(tableName);
                    }
                }
            }
            catch (Exception e)
            {
                openException = e;
            }

            //Assert
            Assert.IsFalse(tableExists);
            Assert.IsNotNull(openException as InvalidOperationException);
        }

        [Test]
        public void TableSchema_GetTableColumnList_ReturnsCorrectColumnList()
        {
            //Arrange
            IColumnInfo[] columnList = null;
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);

            //Act
            using (var db = new SqliteConnection(dbPath, true))
            {
                using (var cmd = new SqliteCommand(String.Format(tableCreateSql1, tableName), db))
                {
                    db.Open();
                    cmd.ExecuteNonQuery();
                    //With the table created, we should be able to get a list of columns
                    columnList = db.GetTableColumnList(tableName);
                }
            }

            //Assert
            Assert.IsNotNull(columnList);
            Assert.AreEqual(3, columnList.Length); //Should be three columns
            Assert.AreEqual("IdColumn", columnList[0].Name);
            Assert.IsTrue(columnList[0].IsPrimaryKey);
            Assert.AreEqual(DbType.Int64, columnList[0].DataType);
            Assert.AreEqual("DateTimeColumn", columnList[1].Name);
            Assert.IsFalse(columnList[1].IsPrimaryKey);
            Assert.IsTrue(columnList[1].IsNotNull);
            Assert.AreEqual(DbType.DateTime, columnList[1].DataType);
            Assert.AreEqual("TextColumn", columnList[2].Name);
            Assert.IsFalse(columnList[2].IsPrimaryKey);
            Assert.AreEqual(DbType.String, columnList[2].DataType);
        }

        [Test]
        public void TableSchema_AddColumns_IsSuccessful()
        {
            //Arrange
            IColumnInfo[] initialColumnList = null;
            IColumnInfo[] finalColumnList = null;
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);

            //Act
            using (var db = new SqliteConnection(dbPath, true))
            {
                using (var cmd = new SqliteCommand(String.Format(tableCreateSql1, tableName), db))
                {
                    db.Open();
                    cmd.ExecuteNonQuery();
                    initialColumnList = db.GetTableColumnList(tableName);
                    cmd.CommandText = String.Format(addColumnSql1, tableName);
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = String.Format(addColumnSql2, tableName);
                    cmd.ExecuteNonQuery();
                    finalColumnList = db.GetTableColumnList(tableName);
                }
            }

            //Assert
            Assert.IsNotNull(initialColumnList);
            Assert.AreEqual(3, initialColumnList.Length); //Should be three columns
            Assert.IsNotNull(finalColumnList);
            Assert.AreEqual(5, finalColumnList.Length); //Should be five columns, with the two that were added
            Assert.AreEqual("EncryptedColumn", finalColumnList[3].Name);
            Assert.AreEqual(DbType.Encrypted, finalColumnList[3].DataType);
            Assert.AreEqual("DateTimeOffsetColumn", finalColumnList[4].Name);
            Assert.AreEqual(DbType.DateTimeOffset, finalColumnList[4].DataType);
        }

        [Test]
        public void TableSchema_AddColumnsInCorrectMode_IsSuccessful()
        {
            //Arrange
            IColumnInfo[] columnList = null;
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);
            bool beginMaintenanceModeSuccess;
            bool endMaintenanceModeSuccess;
            Exception openException = null;

            //Act
            using (var db = new SqliteConnection(dbPath, true))
            {
                using (var cmd = new SqliteCommand(String.Format(tableCreateSql1, tableName), db))
                {
                    db.Open();
                    cmd.ExecuteNonQuery();
                }

                //Adding the first column using Maintenance Mode
                using (var cmd = new SqliteCommand(String.Format(addColumnSql1, tableName), db, true)) //true = maintenance mode
                {
                    beginMaintenanceModeSuccess = db.BeginDatabaseMaintenanceMode();
                    try
                    {
                        //Opening the database should fail, because BeginDatabaseMaintenanceMode() already opens it
                        db.Open();
                    }
                    catch (Exception e)
                    {
                        openException = e;
                    }
                    db.SafeOpen();  //Safe open should be fine - because it is "safe"
                    cmd.ExecuteNonQuery();
                    //GetTableColumnList() should work correctly whether the database is in Maintenance Mode or not
                    columnList = db.GetTableColumnList(tableName);
                    endMaintenanceModeSuccess = db.EndDatabaseMaintenanceMode();
                }

                //Adding the second column NOT using Maintenance Mode
                using (var cmd = new SqliteCommand(String.Format(addColumnSql2, tableName), db, false)) //false = not maintenance mode
                {
                    db.Open();
                    cmd.ExecuteNonQuery();
                    //GetTableColumnList() should work correctly whether the database is in Maintenance Mode or not
                    columnList = db.GetTableColumnList(tableName);
                }
            }

            //Assert
            Assert.IsNotNull(columnList);
            Assert.AreEqual(5, columnList.Length); //Should be five columns, with the two that were added
            Assert.IsTrue(beginMaintenanceModeSuccess);
            Assert.IsTrue(endMaintenanceModeSuccess);
            Assert.IsNotNull(openException as InvalidOperationException);
        }

        [Test]
        public void TableSchema_AddColumnsInWrongMode_CausesException()
        {
            //Arrange
            IColumnInfo[] columnList = null;
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);
            bool beginMaintenanceModeSuccess;
            bool endMaintenanceModeSuccess;
            Exception maintenanceModeException = null;
            Exception notMaintenanceModeException = null;

            //Act
            using (var db = new SqliteConnection(dbPath, true))
            {
                using (var cmd = new SqliteCommand(String.Format(tableCreateSql1, tableName), db))
                {
                    db.Open();
                    cmd.ExecuteNonQuery();
                }

                //Adding the first column using Maintenance Mode
                using (var cmd = new SqliteCommand(String.Format(addColumnSql1, tableName), db, true)) //true = maintenance mode
                {
                    db.SafeOpen();
                    try
                    {
                        //Oops. This should fail, because I am executing the command before I set maintenance mode (below)
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        maintenanceModeException = e;
                    }
                    beginMaintenanceModeSuccess = db.BeginDatabaseMaintenanceMode();

                    //GetTableColumnList() should work correctly whether the database is in Maintenance Mode or not
                    columnList = db.GetTableColumnList(tableName);
                }

                //Adding the second column NOT using Maintenance Mode
                using (var cmd = new SqliteCommand(String.Format(addColumnSql2, tableName), db, false)) //false = not maintenance mode
                {
                    db.SafeOpen();
                    try
                    {
                        //Oops. This should fail, because I am executing the command before I turned off maintenance mode (below)
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        notMaintenanceModeException = e;
                    }                    
                    endMaintenanceModeSuccess = db.EndDatabaseMaintenanceMode();

                    //GetTableColumnList() should work correctly whether the database is in Maintenance Mode or not
                    columnList = db.GetTableColumnList(tableName);
                }
            }

            //Assert
            Assert.IsNotNull(columnList);
            Assert.AreEqual(3, columnList.Length); //Should be three columns, because the add column sql should have failed
            Assert.IsTrue(beginMaintenanceModeSuccess);
            Assert.IsTrue(endMaintenanceModeSuccess);
            Assert.IsNotNull(maintenanceModeException as InvalidOperationException);
            Assert.IsNotNull(notMaintenanceModeException as InvalidOperationException);
        }

        #endregion

        #region Insert and select tests

        [Test]
        public void InsertSelect_DateTimeColumnStoredUsingTicks_MatchesInsertedValue()
        {
            //Arrange
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);
            DateTime dateTimeColumnInsertValue = DateTime.Now;
            string textColumnInsertValue = "Hello SQLite!";
            DateTime dateTimeColumnSelectValue;
            string textColumnSelectValue;
            SqliteConnectionStringBuilder csb;

            //Act
            using (var db = new SqliteConnection(dbPath, true))
            {
                csb = new SqliteConnectionStringBuilder
                {
                    ConnectionString = db.ConnectionString
                };
                using (var cmd = new SqliteCommand(String.Format(tableCreateSql1, tableName), db))
                {
                    db.Open();
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = String.Format(insertSql, tableName, "DateTimeColumn");
                    cmd.Parameters.Add(new SqliteParameter("@date", dateTimeColumnInsertValue));
                    cmd.Parameters.Add(new SqliteParameter("text", textColumnInsertValue)); //prefixing the parameter name with @ is not required
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear(); //This isn't really necessary, because the parameters added above are ignored/not present in the following CommandText values
                    cmd.CommandText = String.Format(selectSql1, "DateTimeColumn", tableName, "DateTimeColumn");
                    dateTimeColumnSelectValue = (DateTime)cmd.ExecuteScalar();
                    cmd.CommandText = String.Format(selectSql1, "TextColumn", tableName, "DateTimeColumn");
                    textColumnSelectValue = (string)cmd.ExecuteScalar();
                }
            }

            //Assert
            Assert.IsTrue(csb.StoreDateTimeAsTicks);
            Assert.AreEqual(dateTimeColumnInsertValue, dateTimeColumnSelectValue);
            Assert.AreEqual(textColumnInsertValue, textColumnSelectValue);
        }

        [Test]
        public void InsertSelect_DateTimeColumnStoredUsingStrings_MatchesInsertedValue()
        {
            //Arrange
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);
            DateTime dateTimeColumnInsertValue = DateTime.Now;
            string textColumnInsertValue = "Hello SQLite!";
            DateTime dateTimeColumnSelectValue;
            string textColumnSelectValue;
            var csb = new SqliteConnectionStringBuilder
            {
                DatabaseFilePath = dbPath,
                JournalMode = SqliteJournalModeEnum.Default,
                StoreDateTimeAsTicks = false
            };

            //Act
            using (var db = new SqliteConnection(csb.ConnectionString))
            {
                using (var cmd = new SqliteCommand(String.Format(tableCreateSql1, tableName), db))
                {
                    db.Open();
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = String.Format(insertSql, tableName, "DateTimeColumn");
                    cmd.Parameters.Add(new SqliteParameter("@date", dateTimeColumnInsertValue));
                    cmd.Parameters.Add(new SqliteParameter("text", textColumnInsertValue)); //prefixing the parameter name with @ is not required
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear(); //This isn't really necessary, because the parameters added above are ignored/not present in the following CommandText values
                    cmd.CommandText = String.Format(selectSql1, "DateTimeColumn", tableName, "DateTimeColumn");
                    dateTimeColumnSelectValue = (DateTime)cmd.ExecuteScalar();
                    cmd.CommandText = String.Format(selectSql1, "TextColumn", tableName, "DateTimeColumn");
                    textColumnSelectValue = (string)cmd.ExecuteScalar();
                }
            }

            //Assert
            Assert.IsFalse(csb.StoreDateTimeAsTicks);
            Assert.AreEqual(dateTimeColumnInsertValue, dateTimeColumnSelectValue);
            Assert.AreEqual(textColumnInsertValue, textColumnSelectValue);
        }

        [Test]
        public void InsertSelect_DateTimeOffsetColumn_MatchesInsertedValue()
        {
            //Arrange
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);
            DateTimeOffset dateTimeColumnInsertValue1 = DateTimeOffset.Now;
            DateTimeOffset dateTimeColumnSelectValue1;
            DateTimeOffset dateTimeColumnInsertValue2 = DateTimeOffset.UtcNow.AddHours(2);
            DateTimeOffset dateTimeColumnSelectValue2;
            string textColumnInsertValue1 = "Hello SQLite!";
            string textColumnSelectValue1;
            string textColumnInsertValue2 = "Goodbye SQLite!";
            string textColumnSelectValue2;
            long recordId1;
            long recordId2;
            Exception duplicateParameterException = null;

            //Act
            using (var db = new SqliteConnection(dbPath, true))
            {
                using (var cmd = new SqliteCommand(String.Format(tableCreateSql2, tableName), db))
                {
                    db.Open();
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = String.Format(insertSql, tableName, "DateTimeOffsetColumn");
                    cmd.Parameters.Add(new SqliteParameter("@date", dateTimeColumnInsertValue1));
                    cmd.Parameters.Add(new SqliteParameter("text", textColumnInsertValue1)); //prefixing the parameter name with @ is not required
                    recordId1 = cmd.ExecuteReturnRowId();
                    try
                    {
                        //This should fail, because adding multiple parameters with the same name is not allowed; we need to clear parameters first.
                        cmd.Parameters.Add(new SqliteParameter("@date", dateTimeColumnInsertValue2));
                        cmd.Parameters.Add(new SqliteParameter("text", textColumnInsertValue2)); //prefixing the parameter name with @ is not required
                        recordId2 = cmd.ExecuteReturnRowId();
                    }
                    catch (Exception e)
                    {
                        duplicateParameterException = e;
                    }
                    cmd.Parameters.Clear(); //allows parameters to be re-added
                    cmd.Parameters.Add(new SqliteParameter("@date", dateTimeColumnInsertValue2));
                    cmd.Parameters.Add(new SqliteParameter("@text", textColumnInsertValue2));
                    recordId2 = cmd.ExecuteReturnRowId();
                    cmd.Parameters.Clear(); //This isn't really necessary, because the parameters added above are ignored/not present in the following CommandText values
                    cmd.CommandText = String.Format(selectSql2, "DateTimeOffsetColumn", tableName, "IdColumn", recordId1);
                    dateTimeColumnSelectValue1 = (DateTimeOffset)cmd.ExecuteScalar();
                    cmd.CommandText = String.Format(selectSql2, "TextColumn", tableName, "IdColumn", recordId1);
                    textColumnSelectValue1 = (string)cmd.ExecuteScalar();
                    cmd.CommandText = String.Format(selectSql2, "DateTimeOffsetColumn", tableName, "IdColumn", recordId2);
                    dateTimeColumnSelectValue2 = (DateTimeOffset)cmd.ExecuteScalar();
                    cmd.CommandText = String.Format(selectSql2, "TextColumn", tableName, "IdColumn", recordId2);
                    textColumnSelectValue2 = (string)cmd.ExecuteScalar();
                }
            }

            //Assert
            Assert.IsNotNull(duplicateParameterException as InvalidOperationException);
            Assert.AreEqual(1, recordId1); //First inserted record should have IdColumn of 1
            Assert.AreEqual(2, recordId2); //Second inserted record should have IdColumn of 2
            Assert.AreEqual(dateTimeColumnInsertValue1, dateTimeColumnSelectValue1);
            Assert.AreEqual(textColumnInsertValue1, textColumnSelectValue1);
            Assert.AreEqual(dateTimeColumnInsertValue2, dateTimeColumnSelectValue2);
            Assert.AreEqual(textColumnInsertValue2, textColumnSelectValue2);
        }

        #endregion

        [OneTimeTearDown]
        public void Cleanup() => TempDatabaseFiles.CleanUp(this.GetType());
    }
}
