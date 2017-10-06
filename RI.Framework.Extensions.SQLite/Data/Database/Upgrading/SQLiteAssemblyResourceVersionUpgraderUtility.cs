﻿using System.Data.SQLite;

namespace RI.Framework.Data.Database.Upgrading
{
	/// <summary>
	/// Implements an assembly version upgrade step extractor for SQLite databases.
	/// </summary>
	public sealed class SQLiteAssemblyResourceVersionUpgraderUtility : AssemblyResourceVersionUpgraderUtility<SQLiteDatabaseVersionUpgradeStep, SQLiteConnection, SQLiteTransaction, SQLiteConnectionStringBuilder, SQLiteDatabaseManager, SQLiteDatabaseManagerConfiguration>
	{
		/// <inheritdoc />
		protected override SQLiteDatabaseVersionUpgradeStep CreateProcessingStep (int sourceVersion, string resourceName)
		{
			SQLiteDatabaseVersionUpgradeStep upgradeStep = new SQLiteDatabaseVersionUpgradeStep(sourceVersion);
			upgradeStep.AddScript(resourceName, DatabaseProcessingStepTransactionRequirement.Required);
			return upgradeStep;
		}
	}
}
