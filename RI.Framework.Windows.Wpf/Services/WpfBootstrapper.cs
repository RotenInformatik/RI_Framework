﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

using RI.Framework.Composition;
using RI.Framework.Composition.Catalogs;
using RI.Framework.Composition.Model;
using RI.Framework.IO.Paths;
using RI.Framework.Services.Logging;
using RI.Framework.Services.Modularization;
using RI.Framework.Utilities;
using RI.Framework.Utilities.ObjectModel;
using RI.Framework.Utilities.Reflection;
using RI.Framework.Utilities.Text;
using RI.Framework.Utilities.Windows;




namespace RI.Framework.Services
{
	/// <summary>
	///     Implements a bootstrapper for WPF applications.
	/// </summary>
	/// <remarks>
	///     <para>
	///         The following sequence is performed when <see cref="Run" /> is called:
	///     </para>
	///     <list type="number">
	///         <item>
	///             <para>
	///                 <see cref="State" /> is set to <see cref="WpfBootstrapperState.Bootstrapping" />.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="DetermineApplicationAssembly" /> is called and <see cref="ApplicationAssembly" /> is set.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="DetermineApplicationProductName" /> is called and <see cref="ApplicationProductName" /> is set.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="DetermineApplicationCompanyName" /> is called and <see cref="ApplicationCompanyName" /> is set.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="DetermineApplicationCopyright" /> is called and <see cref="ApplicationCopyright" /> is set.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="DetermineApplicationVersion" /> is called and <see cref="ApplicationVersion" /> is set.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="DetermineApplicationIdVersionIndependent" /> is called and <see cref="ApplicationIdVersionIndependent" /> is set.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="DetermineApplicationIdVersionDependent" /> is called and <see cref="ApplicationIdVersionDependent" /> is set.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="DetermineProcessCommandLine" /> is called and <see cref="ProcessCommandLine" /> is set.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="DetermineSessionTimestamp" /> is called and <see cref="SessionTimestamp" /> is set.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="DetermineSessionId" /> is called and <see cref="SessionId" /> is set.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="DetermineApplicationExecutableDirectory" /> is called and <see cref="ApplicationExecutableDirectory" /> is set.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="DetermineApplicationDataDirectory" /> is called and <see cref="ApplicationDataDirectory" /> is set.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="CreateContainer" /> is called and <see cref="Container" /> is set.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="ConfigureServiceLocator" /> is called.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="ConfigureBootstrapperSingletons" /> is called.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="ConfigureBootstrapper" /> is called.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="ConfigureLogging" /> is called.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="CreateApplication" /> is called and <see cref="Application" /> is set.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="ConfigureApplication" /> is called.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="ShowSplashScreen" /> is called.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="ConfigureContainer" /> is called.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="ConfigureServices" /> is called.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="ConfigureModularization" /> is called.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="State" /> is set to <see cref="WpfBootstrapperState.Running" />.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="BeginRun" /> is called.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="Application" />.<see cref="System.Windows.Application.Run()" /> is called. The application is now running until <see cref="WpfBootstrapper.Shutdown" /> is called.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="EndRun" /> is called.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="State" /> is set to <see cref="WpfBootstrapperState.ShuttingDown" />.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="DoShutdown" /> is called.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="State" /> is set to <see cref="WpfBootstrapperState.ShutDown" />.
	///             </para>
	///         </item>
	///         <item>
	///             <para>
	///                 <see cref="Run" /> returns.
	///             </para>
	///         </item>
	///     </list>
	/// </remarks>
	[Export]
	public abstract class WpfBootstrapper : IBootstrapper
	{
		#region Instance Constructor/Destructor

		/// <summary>
		///     Creates a new instance of <see cref="WpfBootstrapper" />.
		/// </summary>
		protected WpfBootstrapper ()
		{
			this.State = WpfBootstrapperState.Uninitialized;
			this.ShutdownInitiated = false;

			this.Container = null;
			this.Application = null;
		}

		#endregion




		#region Instance Properties/Indexer

		/// <summary>
		///     Gets the used WPF application object.
		/// </summary>
		/// <value>
		///     The used WPF application object.
		/// </value>
		public Application Application { get; private set; }

		/// <summary>
		///     Gets the main assembly of the application.
		/// </summary>
		/// <value>
		///     The main assembly of the application.
		/// </value>
		public Assembly ApplicationAssembly { get; private set; }

		/// <summary>
		///     Gets the associated company name of the application.
		/// </summary>
		/// <value>
		///     The associated company name of the application.
		/// </value>
		public string ApplicationCompanyName { get; private set; }

		/// <summary>
		///     Gets the copyright statement of the application.
		/// </summary>
		/// <value>
		///     The copyright statement of the application.
		/// </value>
		public string ApplicationCopyright { get; private set; }

		/// <summary>
		///     Gets the read- and writeable directory associated with the application used to store persistent data.
		/// </summary>
		/// <value>
		///     The read- and writeable directory associated with the application used to store persistent data.
		/// </value>
		public DirectoryPath ApplicationDataDirectory { get; private set; }

		/// <summary>
		///     Gets the read-only directory where the applications executable files are stored.
		/// </summary>
		/// <value>
		///     The read-only directory where the applications executable files are stored.
		/// </value>
		public DirectoryPath ApplicationExecutableDirectory { get; private set; }

		/// <summary>
		///     Gets the GUID of the application which is application version dependent.
		/// </summary>
		/// <value>
		///     The GUID of the application which is application version dependent.
		/// </value>
		public Guid ApplicationIdVersionDependent { get; private set; }

		/// <summary>
		///     Gets the GUID of the application which is application version independent.
		/// </summary>
		/// <value>
		///     The GUID of the application which is application version independent.
		/// </value>
		public Guid ApplicationIdVersionIndependent { get; private set; }

		/// <summary>
		///     Gets the anonymized GUID of the local machine.
		/// </summary>
		/// <value>
		///     The anonymized GUID of the local machine.
		/// </value>
		public Guid MachineId { get; private set; }

		/// <summary>
		///     Gets the anonymized GUID of the current user.
		/// </summary>
		/// <value>
		///     The anonymized GUID of the current user.
		/// </value>
		public Guid UserId { get; private set; }

		/// <summary>
		///     Gets the anonymized GUID of the domain this machine belongs to.
		/// </summary>
		/// <value>
		///     The anonymized GUID of the domain this machine belongs to.
		/// </value>
		public Guid DomainId { get; private set; }

		/// <summary>
		///     Gets the product name of the application.
		/// </summary>
		/// <value>
		///     The product name of the application.
		/// </value>
		public string ApplicationProductName { get; private set; }

		/// <summary>
		///     Gets the version of the application.
		/// </summary>
		/// <value>
		///     The version of the application.
		/// </value>
		public Version ApplicationVersion { get; private set; }

		/// <summary>
		///     Gets the used composition container.
		/// </summary>
		/// <value>
		///     The used composition container.
		/// </value>
		public CompositionContainer Container { get; private set; }

		/// <summary>
		///     Gets the command line which was used for the current process.
		/// </summary>
		/// <value>
		///     The command line which was used for the current process.
		/// </value>
		public CommandLine ProcessCommandLine { get; private set; }

		/// <summary>
		///     Gets the GUID of the current session.
		/// </summary>
		/// <value>
		///     The GUID of the current session.
		/// </value>
		public Guid SessionId { get; private set; }

		/// <summary>
		///     Gets the timestamp of the current session.
		/// </summary>
		/// <value>
		///     The timestamp of the current session.
		/// </value>
		public DateTime SessionTimestamp { get; private set; }

		/// <summary>
		///     Gets the current state of the bootstrapper.
		/// </summary>
		/// <value>
		///     The current state of the bootstrapper.
		/// </value>
		public WpfBootstrapperState State { get; private set; }

		/// <summary>
		/// Gets whether the program is executed on a 64 bit machine.
		/// </summary>
		/// <value>
		/// true if executed on a 64 bit machine, false otherwise.
		/// </value>
		public bool Machine64Bit { get; private set; }

		/// <summary>
		/// Gets whether the program is executed in a 64 bit process.
		/// </summary>
		/// <value>
		/// true if executed in a 64 bit process, false otherwise.
		/// </value>
		public bool Session64Bit { get; private set; }

		private bool ShutdownInitiated { get; set; }

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

		/// <summary>
		///     Logs a separator to allow quick visual distinguishing of application bootstrapping states in the a file.
		/// </summary>
		/// <remarks>
		///     <para>
		///         <see cref="ILogService" /> is used, obtained through <see cref="ServiceLocator" />.
		///         If no <see cref="ILogService" /> is available, no logging is performed.
		///     </para>
		///     <para>
		///         A separator consists of 200 dashes (<c> - </c>).
		///     </para>
		/// </remarks>
		protected void LogSeperator ()
		{
			this.Log(LogLevel.Debug, new string('-', 200));
		}

		[SuppressMessage ("ReSharper", "EmptyGeneralCatchClause")]
		private void HandleExceptionInternal (Exception exception)
		{
			try
			{
				if (exception == null)
				{
					return;
				}

				string message = exception.ToDetailedString();

				try
				{
					this.Log(LogLevel.Fatal, "EXCEPTION: {0}", message);
				}
				catch
				{
				}

				if (this.ShutdownInitiated && (exception is ThreadAbortException))
				{
					return;
				}

				try
				{
					this.HandleException(exception);
				}
				catch
				{
				}

				Environment.FailFast(message, exception);
			}
			catch
			{
			}
		}

		#endregion




		#region Virtuals

		/// <summary>
		///     Called when all bootstrapping and initialization is done and actual application operations begin.
		/// </summary>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation does nothing.
		///     </note>
		/// </remarks>
		protected virtual void BeginOperations ()
		{
		}

		/// <summary>
		///     Called before the application begins running after the bootstrapping is completed.
		/// </summary>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation injects module initialization (<see cref="IModuleService.Initialize" />), if available, into the applications dispatcher (<see cref="Application" />.<see cref="DispatcherObject.Dispatcher" />) using <see cref="DispatcherPriority.SystemIdle" /> priority.
		///         Afterwards, it calls <see cref="BeginOperations" />.
		///     </note>
		/// </remarks>
		protected virtual void BeginRun ()
		{
			this.Application.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, new Action(() =>
			                                                                                  {
				                                                                                  this.LogSeperator();
				                                                                                  this.Log(LogLevel.Debug, "Initializing modules");

				                                                                                  this.Container.GetExport<IModuleService>()?.Initialize();

				                                                                                  this.Application.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, new Action(() =>
				                                                                                                                                                                    {
					                                                                                                                                                                    this.LogSeperator();
					                                                                                                                                                                    this.Log(LogLevel.Debug, "Beginning operations");

					                                                                                                                                                                    this.BeginOperations();
				                                                                                                                                                                    }));
			                                                                                  }));
		}

		/// <summary>
		///     Called when the used WPF application object (<see cref="Application" />) needs to be configured.
		/// </summary>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation adds the application object (<see cref="Application" />) to the used composition container as an export using a <see cref="InstanceCatalog" />.
		///         It also setts the WPF applications object <see cref="System.Windows.Application.ShutdownMode" /> property to <see cref="ShutdownMode.OnExplicitShutdown" /> and <see cref="WpfApplication.Bootstrapper" /> property to this boottsrapper instance (if the application object derives from <see cref="WpfApplication" />).
		///     </note>
		/// </remarks>
		protected virtual void ConfigureApplication ()
		{
			this.Application.ShutdownMode = ShutdownMode.OnExplicitShutdown;
			if (this.Application is WpfApplication)
			{
				((WpfApplication)this.Application).Bootstrapper = this;
			}

			this.Container.AddCatalog(new InstanceCatalog(this.Application));
		}

		/// <summary>
		///     Called when the bootstrapper itself needs to be configured.
		/// </summary>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation adds this bootstrapper instance to the used composition container as an export using a <see cref="InstanceCatalog" />.
		///     </note>
		/// </remarks>
		protected virtual void ConfigureBootstrapper ()
		{
			this.Container.AddCatalog(new InstanceCatalog(this));
		}

		/// <summary>
		///     Called when the used composition container (<see cref="Container" />) needs to be configured.
		/// </summary>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation adds the container (<see cref="Container" />) to itself as an export using a <see cref="InstanceCatalog" />.
		///     </note>
		/// </remarks>
		protected virtual void ConfigureContainer ()
		{
			this.Container.AddCatalog(new InstanceCatalog(this.Container));
		}

		/// <summary>
		///     Called when the logging needs to be configured.
		/// </summary>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation does nothing.
		///     </note>
		/// </remarks>
		protected virtual void ConfigureLogging ()
		{
		}

		/// <summary>
		///     Called when the modularization needs to be configured.
		/// </summary>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation does nothing.
		///     </note>
		/// </remarks>
		protected virtual void ConfigureModularization ()
		{
		}

		/// <summary>
		///     Called when the service locator (<see cref="ServiceLocator" />) needs to be configured.
		/// </summary>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation calls <see cref="ServiceLocator.BindToCompositionContainer" /> using the used composition container (<see cref="Container" />).
		///     </note>
		/// </remarks>
		protected virtual void ConfigureServiceLocator ()
		{
			ServiceLocator.BindToCompositionContainer(this.Container);
		}

		/// <summary>
		/// Called when the bootstrapper singletons are to be configured.
		/// </summary>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation sets the singleton instance for <see cref="WpfBootstrapper"/>, <see cref="CompositionContainer"/> (<see cref="Container"/>), and <see cref="System.Windows.Application"/> (<see cref="Application"/>) using <see cref="Singleton{T}"/>.
		///     </note>
		/// </remarks>
		protected virtual void ConfigureBootstrapperSingletons()
		{
			Singleton<WpfBootstrapper>.Ensure(() => this);
			Singleton<CompositionContainer>.Ensure(() => this.Container);
			Singleton<Application>.Ensure(() => this.Application);
		}

		/// <summary>
		///     Called when all the other services of the application need to be configured.
		/// </summary>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation does nothing.
		///     </note>
		/// </remarks>
		protected virtual void ConfigureServices ()
		{
		}

		/// <summary>
		///     Called when the WPF application object needs to be created.
		/// </summary>
		/// <returns>
		///     The WPF application object to be used.
		///     Can be null if a default <see cref="WpfApplication" /> is to be used.
		/// </returns>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation returns null so a default <see cref="WpfApplication" /> will be created and used.
		///     </note>
		/// </remarks>
		protected virtual Application CreateApplication ()
		{
			return null;
		}

		/// <summary>
		///     Called when the composition container needs to be created.
		/// </summary>
		/// <returns>
		///     The composition container to be used.
		///     Can be null if a default <see cref="CompositionContainer" /> is to be used.
		/// </returns>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation returns null so a default <see cref="CompositionContainer" /> will be created and used.
		///     </note>
		/// </remarks>
		protected virtual CompositionContainer CreateContainer ()
		{
			return null;
		}

		/// <summary>
		///     Called to determine the main assembly of the application (<see cref="ApplicationAssembly" />).
		/// </summary>
		/// <returns>
		///     The main assembly of the application.
		///     Cannot be null.
		/// </returns>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation uses <see cref="Assembly.GetEntryAssembly" /> to determine the main assembly.
		///     </note>
		/// </remarks>
		protected virtual Assembly DetermineApplicationAssembly ()
		{
			return Assembly.GetEntryAssembly();
		}

		/// <summary>
		///     Called to determine the associated company name of the application (<see cref="ApplicationCompanyName" />).
		/// </summary>
		/// <returns>
		///     The associated company name of the application.
		///     Cannot be null.
		/// </returns>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation uses <see cref="AssemblyExtensions.GetCompany" /> on <see cref="ApplicationAssembly" />.
		///     </note>
		/// </remarks>
		protected virtual string DetermineApplicationCompanyName ()
		{
			return this.ApplicationAssembly.GetCompany();
		}

		/// <summary>
		///     Called to determine the copyright of the application (<see cref="ApplicationCopyright" />).
		/// </summary>
		/// <returns>
		///     The copyright of the application.
		///     Cannot be null.
		/// </returns>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation uses <see cref="AssemblyExtensions.GetCopyright" /> on <see cref="ApplicationAssembly" />.
		///     </note>
		/// </remarks>
		protected virtual string DetermineApplicationCopyright ()
		{
			return this.ApplicationAssembly.GetCopyright();
		}

		/// <summary>
		///     Called to determine the read- and writeable directory associated with the application used to store persistent data (<see cref="ApplicationDataDirectory" />).
		/// </summary>
		/// <returns>
		///     The read- and writeable directory associated with the application used to store persistent data.
		///     Cannot be null.
		/// </returns>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation uses the directory returned by <see cref="Environment.GetFolderPath(Environment.SpecialFolder)" /> using <see cref="Environment.SpecialFolder.LocalApplicationData" /> appended with <see cref="ApplicationCompanyName" /> and <see cref="ApplicationProductName" />.
		///         The directory is just determined but not accessed in any way.
		///     </note>
		/// </remarks>
		protected virtual DirectoryPath DetermineApplicationDataDirectory ()
		{
			return new DirectoryPath(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)).AppendDirectory(this.ApplicationCompanyName, this.ApplicationProductName);
		}

		/// <summary>
		///     Called to determine the read-only directory where the applications executable files are stored (<see cref="ApplicationExecutableDirectory" />).
		/// </summary>
		/// <returns>
		///     The read-only directory where the applications executable files are stored.
		///     Cannot be null.
		/// </returns>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation uses <see cref="AssemblyExtensions.GetFile" /> on <see cref="ApplicationAssembly" />.
		///         The directory is just determined but not accessed in any way.
		///     </note>
		/// </remarks>
		protected virtual DirectoryPath DetermineApplicationExecutableDirectory ()
		{
			return new FilePath(this.ApplicationAssembly.GetFile()).Directory;
		}

		/// <summary>
		///     Called to determine the GUID of the application which is application version dependent (<see cref="ApplicationIdVersionDependent" />).
		/// </summary>
		/// <returns>
		///     The GUID of the application which is application version dependent.
		/// </returns>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation uses <see cref="AssemblyExtensions.GetGuid" /> on <see cref="ApplicationAssembly" />.
		///     </note>
		/// </remarks>
		protected virtual Guid DetermineApplicationIdVersionDependent ()
		{
			return this.ApplicationAssembly.GetGuid(true, false);
		}

		/// <summary>
		///     Called to determine the GUID of the application which is application version independent (<see cref="ApplicationIdVersionIndependent" />).
		/// </summary>
		/// <returns>
		///     The GUID of the application which is application version independent.
		/// </returns>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation uses <see cref="AssemblyExtensions.GetGuid" /> on <see cref="ApplicationAssembly" />.
		///     </note>
		/// </remarks>
		protected virtual Guid DetermineApplicationIdVersionIndependent ()
		{
			return this.ApplicationAssembly.GetGuid(false, true);
		}

		/// <summary>
		///     Called to determine the GUID of the local machine.
		/// </summary>
		/// <returns>
		///     The GUID of the local machine.
		/// </returns>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation uses <see cref="UniqueIdentification" />.<see cref="UniqueIdentification.GetMachineId" />.
		///     </note>
		/// </remarks>
		protected virtual Guid DetermineMachineId()
		{
			return UniqueIdentification.GetMachineId();
		}

		/// <summary>
		///     Called to determine the GUID of the current user.
		/// </summary>
		/// <returns>
		///     The GUID of the current user.
		/// </returns>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation uses <see cref="UniqueIdentification" />.<see cref="UniqueIdentification.GetUserId" />.
		///     </note>
		/// </remarks>
		protected virtual Guid DetermineUserId()
		{
			return UniqueIdentification.GetUserId();
		}

		/// <summary>
		///     Called to determine the GUID of the domain this machine belongs to.
		/// </summary>
		/// <returns>
		///     The GUID of the the domain this machine belongs to.
		/// </returns>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation uses <see cref="UniqueIdentification" />.<see cref="UniqueIdentification.GetDomainId" />.
		///     </note>
		/// </remarks>
		protected virtual Guid DetermineDomainId()
		{
			return UniqueIdentification.GetDomainId();
		}

		/// <summary>
		///     Called to determine the product name of the application (<see cref="ApplicationProductName" />).
		/// </summary>
		/// <returns>
		///     The product name of the application.
		///     Cannot be null.
		/// </returns>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation uses <see cref="AssemblyExtensions.GetProduct" /> on <see cref="ApplicationAssembly" />.
		///     </note>
		/// </remarks>
		protected virtual string DetermineApplicationProductName ()
		{
			return this.ApplicationAssembly.GetProduct();
		}

		/// <summary>
		///     Called to determine the version of the application (<see cref="ApplicationVersion" />).
		/// </summary>
		/// <returns>
		///     The version of the application.
		///     Cannot be null.
		/// </returns>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation uses <see cref="AssemblyExtensions.GetAssemblyVersion" />, <see cref="AssemblyExtensions.GetFileVersion" />, and <see cref="AssemblyExtensions.GetInformationalVersion" /> on <see cref="ApplicationAssembly" /> (in that order, whichever returns a valid version first).
		///     </note>
		/// </remarks>
		protected virtual Version DetermineApplicationVersion ()
		{
			return (this.ApplicationAssembly.GetAssemblyVersion() ?? this.ApplicationAssembly.GetFileVersion()) ?? this.ApplicationAssembly.GetInformationalVersion();
		}

		/// <summary>
		///     Called to determine the command line of the current process (<see cref="ProcessCommandLine" />).
		/// </summary>
		/// <returns>
		///     The command line of the current process.
		/// </returns>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation uses <see cref="RI.Framework.Utilities.Text.CommandLine.Parse(string,bool,IEqualityComparer{string})" /> with <see cref="Environment" />.<see cref="Environment.CommandLine" />.
		///     </note>
		/// </remarks>
		protected virtual CommandLine DetermineProcessCommandLine ()
		{
			return CommandLine.Parse(Environment.CommandLine, true, StringComparerEx.InvariantCultureIgnoreCase);
		}

		/// <summary>
		///     Called to determine the GUID of the current session (<see cref="SessionId" />).
		/// </summary>
		/// <returns>
		///     The GUID of the current session.
		/// </returns>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation simply creates a new <see cref="Guid" /> using <see cref="Guid.NewGuid" />.
		///     </note>
		/// </remarks>
		protected virtual Guid DetermineSessionId ()
		{
			return Guid.NewGuid();
		}

		/// <summary>
		///     Called to determine the timestamp of the current session (<see cref="SessionTimestamp" />).
		/// </summary>
		/// <returns>
		///     The timestamp of the current session.
		/// </returns>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation simply returns the value of <see cref="DateTime.Now" /> at the time of calling.
		///     </note>
		/// </remarks>
		protected virtual DateTime DetermineSessionTimestamp ()
		{
			return DateTime.Now;
		}

		/// <summary>
		///     Called when the application is shut down.
		/// </summary>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation does nothing.
		///     </note>
		/// </remarks>
		protected virtual void DoShutdown ()
		{
		}

		/// <summary>
		///     Called before the application begins shutting down after the application was running.
		/// </summary>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation does nothing.
		///     </note>
		/// </remarks>
		protected virtual void EndRun ()
		{
			this.LogSeperator();
			this.Log(LogLevel.Debug, "Unloading modules");

			this.Container.GetExport<IModuleService>()?.Unload();
		}

		/// <summary>
		///     Called when an unhandled exception occurs in the application.
		/// </summary>
		/// <param name="exception"> The unhandled exception. </param>
		/// <remarks>
		///     <para>
		///         The default implementation does nothing.
		///     </para>
		///     <para>
		///         Whatever you do in this method, the application is terminated using <see cref="Environment.FailFast(string,Exception)" /> after this method returns.
		///     </para>
		/// </remarks>
		protected virtual void HandleException (Exception exception)
		{
		}

		/// <summary>
		///     Logs some relevant bootstrapper-determined variables.
		/// </summary>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation logs the following variables using <see cref="Log" />: <see cref="ApplicationExecutableDirectory" />, <see cref="ApplicationDataDirectory" />, <see cref="ApplicationAssembly" />, <see cref="ApplicationIdVersionDependent" />, <see cref="ApplicationIdVersionIndependent" />, <see cref="ApplicationVersion" />, <see cref="SessionId" />, <see cref="SessionTimestamp" />, <see cref="ProcessCommandLine" />.
		///     </note>
		/// </remarks>
		protected virtual void LogBootstrapperVariables ()
		{
			this.Log(LogLevel.Debug, "Application name:     {0}", this.ApplicationProductName);
			this.Log(LogLevel.Debug, "Application version:  {0}", this.ApplicationVersion.ToString(4));
			this.Log(LogLevel.Debug, "Executable directory: {0}", this.ApplicationExecutableDirectory.PathResolved);
			this.Log(LogLevel.Debug, "Data directory:       {0}", this.ApplicationDataDirectory.PathResolved);
			this.Log(LogLevel.Debug, "Application assembly: {0}", this.ApplicationAssembly.FullName);
			this.Log(LogLevel.Debug, "Application ID:       {0}", this.ApplicationIdVersionDependent.ToString("N", CultureInfo.InvariantCulture));
			this.Log(LogLevel.Debug, "Version ID:           {0}", this.ApplicationIdVersionIndependent.ToString("N", CultureInfo.InvariantCulture));
			this.Log(LogLevel.Debug, "Session ID:           {0}", this.SessionId.ToString("N", CultureInfo.InvariantCulture));
			this.Log(LogLevel.Debug, "Session timestamp:    {0}", this.SessionTimestamp.ToSortableString('-'));
			this.Log(LogLevel.Debug, "Session 64 bit:       {0}", this.Session64Bit.ToString());
			this.Log(LogLevel.Debug, "Machine 64 bit:       {0}", this.Machine64Bit.ToString());
			this.Log(LogLevel.Debug, "Machine ID:           {0}", this.MachineId.ToString("N", CultureInfo.InvariantCulture));
			this.Log(LogLevel.Debug, "Domain ID:            {0}", this.DomainId.ToString("N", CultureInfo.InvariantCulture));
			this.Log(LogLevel.Debug, "User ID:              {0}", this.UserId.ToString("N", CultureInfo.InvariantCulture));
			this.Log(LogLevel.Debug, "Command line:         {0}", this.ProcessCommandLine.ToString());
		}

		/// <summary>
		///     Called when the splash screen can be created and shown.
		/// </summary>
		/// <remarks>
		///     <note type="implement">
		///         The default implementation does nothing.
		///     </note>
		/// </remarks>
		protected virtual void ShowSplashScreen ()
		{
		}

		/// <summary>
		/// Creates a dictionary which contains anonymous data which can be used as additional data for crash reports.
		/// </summary>
		/// <returns>
		/// The dictionary which contains additional data for crash reports.
		/// </returns>
		/// <remarks>
		/// <para>
		/// The dictionary uses <see cref="StringComparerEx.InvariantCultureIgnoreCase"/> for its keys.
		/// </para>
		/// </remarks>
		protected virtual Dictionary<string, string> CreateAdditionalDataForCrashReport ()
		{
			Dictionary<string, string> additionalData = new Dictionary<string, string>(StringComparerEx.InvariantCultureIgnoreCase);
			additionalData.Add(nameof(this.ApplicationProductName), this.ApplicationProductName);
			additionalData.Add(nameof(this.ApplicationVersion), this.ApplicationVersion.ToString(4));
			additionalData.Add(nameof(this.UserId), this.UserId.ToString("N", CultureInfo.InvariantCulture));
			additionalData.Add(nameof(this.DomainId), this.DomainId.ToString("N", CultureInfo.InvariantCulture));
			additionalData.Add(nameof(this.MachineId), this.MachineId.ToString("N", CultureInfo.InvariantCulture));
			additionalData.Add(nameof(this.Machine64Bit), this.Machine64Bit.ToString());
			additionalData.Add(nameof(this.Session64Bit), this.Session64Bit.ToString());
			additionalData.Add(nameof(this.SessionId), this.SessionId.ToString("N", CultureInfo.InvariantCulture));
			additionalData.Add(nameof(this.SessionTimestamp), this.SessionTimestamp.ToSortableString('-'));
			additionalData.Add(nameof(this.ProcessCommandLine), this.ProcessCommandLine.ToString());
			return additionalData;
		}

		#endregion




		#region Interface: IBootstrapper

		/// <inheritdoc />
		public void Run ()
		{
			if (this.State != WpfBootstrapperState.Uninitialized)
			{
				throw new InvalidOperationException(this.GetType().Name + " is already running.");
			}

			this.Log(LogLevel.Debug, "State: Bootstrapping");
			this.State = WpfBootstrapperState.Bootstrapping;

			bool debuggerAttached = Debugger.IsAttached;
			if (!debuggerAttached)
			{
				AppDomain.CurrentDomain.UnhandledException += (s, a) => this.HandleExceptionInternal(a.ExceptionObject as Exception);
			}

			this.Machine64Bit = Environment.Is64BitOperatingSystem;
			this.Session64Bit = Environment.Is64BitProcess;

			this.DomainId = this.DetermineDomainId();
			this.MachineId = this.DetermineMachineId();
			this.UserId = this.DetermineUserId();

			this.ApplicationAssembly = this.DetermineApplicationAssembly();
			this.ApplicationProductName = this.DetermineApplicationProductName();
			this.ApplicationCompanyName = this.DetermineApplicationCompanyName();
			this.ApplicationCopyright = this.DetermineApplicationCopyright();
			this.ApplicationVersion = this.DetermineApplicationVersion();
			this.ApplicationIdVersionIndependent = this.DetermineApplicationIdVersionIndependent();
			this.ApplicationIdVersionDependent = this.DetermineApplicationIdVersionDependent();

			this.ProcessCommandLine = this.DetermineProcessCommandLine();

			this.SessionTimestamp = this.DetermineSessionTimestamp();
			this.SessionId = this.DetermineSessionId();

			this.ApplicationExecutableDirectory = this.DetermineApplicationExecutableDirectory();
			this.ApplicationDataDirectory = this.DetermineApplicationDataDirectory();

			this.Log(LogLevel.Debug, "Creating container");
			this.Container = this.CreateContainer() ?? new CompositionContainer();

			this.Log(LogLevel.Debug, "Configuring service locator");
			this.ConfigureServiceLocator();

			this.Log(LogLevel.Debug, "Configuring bootstrapper singletons");
			this.ConfigureBootstrapperSingletons();

			this.Log(LogLevel.Debug, "Configuring bootstrapper");
			this.ConfigureBootstrapper();

			this.Log(LogLevel.Debug, "Configuring logging");
			this.ConfigureLogging();

			this.Log(LogLevel.Debug, "Creating application");
			this.Application = this.CreateApplication() ?? new WpfApplication();

			if (!debuggerAttached)
			{
				this.Application.DispatcherUnhandledException += (s, a) => this.HandleExceptionInternal(a.Exception);
			}

			this.Log(LogLevel.Debug, "Configuring application");
			this.ConfigureApplication();

			this.Log(LogLevel.Debug, "Showing splash screen");
			this.ShowSplashScreen();

			this.Log(LogLevel.Debug, "Configuring container");
			this.ConfigureContainer();

			this.Log(LogLevel.Debug, "Configuring services");
			this.ConfigureServices();

			this.Log(LogLevel.Debug, "Configuring modularization");
			this.ConfigureModularization();

			this.Log(LogLevel.Debug, "Logging bootstrapper variables");
			this.LogBootstrapperVariables();

			this.Log(LogLevel.Debug, "State: Running");
			this.State = WpfBootstrapperState.Running;

			this.Log(LogLevel.Debug, "Beginning run");
			this.BeginRun();

			this.Log(LogLevel.Debug, "Handing over to WPF application object");
			this.Application.Run();

			this.Log(LogLevel.Debug, "Ending run");
			this.EndRun();

			this.Log(LogLevel.Debug, "State: Shutting down");
			this.State = WpfBootstrapperState.ShuttingDown;

			this.Log(LogLevel.Debug, "Doing shutdown");
			this.DoShutdown();

			this.Log(LogLevel.Debug, "State: Shut down");
			this.State = WpfBootstrapperState.ShutDown;
		}

		/// <inheritdoc />
		public void Shutdown ()
		{
			if (this.State != WpfBootstrapperState.Running)
			{
				throw new InvalidOperationException(this.GetType().Name + " is not running.");
			}

			if (this.ShutdownInitiated)
			{
				return;
			}

			this.ShutdownInitiated = true;

			this.Log(LogLevel.Debug, "Initiating shutdown");
			this.Application.Shutdown();
		}

		#endregion
	}
}