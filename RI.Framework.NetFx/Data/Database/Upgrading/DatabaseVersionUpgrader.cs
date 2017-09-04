﻿using System.Data.Common;

using RI.Framework.Utilities.Logging;

namespace RI.Framework.Data.Database.Upgrading
{
	/// <summary>
	/// Implements a base class for database version upgraders.
	/// </summary>
	/// <typeparam name="TConnection">The database connection type, subclass of <see cref="DbConnection"/>.</typeparam>
	/// <typeparam name="TTransaction">The database transaction type, subclass of <see cref="DbTransaction"/>.</typeparam>
	/// <typeparam name="TConnectionStringBuilder">The connection string builder type, subclass of <see cref="DbConnectionStringBuilder"/>.</typeparam>
	/// <typeparam name="TManager">The type of the database manager.</typeparam>
	/// <typeparam name="TConfiguration">The type of database configuration.</typeparam>
	/// <remarks>
	/// <para>
	/// It is recommended that database version upgrader implementations use this base class as it already implements most of the logic which is database-independent.
	/// </para>
	/// <para>
	/// See <see cref="IDatabaseVersionUpgrader"/> for more details.
	/// </para>
	/// </remarks>
	public abstract class DatabaseVersionUpgrader<TConnection, TTransaction, TConnectionStringBuilder, TManager, TConfiguration> : IDatabaseVersionUpgrader<TConnection, TTransaction, TConnectionStringBuilder, TManager, TConfiguration>
		where TConnection : DbConnection
		where TTransaction : DbTransaction
		where TConnectionStringBuilder : DbConnectionStringBuilder
		where TManager : IDatabaseManager<TConnection, TTransaction, TConnectionStringBuilder, TManager, TConfiguration>
		where TConfiguration : class, IDatabaseManagerConfiguration<TConnection, TTransaction, TConnectionStringBuilder, TManager, TConfiguration>, new()
	{
		/// <inheritdoc />
		public abstract bool Upgrade (TManager manager, int sourceVersion);

		/// <inheritdoc />
		public abstract bool RequiresScriptLocator { get; }

		int IDatabaseVersionUpgrader.GetMinVersion (IDatabaseManager manager)
		{
			return this.GetMinVersion((TManager)manager);
		}

		int IDatabaseVersionUpgrader.GetMaxVersion (IDatabaseManager manager)
		{
			return this.GetMaxVersion((TManager)manager);
		}

		/// <inheritdoc />
		public abstract int GetMinVersion (TManager manager);

		/// <inheritdoc />
		public abstract int GetMaxVersion (TManager manager);

		/// <inheritdoc />
		bool IDatabaseVersionUpgrader.Upgrade(IDatabaseManager manager, int sourceVersion)
		{
			return this.Upgrade((TManager)manager, sourceVersion);
		}

		private bool _loggingEnabled;

		/// <inheritdoc />
		bool ILogSource.LoggingEnabled
		{
			get
			{
				return this._loggingEnabled;
			}
			set
			{
				this._loggingEnabled = value;
			}
		}

		private ILogger _logger;

		/// <inheritdoc />
		ILogger ILogSource.Logger
		{
			get
			{
				return this._logger;
			}
			set
			{
				this._logger = value;
			}
		}
	}
}
