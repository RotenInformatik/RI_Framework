﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using RI.Framework.Data.Database.Scripts;
using RI.Framework.Utilities;
using RI.Framework.Utilities.Logging;

namespace RI.Framework.Data.Database.Cleanup
{
	/// <summary>
	/// Implements a database cleanup processor for SQL Server databases.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <see cref="SqlServerDatabaseCleanupProcessor"/> can be used with either a default SQL Server cleanup script (<see cref="DefaultCleanupScript"/>) or with a custom processing step.
	/// </para>
	/// </remarks>
	public sealed class SqlServerDatabaseCleanupProcessor : DatabaseCleanupProcessor<SqlConnection, SqlTransaction, SqlConnectionStringBuilder, SqlServerDatabaseManager, SqlServerDatabaseManagerConfiguration>
	{
		/// <summary>
		/// The default cleanup script used when no custom processing step is specified.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The default cleanup script uses <c>DBCC SHRINKDATABASE 0</c>, executed as a single command.
		/// </para>
		/// </remarks>
		public const string DefaultCleanupScript = "DBCC SHRINKDATABASE 0;" + DatabaseScriptLocator.DefaultBatchSeparator;

		/// <summary>
		/// Creates a new instance of <see cref="SqlServerDatabaseCleanupProcessor"/>.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The default cleanup script is used (<see cref="DefaultCleanupScript"/>).
		/// </para>
		/// </remarks>
		public SqlServerDatabaseCleanupProcessor()
			: this(null)
		{
		}

		/// <summary>
		/// Creates a new instance of <see cref="SqlServerDatabaseCleanupProcessor"/>.
		/// </summary>
		/// <param name="cleanupStep">The custom processing step which performs the cleanup or null if the default cleanup script is used (<see cref="DefaultCleanupScript"/>).</param>
		public SqlServerDatabaseCleanupProcessor(SqlServerDatabaseProcessingStep cleanupStep)
		{
			this.CleanupStep = cleanupStep;
		}

		/// <summary>
		/// Gets the custom processing step which performs the cleanup.
		/// </summary>
		/// <value>
		/// The custom processing step which performs the cleanup or null if the default cleanup script is used (<see cref="DefaultCleanupScript"/>).
		/// </value>
		public SqlServerDatabaseProcessingStep CleanupStep { get; }

		/// <inheritdoc />
		public override bool Cleanup(SqlServerDatabaseManager manager)
		{
			if (manager == null)
			{
				throw new ArgumentNullException(nameof(manager));
			}

			try
			{
				SqlConnectionStringBuilder connectionString = new SqlConnectionStringBuilder(manager.Configuration.ConnectionString.ConnectionString);

				this.Log(LogLevel.Information, "Beginning SQL Server database cleanup: Connection=[{0}]", connectionString.ConnectionString);

				using (SqlConnection connection = new SqlConnection(connectionString.ConnectionString))
				{
					connection.Open();

					SqlServerDatabaseProcessingStep cleanupStep = this.CleanupStep;
					if (cleanupStep == null)
					{
						List<string> batches = DatabaseScriptLocator.SplitBatches(SqlServerDatabaseCleanupProcessor.DefaultCleanupScript, DatabaseScriptLocator.DefaultBatchSeparator);
						cleanupStep = new SqlServerDatabaseProcessingStep();
						cleanupStep.AddBatches(batches);
					}

					using (SqlTransaction transaction = cleanupStep.RequiresTransaction ? connection.BeginTransaction(IsolationLevel.Serializable) : null)
					{
						cleanupStep.Execute(manager, connection, transaction);
						transaction?.Commit();
					}
				}

				this.Log(LogLevel.Information, "Finished SQL Server database cleanup: Connection=[{0}]", connectionString.ConnectionString);

				return true;
			}
			catch (Exception exception)
			{
				this.Log(LogLevel.Error, "SQL Server database cleanup failed:{0}{1}", Environment.NewLine, exception.ToDetailedString());
				return false;
			}
		}

		/// <inheritdoc />
		public override bool RequiresScriptLocator => this.CleanupStep?.RequiresScriptLocator ?? false;
	}
}