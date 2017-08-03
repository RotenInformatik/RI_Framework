﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;

using RI.Framework.Data.Database.Backup;
using RI.Framework.Data.Database.Cleanup;
using RI.Framework.Data.Database.Upgrading;
using RI.Framework.Data.Repository;
using RI.Framework.IO.Paths;
using RI.Framework.Utilities.Exceptions;
using RI.Framework.Utilities.Logging;

namespace RI.Framework.Data.Database
{
	/// <summary>
	///     Defines the interface for a database manager.
	/// </summary>
	/// <remarks>
	///     <para>
	///         A database manager encapsulates low-level database functionality so that high-level database functionality (such as <see cref="IRepositoryContext" />) can be implemented on top without having to worry about low-level and/or database-specific details.
	///         A database manager is intended to handle all low-level database stuff, such as connection management and migration, so that the high-level functionality can focus on the actual task of working with the data or accessing the database respectively.
	///     </para>
	///     <para>
	///         The link from the database manager to higher-level functionality are the connections which can be created using <see cref="CreateConnection" />.
	///     </para>
	///     <para>
	///         The database cannot be used if it is in any other state than <see cref="DatabaseState.ReadyUnknown" />, <see cref="DatabaseState.ReadyNew" />, or <see cref="DatabaseState.ReadyOld" />.
	///         However, there are two exceptions:
	///         Upgrades (<see cref="Upgrade(int)"/>, <see cref="Upgrade()"/>) are also possible in the <see cref="DatabaseState.New"/> state.
	///         Restore (<see cref="Restore"/>) is possible in any state except <see cref="DatabaseState.Uninitialized"/>.
	///     </para>
	///     <para>
	///         If the database supports connection tracking (see <see cref="SupportsConnectionTracking" />), <see cref="Close" /> or <see cref="CloseTrackedConnections" /> will close all currently non-closed connections which were created using <see cref="CreateConnection" />.
	///     </para>
	/// <note type="implement">
	/// While a database is initialized, its configuration (<see cref="Configuration"/>) can be changed.
	/// However, it is recommend to change configuration only when the database is in the unitintialized state.
	/// </note>
	/// </remarks>
	public interface IDatabaseManager : IDisposable, ILogSource
	{
		/// <summary>
		/// Gets the configuration used by this database.
		/// </summary>
		/// <value>
		/// The configuration used by this database.
		/// </value>
		/// <remarks>
		/// <note type="implement">
		/// This property must never be null.
		/// </note>
		/// </remarks>
		IDatabaseManagerConfiguration Configuration { get; }

		/// <summary>
		/// Gets the current state of the database.
		/// </summary>
		/// <value>
		/// The current state of the database.
		/// </value>
		DatabaseState State { get; }

		/// <summary>
		/// Gets the current version of the database.
		/// </summary>
		/// <value>
		/// The current version of the database.
		/// </value>
		/// <remarks>
		/// <para>
		/// Any version below zero (usually -1) indicates an invalid version or a damaged database respectively.
		/// </para>
		/// <para>
		/// A version of zero indicates that the database does not yet exist and must be created before it can be used.
		/// </para>
		/// </remarks>
		int Version { get; }

		/// <summary>
		/// Gets the state of the database after initialization.
		/// </summary>
		/// <value>
		/// The state of the database after initialization.
		/// </value>
		/// <remarks>
		/// <para>
		/// This property is set during <see cref="Initialize"/> and reset during <see cref="Close"/>:
		/// </para>
		/// <para>
		/// See <see cref="State"/> for more details.
		/// </para>
		/// </remarks>
		DatabaseState InitialState { get; }

		/// <summary>
		/// Gets the version of the database after initialization.
		/// </summary>
		/// <value>
		/// The version of the database after initialization.
		/// </value>
		/// <remarks>
		/// <para>
		/// This property is set during <see cref="Initialize"/> and reset during <see cref="Close"/>:
		/// </para>
		/// <para>
		/// See <see cref="Version"/> for more details.
		/// </para>
		/// </remarks>
		int InitialVersion { get; }

		/// <summary>
		/// Gets whether the database is ready for use and connections can be created or backups and cleanups can be performed.
		/// </summary>
		/// <value>
		/// true if the database is in <see cref="DatabaseState.ReadyUnknown" />, <see cref="DatabaseState.ReadyNew" />, or <see cref="DatabaseState.ReadyOld" /> state, false otherwise.
		/// </value>
		bool IsReady { get; }

		/// <summary>
		/// Gets whether the database is in a state where it can be upgraded to a newer version.
		/// </summary>
		/// <value>
		/// true if the database supports upgrading, is in a ready or the new state, and the current version is less than the maximum supported version, false otherwise.
		/// </value>
		bool CanUpgrade { get; }

		/// <summary>
		/// Gets the lowest version supported for upgrading.
		/// </summary>
		/// <value>
		/// The lowest version supported for upgrading (as a source version) or -1 if upgrading is not supported.
		/// </value>
		int MinVersion { get; }

		/// <summary>
		/// Gets the highest version supported for upgrading.
		/// </summary>
		/// <value>
		/// The highest version supported for upgrading (as a target version) or -1 if upgrading is not supported.
		/// </value>
		int MaxVersion { get; }

		/// <summary>
		/// Gets whether the database supports connection tracking.
		/// </summary>
		/// <value>
		/// true if the database supports connection tracking, false otherwise.
		/// </value>
		bool SupportsConnectionTracking { get; }

		/// <summary>
		/// Gets whether the database supports read-only connections.
		/// </summary>
		/// <value>
		/// true if the database supports read-only connections, false otherwise.
		/// </value>
		bool SupportsReadOnly { get; }

		/// <summary>
		/// Gets whether the database supports the cleanup functionality.
		/// </summary>
		/// <value>
		/// true if the database supports cleanup, false otherwise.
		/// </value>
		bool SupportsCleanup { get; }

		/// <summary>
		/// Gets whether the database supports the backup functionality.
		/// </summary>
		/// <value>
		/// true if the database supports backup, false otherwise.
		/// </value>
		bool SupportsBackup { get; }

		/// <summary>
		/// Gets whether the database supports the restore functionality.
		/// </summary>
		/// <value>
		/// true if the database supports restore, false otherwise.
		/// </value>
		bool SupportsRestore { get; }

		/// <summary>
		/// Gets whether the database supports the upgrade functionality.
		/// </summary>
		/// <value>
		/// true if the database supports upgrading, false otherwise.
		/// </value>
		bool SupportsUpgrade { get; }

		/// <summary>
		/// Initializes the database.
		/// </summary>
		/// <remarks>
		/// <para>
		/// <see cref="State"/> and <see cref="Version"/> as well as <see cref="InitialState"/> and <see cref="InitialVersion"/> are updated to reflect the current state and version of the database after initialization.
		/// </para>
		/// <note type="note">
		/// If the database is already initialized, it will be closed first (implicitly calling <see cref="Close"/>).
		/// </note>
		/// </remarks>
		/// <exception cref="InvalidDatabaseConfigurationException">The database configuration specified by <see cref="Configuration"/> is not valid.</exception>
		void Initialize ();

		/// <summary>
		/// Closes the database.
		/// </summary>
		/// <remarks>
		/// <para>
		/// <see cref="State"/> and <see cref="InitialState"/> are set to <see cref="DatabaseState.Uninitialized"/> and <see cref="Version"/> and <see cref="InitialVersion"/> are set to -1.
		/// </para>
		/// <note type="note">
		/// All tracked connections will be closed.
		/// Untracked connections remain unchanged.
		/// </note>
		/// </remarks>
		void Close ();

		/// <summary>
		/// Creates a new connection which can be used to work with the database.
		/// </summary>
		/// <param name="readOnly">Specifies whether the connection should be read-only.</param>
		/// <param name="track">Specifies whether the connection should be tracked.</param>
		/// <returns>
		/// The newly created and already opened connection.
		/// </returns>
		/// <exception cref="InvalidOperationException">The database is not in a ready state.</exception>
		/// <exception cref="NotSupportedException"><paramref name="readOnly"/> is true but read-only connections are not supported or <paramref name="track"/> is true but connection tracking is not supported.</exception>
		/// <exception cref="InvalidDatabaseConfigurationException">The database configuration specified by <see cref="Configuration"/> is not valid.</exception>
		DbConnection CreateConnection (bool readOnly, bool track);

		/// <summary>
		/// Performs a database cleanup using the configured <see cref="IDatabaseCleanupProcessor"/>.
		/// </summary>
		/// <remarks>
		/// <para>
		/// <see cref="State"/> and <see cref="Version"/> are updated to reflect the current state and version of the database after cleanup.
		/// </para>
		/// </remarks>
		/// <exception cref="InvalidOperationException">The database is not in a ready state.</exception>
		/// <exception cref="NotSupportedException">Cleanup is not supported by the database or no <see cref="IDatabaseCleanupProcessor"/> is configured.</exception>
		/// <exception cref="InvalidDatabaseConfigurationException">The database configuration specified by <see cref="Configuration"/> is not valid.</exception>
		void Cleanup ();

		/// <summary>
		/// Performs a backup using the configured <see cref="IDatabaseBackupCreator"/>.
		/// </summary>
		/// <param name="backupFile">The file the database backup is written to.</param>
		/// <remarks>
		/// <para>
		/// <see cref="State"/> and <see cref="Version"/> are updated to reflect the current state and version of the database after backup.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException"><paramref name="backupFile"/> is null.</exception>
		/// <exception cref="InvalidPathArgumentException"><paramref name="backupFile"/> is not a valid path.</exception>
		/// <exception cref="InvalidOperationException">The database is not in a ready state.</exception>
		/// <exception cref="NotSupportedException">Backup is not supported by the database or no <see cref="IDatabaseBackupCreator"/> is configured.</exception>
		/// <exception cref="InvalidDatabaseConfigurationException">The database configuration specified by <see cref="Configuration"/> is not valid.</exception>
		void Backup (FilePath backupFile);

		/// <summary>
		/// Performs a backup using the configured <see cref="IDatabaseBackupCreator"/>.
		/// </summary>
		/// <param name="backupFile">The file which is used to restore the database from.</param>
		/// <remarks>
		/// <para>
		/// <see cref="State"/> and <see cref="Version"/> are updated to reflect the current state and version of the database after restore.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException"><paramref name="backupFile"/> is null.</exception>
		/// <exception cref="InvalidPathArgumentException"><paramref name="backupFile"/> is not a valid path.</exception>
		/// <exception cref="FileNotFoundException"><paramref name="backupFile"/> does not exist.</exception>
		/// <exception cref="InvalidOperationException">The database is not initialized.</exception>
		/// <exception cref="NotSupportedException">Restore is not supported by the database or no <see cref="IDatabaseBackupCreator"/> is configured.</exception>
		/// <exception cref="InvalidDatabaseConfigurationException">The database configuration specified by <see cref="Configuration"/> is not valid.</exception>
		void Restore (FilePath backupFile);

		/// <summary>
		/// Performs an upgrade to a specific database version using the configured <see cref="IDatabaseVersionUpgrader"/>.
		/// </summary>
		/// <param name="version">The version to upgrade the database to.</param>
		/// <remarks>
		/// <para>
		/// <see cref="State"/> and <see cref="Version"/> are updated to reflect the current state and version of the database after upgrade.
		/// </para>
		/// <para>
		/// If <paramref name="version"/> is the same as <see cref="Version"/>, nothing is done.
		/// </para>
		/// <note type="implement">
		/// Upgrading is to be performed incrementally, upgrading from n to n+1 until the desired version, as specified by <paramref name="version"/>, is reached.
		/// </note>
		/// </remarks>
		/// <exception cref="InvalidOperationException">The database is not in a ready or the new state.</exception>
		/// <exception cref="NotSupportedException">Upgrading is not supported by the database or no <see cref="IDatabaseVersionUpgrader"/> is configured.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="version"/> is less than <see cref="MinVersion"/>, greater than <see cref="MaxVersion"/>, or less than <see cref="Version"/>.</exception>
		/// <exception cref="InvalidDatabaseConfigurationException">The database configuration specified by <see cref="Configuration"/> is not valid.</exception>
		void Upgrade (int version);

		/// <summary>
		/// Performs an upgrade to highest supported version using the configured <see cref="IDatabaseVersionUpgrader"/>.
		/// </summary>
		/// <remarks>
		/// <para>
		/// <see cref="State"/> and <see cref="Version"/> are updated to reflect the current state and version of the database after upgrade.
		/// </para>
		/// <para>
		/// If <see cref="MaxVersion"/> is the same as <see cref="Version"/>, nothing is done.
		/// </para>
		/// <note type="implement">
		/// Upgrading is to be performed incrementally, upgrading from n to n+1 until the desired version, <see cref="MaxVersion"/>, is reached.
		/// </note>
		/// </remarks>
		/// <exception cref="InvalidOperationException">The database is not in a ready or the new state.</exception>
		/// <exception cref="NotSupportedException">Upgrading is not supported by the database or no <see cref="IDatabaseVersionUpgrader"/> is configured.</exception>
		/// <exception cref="InvalidDatabaseConfigurationException">The database configuration specified by <see cref="Configuration"/> is not valid.</exception>
		void Upgrade ();

		/// <summary>
		/// Gets a list of all tracked connections.
		/// </summary>
		/// <returns>
		/// The list of tracked connections or null if connection tracking is not supported.
		/// If connection tracking is supported but no connections are currently being tracked, an empty list is returned.
		/// </returns>
		/// <remarks>
		/// <note type="implement">
		/// Connections which are in a <see cref="ConnectionState.Closed"/> or <see cref="ConnectionState.Broken"/> state must be removed from the list of tracked connections.
		/// </note>
		/// </remarks>
		List<DbConnection> GetTrackedConnections ();

		/// <summary>
		/// Closes all currently tracked connections.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Connections which were created untracked remain unchanged.
		/// </para>
		/// </remarks>
		void CloseTrackedConnections ();

		/// <summary>
		/// Raised when the state of this database has changed (<see cref="State"/>).
		/// </summary>
		event EventHandler<DatabaseStateChangedEventArgs> StateChanged;

		/// <summary>
		/// Raised when the version of this database has changed (<see cref="Version"/>).
		/// </summary>
		event EventHandler<DatabaseVersionChangedEventArgs> VersionChanged;

		/// <summary>
		/// Raised when the state of a tracked connection has changed.
		/// </summary>
		event EventHandler<DatabaseConnectionChangedEventArgs> ConnectionChanged;

		/// <summary>
		/// Raised when a connection has been created.
		/// </summary>
		event EventHandler<DatabaseConnectionCreatedEventArgs> ConnectionCreated;
	}

	/// <inheritdoc cref="IDatabaseManager"/>
	/// <typeparam name="TConnection">The database connection type, subclass of <see cref="DbConnection"/>.</typeparam>
	/// <typeparam name="TConnectionStringBuilder">The connection string builder type, subclass of <see cref="DbConnectionStringBuilder"/>.</typeparam>
	/// <typeparam name="TManager">The type of the database manager which is implementing this interface.</typeparam>
	public interface IDatabaseManager<TConnection, TConnectionStringBuilder, TManager> : IDatabaseManager
		where TConnection : DbConnection
		where TConnectionStringBuilder : DbConnectionStringBuilder
		where TManager : IDatabaseManager<TConnection, TConnectionStringBuilder, TManager>
	{
		/// <inheritdoc cref="IDatabaseManager.Configuration"/>
		new IDatabaseManagerConfiguration<TConnection, TConnectionStringBuilder, TManager> Configuration { get; }

		/// <inheritdoc cref="IDatabaseManager.CreateConnection"/>
		new TConnection CreateConnection (bool readOnly, bool track);

		/// <inheritdoc cref="IDatabaseManager.GetTrackedConnections"/>
		new List<TConnection> GetTrackedConnections();

		/// <inheritdoc cref="IDatabaseManager.ConnectionChanged"/>
		new event EventHandler<DatabaseConnectionChangedEventArgs<TConnection>> ConnectionChanged;

		/// <inheritdoc cref="IDatabaseManager.ConnectionCreated"/>
		new event EventHandler<DatabaseConnectionCreatedEventArgs<TConnection>> ConnectionCreated;
	}
}
