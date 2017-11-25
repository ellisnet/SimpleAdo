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
using System.Globalization;

using SimpleAdo.Common;

namespace SimpleAdo.Sqlite
{
    /// <summary> A sqlite connection string builder. This class cannot be inherited. </summary>
	public sealed class SqliteConnectionStringBuilder : DbConnectionStringBuilder
	{
        /// <summary> Gets or sets/Sets the cache size for the connection. </summary>
        /// <value> The size of the cache. </value>
		public int CacheSize
		{
			get
			{
			    TryGetValue(CacheSizeKey, out object value);
				return Convert.ToInt32(value, CultureInfo.InvariantCulture);
			}
			set => this[CacheSizeKey] = value;
		}
        /// <summary> The cache size key. </summary>
		internal const string CacheSizeKey = "Cache Size";

        /// <summary> Gets or sets/Sets the filename to open on the connection string. </summary>
        /// <value> The data source. </value>
		public string DataSource
		{
			get
			{
				TryGetValue(DataSourceKey, out object value);
				return (value as string) ?? "";
			}
			set => this[DataSourceKey] = value;
		}
        /// <summary> The data source key. </summary>
		internal const string DataSourceKey = "Data Source";

        /// <summary>
        /// Gets or sets/Sets the filename for the SQLite database (equivalent to DataSource).
        /// </summary>
        /// <value> The full pathname of the database file. </value>
        public string DatabaseFilePath
	    {
	        get => DataSource;
	        set => DataSource = value;
	    }

        /// <summary>
        /// Gets or sets/sets the default command timeout for newly-created commands.  This is especially
        /// useful for commands used internally such as inside a SqliteTransaction, where setting the
        /// timeout is not possible.
        /// </summary>
        /// <value> The default timeout. </value>
		public int DefaultTimeout
		{
			get
			{
				TryGetValue(DefaultTimeoutKey, out object value);
				return Convert.ToInt32(value, CultureInfo.InvariantCulture);
			}
			set => this[DefaultTimeoutKey] = value;
		}
        /// <summary> The default timeout key. </summary>
		internal const string DefaultTimeoutKey = "Default Timeout";

        /// <summary> Gets or sets/sets the busy timeout for the database (in milliseconds). </summary>
        /// <value> The busy timeout. </value>
	    public int BusyTimeout
	    {
	        get
	        {
	            TryGetValue(BusyTimeoutKey, out object value);
	            return Convert.ToInt32(value, CultureInfo.InvariantCulture);
	        }
	        set => this[BusyTimeoutKey] = value;
	    }
        /// <summary> The busy timeout key. </summary>
	    internal const string BusyTimeoutKey = "Busy Timeout";

        /// <summary> If enabled, use foreign key constraints. </summary>
        /// <value> True if foreign keys, false if not. </value>
        public bool ForeignKeys
		{
			get => TryGetValue(ForeignKeysKey, out object value) && ValueIsTrue(value);
		    set => this[ForeignKeysKey] = value;
		}
        /// <summary> The foreign keys key. </summary>
		internal const string ForeignKeysKey = "Foreign Keys";

        /// <summary>
        /// If enabled, use ticks - an Int64 - to store DateTime values; defaults to true (enabled)
        /// </summary>
        /// <value> True if store date time as ticks, false if not. </value>
	    public bool StoreDateTimeAsTicks
        {
	        // ReSharper disable once SimplifyConditionalTernaryExpression
	        get => ContainsKey(StoreDateTimeAsTicksKey)
                ? (TryGetValue(StoreDateTimeAsTicksKey, out object value) && ValueIsTrue(value))
                : true;
	        set => this[StoreDateTimeAsTicksKey] = value;
	    }
        /// <summary> The store date time as ticks key. </summary>
	    internal const string StoreDateTimeAsTicksKey = "Store DateTime As Ticks";

        /// <summary>
        /// If set to true, will throw an exception if the database specified in the connection string
        /// does not exist.  If false, the database will be created automatically.
        /// </summary>
        /// <value> True if fail if missing, false if not. </value>
        public bool FailIfMissing
		{
			get => TryGetValue(FailIfMissingKey, out object value) && ValueIsTrue(value);
		    set => this[FailIfMissingKey] = value;
		}
        /// <summary> The fail if missing key. </summary>
		internal const string FailIfMissingKey = "FailIfMissing";

        /// <summary> Determines how SQLite handles the transaction journal file. </summary>
        /// <value> The journal mode. </value>
		public SqliteJournalModeEnum JournalMode
		{
			get
			{
				TryGetValue(JournalModeKey, out object value);
			    // ReSharper disable once MergeCastWithTypeCheck
			    return value is string
			        ? Utility.ParseEnum<SqliteJournalModeEnum>((string) value)
			        :
			        // ReSharper disable once MergeConditionalExpression
			        value is SqliteJournalModeEnum
			            ? (SqliteJournalModeEnum) value
			            : SqliteJournalModeEnum.Default;
			}
			set => this[JournalModeKey] = value;
		}
        /// <summary> The journal mode key. </summary>
		internal const string JournalModeKey = "Journal Mode";

        /// <summary>
        /// Gets or sets/sets the maximum size of memory-mapped I/O for this connection.
        /// </summary>
        /// <remarks> See <a href="http://www.sqlite.org/mmap.html">Memory-Mapped I/O</a>. </remarks>
        /// <value> The size of the mmap. </value>
		public long MmapSize
		{
			get
			{
				TryGetValue(MmapSizeKey, out object value);
				return Convert.ToInt64(value, CultureInfo.InvariantCulture);
			}
			set => this[MmapSizeKey] = value;
		}
        /// <summary> The mmap size key. </summary>
		internal const string MmapSizeKey = "_MmapSize";

        /// <summary> Gets or sets/Sets the page size for the connection. </summary>
        /// <value> The size of the page. </value>
		public int PageSize
		{
			get
			{
				TryGetValue(PageSizeKey, out object value);
				return Convert.ToInt32(value, CultureInfo.InvariantCulture);
			}
			set => this[PageSizeKey] = value;
		}
        /// <summary> The page size key. </summary>
		internal const string PageSizeKey = "Page Size";

        /// <summary> Gets or sets/sets the database encryption password. </summary>
        /// <value> The password. </value>
		public string Password
		{
			get
			{
				TryGetValue(PasswordKey, out object value);
				return value as string;
			}
			set => this[PasswordKey] = value;
		}
        /// <summary> The password key. </summary>
		internal const string PasswordKey = "Password";

        /// <summary>
        /// When enabled, the database will be opened for read-only access and writing will be disabled.
        /// </summary>
        /// <value> True if read only, false if not. </value>
		public bool ReadOnly
		{
			get => TryGetValue(ReadOnlyKey, out object value) && ValueIsTrue(value);
		    set => this[ReadOnlyKey] = value;
		}
        /// <summary> The read only key. </summary>
		internal const string ReadOnlyKey = "Read Only";

        /// <summary>
        /// Gets or sets/Sets the synchronization mode (file flushing) of the connection string.  Default
        /// is "Normal".
        /// </summary>
        /// <value> The synchronise mode. </value>
		public SynchronizationModes SyncMode
		{
			get
			{
				TryGetValue(SynchronousKey, out object value);
			    // ReSharper disable once MergeCastWithTypeCheck
				return value is string ? Utility.ParseEnum<SynchronizationModes>((string) value) :
				    // ReSharper disable once MergeConditionalExpression
					value is SynchronizationModes ? (SynchronizationModes) value :
					SynchronizationModes.Normal;
			}
			set => this[SynchronousKey] = value;
		}
        /// <summary> The synchronous key. </summary>
		internal const string SynchronousKey = "Synchronous";

        /// <summary>
        /// Gets or sets/sets the storage location for temporary tables and indices. Default is "Default".
        /// </summary>
        /// <value> The temporary store. </value>
		public SqliteTemporaryStore TempStore
		{
			get
			{
				TryGetValue(TempStoreKey, out object value);
			    // ReSharper disable once MergeCastWithTypeCheck
				return value is string ? Utility.ParseEnum<SqliteTemporaryStore>((string) value) :
				    // ReSharper disable once MergeConditionalExpression
					value is SqliteTemporaryStore ? (SqliteTemporaryStore) value :
					SqliteTemporaryStore.Default;
			}
			set => this[TempStoreKey] = value;
		}
        /// <summary> The temporary store key. </summary>
		internal const string TempStoreKey = "_TempStore";

        /// <summary> Value is true. </summary>
        /// <exception cref="ArgumentException"> Thrown when one or more arguments have unsupported or
        ///     illegal values. </exception>
        /// <param name="value"> The value. </param>
        /// <returns> True if it succeeds, false if it fails. </returns>
		private static bool ValueIsTrue(object value)
		{
			if (value is bool b)
			{
			    return b;
			}

		    if (value is string s)
		    {
		        return bool.Parse(s);
		    }

		    throw new ArgumentException("Invalid value", nameof(value));
		}
	}

    /// <summary> This enum determines how SQLite treats its journal file. </summary>
    /// <remarks>
    /// By default SQLite will create and delete the journal file when needed during a transaction.
    /// However, for some computers running certain filesystem monitoring tools, the rapid creation
    /// and deletion of the journal file can cause those programs to fail, or to interfere with
    /// SQLite.
    /// 
    /// If a program or virus scanner is interfering with SQLite's journal file, you may receive
    /// errors like "unable to open database file" when starting a transaction.  If this is happening,
    /// you may want to change the default journal mode to Persist.
    /// </remarks>
	public enum SqliteJournalModeEnum
	{
		/// <summary>
		/// The default mode, this causes SQLite to use the existing journaling mode for the database.
		/// </summary>
		Default = -1,

		/// <summary>
		/// SQLite will create and destroy the journal file as-needed.
		/// </summary>
		Delete = 0,

		/// <summary>
		/// When this is set, SQLite will keep the journal file even after a transaction has completed.  It's contents will be erased,
		/// and the journal re-used as often as needed.  If it is deleted, it will be recreated the next time it is needed.
		/// </summary>
		Persist = 1,

		/// <summary>
		/// This option disables the rollback journal entirely.  Interrupted transactions or a program crash can cause database
		/// corruption in this mode!
		/// </summary>
		Off = 2,

		/// <summary>
		/// SQLite will truncate the journal file to zero-length instead of deleting it.
		/// </summary>
		Truncate = 3,

		/// <summary>
		/// SQLite will store the journal in volatile RAM.  This saves disk I/O but at the expense of database safety and integrity.
		/// If the application using SQLite crashes in the middle of a transaction when the MEMORY journaling mode is set, then the
		/// database file will very likely go corrupt.
		/// </summary>
		Memory = 4,

		/// <summary>
		/// SQLite uses a write-ahead log instead of a rollback journal to implement transactions.  The WAL journaling mode is persistent;
		/// after being set it stays in effect across multiple database connections and after closing and reopening the database. A database
		/// in WAL journaling mode can only be accessed by SQLite version 3.7.0 or later.
		/// </summary>
		Wal = 5
	}

    /// <summary>
    /// Possible values for the "synchronous" database setting.  This setting determines how often
    /// the database engine calls the xSync method of the VFS.
    /// </summary>
	public enum SynchronizationModes
	{
		/// <summary>
		/// The database engine continues without syncing as soon as it has handed
		/// data off to the operating system.  If the application running SQLite
		/// crashes, the data will be safe, but the database might become corrupted
		/// if the operating system crashes or the computer loses power before that
		/// data has been written to the disk surface.
		/// </summary>
		Off = 0,

		/// <summary>
		/// The database engine will still sync at the most critical moments, but
		/// less often than in FULL mode.  There is a very small (though non-zero)
		/// chance that a power failure at just the wrong time could corrupt the
		/// database in NORMAL mode.
		/// </summary>
		Normal = 1,

		/// <summary>
		/// The database engine will use the xSync method of the VFS to ensure that
		/// all content is safely written to the disk surface prior to continuing.
		/// This ensures that an operating system crash or power failure will not
		/// corrupt the database.  FULL synchronous is very safe, but it is also
		/// slower.
		/// </summary>
		Full = 2
	}

    /// <summary> Determines where temporary tables and indices are stored. </summary>
    /// <remarks>
    /// See <a href="http://www.sqlite.org/pragma.html#pragma_temp_store">pragma temp_store</a>.
    /// </remarks>
	public enum SqliteTemporaryStore
	{
		/// <summary>
		/// The SQLite library determines where temporary tables and indices are stored.
		/// </summary>
		Default = 0,

		/// <summary>
		/// Temporary tables and indices are stored in a file. 
		/// </summary>
		File = 1,

		/// <summary>
		/// Temporary tables and indices are kept in memory as if they were pure in-memory databases.
		/// </summary>
		Memory = 2,
	}
}
