﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;




namespace RI.Framework.IO.CSV
{
	/// <summary>
	///     Implements a forward-only CSV reader which iteratively reads CSV data.
	/// </summary>
	/// <remarks>
	///     <para>
	///         See <see cref="CsvDocument" /> for more general and detailed information about working with CSV data.
	///     </para>
	/// </remarks>
	public sealed class CsvReader : IDisposable
	{
		#region Instance Constructor/Destructor

		/// <summary>
		///     Creates a new instance of <see cref="CsvReader" />.
		/// </summary>
		/// <param name="reader"> The used <see cref="TextReader" />. </param>
		/// <remarks>
		///     <para>
		///         CSV reader settings with default values are used.
		///     </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException"> <paramref name="reader" /> is null. </exception>
		public CsvReader (TextReader reader)
			: this(reader, null)
		{
		}

		/// <summary>
		///     Creates a new instance of <see cref="CsvReader" />.
		/// </summary>
		/// <param name="reader"> The used <see cref="TextReader" />. </param>
		/// <param name="settings"> The used CSV reader settings or null if default values should be used. </param>
		/// <exception cref="ArgumentNullException"> <paramref name="reader" /> is null. </exception>
		public CsvReader (TextReader reader, CsvReaderSettings settings)
		{
			if (reader == null)
			{
				throw new ArgumentNullException(nameof(reader));
			}

			this.BaseReader = reader;
			this.Settings = settings ?? new CsvReaderSettings();

			this.CurrentLineNumber = 0;
			this.CurrentRow = null;
			this.CurrentError = CsvReaderError.None;
		}

		/// <summary>
		///     Garbage collects this instance of <see cref="CsvReader" />.
		/// </summary>
		~CsvReader ()
		{
			this.Dispose(false);
		}

		#endregion




		#region Instance Properties/Indexer

		/// <summary>
		///     Gets the <see cref="TextReader" /> which is used by this CSV reader to read the CSV data.
		/// </summary>
		/// <value>
		///     The <see cref="TextReader" /> which is used by this CSV reader to read the CSV data or null if the the CSV reader is closed/disposed.
		/// </value>
		public TextReader BaseReader { get; private set; }

		/// <summary>
		///     Gets the current error which ocurred during the last call to <see cref="ReadNext" />.
		/// </summary>
		/// <value>
		///     The current error.
		/// </value>
		/// <remarks>
		///     <para>
		///         Before the first call to <see cref="ReadNext" />, this property is <see cref="CsvReaderError.None" />.
		///     </para>
		///     <para>
		///         This property keeps its last value even if <see cref="ReadNext" /> returns false.
		///     </para>
		/// </remarks>
		public CsvReaderError CurrentError { get; private set; }

		/// <summary>
		///     Gets the current line number in the CSV data to which <see cref="CurrentRow" /> or <see cref="CurrentError" /> corresponds to.
		/// </summary>
		/// <value>
		///     The current line number.
		/// </value>
		/// <remarks>
		///     <para>
		///         Before the first call to <see cref="ReadNext" />, this property is zero.
		///     </para>
		///     <para>
		///         This property keeps its last value even if <see cref="ReadNext" /> returns false.
		///     </para>
		///     <note type="note">
		///         This value always corresponds to the last line which belongs to the current row (relevant for multiline-values).
		///     </note>
		/// </remarks>
		public int CurrentLineNumber { get; private set; }

		/// <summary>
		///     Gets the current CSV row values which was read during the last call to <see cref="ReadNext" />.
		/// </summary>
		/// <value>
		///     The current CSV row values or null if last call to <see cref="ReadNext" /> created an error (<see cref="CurrentError" />).
		/// </value>
		/// <remarks>
		///     <para>
		///         Before the first call to <see cref="ReadNext" />, this property is null.
		///     </para>
		///     <para>
		///         This property keeps its last value even if <see cref="ReadNext" /> returns false.
		///     </para>
		/// </remarks>
		public List<string> CurrentRow { get; private set; }

		/// <summary>
		///     Gets the used reader settings for this CSV reader.
		/// </summary>
		/// <value>
		///     The used reader settings for this CSV reader.
		/// </value>
		public CsvReaderSettings Settings { get; private set; }

		#endregion




		#region Instance Methods

		/// <summary>
		///     Closes this CSV reader and its underlying <see cref="TextReader" /> (<see cref="BaseReader" />).
		/// </summary>
		public void Close ()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}


		/// <summary>
		///     Reads the next CSV row from the CSV data.
		/// </summary>
		/// <returns>
		///     true if a row was read and <see cref="CurrentRow" /> was updated, false if there are no more CSV rows (<see cref="CurrentRow" /> keeps its last value).
		/// </returns>
		/// <remarks>
		///     <note type="note">
		///         The CSV data is read line-by-line.
		///         <see cref="ReadNext" /> reads logical CSV rows, which can result in reading multiple lines from the underlying <see cref="BaseReader" /> in case of multiline values.
		///     </note>
		/// </remarks>
		/// <exception cref="ObjectDisposedException"> The INI reader has been closed/disposed. </exception>
		public bool ReadNext ()
		{
			this.VerifyNotClosed();

			//TODO: Implement
			return false;
		}

		[SuppressMessage("ReSharper", "UnusedParameter.Local")]
		private void Dispose (bool disposing)
		{
			if (this.BaseReader != null)
			{
				this.BaseReader.Close();
				this.BaseReader = null;
			}
		}

		private void VerifyNotClosed ()
		{
			if (this.BaseReader == null)
			{
				throw new ObjectDisposedException(nameof(CsvReader));
			}
		}

		#endregion




		#region Interface: IDisposable

		/// <inheritdoc />
		void IDisposable.Dispose ()
		{
			this.Close();
		}

		#endregion
	}
}
