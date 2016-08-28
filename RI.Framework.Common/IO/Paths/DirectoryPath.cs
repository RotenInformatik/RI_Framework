using System;
using System.IO;

using RI.Framework.Utilities.Exceptions;
using RI.Framework.Utilities.ObjectModel;




namespace RI.Framework.IO.Paths
{
	/// <summary>
	///     Describes a path to a directory.
	/// </summary>
	/// <remarks>
	///     <para>
	///         <see cref="DirectoryPath" /> uses <see cref="PathProperties" /> to extract and store path information.
	///         See <see cref="PathProperties" /> for more details about the supported types of directory paths.
	///     </para>
	///     <para>
	///         <see cref="DirectoryPath" /> provides more directory path specific functionalities compared to <see cref="string" /> and offers a more consistent way of working with paths than <see cref="Path" />.
	///         It is supported by this library where directory paths are used and can also be implicitly converted to a <see cref="string" /> to work with other libraries and the Base Class Library.
	///     </para>
	///     <para>
	///         See <see cref="FilePath" /> for an example how to use <see cref="DirectoryPath" /> and <see cref="FilePath" />.
	///     </para>
	/// </remarks>
	[Serializable]
	public sealed class DirectoryPath : PathString,
	                                    ICloneable<DirectoryPath>
	{
		#region Instance Constructor/Destructor

		/// <summary>
		///     Creates a new instance of <see cref="DirectoryPath" />.
		/// </summary>
		/// <param name="path"> The path. </param>
		/// <remarks>
		///     <para>
		///         Using this constructor, wildcards and relative paths are not allowed and the type of the path must be clearly determinable.
		///     </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException"> <paramref name="path" /> is null. </exception>
		public DirectoryPath (string path)
			: this(PathProperties.FromPath(path, false, false, null))
		{
		}

		/// <summary>
		///     Creates a new instance of <see cref="DirectoryPath" />.
		/// </summary>
		/// <param name="path"> The path. </param>
		/// <param name="allowWildcards"> Specifies whether wildcards are allowed or not. </param>
		/// <param name="allowRelatives"> Specifies whether relative directory names are allowed or not. </param>
		/// <param name="assumedType"> Optionally specifies the type of the path which is assumed if the type cannot be clearly determined through analysis of <paramref name="path" />. </param>
		/// <remarks>
		///     <para>
		///         See <see cref="PathProperties.FromPath" /> for more details about the parameters, especially <paramref name="assumedType" />.
		///     </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException"> <paramref name="path" /> is null. </exception>
		/// <exception cref="InvalidPathArgumentException"> <paramref name="path" /> is not a valid directory path. </exception>
		public DirectoryPath (string path, bool allowWildcards, bool allowRelatives, PathType? assumedType)
			: this(PathProperties.FromPath(path, allowWildcards, allowRelatives, assumedType))
		{
		}

		/// <summary>
		///     Creates a new instance of <see cref="DirectoryPath" />.
		/// </summary>
		/// <param name="path"> The <see cref="PathProperties" /> object which describes the path. </param>
		/// <exception cref="ArgumentNullException"> <paramref name="path" /> is null. </exception>
		public DirectoryPath (PathProperties path)
			: base(path)
		{
		}

		#endregion




		#region Instance Properties/Indexer

		/// <summary>
		///     Gets the name of the directory.
		/// </summary>
		/// <value>
		///     The name of the directory.
		/// </value>
		public string DirectoryName
		{
			get
			{
				return this.PathInternal.Name;
			}
		}

		/// <summary>
		///     Gets the parent directory.
		/// </summary>
		/// <value>
		///     The parent directory or null if this directory is a root or does not have a parent directory.
		/// </value>
		public DirectoryPath Parent
		{
			get
			{
				string parent = this.PathInternal.Parent;
				if (parent == null)
				{
					return null;
				}
				return new DirectoryPath(parent, this.PathInternal.AllowWildcards, this.PathInternal.AllowRelatives, this.Type);
			}
		}

		#endregion




		#region Instance Methods

		public DirectoryPath Append (params DirectoryPath[] directories)
		{
			directories = directories ?? new DirectoryPath[0];

			for (int i1 = 0; i1 < directories.Length; i1++)
			{
				if (directories[i1].IsAbsolute)
				{
					throw ( new PathNotRelativeArgumentException(nameof(directories)) );
				}
			}

			string path = this.Path;

			for (int i1 = 0; i1 < directories.Length; i1++)
			{
				path = Path.Combine(path, directories[i1].Path);
			}

			return ( new DirectoryPath(path) );
		}

		public FilePath Append (FilePath file)
		{
			if (file == null)
			{
				throw ( new ArgumentNullException(nameof(file)) );
			}

			if (file.IsAbsolute)
			{
				throw ( new PathNotRelativeArgumentException(nameof(file)) );
			}

			return ( new FilePath(Path.Combine(this.Path, file.Path)) );
		}

		public DirectoryPath ChangeDirectoryName (string directoryName)
		{
			if (directoryName == null)
			{
				throw ( new ArgumentNullException(nameof(directoryName)) );
			}

			if (!DirectoryPath.IsDirectoryPath(directoryName, false))
			{
				throw ( new InvalidPathArgumentException(nameof(directoryName)) );
			}

			if (this.IsRoot)
			{
				return ( new DirectoryPath(directoryName + ":") );
			}

			return ( new DirectoryPath(Path.Combine(this.Parent.Path, directoryName)) );
		}

		public DirectoryPath ChangeParent (DirectoryPath newParent)
		{
			if (newParent == null)
			{
				throw ( new ArgumentNullException(nameof(newParent)) );
			}

			return ( newParent.Append(new DirectoryPath(this.DirectoryName)) );
		}

		public DirectoryPath ToAbsolutePath (DirectoryPath root)
		{
			if (root == null)
			{
				throw ( new ArgumentNullException(nameof(root)) );
			}

			if (!root.IsAbsolute)
			{
				throw ( new PathNotAbsoluteArgumentException(nameof(root)) );
			}

			return ( new DirectoryPath(PathString.ToAbsolutePath(this, root)) );
		}

		public DirectoryPath ToRelativePath (DirectoryPath root)
		{
			if (root == null)
			{
				throw ( new ArgumentNullException(nameof(root)) );
			}

			if (!root.IsAbsolute)
			{
				throw ( new PathNotAbsoluteArgumentException(nameof(root)) );
			}

			return ( new DirectoryPath(PathString.ToRelativePath(this, root)) );
		}

		public DirectoryPath ToRelativePath (DirectoryPath root, bool includeCurrentSpecifier)
		{
			if (root == null)
			{
				throw ( new ArgumentNullException(nameof(root)) );
			}

			if (!root.IsAbsolute)
			{
				throw ( new PathNotAbsoluteArgumentException(nameof(root)) );
			}

			return ( new DirectoryPath(PathString.ToRelativePath(this, root, includeCurrentSpecifier)) );
		}

		#endregion




		#region Overrides

		/// <inheritdoc />
		protected override PathString CloneInternal ()
		{
			return this.Clone();
		}

		#endregion




		#region Interface: ICloneable<DirectoryPath>

		/// <inheritdoc />
		public DirectoryPath Clone ()
		{
			return new DirectoryPath(this.GetPathProperties());
		}

		#endregion
	}
}
