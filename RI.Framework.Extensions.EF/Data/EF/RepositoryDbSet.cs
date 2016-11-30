﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Expressions;

using RI.Framework.Data.EF.Filter;
using RI.Framework.Data.EF.Validation;
using RI.Framework.Data.Repository;
using RI.Framework.Services;
using RI.Framework.Services.Logging;




namespace RI.Framework.Data.EF
{
	/// <summary>
	///     Implements a non-generic repository set using an Entity Frameworks <see cref="DbSet{TEntity}" />.
	/// </summary>
	/// <remarks>
	///     <para>
	///         This is only a non-generic base class inherited by <see cref="RepositoryDbSet{T}" /> in order to provide an implementation of <see cref="IRepositoryDbSet" />.
	///         This implementation simply forwards everything to <see cref="RepositoryDbSet{T}" />.
	///         You cannot inherit from this class.
	///         See <see cref="RepositoryDbSet{T}" /> for more details.
	///     </para>
	/// </remarks>
	public abstract class RepositoryDbSet : IRepositorySet
	{
		internal RepositoryDbSet()
		{
		}
		
		//TODO: Inheritdoc
		
		/// <inheritdoc />
		protected abstract Type EntityTypeInternal { get; }

		/// <inheritdoc />
		protected abstract int GetCountInternal ();

		/// <inheritdoc />
		protected abstract bool CanCreateInternal ();

		/// <inheritdoc />
		protected abstract void AddInternal (object entity);

		/// <inheritdoc />
		protected abstract void AttachInternal (object entity);

		/// <inheritdoc />
		protected abstract object CreateInternal ();

		/// <inheritdoc />
		protected abstract void DeleteInternal (object entity);
		
		/// <inheritdoc />
		protected abstract bool IsModifiedInternal (object entity);

		/// <inheritdoc />
		protected abstract bool IsValidInternal (object entity);

		/// <inheritdoc />
		protected abstract void ModifyInternal (object entity);

		/// <inheritdoc />
		protected abstract void ReloadInternal (object entity);

		/// <inheritdoc />
		protected abstract RepositorySetErrors ValidateInternal (object entity);

		/// <inheritdoc />
		protected abstract IEnumerable<object> GetAllInternal ();

		/// <inheritdoc />
		protected abstract bool CanAddInternal (object entity);

		/// <inheritdoc />
		protected abstract bool CanAttachInternal (object entity);

		/// <inheritdoc />
		protected abstract bool CanDeleteInternal (object entity);

		/// <inheritdoc />
		protected abstract bool CanModifyInternal (object entity);

		/// <inheritdoc />
		protected abstract bool CanReloadInternal (object entity);

		/// <inheritdoc />
		protected abstract bool CanValidateInternal (object entity);

		/// <inheritdoc />
		protected abstract IEnumerable<object> GetFilteredInternal (object filter, int pageIndex, int pageSize, out int entityCount, out int pageCount);

		/// <inheritdoc />
		protected abstract IEnumerable<object> GetFilteredInternal (IEnumerable entities, object filter, int pageIndex, int pageSize, out int entityCount, out int pageCount);
		
		
		
		
		/// <inheritdoc />
		public Type EntityType => return this.EntityTypeInternal;

		/// <inheritdoc />
		public int GetCount ()
		{
			return this.GetCountInternal();
		}

		/// <inheritdoc />
		public bool CanCreate ()
		{
			return this.CanCreateInternal();
		}

		/// <inheritdoc />
		public void Add (object entity)
		{
			this.AddInternal(entity);
		}

		/// <inheritdoc />
		public void Attach (object entity)
		{
			this.AttachInternal(entity);
		}

		/// <inheritdoc />
		public object Create ()
		{
			return this.CreateInternal();
		}

		/// <inheritdoc />
		public void Delete (object entity)
		{
			this.DeleteInternal(entity);
		}
		
		/// <inheritdoc />
		public bool IsModified (object entity)
		{
			return this.IsModifiedInternal(entity);
		}

		/// <inheritdoc />
		public bool IsValid (object entity)
		{
			return this.IsValidInternal(entity);
		}

		/// <inheritdoc />
		public void Modify (object entity)
		{
			this.ModifyInternal(entity);
		}

		/// <inheritdoc />
		public void Reload (object entity)
		{
			this.ReloadInternal(entity);
		}

		/// <inheritdoc />
		public RepositorySetErrors Validate (object entity)
		{
			return this.ValidateInternal(entity);
		}

		/// <inheritdoc />
		public IEnumerable<object> GetAll ()
		{
			return this.GetAllInternal();
		}

		/// <inheritdoc />
		public bool CanAdd (object entity)
		{
			return this.CanAddInternal(entity);
		}

		/// <inheritdoc />
		public bool CanAttach (object entity)
		{
			return this.CanAttachInternal(entity);
		}

		/// <inheritdoc />
		public bool CanDelete (object entity)
		{
			return this.CanDeleteInternal(entity);
		}

		/// <inheritdoc />
		public bool CanModify (object entity)
		{
			return this.CanModifyInternal(entity);
		}

		/// <inheritdoc />
		public bool CanReload (object entity)
		{
			return this.CanReloadInternal(entity);
		}

		/// <inheritdoc />
		public bool CanValidate (object entity)
		{
			return this.CanValidateInternal(entity);
		}

		/// <inheritdoc />
		public IEnumerable<object> GetFiltered (object filter, int pageIndex, int pageSize, out int entityCount, out int pageCount)
		{
			return this.GetFilteredInternal(filter, pageIndex, pageSIze, out entityCount, out pageCount);
		}

		/// <inheritdoc />
		public IEnumerable<object> GetFiltered (IEnumerable entities, object filter, int pageIndex, int pageSize, out int entityCount, out int pageCount)
		{
			return this.GetFilteredInternal(entities, filter, pageIndex, pageSIze, out entityCount, out pageCount);
		}
	}
	
	/// <summary>
	///     Implements a generic repository set using an Entity Frameworks <see cref="DbSet{TEntity}" />.
	/// </summary>
	/// <typeparam name="T"> The type of the entities which are represented by this repository set. </typeparam>
	/// <remarks>
	///     <para>
	///         See <see cref="IRepositorySet{T}" /> and <see cref="DbSet{TEntity}" /> for more details.
	///     </para>
	/// </remarks>
	public class RepositoryDbSet <T> : RepositoryDbSet, IRepositorySet<T>
		where T : class
	{
		#region Instance Constructor/Destructor

		/// <summary>
		///     Creates a new instance of <see cref="RepositoryDbSet{T}" />.
		/// </summary>
		/// <param name="repository"> The repository this repository set belongs to. </param>
		/// <param name="set"> The underlying Entity Framework <see cref="DbSet{TEntity}" /> used by this repository set. </param>
		/// <exception cref="ArgumentNullException"> <paramref name="repository" /> or <paramref name="set" /> is null. </exception>
		public RepositoryDbSet (RepositoryDbContext repository, DbSet<T> set)
		{
			if (repository == null)
			{
				throw new ArgumentNullException(nameof(repository));
			}

			if (set == null)
			{
				throw new ArgumentNullException(nameof(set));
			}

			this.Repository = repository;
			this.Set = set;
		}

		#endregion




		#region Instance Properties/Indexer

		/// <summary>
		///     Gets the repository this repository set belongs to.
		/// </summary>
		/// <value>
		///     The repository this repository set belongs to.
		/// </value>
		public RepositoryDbContext Repository { get; private set; }

		/// <summary>
		///     Gets the underlying Entity Framework <see cref="DbSet{TEntity}" /> used by this repository set.
		/// </summary>
		/// <value>
		///     The underlying Entity Framework <see cref="DbSet{TEntity}" /> used by this repository set.
		/// </value>
		public DbSet<T> Set { get; private set; }

		#endregion




		#region Instance Methods

		/// <summary>
		///     Logs a message.
		/// </summary>
		/// <param name="severity"> The severity of the message. </param>
		/// <param name="format"> The message. </param>
		/// <param name="args"> The arguments which will be expanded into the message (comparable to <see cref="string.Format(string, object[])" />). </param>
		/// <remarks>
		///     <para>
		///         <see cref="ILogService" /> is used, obtained through <see cref="ServiceLocator" />.
		///         If no <see cref="ILogService" /> is available, no logging is performed.
		///     </para>
		/// </remarks>
		protected void Log (LogLevel severity, string format, params object[] args)
		{
			LogLocator.Log(severity, this.GetType().Name, format, args);
		}

		#endregion




		#region Interface: IRepositorySet<T>

		/// <inheritdoc />
		public override Type EntityType => typeof(T);

		/// <inheritdoc />
		public new void Add (T entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException(nameof(entity));
			}

			if (!this.CanAdd(entity))
			{
				throw new InvalidOperationException("The entity cannot be added.");
			}

			this.Set.Add(entity);
		}

		/// <inheritdoc />
		public new void Attach (T entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException(nameof(entity));
			}

			if (!this.CanAttach(entity))
			{
				throw new InvalidOperationException("The entity cannot be attached.");
			}

			this.Set.Attach(entity);
		}

		/// <inheritdoc />
		public new virtual bool CanAdd (T entity)
		{
			return (((IEntityValidation)this.Repository.GetValidator<T>())?.CanAdd(this.Repository, entity)).GetValueOrDefault(true);
		}

		/// <inheritdoc />
		public new virtual bool CanAttach (T entity)
		{
			return (((IEntityValidation)this.Repository.GetValidator<T>())?.CanAttach(this.Repository, entity)).GetValueOrDefault(true);
		}

		/// <inheritdoc />
		public override bool CanCreate ()
		{
			return (((IEntityValidation)this.Repository.GetValidator<T>())?.CanCreate(this.Repository)).GetValueOrDefault(true);
		}

		/// <inheritdoc />
		public new virtual bool CanDelete (T entity)
		{
			return (((IEntityValidation)this.Repository.GetValidator<T>())?.CanDelete(this.Repository, this.Repository.Entry(entity))).GetValueOrDefault(true);
		}

		/// <inheritdoc />
		public new virtual bool CanModify (T entity)
		{
			return (((IEntityValidation)this.Repository.GetValidator<T>())?.CanModify(this.Repository, this.Repository.Entry(entity))).GetValueOrDefault(true);
		}

		/// <inheritdoc />
		public new virtual bool CanReload (T entity)
		{
			return (((IEntityValidation)this.Repository.GetValidator<T>())?.CanReload(this.Repository, this.Repository.Entry(entity))).GetValueOrDefault(true);
		}

		/// <inheritdoc />
		public new virtual bool CanValidate (T entity)
		{
			return (((IEntityValidation)this.Repository.GetValidator<T>())?.CanValidate(this.Repository, this.Repository.Entry(entity))).GetValueOrDefault(true);
		}

		/// <inheritdoc />
		public new T Create ()
		{
			if (!this.CanCreate())
			{
				throw new InvalidOperationException("New entities cannot be created.");
			}

			return this.Set.Create();
		}

		/// <inheritdoc />
		public new void Delete (T entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException(nameof(entity));
			}

			if (!this.CanDelete(entity))
			{
				throw new InvalidOperationException("The entity cannot be deleted.");
			}

			this.Set.Remove(entity);
		}

		/// <inheritdoc />
		public new virtual IEnumerable<T> GetAll ()
		{
			return this.Set;
		}

		/// <inheritdoc />
		public override int GetCount ()
		{
			return this.Set.Count();
		}

		/// <inheritdoc />
		public new virtual IEnumerable<T> GetFiltered (object filter, int pageIndex, int pageSize, out int entityCount, out int pageCount)
		{
			return this.GetFiltered(null, filter, pageIndex, pageSize, out entityCount, out pageCount);
		}

		/// <inheritdoc />
		public new virtual IEnumerable<T> GetFiltered (IEnumerable<T> entities, object filter, int pageIndex, int pageSize, out int entityCount, out int pageCount)
		{
			if (pageIndex < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(pageIndex));
			}

			if (pageSize < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(pageSize));
			}

			if ((pageSize == 0) && (pageIndex != 0))
			{
				throw new ArgumentOutOfRangeException(nameof(pageSize));
			}

			entityCount = this.GetCount();
			pageCount = ((pageSize == 0) || (entityCount == 0)) ? 1 : ((entityCount / pageSize) + (((entityCount % pageSize) == 0) ? 0 : 1));
			
			if ((pageIndex != 0) && (pageIndex >= pageCount))
			{
				throw new ArgumentOutOfRangeException(nameof(pageIndex));
			}
			
			EntityFilter<T> entityFilter = this.Repository.GetFilter<T>();
			if(entityFilter == null)
			{
				throw new InvalidOperationException("No entity filter available.");
			}
			
			int offset = pageIndex * pageSize;

			IOrderedQueryable<T> queryable = entityFilter.Filter(this.Repository, this, entities, filter);
			return queryable.Skip(offset).Take(pageSize);
		}

		/// <inheritdoc />
		public new bool IsModified (T entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException(nameof(entity));
			}

			this.Repository.ChangeTracker.DetectChanges();

			DbEntityEntry<T> entry = this.Repository.Entry(entity);
			return entry == null ? false : (entry.State & (EntityState.Modified | EntityState.Added | EntityState.Deleted)) != 0;
		}

		/// <inheritdoc />
		public new bool IsValid (T entity)
		{
			return this.Validate(entity) == null;
		}

		/// <inheritdoc />
		public new void Modify (T entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException(nameof(entity));
			}

			if (!this.CanModify(entity))
			{
				throw new InvalidOperationException("The entity cannot be modified.");
			}

			this.Repository.ChangeTracker.DetectChanges();

			DbEntityEntry<T> entry = this.Repository.Entry(entity);
			if ((entry != null) && ((entry.State & (EntityState.Unchanged)) != 0))
			{
				entry.State = EntityState.Modified;
			}
		}

		/// <inheritdoc />
		public new void Reload (T entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException(nameof(entity));
			}

			if (!this.CanReload(entity))
			{
				throw new InvalidOperationException("The entity cannot be reloaded.");
			}

			this.Repository.ChangeTracker.DetectChanges();

			DbEntityEntry<T> entry = this.Repository.Entry(entity);
			if ((entry != null) && ((entry.State & (EntityState.Modified)) != 0))
			{
				entry.Reload();
			}
		}

		/// <inheritdoc />
		public new RepositorySetErrors Validate (T entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException(nameof(entity));
			}

			if (!this.CanValidate(entity))
			{
				throw new InvalidOperationException("The entity cannot be validated.");
			}

			this.Repository.ChangeTracker.DetectChanges();

			DbEntityEntry<T> entry = this.Repository.Entry(entity);
			DbEntityValidationResult results = entry?.GetValidationResult();
			return results?.ToRepositoryErrors();
		}

		#endregion
	}
}
