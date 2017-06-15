﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using RI.Framework.IO.Paths;
using RI.Framework.Services.Logging.Writers;
using RI.Framework.Utilities;
using RI.Framework.Utilities.Exceptions;




namespace RI.Framework.Services.Logging.Readers
{
	/// <summary>
	///     Converts log files generated by <see cref="FileLogWriter" /> and <see cref="DirectoryLogWriter" /> and adds their entries to a SQLite database file.
	/// </summary>
	/// <remarks>
	///     <para>
	///         If the specified database file already exists and has already log entries, the entries are added to the existing ones.
	///     </para>
	///     <para>
	///         The log entries are written to the table &quot;Log&quot;, using the following columns: File, Timestamp, ThreadId, Severity, Source, Message.
	///     </para>
	/// </remarks>
	public sealed class LogFileToDbConverter
	{
		#region Instance Constructor/Destructor

		/// <summary>
		///     Creates a new instance of <see cref="LogFileToDbConverter" />.
		/// </summary>
		/// <remarks>
		///     <para>
		///         The default encoding <see cref="LogFileReader.DefaultEncoding" /> is used as the text encoding to read the log file.
		///     </para>
		/// </remarks>
		public LogFileToDbConverter ()
			: this(null)
		{
		}

		/// <summary>
		///     Creates a new instance of <see cref="LogFileToDbConverter" />.
		/// </summary>
		/// <param name="encoding"> The text encoding which is used to read the log file (can be null to use <see cref="LogFileReader.DefaultEncoding" />). </param>
		public LogFileToDbConverter (Encoding encoding)
		{
			this.Encoding = encoding ?? LogFileReader.DefaultEncoding;
		}

		#endregion




		#region Instance Properties/Indexer

		/// <summary>
		///     Gets the encoding which is used to read the log file.
		/// </summary>
		/// <value>
		///     The encoding to read the log file.
		/// </value>
		public Encoding Encoding { get; private set; }

		#endregion




		#region Instance Methods

		/// <summary>
		///     Converts all log files in a log directory, as created by <see cref="DirectoryLogWriter" />, and adds them to a log database.
		/// </summary>
		/// <param name="logDirectory"> The log directory. </param>
		/// <param name="dbFile"> The log database. </param>
		/// <returns>
		///     The conversion results.
		/// </returns>
		/// <remarks>
		///     <para>
		///         The default file name <see cref="DirectoryLogWriter.DefaultFileName" /> is used as the file name of the text log file in the log subdirectories.
		///     </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException"> <paramref name="logDirectory" /> or <paramref name="dbFile" /> is null. </exception>
		/// <exception cref="InvalidPathArgumentException"> <paramref name="logDirectory" /> or <paramref name="dbFile" /> contains wildcards. </exception>
		/// <exception cref="DirectoryNotFoundException"> The log directory as specified by <paramref name="logDirectory" /> does not exist. </exception>
		public LogFileDbConverterResults ConvertDirectories (DirectoryPath logDirectory, FilePath dbFile)
		{
			return this.ConvertDirectories(logDirectory, dbFile, null);
		}

		/// <summary>
		///     Converts all log files in a log directory, as created by <see cref="DirectoryLogWriter" />, and adds them to a log database.
		/// </summary>
		/// <param name="logDirectory"> The log directory. </param>
		/// <param name="dbFile"> The log database. </param>
		/// <param name="fileName"> The file name of the text log files in the log subdirectories (can be null to use <see cref="DirectoryLogWriter.DefaultFileName" />). </param>
		/// <returns>
		///     The conversion results.
		/// </returns>
		/// <exception cref="ArgumentNullException"> <paramref name="logDirectory" /> or <paramref name="dbFile" /> is null. </exception>
		/// <exception cref="InvalidPathArgumentException"> <paramref name="logDirectory" /> or <paramref name="dbFile" /> contains wildcards or <paramref name="fileName" /> is not a valid file name. </exception>
		/// <exception cref="DirectoryNotFoundException"> The log directory as specified by <paramref name="logDirectory" /> does not exist. </exception>
		public LogFileDbConverterResults ConvertDirectories (DirectoryPath logDirectory, FilePath dbFile, string fileName)
		{
			if (logDirectory == null)
			{
				throw new ArgumentNullException(nameof(logDirectory));
			}

			if (logDirectory.HasWildcards)
			{
				throw new InvalidPathArgumentException(nameof(logDirectory), "Wildcards are not allowed.");
			}

			if (!logDirectory.Exists)
			{
				throw new DirectoryNotFoundException("The log directory does not exist: " + logDirectory + ".");
			}

			if (dbFile == null)
			{
				throw new ArgumentNullException(nameof(dbFile));
			}

			if (dbFile.HasWildcards)
			{
				throw new InvalidPathArgumentException(nameof(dbFile), "Wildcards are not allowed.");
			}

			FilePath fileNamePath;
			try
			{
				fileNamePath = new FilePath(fileName ?? DirectoryLogWriter.DefaultFileName, false, true, PathProperties.GetSystemType());
			}
			catch (InvalidPathArgumentException)
			{
				throw new InvalidPathArgumentException(nameof(fileName), "Invalid file name.");
			}

			HashSet<FilePath> files = this.GetLogFilesFromDirectory(logDirectory, fileNamePath);
			LogFileDbConverterResults results = this.ConvertFiles(files, dbFile);
			return results;
		}

		/// <summary>
		///     Converts all log files in a log directory, as created by <see cref="DirectoryLogWriter" />, and adds them to a log database.
		/// </summary>
		/// <param name="logDirectory"> The log directory. </param>
		/// <param name="dbFile"> The log database. </param>
		/// <returns>
		///     The conversion results.
		/// </returns>
		/// <remarks>
		///     <para>
		///         The default file name <see cref="DirectoryLogWriter.DefaultFileName" /> is used as the file name of the text log file in the log subdirectories.
		///     </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException"> <paramref name="logDirectory" /> or <paramref name="dbFile" /> is null. </exception>
		/// <exception cref="InvalidPathArgumentException"> <paramref name="logDirectory" /> or <paramref name="dbFile" /> contains wildcards. </exception>
		/// <exception cref="DirectoryNotFoundException"> The log directory as specified by <paramref name="logDirectory" /> does not exist. </exception>
		public async Task<LogFileDbConverterResults> ConvertDirectoriesAsync (DirectoryPath logDirectory, FilePath dbFile)
		{
			return await this.ConvertDirectoriesAsync(logDirectory, dbFile, null);
		}

		/// <summary>
		///     Converts all log files in a log directory, as created by <see cref="DirectoryLogWriter" />, and adds them to a log database.
		/// </summary>
		/// <param name="logDirectory"> The log directory. </param>
		/// <param name="dbFile"> The log database. </param>
		/// <param name="fileName"> The file name of the text log files in the log subdirectories (can be null to use <see cref="DirectoryLogWriter.DefaultFileName" />). </param>
		/// <returns>
		///     The conversion results.
		/// </returns>
		/// <exception cref="ArgumentNullException"> <paramref name="logDirectory" /> or <paramref name="dbFile" /> is null. </exception>
		/// <exception cref="InvalidPathArgumentException"> <paramref name="logDirectory" /> or <paramref name="dbFile" /> contains wildcards or <paramref name="fileName" /> is not a valid file name. </exception>
		/// <exception cref="DirectoryNotFoundException"> The log directory as specified by <paramref name="logDirectory" /> does not exist. </exception>
		public async Task<LogFileDbConverterResults> ConvertDirectoriesAsync (DirectoryPath logDirectory, FilePath dbFile, string fileName)
		{
			if (logDirectory == null)
			{
				throw new ArgumentNullException(nameof(logDirectory));
			}

			if (logDirectory.HasWildcards)
			{
				throw new InvalidPathArgumentException(nameof(logDirectory), "Wildcards are not allowed.");
			}

			if (!logDirectory.Exists)
			{
				throw new DirectoryNotFoundException("The log directory does not exist: " + logDirectory + ".");
			}

			if (dbFile == null)
			{
				throw new ArgumentNullException(nameof(dbFile));
			}

			if (dbFile.HasWildcards)
			{
				throw new InvalidPathArgumentException(nameof(dbFile), "Wildcards are not allowed.");
			}

			FilePath fileNamePath;
			try
			{
				fileNamePath = new FilePath(fileName ?? DirectoryLogWriter.DefaultFileName, false, true, PathProperties.GetSystemType());
			}
			catch (InvalidPathArgumentException)
			{
				throw new InvalidPathArgumentException(nameof(fileName), "Invalid file name.");
			}

			return await Task<LogFileDbConverterResults>.Factory.StartNew(x =>
			{
				Tuple<DirectoryPath, FilePath, string> state = (Tuple<DirectoryPath, FilePath, string>)x;
				return this.ConvertDirectories(state.Item1, state.Item2, state.Item3);
			}, new Tuple<DirectoryPath, FilePath, string>(logDirectory, dbFile, fileNamePath));
		}

		/// <summary>
		///     Converts a single log file and adds it to a log database.
		/// </summary>
		/// <param name="logFile"> The log file. </param>
		/// <param name="dbFile"> The log database. </param>
		/// <returns>
		///     The conversion results.
		/// </returns>
		/// <exception cref="ArgumentNullException"> <paramref name="logFile" /> or <paramref name="dbFile" /> is null. </exception>
		/// <exception cref="InvalidPathArgumentException"> <paramref name="logFile" /> or <paramref name="dbFile" /> contains wildcards. </exception>
		/// <exception cref="FileNotFoundException"> The log file as specified by <paramref name="logFile" /> does not exist. </exception>
		public LogFileDbConverterResults ConvertFile (FilePath logFile, FilePath dbFile)
		{
			if (logFile == null)
			{
				throw new ArgumentNullException(nameof(logFile));
			}

			if (logFile.HasWildcards)
			{
				throw new InvalidPathArgumentException(nameof(logFile), "Wildcards are not allowed.");
			}

			if (!logFile.Exists)
			{
				throw new FileNotFoundException("The log file does not exist: " + logFile + ".", logFile);
			}

			if (dbFile == null)
			{
				throw new ArgumentNullException(nameof(dbFile));
			}

			if (dbFile.HasWildcards)
			{
				throw new InvalidPathArgumentException(nameof(dbFile), "Wildcards are not allowed.");
			}

			LogFileDbConverterResults results = this.ConvertFiles(new[] {logFile}, dbFile);
			return results;
		}

		/// <summary>
		///     Converts a single log file and adds it to a log database.
		/// </summary>
		/// <param name="logFile"> The log file. </param>
		/// <param name="dbFile"> The log database. </param>
		/// <returns>
		///     The conversion results.
		/// </returns>
		/// <exception cref="ArgumentNullException"> <paramref name="logFile" /> or <paramref name="dbFile" /> is null. </exception>
		/// <exception cref="InvalidPathArgumentException"> <paramref name="logFile" /> or <paramref name="dbFile" /> contains wildcards. </exception>
		/// <exception cref="FileNotFoundException"> The log file as specified by <paramref name="logFile" /> does not exist. </exception>
		public async Task<LogFileDbConverterResults> ConvertFileAsync (FilePath logFile, FilePath dbFile)
		{
			if (logFile == null)
			{
				throw new ArgumentNullException(nameof(logFile));
			}

			if (logFile.HasWildcards)
			{
				throw new InvalidPathArgumentException(nameof(logFile), "Wildcards are not allowed.");
			}

			if (!logFile.Exists)
			{
				throw new FileNotFoundException("The log file does not exist: " + logFile + ".", logFile);
			}

			if (dbFile == null)
			{
				throw new ArgumentNullException(nameof(dbFile));
			}

			if (dbFile.HasWildcards)
			{
				throw new InvalidPathArgumentException(nameof(dbFile), "Wildcards are not allowed.");
			}

			return await Task<LogFileDbConverterResults>.Factory.StartNew(x =>
			{
				Tuple<FilePath, FilePath> state = (Tuple<FilePath, FilePath>)x;
				return this.ConvertFile(state.Item1, state.Item2);
			}, new Tuple<FilePath, FilePath>(logFile, dbFile));
		}

		private LogFileDbConverterResults ConvertFiles (IEnumerable<FilePath> files, FilePath dbFile)
		{
			SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();
			builder.DataSource = dbFile.PathResolved;
			builder.ForeignKeys = true;
			builder.FailIfMissing = false;

			LogFileDbConverterResults results = new LogFileDbConverterResults();

			using (SQLiteConnection connection = new SQLiteConnection(builder.ToString()))
			{
				connection.Open();

				using (SQLiteCommand createTableCommand = new SQLiteCommand("CREATE TABLE IF NOT EXISTS [Log] ([File] TEXT NOT NULL, [Timestamp] DATETIME NOT NULL, [ThreadId] INTEGER NOT NULL, [Severity] INTEGER NOT NULL, [Source] TEXT NOT NULL, [Message] TEXT NOT NULL);", connection))
				{
					createTableCommand.ExecuteNonQuery();
				}

				using (SQLiteCommand createIndicesCommand = new SQLiteCommand("CREATE INDEX IF NOT EXISTS [Log_File] ON [Log] ([File]); CREATE INDEX IF NOT EXISTS [Log_Timestamp] ON [Log] ([Timestamp]); CREATE INDEX IF NOT EXISTS [Log_ThreadId] ON [Log] ([ThreadId]); CREATE INDEX IF NOT EXISTS [Log_Severity] ON [Log] ([Severity]); CREATE INDEX IF NOT EXISTS [Log_Source] ON [Log] ([Source]);", connection))
				{
					createIndicesCommand.ExecuteNonQuery();
				}

				using (SQLiteTransaction transaction = connection.BeginTransaction())
				{
					foreach (FilePath file in files)
					{
						results.Files.Add(file);

						int count = 0;
						using (LogFileReader lfr = new LogFileReader(file, this.Encoding))
						{
							while (lfr.ReadNext())
							{
								if (lfr.CurrentValid)
								{
									using (SQLiteCommand insertCommand = new SQLiteCommand("INSERT INTO [Log] ([File], [Timestamp], [ThreadId], [Severity], [Source], [Message]) VALUES (@file, @timestamp, @threadId, @severity, @source, @message)", connection, transaction))
									{
										insertCommand.Parameters.AddWithValue("@file", file);
										insertCommand.Parameters.AddWithValue("@timestamp", lfr.CurrentEntry.Timestamp);
										insertCommand.Parameters.AddWithValue("@threadId", lfr.CurrentEntry.ThreadId);
										insertCommand.Parameters.AddWithValue("@severity", lfr.CurrentEntry.Severity);
										insertCommand.Parameters.AddWithValue("@source", lfr.CurrentEntry.Source);
										insertCommand.Parameters.AddWithValue("@message", lfr.CurrentEntry.Message);
										insertCommand.ExecuteNonQuery();
									}
								}
								else
								{
									results.Errors.Add(new Tuple<FilePath, int>(file, lfr.CurrentLineNumber));
								}
							}
						}

						results.Entries.Add(new Tuple<FilePath, int>(file, count));
					}

					transaction.Commit();
				}
			}

			return results;
		}

		private HashSet<FilePath> GetLogFilesFromDirectory (DirectoryPath logDirectory, FilePath fileName)
		{
			HashSet<FilePath> files = new HashSet<FilePath>();
			List<DirectoryPath> subdirectories = logDirectory.GetSubdirectories(false, false);
			foreach (DirectoryPath subdirectory in subdirectories)
			{
				DateTime? timestamp = subdirectory.DirectoryName.ToDateTimeFromSortable('-');
				if (!timestamp.HasValue)
				{
					continue;
				}

				FilePath fileCandidate = subdirectory.AppendFile(fileName);
				if (!fileCandidate.Exists)
				{
					continue;
				}

				files.Add(fileCandidate);
			}
			return files;
		}

		#endregion
	}
}
