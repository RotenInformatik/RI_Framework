﻿using System;
using System.Security.Cryptography;
using System.Text;




namespace RI.Framework.Utilities.Windows
{
	/// <summary>
	/// Provides functionality to obtain various unique IDs.
	/// </summary>
	public static class UniqueIdentification
	{
		private static Guid CreateGuidFromString (string data)
		{
			byte[] guidBytes;
			using (MD5 hasher = MD5.Create())
			{
				guidBytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(data));
			}
			Guid guid = new Guid(guidBytes);
			return guid;
		}

		private const string InnerGuid = "D2810CA2E2B74A1CB859644FD5BE32C2";

		/// <summary>
		/// Gets an anonymous ID identifying the current machine.
		/// </summary>
		/// <returns>
		/// The ID as a GUID.
		/// </returns>
		public static Guid GetMachineId ()
		{
			string cipher = LocalEncryption.Encrypt(false, UniqueIdentification.InnerGuid, null);
			Guid guid = UniqueIdentification.CreateGuidFromString(cipher);
			return guid;
		}

		/// <summary>
		/// Gets an anonymous ID identifying the current user.
		/// </summary>
		/// <returns>
		/// The ID as a GUID.
		/// </returns>
		public static Guid GetUserId()
		{
			string cipher = LocalEncryption.Encrypt(true, UniqueIdentification.InnerGuid, null);
			Guid guid = UniqueIdentification.CreateGuidFromString(cipher);
			return guid;
		}

		/// <summary>
		/// Gets an anonymous ID identifying the current network domain.
		/// </summary>
		/// <returns>
		/// The ID as a GUID.
		/// </returns>
		public static Guid GetDomainId()
		{
			string cipher = WindowsUser.GetNetworkDomain();
			Guid guid = UniqueIdentification.CreateGuidFromString(cipher);
			return guid;
		}
	}
}