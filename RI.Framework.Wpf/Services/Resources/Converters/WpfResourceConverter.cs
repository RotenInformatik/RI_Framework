﻿using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using RI.Framework.Composition.Model;
using RI.Framework.Utilities.Exceptions;




namespace RI.Framework.Services.Resources.Converters
{
	/// <summary>
	///     Implements a resource converter which handles common WPF resource types.
	/// </summary>
	/// <remarks>
	///     <para>
	///         The common WPF resource types which are supported by this resource converter are:
	///         <see cref="ImageSource" /> to <see cref="ImageSource" />, <see cref="BitmapSource" /> to <see cref="BitmapSource" />, <see cref="BitmapImage" /> to <see cref="BitmapImage" />, arrays of <see cref="byte" /> to <see cref="ImageSource" />, arrays of <see cref="byte" /> to <see cref="BitmapSource" />, and arrays of <see cref="byte" /> to <see cref="BitmapImage" />.
	///     </para>
	///     <para>
	///         See <see cref="IResourceConverter" /> for more details.
	///     </para>
	/// </remarks>
	[Export]
	public sealed class WpfResourceConverter : IResourceConverter
	{
		#region Interface: IResourceConverter

		/// <inheritdoc />
		public bool CanConvert (Type sourceType, Type targetType)
		{
			if (sourceType == null)
			{
				throw new ArgumentNullException(nameof(sourceType));
			}

			if (targetType == null)
			{
				throw new ArgumentNullException(nameof(targetType));
			}

			if ((sourceType == typeof(BitmapImage)) && ((targetType == typeof(ImageSource)) || (targetType == typeof(BitmapSource)) || (targetType == typeof(BitmapImage))))
			{
				return true;
			}

			if ((sourceType == typeof(BitmapSource)) && ((targetType == typeof(ImageSource)) || (targetType == typeof(BitmapSource))))
			{
				return true;
			}

			if ((sourceType == typeof(ImageSource)) && (targetType == typeof(ImageSource)))
			{
				return true;
			}

			if ((sourceType == typeof(byte[])) && ((targetType == typeof(ImageSource)) || (targetType == typeof(BitmapSource)) || (targetType == typeof(BitmapImage))))
			{
				return true;
			}

			return false;
		}

		/// <inheritdoc />
		public object Convert (Type type, object value)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			if ((value is BitmapImage) && ((type == typeof(ImageSource)) || (type == typeof(BitmapSource)) || (type == typeof(BitmapImage))))
			{
				return value;
			}

			if ((value is BitmapSource) && ((type == typeof(ImageSource)) || (type == typeof(BitmapSource))))
			{
				return value;
			}

			if ((value is ImageSource) && (type == typeof(ImageSource)))
			{
				return value;
			}

			if ((value is byte[]) && ((type == typeof(ImageSource)) || (type == typeof(BitmapSource)) || (type == typeof(BitmapImage))))
			{
				using (MemoryStream ms = new MemoryStream((byte[])value, false))
				{
					BitmapImage image = new BitmapImage();
					image.BeginInit();
					image.CacheOption = BitmapCacheOption.OnLoad;
					image.CreateOptions = BitmapCreateOptions.None;
					image.StreamSource = ms;
					image.EndInit();
					return image.Clone();
				}
			}

			throw new InvalidTypeArgumentException(nameof(value));
		}

		#endregion
	}
}
