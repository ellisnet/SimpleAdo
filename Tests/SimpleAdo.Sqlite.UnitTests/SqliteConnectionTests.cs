using System;
using System.IO;
using NUnit.Framework;

namespace SimpleAdo.Sqlite.UnitTests
{
    [TestFixture]
    public class SqliteConnectionTests
    {
        #region Opening database tests

        [Test]
        public void OpenConnection_WithFilePathOnly_CreatesFile()
        {
            //Arrange
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);

            //Act
            using (var db = new SqliteConnection(dbPath, true))
            {
                db.Open();
            }

            //Assert
            Assert.IsTrue(File.Exists(dbPath));
        }

        [Test]
        public void OpenConnection_WithConnectionString_CreatesFile()
        {
            //Arrange
            var csb = new SqliteConnectionStringBuilder
            {
                DatabaseFilePath = TempDatabaseFiles.GetNewDatabasePath(this),
                BusyTimeout = 100,
                JournalMode = SqliteJournalModeEnum.Wal
            };

            //Act
            using (var db = new SqliteConnection(csb.ConnectionString))
            {
                db.Open();
            }

            //Assert
            Assert.IsTrue(File.Exists(csb.DatabaseFilePath));
        }

        [Test]
        public void OpenConnection_WithFailIfMissingOnNewDatabase_CausesException()
        {
            //Arrange
            var csb = new SqliteConnectionStringBuilder
            {
                DatabaseFilePath = TempDatabaseFiles.GetNewDatabasePath(this),
                FailIfMissing = true
            };
            Exception openException = null;

            //Act
            try
            {
                using (var db = new SqliteConnection(csb.ConnectionString))
                {
                    db.Open();
                }
            }
            catch (Exception e)
            {
                openException = e;
            }

            //Assert
            Assert.IsNotNull(openException as SqliteException);
            Assert.IsFalse(File.Exists(csb.DatabaseFilePath));
        }

        [Test]
        public void OpenConnection_WithOpenMethodCall_DatabaseIsOpen()
        {
            //Arrange
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);

            //Act
            using (var db = new SqliteConnection(dbPath, true))
            {
                db.Open();

                //Assert
                Assert.AreEqual(ConnectionState.Open, db.State);
            }
        }

        [Test]
        public void OpenConnection_WithSafeOpenMethodCall_DatabaseIsOpen()
        {
            //Arrange
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);

            //Act
            using (var db = new SqliteConnection(dbPath, true))
            {
                db.SafeOpen();

                //Assert
                Assert.AreEqual(ConnectionState.Open, db.State);
            }
        }

        [Test]
        public void OpenConnection_WithMissingOpenMethodCall_DatabaseIsClosed()
        {
            //Arrange
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);

            //Act
            using (var db = new SqliteConnection(dbPath, true))
            {
                //Not calling Open() for this test
                //db.Open();

                //Assert
                Assert.AreEqual(ConnectionState.Closed, db.State);
            }
        }

        [Test]
        public void OpenConnection_WithExplicitCloseMethodCall_DatabaseIsClosed()
        {
            //Arrange
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);

            //Act
            using (var db = new SqliteConnection(dbPath, true))
            {
                db.Open();

                db.Close();

                //Assert
                Assert.AreEqual(ConnectionState.Closed, db.State);
            }
        }

        [Test]
        public void OpenConnection_WithOpenCallWhenOpen_CausesException()
        {
            //Arrange
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);
            Exception openException = null;

            //Act
            try
            {
                using (var db = new SqliteConnection(dbPath, true))
                {
                    db.Open();

                    //Since the database should already be open, this should throw an exception
                    db.Open();
                }
            }
            catch (Exception e)
            {
                openException = e;
            }

            //Assert
            Assert.IsNotNull(openException as InvalidOperationException);
        }

        [Test]
        public void OpenConnection_WithSafeOpenCallWhenOpen_DatabaseIsOpen()
        {
            //Arrange
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);
            Exception openException = null;
            ConnectionState dbState = ConnectionState.Broken;

            //Act
            try
            {
                using (var db = new SqliteConnection(dbPath, true))
                {
                    db.Open();

                    //It should be safe to call SafeOpen() on an open database
                    db.SafeOpen();
                    dbState = db.State;
                }
            }
            catch (Exception e)
            {
                openException = e;
            }

            //Assert
            Assert.IsNull(openException);
            Assert.AreEqual(ConnectionState.Open, dbState);
        }

        [Test]
        public void OpenConnection_WithCloseCallWhenClosed_CausesException()
        {
            //Arrange
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);
            Exception openException = null;

            //Act
            try
            {
                using (var db = new SqliteConnection(dbPath, true))
                {
                    //Since the database is not open, this should throw an exception
                    db.Close();
                }
            }
            catch (Exception e)
            {
                openException = e;
            }

            //Assert
            Assert.IsNotNull(openException as InvalidOperationException);
        }

        [Test]
        public void OpenConnection_WithSafeCloseCallWhenClosed_DatabaseIsClosed()
        {
            //Arrange
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);
            Exception openException = null;
            ConnectionState dbState = ConnectionState.Broken;

            //Act
            try
            {
                using (var db = new SqliteConnection(dbPath, true))
                {
                    //It should be safe to call SafeClose() on a closed database
                    db.SafeClose();
                    dbState = db.State;
                }
            }
            catch (Exception e)
            {
                openException = e;
            }

            //Assert
            Assert.IsNull(openException);
            Assert.AreEqual(ConnectionState.Closed, dbState);
        }

        [Test]
        public void OpenConnection_WithOpenCloseSafeOpenCalls_DatabaseIsOpen()
        {
            //Arrange
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);
            Exception openException = null;
            ConnectionState dbStateAfterOpen = ConnectionState.Broken;
            ConnectionState dbStateAfterClose = ConnectionState.Broken;
            ConnectionState dbStateAfterSafeOpen = ConnectionState.Broken;

            //Act
            try
            {
                using (var db = new SqliteConnection(dbPath, true))
                {
                    db.Open();
                    dbStateAfterOpen = db.State;
                    db.Close();
                    dbStateAfterClose = db.State;
                    db.SafeOpen();
                    dbStateAfterSafeOpen = db.State;
                }
            }
            catch (Exception e)
            {
                openException = e;
            }

            //Assert
            Assert.IsNull(openException);
            Assert.AreEqual(ConnectionState.Open, dbStateAfterOpen);
            Assert.AreEqual(ConnectionState.Closed, dbStateAfterClose);
            Assert.AreEqual(ConnectionState.Open, dbStateAfterSafeOpen);
        }

        [Test]
        public void OpenConnection_CheckingStateOnDisposedConnection_CausesException()
        {
            //Arrange
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);
            Exception disposedException = null;
            // ReSharper disable once NotAccessedVariable
            ConnectionState dbStateBeforeOpen = ConnectionState.Broken;

            //Act
            var db = new SqliteConnection(dbPath, true);
            db.Dispose();
            try
            {
                // ReSharper disable once RedundantAssignment
                dbStateBeforeOpen = db.State;
            }
            catch (Exception e)
            {
                disposedException = e;
            }

            //Assert
            Assert.IsNotNull(disposedException as ObjectDisposedException);
        }

        [Test]
        public void OpenConnection_CallingOpenOnDisposedConnection_CausesException()
        {
            //Arrange
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);
            Exception disposedException = null;

            //Act
            var db = new SqliteConnection(dbPath, true);
            db.Dispose();
            try
            {
                db.Open();
            }
            catch (Exception e)
            {
                disposedException = e;
            }

            //Assert
            Assert.IsNotNull(disposedException as ObjectDisposedException);
        }

        [Test]
        public void OpenConnection_CallingSafeOpenOnDisposedConnection_CausesException()
        {
            //Arrange
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);
            Exception disposedException = null;

            //Act
            var db = new SqliteConnection(dbPath, true);
            db.Dispose();
            try
            {
                db.SafeOpen();
            }
            catch (Exception e)
            {
                disposedException = e;
            }

            //Assert
            Assert.IsNotNull(disposedException as ObjectDisposedException);
        }

        [Test]
        public void OpenConnection_CallingCloseOnDisposedConnection_CausesException()
        {
            //Arrange
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);
            Exception disposedException = null;

            //Act
            var db = new SqliteConnection(dbPath, true);
            db.Dispose();
            try
            {
                db.Close();
            }
            catch (Exception e)
            {
                disposedException = e;
            }

            //Assert
            Assert.IsNotNull(disposedException as ObjectDisposedException);
        }

        [Test]
        public void OpenConnection_CallingSafeCloseOnDisposedConnection_CausesNoException()
        {
            //Arrange
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);
            Exception disposedException = null;

            //Act
            var db = new SqliteConnection(dbPath, true);
            db.Dispose();
            try
            {
                db.SafeClose();
            }
            catch (Exception e)
            {
                disposedException = e;
            }

            //Assert
            Assert.IsNull(disposedException);
        }

        [Test]
        public void OpenConnection_MultipleConnectionsWithTheSameDatabaseFilePath_CausesException()
        {
            //Arrange
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);
            Exception secondConnectionException = null;
            Exception thirdConnectionException = null;
            Exception fourthConnectionException = null;

            //Calling ShutdownSqlite3Provider() should not be necessary, but it is the only way
            // to dispose of the default database; which is necessary here because we are running
            // multiple tests that need to set the default database.
            SqliteConnection.ShutdownSqlite3Provider();

            //Act
            using (var db = new SqliteConnection(dbPath, true, SqliteOpenFlags.ReadWrite, false)) //false = not default database
            {
                db.Open();

                try
                {
                    //This should fail, because there cannot be multiple simultaneous connections to the same database file
                    using (var db2 = new SqliteConnection(dbPath, true)) { db2.Open(); }
                }
                catch (Exception e)
                {
                    secondConnectionException = e;
                }

                try
                {
                    //This should fail, because there cannot be multiple simultaneous connections to the same database file
                    using (var db3 = new SqliteConnection(dbPath, true)) { db3.Open(); }
                }
                catch (Exception e)
                {
                    thirdConnectionException = e;
                }
            }

            try
            {
                //This should be fine, because the other connection has been disposed.
                using (var db4 = new SqliteConnection(dbPath, true)) { db4.Open(); }
            }
            catch (Exception e)
            {
                fourthConnectionException = e;
            }

            //Assert
            Assert.IsTrue(File.Exists(dbPath));
            Assert.IsNotNull(secondConnectionException as InvalidOperationException);
            Assert.IsNotNull(thirdConnectionException as InvalidOperationException);
            Assert.IsNull(fourthConnectionException);
        }

        [Test]
        public void OpenConnection_MultipleConnectionsWithTheDefaultDatabase_IsSuccessful()
        {
            //Arrange
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);
            Exception secondConnectionException = null;
            Exception fourthConnectionException = null;
            Exception thirdConnectionException = null;

            //Calling ShutdownSqlite3Provider() should not be necessary, but it is the only way
            // to dispose of the default database; which is necessary here because we are running
            // multiple tests that need to set the default database.
            SqliteConnection.ShutdownSqlite3Provider();

            //Act
            using (var db = new SqliteConnection(dbPath, true, SqliteOpenFlags.ReadWrite, true)) //true (as last parameter) = make this default database
            {
                db.Open();

                try
                {
                    //Using the default constructor for SqliteConnection causes it to use the default database, if that has been set
                    // - this should be successful - i.e. not throw an exception
                    using (var db2 = new SqliteConnection()) { db2.Open(); }
                }
                catch (Exception e)
                {
                    secondConnectionException = e;
                }

                try
                {
                    //Opening a new database pointing to the same file, should still fail
                    using (var db3 = new SqliteConnection(dbPath, true)) { db3.Open(); }
                }
                catch (Exception e)
                {
                    thirdConnectionException = e;
                }
            }

            try
            {
                //Using the default constructor for SqliteConnection causes it to use the default database, if that has been set
                // - this should be successful even though the original connection was disposed, because default databases do not get disposed
                using (var db4 = new SqliteConnection()) { db4.Open(); }
            }
            catch (Exception e)
            {
                fourthConnectionException = e;
            }

            //Assert
            Assert.IsTrue(File.Exists(dbPath));
            Assert.IsNull(secondConnectionException);
            Assert.IsNotNull(thirdConnectionException as InvalidOperationException);
            Assert.IsNull(fourthConnectionException);
        }

        [Test]
        public void OpenConnection_MultipleDefaultDatabases_CausesException()
        {
            //Arrange
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);
            string dbPath2 = TempDatabaseFiles.GetNewDatabasePath(this);
            Exception secondConnectionException = null;

            //Calling ShutdownSqlite3Provider() should not be necessary, but it is the only way
            // to dispose of the default database; which is necessary here because we are running
            // multiple tests that need to set the default database.
            SqliteConnection.ShutdownSqlite3Provider();

            //Act
            using (var db = new SqliteConnection(dbPath, true, SqliteOpenFlags.ReadWrite, true)) //true (as last parameter) = make this default database
            {
                db.Open();
            }

            try
            {
                //This should fail, because it is not possible to set two databases to be the default
                using (var db2 = new SqliteConnection(dbPath2, true, SqliteOpenFlags.ReadWrite, true)) //true (as last parameter) = make this default database
                {
                    db2.Open();
                }
            }
            catch (Exception e)
            {
                secondConnectionException = e;
            }

            //Assert
            Assert.IsTrue(File.Exists(dbPath));
            Assert.IsFalse(File.Exists(dbPath2));
            Assert.IsNotNull(secondConnectionException as InvalidOperationException);
        }

        [Test]
        public void OpenConnection_OpenDefaultDatabaseWithMissingDefaultDatabase_CausesException()
        {
            //Arrange
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);
            Exception secondConnectionException = null;

            //Calling ShutdownSqlite3Provider() should not be necessary, but it is the only way
            // to dispose of the default database; which is necessary here because we are running
            // multiple tests that need to set the default database.
            SqliteConnection.ShutdownSqlite3Provider();

            //Act
            using (var db = new SqliteConnection(dbPath, true, SqliteOpenFlags.ReadWrite, false)) //false = not default database
            {
                db.Open();

                try
                {
                    //This should fail, because the default database has not been set.
                    using (var db2 = new SqliteConnection()) { db2.Open(); }
                }
                catch (Exception e)
                {
                    secondConnectionException = e;
                }
            }

            //Assert
            Assert.IsTrue(File.Exists(dbPath));
            Assert.IsNotNull(secondConnectionException as InvalidOperationException);
        }

        #endregion

        #region Schema version tests

        [Test]
        public void SchemaVersion_CheckVersionOnNewDatabase_ReturnsZero()
        {
            //Arrange
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);
            long schemaVersion;

            //Act
            using (var db = new SqliteConnection(dbPath, true))
            {
                schemaVersion = db.GetDatabaseSchemaVersion();
            }

            //Assert
            Assert.AreEqual(0, schemaVersion);
        }

        [Test]
        public void SchemaVersion_SetVersionOnNewDatabase_IsSuccessful()
        {
            //Arrange
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);
            long schemaVersionBeforeSet;
            long intendedSchemaVersion = 23;
            long schemaVersionAfterSet;

            //Act
            using (var db = new SqliteConnection(dbPath, true))
            {
                schemaVersionBeforeSet = db.GetDatabaseSchemaVersion();
                db.SetDatabaseSchemaVersion(intendedSchemaVersion);
            }

            using (var db = new SqliteConnection(dbPath, true))
            {
                schemaVersionAfterSet = db.GetDatabaseSchemaVersion();
            }

            //Assert
            Assert.AreEqual(0, schemaVersionBeforeSet);
            Assert.AreEqual(intendedSchemaVersion, schemaVersionAfterSet);
        }

        [Test]
        public void SchemaVersion_CheckAndSetVersion_ManagesConnectionStateProperly()
        {
            //Arrange
            string dbPath = TempDatabaseFiles.GetNewDatabasePath(this);
            long schemaVersionBeforeSet;
            long intendedSchemaVersion = 23;
            long schemaVersionAfterSet;
            ConnectionState beforeGetClosed;
            ConnectionState afterGetClosed;
            ConnectionState beforeGetOpen;
            ConnectionState afterGetOpen;
            ConnectionState beforeSetClosed;
            ConnectionState afterSetClosed;
            ConnectionState beforeSetOpen;
            ConnectionState afterSetOpen;

            //Act
            using (var db = new SqliteConnection(dbPath, true))
            {
                schemaVersionBeforeSet = db.GetDatabaseSchemaVersion();
                db.SafeClose(); //Want the database closed so we can make sure that setting
                                //  the schema doesn't require the connection to be open.
                beforeSetClosed = db.State;
                db.SetDatabaseSchemaVersion(intendedSchemaVersion);
                afterSetClosed = db.State;
                db.SafeOpen();  //Should now be open
                beforeSetOpen = db.State;
                db.SetDatabaseSchemaVersion(intendedSchemaVersion);
                afterSetOpen = db.State;
            }

            using (var db = new SqliteConnection(dbPath, true))
            {
                beforeGetClosed = db.State;
                schemaVersionAfterSet = db.GetDatabaseSchemaVersion();
                afterGetClosed = db.State;
                db.SafeOpen();
                beforeGetOpen = db.State;
                schemaVersionAfterSet = db.GetDatabaseSchemaVersion();
                afterGetOpen = db.State;
            }

            //Assert
            Assert.AreEqual(0, schemaVersionBeforeSet);
            Assert.AreEqual(intendedSchemaVersion, schemaVersionAfterSet);

            Assert.AreEqual(ConnectionState.Closed, beforeSetClosed); //Should be closed because just closed
            Assert.AreEqual(ConnectionState.Closed, afterSetClosed); //Should be closed if previously closed
            Assert.AreEqual(ConnectionState.Open, beforeSetOpen); //Should be open because just opened
            Assert.AreEqual(ConnectionState.Open, afterSetOpen); //Should be open if previously open

            Assert.AreEqual(ConnectionState.Closed, beforeGetClosed); //Should be closed before activity, or explicit Open()
            Assert.AreEqual(ConnectionState.Closed, afterGetClosed); //Should be closed if previously closed
            Assert.AreEqual(ConnectionState.Open, beforeGetOpen); //Should be open because just opened
            Assert.AreEqual(ConnectionState.Open, afterGetOpen); //Should be open if previously open
        }

        #endregion

        [OneTimeTearDown]
        public void Cleanup() => TempDatabaseFiles.CleanUp(this.GetType());
    }
}
