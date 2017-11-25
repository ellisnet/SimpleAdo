using System;
using System.Globalization;

namespace SimpleAdo
{
    /// <summary> An utility. </summary>
	public static class Utility
	{
		public static void Dispose<T>(ref T disposable)
			where T : class, IDisposable
		{
			if (disposable != null)
			{
				disposable.Dispose();
				disposable = null;
			}
		}

        /// <summary> An IDbConnection extension method that executes the non query operation. </summary>
        /// <param name="connection"> The connection to act on. </param>
        /// <param name="commandText"> The command text. </param>
        /// <param name="forMaintenance"> (Optional) True to for maintenance. </param>
        /// <returns> An int. </returns>
		public static int ExecuteNonQuery(this IDbConnection connection, string commandText, bool forMaintenance = false)
		{
			return ExecuteNonQuery(connection, null, commandText, forMaintenance);
		}

        /// <summary>
        /// An IDbConnection extension method that executes the non query operation.
        /// </summary>
        /// <param name="connection"> The connection to act on. </param>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="commandText"> The command text. </param>
        /// <param name="forMaintenance"> (Optional) True to for maintenance. </param>
        /// <returns> An int. </returns>
		public static int ExecuteNonQuery(this IDbConnection connection, IDbTransaction transaction, string commandText, bool forMaintenance = false)
		{
			using (var command = connection.CreateCommand(forMaintenance))
			{
				command.CommandText = commandText;
				command.Transaction = transaction;
				return command.ExecuteNonQuery();
			}
		}

        /// <summary> An IDbConnection extension method that executes the scalar operation. </summary>
        /// <param name="connection"> The connection to act on. </param>
        /// <param name="commandText"> The command text. </param>
        /// <param name="forMaintenance"> (Optional) True to for maintenance. </param>
        /// <returns> An object. </returns>
	    public static object ExecuteScalar(this IDbConnection connection, string commandText, bool forMaintenance = false)
	    {
	        return ExecuteScalar(connection, null, commandText, forMaintenance);
	    }

        /// <summary> An IDbConnection extension method that executes the scalar operation. </summary>
        /// <param name="connection"> The connection to act on. </param>
        /// <param name="transaction"> The transaction. </param>
        /// <param name="commandText"> The command text. </param>
        /// <param name="forMaintenance"> (Optional) True to for maintenance. </param>
        /// <returns> An object. </returns>
        public static object ExecuteScalar(this IDbConnection connection, IDbTransaction transaction, string commandText, bool forMaintenance = false)
	    {
	        using (var command = connection.CreateCommand(forMaintenance))
	        {
	            command.CommandText = commandText;
	            command.Transaction = transaction;
	            return command.ExecuteScalar();
	        }
        }

        /// <summary> An IDbConnection extension method that executes the reader operation. </summary>
        /// <param name="connection"> The connection to act on. </param>
        /// <param name="commandText"> The command text. </param>
        /// <param name="forMaintenance"> (Optional) True to for maintenance. </param>
        /// <returns> An IDataReader. </returns>
		public static IDataReader ExecuteReader(this IDbConnection connection, string commandText, bool forMaintenance = false)
		{
			using (var command = connection.CreateCommand(forMaintenance))
			{
				command.CommandText = commandText;
				return command.ExecuteReader();
			}
		}

        /// <summary> A string extension method that format invariant. </summary>
        /// <param name="format"> The format to act on. </param>
        /// <param name="args"> A variable-length parameters list containing arguments. </param>
        /// <returns> The formatted invariant. </returns>
		public static string FormatInvariant(this string format, params object[] args)
		{
			return string.Format(CultureInfo.InvariantCulture, format, args);
		}

        /// <summary> Parse enum. </summary>
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="value"> The value. </param>
        /// <returns> A T. </returns>
		public static T ParseEnum<T>(string value)
			where T : struct
		{
			return (T) Enum.Parse(typeof(T), value, true);
		}
	}
}
