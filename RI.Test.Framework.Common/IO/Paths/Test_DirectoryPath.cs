﻿using System;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using RI.Framework.Collections;
using RI.Framework.Collections.Comparison;
using RI.Framework.IO.Paths;
using RI.Framework.Utilities.Exceptions;
using RI.Framework.Utilities.ObjectModel;




namespace RI.Test.Framework.IO.Paths
{
	[TestClass]
	public sealed class Test_DirectoryPath
	{
		[TestMethod]
		public void Operations_Test()
		{
			DirectoryPath test = DirectoryPath.GetTempDirectory().AppendDirectories("DirectoryPathTest");

			//-------------------
			// Creation, deletion
			//-------------------

			if (test.Exists)
			{
				throw new TestAssertionException();
			}

			if (!test.Create())
			{
				throw new TestAssertionException();
			}

			if (test.Create())
			{
				throw new TestAssertionException();
			}

			if (!test.Exists)
			{
				throw new TestAssertionException();
			}

			if (!test.Delete())
			{
				throw new TestAssertionException();
			}

			if (test.Delete())
			{
				throw new TestAssertionException();
			}

			//----------------------------------
			// Files not possible for operations
			//----------------------------------

			test = DirectoryPath.GetCurrentDirectory().AppendDirectories("*test*");

			try
			{
				bool temp = test.Exists;
			}
			catch (InvalidOperationException)
			{
			}

			try
			{
				test.Create();
			}
			catch (InvalidOperationException)
			{
			}

			try
			{
				test.Delete();
			}
			catch (InvalidOperationException)
			{
			}
		}

		[TestMethod]
		public void Properties_Test()
		{
			//---------
			// Relative
			//---------

			DirectoryPath test = new DirectoryPath(@"test");

			if (test.DirectoryName != "test")
			{
				throw new TestAssertionException();
			}

			if (test.HasRelatives)
			{
				throw new TestAssertionException();
			}

			if (test.HasWildcards)
			{
				throw new TestAssertionException();
			}

			if (test.IsRoot)
			{
				throw new TestAssertionException();
			}

			if (test.IsRooted)
			{
				throw new TestAssertionException();
			}

			if (test.PathNormalized != @"test")
			{
				throw new TestAssertionException();
			}

			if (test.PathResolved != @"test")
			{
				throw new TestAssertionException();
			}

			if (test.PathOriginal != @"test")
			{
				throw new TestAssertionException();
			}

			if (test.Root != null)
			{
				throw new TestAssertionException();
			}

			if (test.Parent != null)
			{
				throw new TestAssertionException();
			}

			if (test.Type != PathProperties.GetSystemType())
			{
				throw new TestAssertionException();
			}

			//---------
			// Absolute
			//---------

			test = new DirectoryPath(@"c:\abcd\test");

			if (test.DirectoryName != "test")
			{
				throw new TestAssertionException();
			}

			if (test.HasRelatives)
			{
				throw new TestAssertionException();
			}

			if (test.HasWildcards)
			{
				throw new TestAssertionException();
			}

			if (test.IsRoot)
			{
				throw new TestAssertionException();
			}

			if (!test.IsRooted)
			{
				throw new TestAssertionException();
			}

			if (test.PathNormalized != @"c:\abcd\test")
			{
				throw new TestAssertionException();
			}

			if (test.PathResolved != @"c:\abcd\test")
			{
				throw new TestAssertionException();
			}

			if (test.PathOriginal != @"c:\abcd\test")
			{
				throw new TestAssertionException();
			}

			if (test.Root != @"c:")
			{
				throw new TestAssertionException();
			}

			if (test.Parent != @"c:\abcd")
			{
				throw new TestAssertionException();
			}

			if (test.Type != PathProperties.GetSystemType())
			{
				throw new TestAssertionException();
			}
		}

		[TestMethod]
		public void Changes_Test()
		{
			if (new DirectoryPath(@"test").ChangeParent(null) != @"test")
			{
				throw new TestAssertionException();
			}

			if (new DirectoryPath(@"c:\test").ChangeParent(null) != @"test")
			{
				throw new TestAssertionException();
			}

			if (new DirectoryPath(@"test").ChangeParent(@"abcd") != @"abcd\test")
			{
				throw new TestAssertionException();
			}

			if (new DirectoryPath(@"c:\test").ChangeParent(@"abcd") != @"abcd\test")
			{
				throw new TestAssertionException();
			}

			if (new DirectoryPath(@"test").ChangeParent(@"c:\abcd") != @"c:\abcd\test")
			{
				throw new TestAssertionException();
			}

			if (new DirectoryPath(@"c:\test").ChangeParent(@"d:\") != @"d:\test")
			{
				throw new TestAssertionException();
			}

			if (new DirectoryPath(@"c:\test").ChangeParent(@"d:\test") != @"d:\test\test")
			{
				throw new TestAssertionException();
			}

			if (new DirectoryPath(@"c:\test\abcd").ChangeParent(@"c:\1234") != @"c:\1234\abcd")
			{
				throw new TestAssertionException();
			}

			if (new DirectoryPath(@"test").ChangeDirectoryName(@"abcd") != @"abcd")
			{
				throw new TestAssertionException();
			}

			if (new DirectoryPath(@"c:\test").ChangeDirectoryName(@"abcd") != @"c:\abcd")
			{
				throw new TestAssertionException();
			}

			if (new DirectoryPath(@"c:\test\1234").ChangeDirectoryName(@"abcd") != @"c:\test\abcd")
			{
				throw new TestAssertionException();
			}

			try
			{
				new DirectoryPath(@"c:\test").ChangeDirectoryName(null);
				throw new TestAssertionException();
			}
			catch (ArgumentNullException)
			{
			}

			try
			{
				new DirectoryPath(@"c:\test").ChangeDirectoryName(@"");
				throw new TestAssertionException();
			}
			catch (EmptyStringArgumentException)
			{
			}

			try
			{
				new DirectoryPath(@"c:\test").ChangeDirectoryName(@"|");
				throw new TestAssertionException();
			}
			catch (InvalidPathArgumentException)
			{
			}
		}

		[TestMethod]
		public void Equals_Test()
		{
			//---------
			// Operator
			//---------

			if (new DirectoryPath("test.tmp") == (DirectoryPath)"test.dat")
			{
				throw new TestAssertionException();
			}

			if (!(new DirectoryPath("test.tmp") == (DirectoryPath)"test.tmp"))
			{
				throw new TestAssertionException();
			}

			if (new DirectoryPath("test.tmp") != (DirectoryPath)"test.tmp")
			{
				throw new TestAssertionException();
			}

			if (!(new DirectoryPath("test.tmp") != (DirectoryPath)"test.dat"))
			{
				throw new TestAssertionException();
			}

			if (new DirectoryPath("test.tmp") == (DirectoryPath)(string)null)
			{
				throw new TestAssertionException();
			}

			if (!(new DirectoryPath("test.tmp") != (DirectoryPath)(string)null))
			{
				throw new TestAssertionException();
			}

			//-------
			// Static
			//-------

			if (!PathString.Equals((DirectoryPath)null, (DirectoryPath)null))
			{
				throw new TestAssertionException();
			}

			if (PathString.Equals((DirectoryPath)"test.tmp", (DirectoryPath)null))
			{
				throw new TestAssertionException();
			}

			if (PathString.Equals((DirectoryPath)null, (DirectoryPath)"test.tmp"))
			{
				throw new TestAssertionException();
			}

			if (!PathString.Equals((DirectoryPath)"test.tmp", (DirectoryPath)"test.tmp"))
			{
				throw new TestAssertionException();
			}

			if (PathString.Equals((DirectoryPath)"test.tmp", (DirectoryPath)"test.dat"))
			{
				throw new TestAssertionException();
			}

			//-------
			// Object
			//-------

			if (new DirectoryPath("test.tmp").Equals((object)null))
			{
				throw new TestAssertionException();
			}

			if (!new DirectoryPath("test.tmp").Equals((object)(DirectoryPath)"test.tmp"))
			{
				throw new TestAssertionException();
			}

			if (!new DirectoryPath("test.tmp").Equals((object)PathProperties.FromPath("test.tmp", false, false, PathProperties.GetSystemType())))
			{
				throw new TestAssertionException();
			}

			//--------------
			// DirectoryPath
			//--------------

			if (new DirectoryPath("test.tmp").Equals((DirectoryPath)(string)null))
			{
				throw new TestAssertionException();
			}

			if (new DirectoryPath("test.tmp").Equals((DirectoryPath)"test.dat"))
			{
				throw new TestAssertionException();
			}

			if (!new DirectoryPath("test.tmp").Equals((DirectoryPath)"test.tmp"))
			{
				throw new TestAssertionException();
			}

			//---------------
			// PathProperties
			//---------------

			if (new DirectoryPath("test.tmp").Equals((PathProperties)null))
			{
				throw new TestAssertionException();
			}

			if (new DirectoryPath("test.tmp").Equals(PathProperties.FromPath("test.dat", false, false, PathProperties.GetSystemType())))
			{
				throw new TestAssertionException();
			}

			if (!new DirectoryPath("test.tmp").Equals(PathProperties.FromPath("test.tmp", false, false, PathProperties.GetSystemType())))
			{
				throw new TestAssertionException();
			}

			//----------
			// Hash code
			//----------

			if (new DirectoryPath("test.tmp").GetHashCode() != new DirectoryPath("test.tmp").GetHashCode())
			{
				throw new TestAssertionException();
			}

			if (new DirectoryPath("test.tmp").GetHashCode() == new DirectoryPath("test.dat").GetHashCode())
			{
				throw new TestAssertionException();
			}

			//-------
			// String
			//-------

			if (new DirectoryPath("test.tmp").ToString() != "test.tmp")
			{
				throw new TestAssertionException();
			}

			if (new DirectoryPath(@"c:\abcd\test.tmp\").ToString() != @"c:\abcd\test.tmp\")
			{
				throw new TestAssertionException();
			}

			if (((string)((DirectoryPath)null)) != null)
			{
				throw new TestAssertionException();
			}
		}

		[TestMethod]
		public void Compare_Test()
		{
			//---------
			// Operator
			//---------

			if (!(new DirectoryPath("test.tmp") >= new DirectoryPath("test.tmp")))
			{
				throw new TestAssertionException();
			}

			if (!(new DirectoryPath("test.tmp") <= new DirectoryPath("test.tmp")))
			{
				throw new TestAssertionException();
			}

			if (!(new DirectoryPath("test1234.tmp") >= new DirectoryPath("test.tmp")))
			{
				throw new TestAssertionException();
			}

			if (!(new DirectoryPath("test.tmp") <= new DirectoryPath("test1234.tmp")))
			{
				throw new TestAssertionException();
			}

			if (!(new DirectoryPath("test1234.tmp") > new DirectoryPath("test.tmp")))
			{
				throw new TestAssertionException();
			}

			if (!(new DirectoryPath("test.tmp") < new DirectoryPath("test1234.tmp")))
			{
				throw new TestAssertionException();
			}

			if (!(new DirectoryPath("test1234.tmp") >= null))
			{
				throw new TestAssertionException();
			}

			if (new DirectoryPath("test.tmp") <= null)
			{
				throw new TestAssertionException();
			}

			if (!(new DirectoryPath("test1234.tmp") > null))
			{
				throw new TestAssertionException();
			}

			if (new DirectoryPath("test.tmp") < null)
			{
				throw new TestAssertionException();
			}

			//-------
			// Static
			//-------

			if (PathString.Compare((DirectoryPath)null, (DirectoryPath)null) != 0)
			{
				throw new TestAssertionException();
			}

			if (PathString.Compare((DirectoryPath)"test.tmp", (DirectoryPath)null) != 1)
			{
				throw new TestAssertionException();
			}

			if (PathString.Compare((DirectoryPath)null, (DirectoryPath)"test.tmp") != -1)
			{
				throw new TestAssertionException();
			}

			if (PathString.Compare((DirectoryPath)"test.tmp", (DirectoryPath)"test.tmp") != 0)
			{
				throw new TestAssertionException();
			}

			if (PathString.Compare((DirectoryPath)"test.tmp", (DirectoryPath)"test.dat") == 0)
			{
				throw new TestAssertionException();
			}

			//-------
			// Object
			//-------

			if (new DirectoryPath("test.tmp").CompareTo((object)null) != 1)
			{
				throw new TestAssertionException();
			}

			if (new DirectoryPath("test.tmp").CompareTo((object)(DirectoryPath)"test.tmp") != 0)
			{
				throw new TestAssertionException();
			}

			if (new DirectoryPath("test.tmp").CompareTo((object)PathProperties.FromPath("test.tmp", false, false, PathProperties.GetSystemType())) != 0)
			{
				throw new TestAssertionException();
			}

			//--------------
			// DirectoryPath
			//--------------

			if (new DirectoryPath("test.tmp").CompareTo((DirectoryPath)(string)null) != 1)
			{
				throw new TestAssertionException();
			}

			if (new DirectoryPath("test.tmp").CompareTo((DirectoryPath)"test.dat") == 0)
			{
				throw new TestAssertionException();
			}

			if (new DirectoryPath("test.tmp").CompareTo((DirectoryPath)"test.tmp") != 0)
			{
				throw new TestAssertionException();
			}

			//---------------
			// PathProperties
			//---------------

			if (new DirectoryPath("test.tmp").CompareTo((PathProperties)null) != 1)
			{
				throw new TestAssertionException();
			}

			if (new DirectoryPath("test.tmp").CompareTo(PathProperties.FromPath("test.dat", false, false, PathProperties.GetSystemType())) == 0)
			{
				throw new TestAssertionException();
			}

			if (new DirectoryPath("test.tmp").CompareTo(PathProperties.FromPath("test.tmp", false, false, PathProperties.GetSystemType())) != 0)
			{
				throw new TestAssertionException();
			}
		}

		[TestMethod]
		public void Clone_Test()
		{
			ICloneable test1 = new DirectoryPath(@"c:\test\abcd");
			if ((DirectoryPath)test1.Clone() != (DirectoryPath)@"c:\test\abcd")
			{
				throw new TestAssertionException();
			}

			ICloneable<DirectoryPath> test2 = new DirectoryPath(@"c:\test\abcd");
			if (test2.Clone() != (DirectoryPath)@"c:\test\abcd")
			{
				throw new TestAssertionException();
			}

			ICloneable<PathString> test3 = new DirectoryPath(@"c:\test\abcd");
			if (test3.Clone() != (DirectoryPath)@"c:\test\abcd")
			{
				throw new TestAssertionException();
			}
		}

		[TestMethod]
		public void MakeRelativeTo_Test()
		{
			try
			{
				new FilePath(@"c:\abcd\test.tmp").MakeRelativeTo(null);
				throw new TestAssertionException();
			}
			catch (ArgumentNullException)
			{
			}

			try
			{
				new FilePath(@"c:\abcd\test.tmp").MakeRelativeTo(@"1234\abcd");
				throw new TestAssertionException();
			}
			catch (InvalidPathArgumentException)
			{
			}

			if (new FilePath(@"abcd\test.tmp").MakeRelativeTo(@"c:\1234") != @"abcd\test.tmp")
			{
				throw new TestAssertionException();
			}

			if (new FilePath(@"c:\abcd\test.tmp").MakeRelativeTo(@"c:\abcd") != @"test.tmp")
			{
				throw new TestAssertionException();
			}

			if (new FilePath(@"c:\abcd\test.tmp").MakeRelativeTo(@"c:\abcd\1234") != @"..\test.tmp")
			{
				throw new TestAssertionException();
			}

			if (new FilePath(@"c:\abcd\test.tmp").MakeRelativeTo(@"c:\") != @"abcd\test.tmp")
			{
				throw new TestAssertionException();
			}

			if (new FilePath(@"c:\abcd\test.tmp").MakeRelativeTo(@"d:\abcd\test.tmp") != @"c:\abcd\test.tmp")
			{
				throw new TestAssertionException();
			}
		}

		[TestMethod]
		public void MakeAbsoluteFrom_Test()
		{
			try
			{
				new FilePath(@"test.tmp").MakeAbsoluteFrom(null);
				throw new TestAssertionException();
			}
			catch (ArgumentNullException)
			{
			}

			try
			{
				new FilePath(@"test.tmp").MakeAbsoluteFrom(@"1234\abcd");
				throw new TestAssertionException();
			}
			catch (InvalidPathArgumentException)
			{
			}

			if (new FilePath(@"c:\test\1234.tmp").MakeAbsoluteFrom(@"c:\abcd") != @"c:\test\1234.tmp")
			{
				throw new TestAssertionException();
			}

			if (new FilePath(@"test.tmp").MakeAbsoluteFrom(@"c:\abcd") != @"c:\abcd\test.tmp")
			{
				throw new TestAssertionException();
			}

			if (new FilePath(@"..\test.tmp").MakeAbsoluteFrom(@"c:\abcd") != @"c:\test.tmp")
			{
				throw new TestAssertionException();
			}

			if (new FilePath(@"abcd\test.tmp").MakeAbsoluteFrom(@"c:\1234") != @"c:\1234\abcd\test.tmp")
			{
				throw new TestAssertionException();
			}
		}
	}
}
