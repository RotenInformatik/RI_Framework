﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using RI.Framework.Collections.Linq;
using RI.Framework.IO.INI;
using RI.Framework.IO.Paths;
using RI.Framework.Services.Logging;
using RI.Framework.Utilities;
using RI.Framework.Utilities.Exceptions;




namespace RI.Framework.Services.Resources.Sources
{
	/// <summary>
	///     Implements a resource set associated with a directory of a <see cref="DirectoryResourceSource" />.
	/// </summary>
	/// <remarks>
	///     <para>
	///         See <see cref="IResourceSet" /> and <see cref="DirectoryResourceSource" /> for more details.
	///     </para>
	/// </remarks>
	public sealed class DirectoryResourceSet : IResourceSet
	{
		#region Constants

		/// <summary>
		///     The file name of the settings file.
		/// </summary>
		/// <remarks>
		///     <para>
		///         The value is <c> [Settings].ini </c>.
		///     </para>
		/// </remarks>
		public const string SettingsFileName = "[Settings].ini";

		#endregion




		#region Instance Constructor/Destructor

		internal DirectoryResourceSet (DirectoryPath directory, DirectoryResourceSource source)
		{
			if (directory == null)
			{
				throw new ArgumentNullException(nameof(directory));
			}

			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			this.Id = directory.DirectoryName;

			this.Directory = directory;
			this.Source = source;

			this.SettingsFile = this.Directory.AppendFile(DirectoryResourceSet.SettingsFileName);
			this.IsValid = null;

			this.Name = null;
			this.Group = null;
			this.AlwaysLoad = false;
			this.Priority = 0;
			this.UiCulture = null;
			this.FormattingCulture = null;

			this.IsLoaded = false;
			this.IsLazyLoaded = false;

			this.Resources = new Dictionary<string, Tuple<FilePath, Loader>>(StringComparerEx.TrimmedInvariantCultureIgnoreCase);
		}

		#endregion




		#region Instance Properties/Indexer

		/// <summary>
		///     Gets the directory of this resource set.
		/// </summary>
		/// <value>
		///     The directory of this resource set.
		/// </value>
		public DirectoryPath Directory { get; private set; }

		/// <summary>
		///     Gets the settings file path of this resource set.
		/// </summary>
		/// <value>
		///     The settings file path of this resource set.
		/// </value>
		public FilePath SettingsFile { get; private set; }

		internal bool? IsValid { get; private set; }

		internal DirectoryResourceSource Source { get; private set; }

		private Dictionary<string, Tuple<FilePath, Loader>> Resources { get; set; }

		#endregion




		#region Instance Methods

		internal void Prepare ()
		{
			if (this.IsValid.HasValue)
			{
				return;
			}

			this.Log(LogLevel.Debug, "Preparing directory resource set: {0}", this.Directory);

			this.Name = null;
			this.Group = null;
			this.AlwaysLoad = false;
			this.Priority = 0;
			this.UiCulture = null;
			this.FormattingCulture = null;

			if (!this.SettingsFile.Exists)
			{
				this.Log(LogLevel.Error, "Settings file does not exist for directory resource set: {0}", this.SettingsFile);
				this.IsValid = false;
				return;
			}

			IniDocument iniDocument = new IniDocument();
			try
			{
				iniDocument.Load(this.SettingsFile, this.Source.FileEncoding);
			}
			catch (IniParsingException exception)
			{
				this.Log(LogLevel.Error, "Settings file is not a valid INI file: {0}{1}{2}", this.SettingsFile, Environment.NewLine, exception.ToDetailedString());
				this.IsValid = false;
				return;
			}

			Dictionary<string, string> settings = iniDocument.GetSection(null);

			string nameKey = nameof(this.Name);
			string groupKey = nameof(this.Group);
			string alwaysLoadKey = nameof(this.AlwaysLoad);
			string priorityKey = nameof(this.Priority);
			string uiCultureKey = nameof(this.UiCulture);
			string formattingCultureKey = nameof(this.FormattingCulture);

			if (settings.ContainsKey(nameKey))
			{
				string value = settings[nameKey];
				if (!value.IsEmpty())
				{
					this.Log(LogLevel.Debug, "Settings value: {0}={1} @ {2}", nameKey, value, this.SettingsFile);
					this.Name = value;
				}
				else
				{
					this.Log(LogLevel.Error, "Invalid settings value in settings file: {0}={1} @ {2}", nameKey, value, this.SettingsFile);
					this.IsValid = false;
					return;
				}
			}
			else
			{
				this.Log(LogLevel.Error, "Missing settings value in settings file: {0} @ {1}", nameKey, this.SettingsFile);
				this.IsValid = false;
				return;
			}

			if (settings.ContainsKey(groupKey))
			{
				string value = settings[groupKey];
				if (!value.IsEmpty())
				{
					this.Log(LogLevel.Debug, "Settings value: {0}={1} @ {2}", groupKey, value, this.SettingsFile);
					this.Group = value;
				}
				else
				{
					this.Log(LogLevel.Warning, "Invalid settings value in settings file: {0}={1} @ {2}", groupKey, value, this.SettingsFile);
				}
			}
			else
			{
				this.Log(LogLevel.Warning, "Missing settings value in settings file: {0} @ {1}", groupKey, this.SettingsFile);
			}

			if (settings.ContainsKey(alwaysLoadKey))
			{
				string value = settings[alwaysLoadKey];
				bool? candidate = value.ToBoolean();
				if (candidate.HasValue)
				{
					this.Log(LogLevel.Debug, "Settings value: {0}={1} @ {2}", alwaysLoadKey, value, this.SettingsFile);
					this.AlwaysLoad = candidate.Value;
				}
				else
				{
					this.Log(LogLevel.Warning, "Invalid settings value in settings file: {0}={1} @ {2}", alwaysLoadKey, value, this.SettingsFile);
				}
			}
			else
			{
				this.Log(LogLevel.Warning, "Missing settings value in settings file: {0} @ {1}", alwaysLoadKey, this.SettingsFile);
			}

			if (settings.ContainsKey(priorityKey))
			{
				string value = settings[priorityKey];
				int? candidate = value.ToInt32(NumberStyles.Integer, CultureInfo.InvariantCulture);
				if (candidate.HasValue)
				{
					this.Log(LogLevel.Debug, "Settings value: {0}={1} @ {2}", priorityKey, value, this.SettingsFile);
					this.Priority = candidate.Value;
				}
				else
				{
					this.Log(LogLevel.Warning, "Invalid settings value in settings file: {0}={1} @ {2}", priorityKey, value, this.SettingsFile);
				}
			}
			else
			{
				this.Log(LogLevel.Warning, "Missing settings value in settings file: {0} @ {1}", priorityKey, this.SettingsFile);
			}

			if (settings.ContainsKey(uiCultureKey))
			{
				string value = settings[uiCultureKey];
				CultureInfo candidate = null;
				try
				{
					candidate = new CultureInfo(value, false);
				}
				catch (CultureNotFoundException)
				{
					candidate = null;
				}
				if (candidate != null)
				{
					this.Log(LogLevel.Debug, "Settings value: {0}={1} @ {2}", uiCultureKey, value, this.SettingsFile);
					this.UiCulture = candidate;
				}
				else
				{
					this.Log(LogLevel.Warning, "Invalid settings value in settings file: {0}={1} @ {2}", uiCultureKey, value, this.SettingsFile);
				}
			}
			else
			{
				this.Log(LogLevel.Warning, "Missing settings value in settings file: {0} @ {1}", uiCultureKey, this.SettingsFile);
			}

			if (settings.ContainsKey(formattingCultureKey))
			{
				string value = settings[formattingCultureKey];
				CultureInfo candidate = null;
				try
				{
					candidate = new CultureInfo(value, false);
				}
				catch (CultureNotFoundException)
				{
					candidate = null;
				}
				if (candidate != null)
				{
					this.Log(LogLevel.Debug, "Settings value: {0}={1} @ {2}", formattingCultureKey, value, this.SettingsFile);
					this.FormattingCulture = candidate;
				}
				else
				{
					this.Log(LogLevel.Warning, "Invalid settings value in settings file: {0}={1} @ {2}", formattingCultureKey, value, this.SettingsFile);
				}
			}
			else
			{
				this.Log(LogLevel.Warning, "Missing settings value in settings file: {0} @ {1}", formattingCultureKey, this.SettingsFile);
			}

			this.IsValid = true;
		}

		private void Log (LogLevel severity, string format, params object[] args)
		{
			LogLocator.Log(severity, this.GetType().Name, format, args);
		}

		#endregion




		#region Interface: IResourceSet

		/// <inheritdoc />
		public bool AlwaysLoad { get; private set; }

		/// <inheritdoc />
		public IEnumerable<string> AvailableResources => this.Resources.Keys;

		/// <inheritdoc />
		public CultureInfo FormattingCulture { get; private set; }

		/// <inheritdoc />
		public string Group { get; private set; }

		/// <inheritdoc />
		public string Id { get; private set; }

		/// <inheritdoc />
		public bool IsLazyLoaded { get; private set; }

		/// <inheritdoc />
		public bool IsLoaded { get; private set; }

		/// <inheritdoc />
		public string Name { get; private set; }

		/// <inheritdoc />
		public int Priority { get; private set; }

		/// <inheritdoc />
		public CultureInfo UiCulture { get; private set; }

		/// <inheritdoc />
		public object GetRawValue (string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			if (name.IsEmpty())
			{
				throw new EmptyStringArgumentException(nameof(name));
			}

			if (!this.Resources.ContainsKey(name))
			{
				return null;
			}

			return this.Resources[name].Item2.Load();
		}

		/// <inheritdoc />
		public bool Load (bool lazyLoad)
		{
			this.Log(LogLevel.Debug, "Loading directory resource set: {0}", this.Directory);

			this.IsLoaded = true;
			this.IsLazyLoaded = lazyLoad;

			this.Resources.Clear();

			this.UpdateAvailable();

			if (!lazyLoad)
			{
				foreach (string resource in this.AvailableResources)
				{
					this.GetRawValue(resource);
				}
			}

			return lazyLoad;
		}

		/// <inheritdoc />
		public void Unload ()
		{
			this.Log(LogLevel.Debug, "Unloading directory resource set: {0}", this.Directory);

			this.Resources.Clear();

			this.IsLoaded = false;
			this.IsLazyLoaded = false;
		}

		/// <inheritdoc />
		public void UpdateAvailable ()
		{
			List<FilePath> existingFiles = this.Directory.GetFiles(false, false);
			HashSet<FilePath> newFiles = DirectLinq.Except(existingFiles, from x in this.Resources select x.Value.Item1);

			foreach (FilePath file in newFiles)
			{
				switch (file.ExtensionWithoutDot.ToUpperInvariant())
				{
					default:
					{
						this.Log(LogLevel.Warning, "Unknown resource file type: {0}", file);
						break;
					}

					case "PNG":
					{
						string name = file.FileNameWithoutExtension;
						ByteArrayLoader loader = new ByteArrayLoader(file);
						this.Resources.Add(name, new Tuple<FilePath, Loader>(file, loader));
						this.Log(LogLevel.Debug, "Added PNG resource file: {0} = {1}", name, file);
						break;
					}

					case "TXT":
					{
						string name = file.FileNameWithoutExtension;
						StringLoader loader = new StringLoader(file, this.Source.FileEncoding);
						this.Resources.Add(name, new Tuple<FilePath, Loader>(file, loader));
						this.Log(LogLevel.Debug, "Added TXT resource file: {0} = {1}", name, file);
						break;
					}

					case "INI":
					{
						IniDocument iniDocument = new IniDocument();
						try
						{
							iniDocument.Load(file, this.Source.FileEncoding);
							Dictionary<string, string> values = iniDocument.GetSection(null);
							foreach (KeyValuePair<string, string> value in values)
							{
								string name = value.Key;
								EagerLoader loader = new EagerLoader(value.Value);
								this.Resources.Add(name, new Tuple<FilePath, Loader>(file, loader));
							}
							this.Log(LogLevel.Debug, "Added INI resource file: {0}", file);
						}
						catch (IniParsingException exception)
						{
							this.Log(LogLevel.Error, "Invalid INI resource file: {0}{1}{2}", file, Environment.NewLine, exception.ToDetailedString());
						}
						break;
					}
				}
			}
		}

		#endregion




		#region Type: ByteArrayLoader

		private sealed class ByteArrayLoader : Loader
		{
			#region Instance Constructor/Destructor

			public ByteArrayLoader (FilePath file)
			{
				if (file == null)
				{
					throw new ArgumentNullException(nameof(file));
				}

				if (file.HasWildcards)
				{
					throw new InvalidPathArgumentException(nameof(file));
				}

				this.File = file;

				this.Data = null;
			}

			#endregion




			#region Instance Properties/Indexer

			private byte[] Data { get; set; }

			private FilePath File { get; set; }

			#endregion




			#region Overrides

			public override object Load ()
			{
				if (this.Data == null)
				{
					this.Data = this.File.ReadBytes();
				}

				return this.Data;
			}

			#endregion
		}

		#endregion




		#region Type: EagerLoader

		private sealed class EagerLoader : Loader
		{
			#region Instance Constructor/Destructor

			public EagerLoader (object value)
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				this.Value = value;
			}

			#endregion




			#region Instance Properties/Indexer

			private object Value { get; set; }

			#endregion




			#region Overrides

			public override object Load ()
			{
				return this.Value;
			}

			#endregion
		}

		#endregion




		#region Type: Loader

		private abstract class Loader
		{
			#region Abstracts

			public abstract object Load ();

			#endregion
		}

		#endregion




		#region Type: StringLoader

		private sealed class StringLoader : Loader
		{
			#region Instance Constructor/Destructor

			public StringLoader (FilePath file, Encoding encoding)
			{
				if (file == null)
				{
					throw new ArgumentNullException(nameof(file));
				}

				if (file.HasWildcards)
				{
					throw new InvalidPathArgumentException(nameof(file));
				}

				if (encoding == null)
				{
					throw new ArgumentNullException(nameof(encoding));
				}

				this.File = file;
				this.Encoding = encoding;

				this.Data = null;
			}

			#endregion




			#region Instance Properties/Indexer

			private string Data { get; set; }

			private Encoding Encoding { get; set; }

			private FilePath File { get; set; }

			#endregion




			#region Overrides

			public override object Load ()
			{
				if (this.Data == null)
				{
					this.Data = this.File.ReadText(this.Encoding);
				}

				return this.Data;
			}

			#endregion
		}

		#endregion
	}
}