﻿using RI.Framework.Utilities.ObjectModel;

namespace RI.Framework.Data.EF.Resolvers
{
	/// <summary>
	///     Implements a dependency resolver for Entity Framework contexts which uses <see cref="Singleton" />.
	/// </summary>
	/// <remarks>
	///     <note type="note">
	///         The key parameter of <see cref="DependencyResolverDbDependencyResolver.GetService" /> and <see cref="DependencyResolverDbDependencyResolver.GetServices" /> is ignored.
	///     </note>
	/// </remarks>
	public sealed class SingletonDbDependencyResolver : DependencyResolverDbDependencyResolver
	{
		/// <summary>
		/// Creates a new instance of <see cref="SingletonDbDependencyResolver"/>.
		/// </summary>
		public SingletonDbDependencyResolver()
			: base(Singleton.Resolver)
		{
		}
	}
}