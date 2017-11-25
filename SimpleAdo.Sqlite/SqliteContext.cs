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

using DbProvider = SQLitePCL;
using DbProviderOperations = SQLitePCL.raw;

namespace SimpleAdo.Sqlite
{
    /// <summary> A sqlite context. </summary>
    internal class SqliteContext
    {
        /// <summary> The step locker. </summary>
        private readonly object _stepLocker = new object();

        /// <summary> Sqlite 3 backup initialize. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when one or more arguments are outside
        ///     the required range. </exception>
        /// <exception cref="InvalidOperationException"> Thrown when the requested operation is invalid. </exception>
        /// <exception cref="SqliteException"> Thrown when a Sqlite error condition occurs. </exception>
        /// <param name="destinationDb"> Destination database. </param>
        /// <param name="destinationName"> Name of the destination. </param>
        /// <param name="sourceDb"> Source database. </param>
        /// <param name="sourceName"> Name of the source. </param>
        /// <returns> A SqliteBackupHandle. </returns>
        internal SqliteBackupHandle sqlite3_backup_init(
            SqliteDatabaseHandle destinationDb, 
            string destinationName,
            SqliteDatabaseHandle sourceDb, 
            string sourceName)
        {
            if (destinationDb == null) { throw new ArgumentNullException(nameof(destinationDb));}
            if (sourceDb == null) { throw new ArgumentNullException(nameof(sourceDb));}
            if (String.IsNullOrWhiteSpace(destinationName)) { throw new ArgumentOutOfRangeException(nameof(destinationName));}
            if (String.IsNullOrWhiteSpace(sourceName)) { throw new ArgumentOutOfRangeException(nameof(sourceName));}

            if (!destinationDb.IsDatabaseInMaintenanceMode)
            {
                throw new InvalidOperationException("The destination database is not currently in maintenance mode.");
            }

            if (!sourceDb.IsDatabaseInMaintenanceMode)
            {
                throw new InvalidOperationException("The source database is not currently in maintenance mode.");
            }

            DbProvider.sqlite3_backup result = DbProviderOperations.sqlite3_backup_init(destinationDb.MaintenanceDb, destinationName,
                sourceDb.MaintenanceDb, sourceName);

            if (result == null)
            {
                throw new SqliteException("The resulting backup handle was NULL", SqliteResultCode.Empty);
            }

            return new SqliteBackupHandle(destinationDb, destinationName, sourceDb, sourceName, result);
        }

        /// <summary> Sqlite 3 backup step. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="backup"> The backup. </param>
        /// <param name="nPage"> The page. </param>
        /// <returns> A SqliteResultCode. </returns>
        internal SqliteResultCode sqlite3_backup_step(SqliteBackupHandle backup, int nPage)
        {
            if (backup == null) { throw new ArgumentNullException(nameof(backup));}
            backup.CheckMaintenanceMode();
            return (SqliteResultCode)DbProviderOperations.sqlite3_backup_step(backup.Backup, nPage);
        }

        /// <summary> Sqlite 3 backup finish. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="backup"> The backup. </param>
        /// <returns> A SqliteResultCode. </returns>
        internal SqliteResultCode sqlite3_backup_finish(SqliteBackupHandle backup)
        {
            if (backup == null) { throw new ArgumentNullException(nameof(backup)); }
            backup.CheckMaintenanceMode();
            return (SqliteResultCode)DbProviderOperations.sqlite3_backup_finish(backup.Backup);
        }

        /// <summary> Sqlite 3 backup remaining. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="backup"> The backup. </param>
        /// <returns> An int. </returns>
        internal int sqlite3_backup_remaining(SqliteBackupHandle backup)
        {
            if (backup == null) { throw new ArgumentNullException(nameof(backup)); }
            backup.CheckMaintenanceMode();
            return DbProviderOperations.sqlite3_backup_pagecount(backup.Backup);
        }

        /// <summary> Sqlite 3 backup pagecount. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="backup"> The backup. </param>
        /// <returns> An int. </returns>
        internal int sqlite3_backup_pagecount(SqliteBackupHandle backup)
        {
            if (backup == null) { throw new ArgumentNullException(nameof(backup)); }
            backup.CheckMaintenanceMode();
            return DbProviderOperations.sqlite3_backup_remaining(backup.Backup);
        }

        /// <summary> Sqlite 3 bind BLOB. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="stmt"> The statement. </param>
        /// <param name="index"> Zero-based index of the. </param>
        /// <param name="value"> The value. </param>
        /// <returns> A SqliteResultCode. </returns>
        internal SqliteResultCode sqlite3_bind_blob(SqliteStatementHandle stmt, int index, byte[] value)
        {
            if (stmt == null) { throw new ArgumentNullException(nameof(stmt));}
            stmt.CheckMaintenanceMode();
            return (SqliteResultCode)DbProviderOperations.sqlite3_bind_blob(stmt.Statement, index, value);
        }

        /// <summary> Sqlite 3 bind double. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="stmt"> The statement. </param>
        /// <param name="index"> Zero-based index of the. </param>
        /// <param name="value"> The value. </param>
        /// <returns> A SqliteResultCode. </returns>
        internal SqliteResultCode sqlite3_bind_double(SqliteStatementHandle stmt, int index, double value)
        {
            if (stmt == null) { throw new ArgumentNullException(nameof(stmt)); }
            stmt.CheckMaintenanceMode();
            return (SqliteResultCode)DbProviderOperations.sqlite3_bind_double(stmt.Statement, index, value);
        }

        /// <summary> Sqlite 3 bind int. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="stmt"> The statement. </param>
        /// <param name="index"> Zero-based index of the. </param>
        /// <param name="value"> The value. </param>
        /// <returns> A SqliteResultCode. </returns>
        internal SqliteResultCode sqlite3_bind_int(SqliteStatementHandle stmt, int index, int value)
        {
            if (stmt == null) { throw new ArgumentNullException(nameof(stmt)); }
            stmt.CheckMaintenanceMode();
            return (SqliteResultCode)DbProviderOperations.sqlite3_bind_int(stmt.Statement, index, value);
        }

        /// <summary> Sqlite 3 bind int 64. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="stmt"> The statement. </param>
        /// <param name="index"> Zero-based index of the. </param>
        /// <param name="value"> The value. </param>
        /// <returns> A SqliteResultCode. </returns>
        internal SqliteResultCode sqlite3_bind_int64(SqliteStatementHandle stmt, int index, long value)
        {
            if (stmt == null) { throw new ArgumentNullException(nameof(stmt)); }
            stmt.CheckMaintenanceMode();
            return (SqliteResultCode)DbProviderOperations.sqlite3_bind_int64(stmt.Statement, index, value);
        }

        /// <summary> Sqlite 3 bind null. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="stmt"> The statement. </param>
        /// <param name="index"> Zero-based index of the. </param>
        /// <returns> A SqliteResultCode. </returns>
        internal SqliteResultCode sqlite3_bind_null(SqliteStatementHandle stmt, int index)
        {
            if (stmt == null) { throw new ArgumentNullException(nameof(stmt)); }
            stmt.CheckMaintenanceMode();
            return (SqliteResultCode)DbProviderOperations.sqlite3_bind_null(stmt.Statement, index);
        }

        /// <summary> Sqlite 3 bind parameter index. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="stmt"> The statement. </param>
        /// <param name="name"> The name. </param>
        /// <returns> An int. </returns>
        internal int sqlite3_bind_parameter_index(SqliteStatementHandle stmt, string name)
        {
            if (stmt == null) { throw new ArgumentNullException(nameof(stmt)); }
            stmt.CheckMaintenanceMode();
            return DbProviderOperations.sqlite3_bind_parameter_index(stmt.Statement, name);
        }

        /// <summary> Sqlite 3 bind text. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="stmt"> The statement. </param>
        /// <param name="index"> Zero-based index of the. </param>
        /// <param name="value"> The value. </param>
        /// <returns> A SqliteResultCode. </returns>
        internal SqliteResultCode sqlite3_bind_text(SqliteStatementHandle stmt, int index, string value)
        {
            if (stmt == null) { throw new ArgumentNullException(nameof(stmt)); }
            stmt.CheckMaintenanceMode();
            return (SqliteResultCode)DbProviderOperations.sqlite3_bind_text(stmt.Statement, index, value);
        }

        /// <summary> Sqlite 3 column BLOB. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="stmt"> The statement. </param>
        /// <param name="index"> Zero-based index of the. </param>
        /// <returns> A byte[]. </returns>
        internal byte[] sqlite3_column_blob(SqliteStatementHandle stmt, int index)
        {
            if (stmt == null) { throw new ArgumentNullException(nameof(stmt)); }
            stmt.CheckMaintenanceMode();
            return DbProviderOperations.sqlite3_column_blob(stmt.Statement, index);
        }

        /// <summary> Sqlite 3 column bytes. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="stmt"> The statement. </param>
        /// <param name="index"> Zero-based index of the. </param>
        /// <returns> An int. </returns>
        internal int sqlite3_column_bytes(SqliteStatementHandle stmt, int index)
        {
            if (stmt == null) { throw new ArgumentNullException(nameof(stmt)); }
            stmt.CheckMaintenanceMode();
            return DbProviderOperations.sqlite3_column_bytes(stmt.Statement, index);
        }

        /// <summary> Sqlite 3 column count. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="stmt"> The statement. </param>
        /// <returns> An int. </returns>
        internal int sqlite3_column_count(SqliteStatementHandle stmt) {
            if (stmt == null) { throw new ArgumentNullException(nameof(stmt)); }
            stmt.CheckMaintenanceMode();
            return DbProviderOperations.sqlite3_column_count(stmt.Statement);
        }

        /// <summary> Sqlite 3 column decltype. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="stmt"> The statement. </param>
        /// <param name="index"> Zero-based index of the. </param>
        /// <returns> A string. </returns>
        internal string sqlite3_column_decltype(SqliteStatementHandle stmt, int index)
        {
            if (stmt == null) { throw new ArgumentNullException(nameof(stmt)); }
            stmt.CheckMaintenanceMode();
            return DbProviderOperations.sqlite3_column_decltype(stmt.Statement, index);
        }

        /// <summary> Sqlite 3 column double. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="stmt"> The statement. </param>
        /// <param name="index"> Zero-based index of the. </param>
        /// <returns> A double. </returns>
        internal double sqlite3_column_double(SqliteStatementHandle stmt, int index)
        {
            if (stmt == null) { throw new ArgumentNullException(nameof(stmt)); }
            stmt.CheckMaintenanceMode();
            return DbProviderOperations.sqlite3_column_double(stmt.Statement, index);
        }

        /// <summary> Sqlite 3 column int. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="stmt"> The statement. </param>
        /// <param name="index"> Zero-based index of the. </param>
        /// <returns> An int. </returns>
        internal int sqlite3_column_int(SqliteStatementHandle stmt, int index)
        {
            if (stmt == null) { throw new ArgumentNullException(nameof(stmt)); }
            stmt.CheckMaintenanceMode();
            return DbProviderOperations.sqlite3_column_int(stmt.Statement, index);
        }

        /// <summary> Sqlite 3 column int 64. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="stmt"> The statement. </param>
        /// <param name="index"> Zero-based index of the. </param>
        /// <returns> A long. </returns>
        internal long sqlite3_column_int64(SqliteStatementHandle stmt, int index)
        {
            if (stmt == null) { throw new ArgumentNullException(nameof(stmt)); }
            stmt.CheckMaintenanceMode();
            return DbProviderOperations.sqlite3_column_int64(stmt.Statement, index);
        }

        /// <summary> Sqlite 3 column name. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="stmt"> The statement. </param>
        /// <param name="index"> Zero-based index of the. </param>
        /// <returns> A string. </returns>
        internal string sqlite3_column_name(SqliteStatementHandle stmt, int index)
        {
            if (stmt == null) { throw new ArgumentNullException(nameof(stmt)); }
            stmt.CheckMaintenanceMode();
            return DbProviderOperations.sqlite3_column_name(stmt.Statement, index);
        }

        /// <summary> Sqlite 3 column text. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="stmt"> The statement. </param>
        /// <param name="index"> Zero-based index of the. </param>
        /// <returns> A string. </returns>
        internal string sqlite3_column_text(SqliteStatementHandle stmt, int index)
        {
            if (stmt == null) { throw new ArgumentNullException(nameof(stmt)); }
            stmt.CheckMaintenanceMode();
            return DbProviderOperations.sqlite3_column_text(stmt.Statement, index);
        }

        /// <summary> Sqlite 3 column type. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="stmt"> The statement. </param>
        /// <param name="index"> Zero-based index of the. </param>
        /// <returns> A SqliteColumnType. </returns>
        internal SqliteColumnType sqlite3_column_type(SqliteStatementHandle stmt, int index)
        {
            if (stmt == null) { throw new ArgumentNullException(nameof(stmt)); }
            stmt.CheckMaintenanceMode();
            return (SqliteColumnType)DbProviderOperations.sqlite3_column_type(stmt.Statement, index);
        }

        /// <summary> Sqlite 3 configuration log. </summary>
        /// <param name="func"> The function. </param>
        /// <param name="value"> The value. </param>
        internal void sqlite3_config_log(DbProvider.delegate_log func, object value)
        {
            DbProviderOperations.sqlite3_config_log(func, value);
        }

        /// <summary>
        /// The sqlite3_db_readonly(D,N) interface returns 1 if the database N of connection D is read-
        /// only, 0 if it is read/write, or -1 if N is not the name of a database on connection D.
        /// </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <exception cref="ArgumentException"> Thrown when one or more arguments have unsupported or
        ///     illegal values. </exception>
        /// <param name="db"> The database. </param>
        /// <param name="dbName"> Name of the database. </param>
        /// <param name="isMaintenanceDb"> (Optional) True if this object is maintenance database. </param>
        /// <returns> An int. </returns>
        internal int sqlite3_db_readonly(SqliteDatabaseHandle db, string dbName, bool isMaintenanceDb = false)
        {
            if (db == null) { throw new ArgumentNullException(nameof(db));}
            DbProvider.sqlite3 database = (isMaintenanceDb ? db.MaintenanceDb : db.Db)
                                          ?? throw new ArgumentException((isMaintenanceDb 
                                                ? "Maintenance mode is specified, but the database is not in Maintenance Mode." 
                                                : "The database is in Maintenance Mode, but this was not properly specified."),
                                              nameof(isMaintenanceDb));
            return DbProviderOperations.sqlite3_db_readonly(database, dbName);
        }

        /// <summary> Sqlite 3 errcode. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <exception cref="ArgumentException"> Thrown when one or more arguments have unsupported or
        ///     illegal values. </exception>
        /// <param name="db"> The database. </param>
        /// <param name="isMaintenanceDb"> (Optional) True if this object is maintenance database. </param>
        /// <returns> A SqliteResultCode. </returns>
        internal SqliteResultCode sqlite3_errcode(SqliteDatabaseHandle db, bool isMaintenanceDb = false)
        {
            if (db == null) { throw new ArgumentNullException(nameof(db)); }
            DbProvider.sqlite3 database = (isMaintenanceDb ? db.MaintenanceDb : db.Db)
                                          ?? throw new ArgumentException((isMaintenanceDb
                                                  ? "Maintenance mode is specified, but the database is not in Maintenance Mode."
                                                  : "The database is in Maintenance Mode, but this was not properly specified."),
                                              nameof(isMaintenanceDb));
            return (SqliteResultCode)DbProviderOperations.sqlite3_errcode(database);
        }

        /// <summary> Sqlite 3 errmsg. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <exception cref="ArgumentException"> Thrown when one or more arguments have unsupported or
        ///     illegal values. </exception>
        /// <param name="db"> The database. </param>
        /// <param name="isMaintenanceDb"> (Optional) True if this object is maintenance database. </param>
        /// <returns> A string. </returns>
        internal string sqlite3_errmsg(SqliteDatabaseHandle db, bool isMaintenanceDb = false)
        {
            if (db == null) { throw new ArgumentNullException(nameof(db)); }
            DbProvider.sqlite3 database = (isMaintenanceDb ? db.MaintenanceDb : db.Db)
                                          ?? throw new ArgumentException((isMaintenanceDb
                                                  ? "Maintenance mode is specified, but the database is not in Maintenance Mode."
                                                  : "The database is in Maintenance Mode, but this was not properly specified."),
                                              nameof(isMaintenanceDb));
            return DbProviderOperations.sqlite3_errmsg(database);
        }

        /// <summary> Sqlite 3 errstr. </summary>
        /// <param name="rc"> The rectangle. </param>
        /// <returns> A string. </returns>
        internal string sqlite3_errstr(SqliteResultCode rc) {
            return DbProviderOperations.sqlite3_errstr((int)rc);
        }

        /// <summary> Sqlite 3 busy timeout. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <exception cref="ArgumentException"> Thrown when one or more arguments have unsupported or
        ///     illegal values. </exception>
        /// <param name="db"> The database. </param>
        /// <param name="milliseconds"> The milliseconds. </param>
        /// <param name="isMaintenanceDb"> (Optional) True if this object is maintenance database. </param>
        /// <returns> A SqliteResultCode. </returns>
        internal SqliteResultCode sqlite3_busy_timeout(SqliteDatabaseHandle db, int milliseconds, bool isMaintenanceDb = false)
        {
            if (db == null) { throw new ArgumentNullException(nameof(db)); }
            DbProvider.sqlite3 database = (isMaintenanceDb ? db.MaintenanceDb : db.Db)
                                          ?? throw new ArgumentException((isMaintenanceDb
                                                  ? "Maintenance mode is specified, but the database is not in Maintenance Mode."
                                                  : "The database is in Maintenance Mode, but this was not properly specified."),
                                              nameof(isMaintenanceDb));
            return (SqliteResultCode)DbProviderOperations.sqlite3_busy_timeout(database, milliseconds);
        }

        /// <summary> Sqlite 3 profile. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <exception cref="ArgumentException"> Thrown when one or more arguments have unsupported or
        ///     illegal values. </exception>
        /// <param name="db"> The database. </param>
        /// <param name="func"> The function. </param>
        /// <param name="value"> The value. </param>
        /// <param name="isMaintenanceDb"> (Optional) True if this object is maintenance database. </param>
        internal void sqlite3_profile(SqliteDatabaseHandle db, DbProvider.delegate_profile func, object value, bool isMaintenanceDb = false)
        {
            if (db == null) { throw new ArgumentNullException(nameof(db)); }
            DbProvider.sqlite3 database = (isMaintenanceDb ? db.MaintenanceDb : db.Db)
                                          ?? throw new ArgumentException((isMaintenanceDb
                                                  ? "Maintenance mode is specified, but the database is not in Maintenance Mode."
                                                  : "The database is in Maintenance Mode, but this was not properly specified."),
                                              nameof(isMaintenanceDb));
            DbProviderOperations.sqlite3_profile(database, func, value);
        }

        /// <summary> Handler, called when the sqlite 3 progress. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <exception cref="ArgumentException"> Thrown when one or more arguments have unsupported or
        ///     illegal values. </exception>
        /// <param name="db"> The database. </param>
        /// <param name="virtualMachineInstructions"> The virtual machine instructions. </param>
        /// <param name="func"> The function. </param>
        /// <param name="value"> The value. </param>
        /// <param name="isMaintenanceDb"> (Optional) True if this object is maintenance database. </param>
        internal void sqlite3_progress_handler(SqliteDatabaseHandle db, int virtualMachineInstructions, DbProvider.delegate_progress_handler func, object value, bool isMaintenanceDb = false)
        {
            if (db == null) { throw new ArgumentNullException(nameof(db)); }
            DbProvider.sqlite3 database = (isMaintenanceDb ? db.MaintenanceDb : db.Db)
                                          ?? throw new ArgumentException((isMaintenanceDb
                                                  ? "Maintenance mode is specified, but the database is not in Maintenance Mode."
                                                  : "The database is in Maintenance Mode, but this was not properly specified."),
                                              nameof(isMaintenanceDb));
            DbProviderOperations.sqlite3_progress_handler(database, virtualMachineInstructions, func, value);
        }

        /// <summary> Sqlite 3 reset. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="stmt"> The statement. </param>
        /// <returns> A SqliteResultCode. </returns>
        internal SqliteResultCode sqlite3_reset(SqliteStatementHandle stmt) {
            if (stmt == null) { throw new ArgumentNullException(nameof(stmt)); }
            stmt.CheckMaintenanceMode();
            return (SqliteResultCode)DbProviderOperations.sqlite3_reset(stmt.Statement);
        }

        /// <summary> Sqlite 3 step. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="stmt"> The statement. </param>
        /// <returns> A SqliteResultCode. </returns>
        internal SqliteResultCode sqlite3_step(SqliteStatementHandle stmt) {
            if (stmt == null) { throw new ArgumentNullException(nameof(stmt)); }
            lock (_stepLocker)
            {
                stmt.CheckMaintenanceMode();
                return (SqliteResultCode)DbProviderOperations.sqlite3_step(stmt.Statement);
            }
        }

        /// <summary> Sqlite 3 last insert rowid. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <exception cref="ArgumentException"> Thrown when one or more arguments have unsupported or
        ///     illegal values. </exception>
        /// <param name="db"> The database. </param>
        /// <param name="isMaintenanceDb"> (Optional) True if this object is maintenance database. </param>
        /// <returns> A long. </returns>
        internal long sqlite3_last_insert_rowid(SqliteDatabaseHandle db, bool isMaintenanceDb = false) {
            if (db == null) { throw new ArgumentNullException(nameof(db)); }
            lock (_stepLocker) {
                DbProvider.sqlite3 database = (isMaintenanceDb ? db.MaintenanceDb : db.Db)
                                              ?? throw new ArgumentException((isMaintenanceDb
                                                      ? "Maintenance mode is specified, but the database is not in Maintenance Mode."
                                                      : "The database is in Maintenance Mode, but this was not properly specified."),
                                                  nameof(isMaintenanceDb));
                return DbProviderOperations.sqlite3_last_insert_rowid(database);
            }
        }

        /// <summary> Sqlite 3 step return rowid. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <param name="db"> The database. </param>
        /// <param name="stmt"> The statement. </param>
        /// <param name="code"> [out] The code. </param>
        /// <returns> A long. </returns>
        internal long sqlite3_step_return_rowid(SqliteDatabaseHandle db, SqliteStatementHandle stmt, out SqliteResultCode code) {
            if (db == null) { throw new ArgumentNullException(nameof(db)); }
            if (stmt == null) { throw new ArgumentNullException(nameof(stmt)); }
            lock (_stepLocker) {
                stmt.CheckMaintenanceMode();
                code = (SqliteResultCode)DbProviderOperations.sqlite3_step(stmt.Statement);
                return code.IsSuccessCode()
                    ? DbProviderOperations.sqlite3_last_insert_rowid(stmt.ForMaintenance ? db.MaintenanceDb : db.Db)
                    : -1;
            }
        }

        /// <summary> Sqlite 3 total changes. </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null. </exception>
        /// <exception cref="ArgumentException"> Thrown when one or more arguments have unsupported or
        ///     illegal values. </exception>
        /// <param name="db"> The database. </param>
        /// <param name="isMaintenanceDb"> (Optional) True if this object is maintenance database. </param>
        /// <returns> An int. </returns>
        internal int sqlite3_total_changes(SqliteDatabaseHandle db, bool isMaintenanceDb = false) {
            if (db == null) { throw new ArgumentNullException(nameof(db)); }
            DbProvider.sqlite3 database = (isMaintenanceDb ? db.MaintenanceDb : db.Db)
                                          ?? throw new ArgumentException((isMaintenanceDb
                                                  ? "Maintenance mode is specified, but the database is not in Maintenance Mode."
                                                  : "The database is in Maintenance Mode, but this was not properly specified."),
                                              nameof(isMaintenanceDb));
            return DbProviderOperations.sqlite3_total_changes(database);
        }

    }
}
