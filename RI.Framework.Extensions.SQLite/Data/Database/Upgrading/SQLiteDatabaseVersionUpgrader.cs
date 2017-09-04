﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using RI.Framework.Utilities;
using RI.Framework.Utilities.Logging;

namespace RI.Framework.Data.Database.Upgrading
{
	/// <summary>
	/// Implements a database version upgrader for SQLite databases.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <see cref="SQLiteDatabaseVersionUpgrader"/> uses upgrade steps associated to specific source versions to perform the upgrade.
	/// </para>
	/// </remarks>
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public sealed class SQLiteDatabaseVersionUpgrader : DatabaseVersionUpgrader<SQLiteConnection, SQLiteTransaction, SQLiteConnectionStringBuilder, SQLiteDatabaseManager, SQLiteDatabaseManagerConfiguration>
	{
		/// <summary>
		/// Creates a new instance of <see cref="SQLiteDatabaseVersionUpgrader"/>.
		/// </summary>
		/// <param name="upgradeSteps">The sequence of upgrade steps supported by this version upgrader.</param>
		/// <remarks>
		/// <para>
		/// <paramref name="upgradeSteps"/> is enumerated only once.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException"><paramref name="upgradeSteps"/> is null.</exception>
		/// <exception cref="ArgumentException"><paramref name="upgradeSteps"/> is an empty sequence or contains the same source version multiple times.</exception>
		public SQLiteDatabaseVersionUpgrader (IEnumerable<SQLiteDatabaseVersionUpgradeStep> upgradeSteps)
		{
			if (upgradeSteps == null)
			{
				throw new ArgumentNullException(nameof(upgradeSteps));
			}

			this.UpgradeSteps = new List<SQLiteDatabaseVersionUpgradeStep>(upgradeSteps);

			if (this.UpgradeSteps.Count == 0)
			{
				throw new ArgumentException("No upgrade steps.", nameof(upgradeSteps));
			}

			foreach (int sourceVersion in this.UpgradeSteps.Select(x => x.SourceVersion))
			{
				int count = this.UpgradeSteps.Count(x => x.SourceVersion == sourceVersion);
				if (count != 1)
				{
					throw new ArgumentException("Source version " + sourceVersion + " specified multiple times.", nameof(upgradeSteps));
				}
			}
		}

		/// <summary>
		/// Creates a new instance of <see cref="SQLiteDatabaseVersionUpgrader"/>.
		/// </summary>
		/// <param name="upgradeSteps">The array of upgrade steps supported by this version upgrader.</param>
		/// <exception cref="ArgumentNullException"><paramref name="upgradeSteps"/> is null.</exception>
		/// <exception cref="ArgumentException"><paramref name="upgradeSteps"/> is an empty array or contains the same source version multiple times.</exception>
		public SQLiteDatabaseVersionUpgrader (params SQLiteDatabaseVersionUpgradeStep[] upgradeSteps)
			: this((IEnumerable<SQLiteDatabaseVersionUpgradeStep>)upgradeSteps)
		{
		}

		private List<SQLiteDatabaseVersionUpgradeStep> UpgradeSteps { get; }

		/// <summary>
		/// Gets the list of available upgrade steps.
		/// </summary>
		/// <returns>
		/// The list of available upgrade steps.
		/// The list is never empty.
		/// </returns>
		public List<SQLiteDatabaseVersionUpgradeStep> GetUpgradeSteps () => new List<SQLiteDatabaseVersionUpgradeStep>(this.UpgradeSteps);

		/// <inheritdoc />
		public override bool Upgrade (SQLiteDatabaseManager manager, int sourceVersion)
		{
			if (manager == null)
			{
				throw new ArgumentNullException(nameof(manager));
			}

			if (sourceVersion < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(sourceVersion));
			}

			try
			{
				SQLiteConnectionStringBuilder connectionString = new SQLiteConnectionStringBuilder(manager.Configuration.ConnectionString.ConnectionString);

				this.Log(LogLevel.Information, "Beginning SQLite database upgrade step: SourceVersion=[{0}]; Connection=[{1}]", sourceVersion, connectionString.ConnectionString);

				SQLiteDatabaseVersionUpgradeStep upgradeStep = this.UpgradeSteps.FirstOrDefault(x => x.SourceVersion == sourceVersion);
				if (upgradeStep == null)
				{
					throw new Exception("No upgrade step found for source version: " + sourceVersion);
				}

				using (SQLiteConnection connection = new SQLiteConnection(connectionString.ConnectionString))
				{
					connection.Open();

					using (SQLiteTransaction transaction = upgradeStep.RequiresTransaction ? connection.BeginTransaction(IsolationLevel.Serializable) : null)
					{
						upgradeStep.Execute(manager, connection, transaction);
						transaction?.Commit();
					}
				}

				this.Log(LogLevel.Information, "Finished SQLite database upgrade step: SourceVersion=[{0}]; Connection=[{1}]", sourceVersion, connectionString.ConnectionString);

				return true;
			}
			catch (Exception exception)
			{
				this.Log(LogLevel.Error, "SQLite database upgrade step failed:{0}{1}", Environment.NewLine, exception.ToDetailedString());
				return false;
			}
		}

		/// <inheritdoc />
		public override bool RequiresScriptLocator => this.UpgradeSteps.Any(x => x.RequiresScriptLocator);

		/// <inheritdoc />
		public override int GetMinVersion (SQLiteDatabaseManager manager) => this.UpgradeSteps.Select(x => x.SourceVersion).Min();

		/// <inheritdoc />
		public override int GetMaxVersion (SQLiteDatabaseManager manager) => this.UpgradeSteps.Select(x => x.SourceVersion).Max() + 1;
	}
}