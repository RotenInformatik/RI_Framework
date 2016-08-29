﻿namespace RI.Framework.IO.INI.Elements
{
	/// <summary>
	///     Represents arbitrary text in INI data.
	/// </summary>
	/// <remarks>
	///     <para>
	///         See <see cref="IniDocument" /> for more general and detailed information about working with INI data.
	///     </para>
	///     <para>
	///         Arbitrary text is everything in an INI file which is not a section header, comment, or name-value-pair.
	///     </para>
	/// </remarks>
	public sealed class TextIniElement : IniElement
	{
		#region Instance Constructor/Destructor

		/// <summary>
		///     Creates a new instance of <see cref="TextIniElement" />.
		/// </summary>
		/// <param name="text"> The arbitrary text. </param>
		public TextIniElement (string text)
		{
			this.Text = text;
		}

		#endregion




		#region Instance Fields

		private string _text;

		#endregion




		#region Instance Properties/Indexer

		/// <summary>
		///     Gets or sets the arbitrary text.
		/// </summary>
		/// <value>
		///     The arbitrary text.
		/// </value>
		/// <remarks>
		///     <note type="note">
		///         The value returned by this property is never null.
		///         If null is set, it is replaced with <see cref="string.Empty" />.
		///     </note>
		/// </remarks>
		public string Text
		{
			get
			{
				return this._text;
			}
			set
			{
				this._text = value ?? string.Empty;
			}
		}

		#endregion
	}
}
