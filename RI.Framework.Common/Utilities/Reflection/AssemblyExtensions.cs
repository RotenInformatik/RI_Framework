﻿using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

using RI.Framework.Utilities.Exceptions;

namespace RI.Framework.Utilities.Reflection
{
	/// <summary>
	///     Provides utility/extension methods for the <see cref="Assembly" /> type.
	/// </summary>
	public static class AssemblyExtensions
	{
		#region Static Methods

		/// <summary>
		///     Gets the assembly version of an assembly.
		/// </summary>
		/// <param name="assembly"> The assembly. </param>
		/// <returns>
		///     The assembly version of the assembly or null if the version could not be determined.
		/// </returns>
		/// <remarks>
		///     <para>
		///         The <see cref="AssemblyVersionAttribute" /> is used to determine the assembly version of an assembly.
		///     </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException"> <paramref name="assembly" /> is null. </exception>
		public static Version GetAssemblyVersion (this Assembly assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			return assembly.GetName().Version;
		}

		/// <summary>
		///     Gets the company of an assembly.
		/// </summary>
		/// <param name="assembly"> The assembly. </param>
		/// <returns>
		///     The company of the assembly or null if the company could not be determined.
		/// </returns>
		/// <remarks>
		///     <para>
		///         The <see cref="AssemblyCompanyAttribute" /> is used to determine the company of an assembly.
		///     </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException"> <paramref name="assembly" /> is null. </exception>
		public static string GetCompany (this Assembly assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true);

			if (attributes.Length == 0)
			{
				return null;
			}

			return ((AssemblyCompanyAttribute)attributes[0]).Company;
		}

		/// <summary>
		///     Gets the copyright of an assembly.
		/// </summary>
		/// <param name="assembly"> The assembly. </param>
		/// <returns>
		///     The copyright of the assembly or null if the copyright could not be determined.
		/// </returns>
		/// <remarks>
		///     <para>
		///         The <see cref="AssemblyCopyrightAttribute" /> is used to determine the copyright of an assembly.
		///     </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException"> <paramref name="assembly" /> is null. </exception>
		public static string GetCopyright (this Assembly assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true);

			if (attributes.Length == 0)
			{
				return null;
			}

			return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
		}

		/// <summary>
		///     Gets the description of an assembly.
		/// </summary>
		/// <param name="assembly"> The assembly. </param>
		/// <returns>
		///     The description of the assembly or null if the description could not be determined.
		/// </returns>
		/// <remarks>
		///     <para>
		///         The <see cref="AssemblyDescriptionAttribute" /> is used to determine the description of an assembly.
		///     </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException"> <paramref name="assembly" /> is null. </exception>
		public static string GetDescription (this Assembly assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), true);

			if (attributes.Length == 0)
			{
				return null;
			}

			return ((AssemblyDescriptionAttribute)attributes[0]).Description;
		}

		/// <summary>
		///     Gets the file of an assembly.
		/// </summary>
		/// <param name="assembly"> The assembly. </param>
		/// <returns>
		///     The file of the assembly or null if the file could not be determined.
		/// </returns>
		/// <remarks>
		///     <note type="important">
		///         This method is only supported on Windows (without Store Apps) and Mac OS.
		///     </note>
		/// </remarks>
		/// <exception cref="ArgumentNullException"> <paramref name="assembly" /> is null. </exception>
		/// <exception cref="NotSupportedException"> Called on a platform other than Windows or Mac OS. </exception>
		public static string GetFile (this Assembly assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

#if PLATFORM_UNITY
			if ((UnityEngine.Application.platform != UnityEngine.RuntimePlatform.WindowsEditor) && (UnityEngine.Application.platform != UnityEngine.RuntimePlatform.WindowsPlayer) && (UnityEngine.Application.platform != UnityEngine.RuntimePlatform.OSXEditor) && (UnityEngine.Application.platform != UnityEngine.RuntimePlatform.OSXPlayer) && (UnityEngine.Application.platform != UnityEngine.RuntimePlatform.OSXDashboardPlayer))
			{
				throw new NotSupportedException(nameof(AssemblyExtensions.GetFile) + " is only supported on Windows and Mac OS.");
			}
#endif

#if PLATFORM_NETFX
			if (assembly.IsDynamic)
			{
				return null;
			}
#endif

			string location;

			try
			{
				location = assembly.Location;
			}
			catch
			{
				return null;
			}

			if (location.IsEmptyOrWhitespace())
			{
				return null;
			}

			return location;
		}

		/// <summary>
		///     Gets the file version of an assembly.
		/// </summary>
		/// <param name="assembly"> The assembly. </param>
		/// <returns>
		///     The file version of the assembly or null if the version could not be determined.
		/// </returns>
		/// <remarks>
		///     <para>
		///         The <see cref="AssemblyFileVersionAttribute" /> is used to determine the file version of an assembly.
		///     </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException"> <paramref name="assembly" /> is null. </exception>
		public static Version GetFileVersion (this Assembly assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true);

			if (attributes.Length == 0)
			{
				return null;
			}

			return ((AssemblyFileVersionAttribute)attributes[0]).Version.ToVersion();
		}

		/// <summary>
		///     Gets a GUID associated with an assembly.
		/// </summary>
		/// <param name="assembly"> The assembly. </param>
		/// <param name="ignoreGuidAttribute"> Specifies whether <see cref="GuidAttribute" /> is ignored for determining the GUID of the assembly. </param>
		/// <param name="ignoreVersion"> Specifies whether the assemblies version should be ignored for determining the GUID of the assembly. </param>
		/// <returns>
		///     The GUID of the assembly.
		/// </returns>
		/// <remarks>
		///     <para>
		///         If <paramref name="ignoreGuidAttribute" /> is false and the assembly has a <see cref="GuidAttribute" />, the GUID from that attribute is returned.
		///     </para>
		///     <para>
		///         If <paramref name="ignoreGuidAttribute" /> is true or <see cref="GuidAttribute" /> is not defined, the following is used to calculate a GUID:
		///         <see cref="AssemblyName.Name" /> when <paramref name="ignoreVersion" /> is true, <see cref="AssemblyName.FullName" /> otherwise.
		///     </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException"> <paramref name="assembly" /> is null. </exception>
		public static Guid GetGuid (this Assembly assembly, bool ignoreGuidAttribute, bool ignoreVersion)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			object[] attributes = assembly.GetCustomAttributes(typeof(GuidAttribute), true);

			if ((attributes.Length > 0) && (!ignoreGuidAttribute))
			{
				Guid? guidCandidate = ((GuidAttribute)attributes[0]).Value.ToGuid();
				if (guidCandidate.HasValue)
				{
					return guidCandidate.Value;
				}
			}

			AssemblyName assemblyName = assembly.GetName();

			string guidInformationString = ignoreVersion ? assemblyName.Name : assemblyName.FullName;
			byte[] guidInformationBytes = Encoding.UTF8.GetBytes(guidInformationString);

			byte[] guidBytes = new byte[16];

			for (int i1 = 0; i1 < guidInformationBytes.Length; i1++)
			{
				guidBytes[i1 % 16] = (byte)((guidBytes[i1 % 16] + guidInformationBytes[i1]) % 255);
			}

			Guid guid = new Guid(guidBytes);
			return guid;
		}

		/// <summary>
		///     Gets the informational version of an assembly.
		/// </summary>
		/// <param name="assembly"> The assembly. </param>
		/// <returns>
		///     The informational version of the assembly or null if the version could not be determined.
		/// </returns>
		/// <remarks>
		///     <para>
		///         The <see cref="AssemblyInformationalVersionAttribute" /> is used to determine the informational version of an assembly.
		///     </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException"> <paramref name="assembly" /> is null. </exception>
		public static Version GetInformationalVersion (this Assembly assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), true);

			if (attributes.Length == 0)
			{
				return null;
			}

			return ((AssemblyInformationalVersionAttribute)attributes[0]).InformationalVersion.ToVersion();
		}

		/// <summary>
		///     Gets the product name of an assembly.
		/// </summary>
		/// <param name="assembly"> The assembly. </param>
		/// <returns>
		///     The product name of the assembly or null if the product name could not be determined.
		/// </returns>
		/// <remarks>
		///     <para>
		///         The <see cref="AssemblyProductAttribute" /> is used to determine the product name of an assembly.
		///     </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException"> <paramref name="assembly" /> is null. </exception>
		public static string GetProduct (this Assembly assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), true);

			if (attributes.Length == 0)
			{
				return null;
			}

			return ((AssemblyProductAttribute)attributes[0]).Product;
		}

		/// <summary>
		///     Gets the title of an assembly.
		/// </summary>
		/// <param name="assembly"> The assembly. </param>
		/// <returns>
		///     The title of the assembly or null if the title could not be determined.
		/// </returns>
		/// <remarks>
		///     <para>
		///         The <see cref="AssemblyTitleAttribute" /> is used to determine the title of an assembly.
		///     </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException"> <paramref name="assembly" /> is null. </exception>
		public static string GetTitle (this Assembly assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), true);

			if (attributes.Length == 0)
			{
				return null;
			}

			return ((AssemblyTitleAttribute)attributes[0]).Title;
		}

		/// <summary>
		/// Gets the stream of an embedded file.
		/// </summary>
		/// <param name="assembly"> The assembly. </param>
		/// <param name="file">The name of the embedded resource or the file which is embedded in the assembly respectively.</param>
		/// <returns>
		/// The stream to access the embedded file or null if the embedded file was not found.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> or <paramref name="file"/> is null.</exception>
		/// <exception cref="EmptyStringArgumentException"><paramref name="file"/> is an empty string.</exception>
		public static Stream GetEmbeddedFileStream(this Assembly assembly, string file)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (file == null)
			{
				throw new ArgumentNullException(nameof(file));
			}

			if (file.IsNullOrEmptyOrWhitespace())
			{
				throw new EmptyStringArgumentException(nameof(file));
			}

			AssemblyName name = assembly.GetName();
			string prefix = name.Name + ".";

			if (file.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
			{
				file = file.Substring(prefix.Length);
			}

			file = file.Replace('/', '.');
			file = file.Replace('\\', '.');
			file = file.TrimStart('.');

			return assembly.GetManifestResourceStream(prefix + file);
		}

		#endregion
	}
}
