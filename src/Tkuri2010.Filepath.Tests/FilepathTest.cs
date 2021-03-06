using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Tkuri2010.Filepath.PathPrefix;

namespace Tkuri2010.Filepath.Tests
{
	[TestClass]
	public class FilepathTest
	{
		[TestMethod]
		public void Test_Empty()
		{
			var path = Filepath.Parse("");
			Assert.IsTrue(path.Prefix is PathPrefix.Empty);
			Assert.AreEqual("", path.ToString());
		}


		[TestMethod]
		public void Test_Simple_0()
		{
			var path = Filepath.Parse("/");
			Assert.IsTrue(path.Absolute);
			Assert.AreEqual("/", path.ToString("/"));
			Assert.AreEqual("\\", path.ToString("\\"));
		}


		[TestMethod]
		public void Test_Simple_Afile()
		{
			var path = Filepath.Parse("file.txt");
			Assert.IsFalse(path.Absolute);
			Assert.AreEqual("file.txt", path.ToString("/"));
		}


		[TestMethod]
		public void Test_Simple_1()
		{
			var path = Filepath.Parse("a/b/c.tar.gz");
			Assert.IsTrue(path.Prefix is PathPrefix.Empty);
			Assert.IsFalse(path.Absolute);
			Assert.AreEqual(3, path.Items.Length);
			Assert.AreEqual("a", path.Items[0]);
			Assert.AreEqual("b", path.Items[1]);
			Assert.AreEqual("c.tar.gz", path.Items[2]);
			Assert.AreEqual("a/b/c.tar.gz", path.ToString("/"));
		}


		[TestMethod]
		public void Test_Simple_2()
		{
			var path = Filepath.Parse(@"a/\/b///c/\/\/");
			Assert.IsFalse(path.Absolute);
			Assert.AreEqual(3, path.Items.Length);
			Assert.AreEqual("c", path.Items[2]);
		}


		[TestMethod]
		public void Test_Simple_Absolute_1()
		{
			var path = Filepath.Parse("/");
			Assert.IsTrue(path.Prefix is PathPrefix.Empty);
			Assert.IsTrue(path.Absolute);
			Assert.AreEqual(0, path.Items.Length);
			Assert.AreEqual("/", path.ToString("/"));
		}


		[TestMethod]
		public void Test_Simple_Absolute_2()
		{
			Action<string> test_ = pathStr =>
			{
				var path = Filepath.Parse(pathStr);
				Assert.IsTrue(path.Prefix is PathPrefix.Empty);
				Assert.IsTrue(path.Absolute);
				Assert.AreEqual(3, path.Items.Length);
				Assert.AreEqual("/usr/local/bin", path.ToString("/"));
			};

			test_("/usr/local/bin");
			test_("/usr/local/bin/"); // ends with "/"
		}


		[TestMethod]
		public void Test_TraditionalDos_1()
		{
			{ // relative
				var path = Filepath.Parse(@"c:");

				var prefix = path.Prefix as Dos;
				Assert.IsNotNull(prefix);
				Assert.AreEqual("c:", prefix!.Drive);
				Assert.AreEqual("C", prefix!.DriveLetter.ToUpper());
				Assert.IsFalse(path.Absolute);
				Assert.AreEqual(0, path.Items.Length);
			}

			{ // absolute
				var path = Filepath.Parse(@"z:\");

				var prefix = path.Prefix as Dos;
				Assert.IsNotNull(prefix);
				Assert.AreEqual("z:", prefix!.Drive);
				Assert.AreEqual("Z", prefix!.DriveLetter.ToUpper());
				Assert.IsTrue(path.Absolute);
				Assert.AreEqual(0, path.Items.Length);
			}
		}


		[TestMethod]
		public void Test_TraditionalDos_2()
		{
			Action<string> test_ = pathStr =>
			{
				var path = Filepath.Parse(pathStr);
				Assert.IsTrue(path.Absolute);
				Assert.AreEqual(1, path.Items.Length);
			};

			test_(@"C:\a");
			test_(@"C:\a\");
		}


		/// <summary>
		/// win32 drive letter and RELATIVE path
		/// </summary>
		[TestMethod]
		public void Test_TraditionalDos_3()
		{
			{
				var path = Filepath.Parse(@"c:a\b\");
				Assert.IsFalse(path.Absolute);
				Assert.AreEqual(2, path.Items.Length);
			}

			{
				var path = Filepath.Parse(@"c:a\b.txt");
				Assert.IsFalse(path.Absolute);
				Assert.AreEqual(2, path.Items.Length);
			}
		}


		[TestMethod]
		public void Test_DosDevice_1()
		{
			var path = Filepath.Parse(@"\\.\foo\bar\baz");

			var prefix = path.Prefix as DosDevice;
			Assert.IsNotNull(prefix);
			Assert.IsFalse(prefix!.IsUnc);
			Assert.IsTrue(prefix!.Server == null);
			Assert.AreEqual("foo", prefix.Volume);
			Assert.AreEqual(2, path.Items.Length);
			Assert.AreEqual("bar", path.Items[0]);
			Assert.AreEqual("baz", path.Items[1]);
		}


		[TestMethod]
		public void Test_DosDevice_2_UNC()
		{
			var path = Filepath.Parse(@"\\.\UNC\foo\bar\baz");

			var prefix = path.Prefix as DosDevice;
			Assert.IsNotNull(prefix);
			Assert.IsTrue(prefix!.IsUnc);
			Assert.AreEqual("foo", prefix!.Server);
			Assert.AreEqual("bar", prefix!.Share);
			Assert.AreEqual(@"foo\bar", prefix.Volume);
			Assert.AreEqual(1, path.Items.Length);
			Assert.AreEqual("baz", path.Items[0]);
		}


		[TestMethod]
		public void Test_UNC_1()
		{
			{
				var path = Filepath.Parse(@"\\system7\C$");

				var prefix = path.Prefix as Unc;
				Assert.IsNotNull(prefix);
				Assert.AreEqual("system7", prefix!.Server);
				Assert.AreEqual("C$", prefix!.Share);
				Assert.AreEqual(0, path.Items.Length);
			}

			{
				var path = Filepath.Parse(@"\\localhost\share-name\dir\file.txt");
				Assert.AreEqual(2, path.Items.Length);
				Assert.AreEqual("file.txt", path.Items[1]);
			}
		}


		[TestMethod]
		public void Test_LastItem()
		{
			{
				var path = Filepath.Parse("");
				Assert.AreEqual(string.Empty, path.LastItem);
			}

			{
				var path = Filepath.Parse("file.txt");
				Assert.AreEqual("file.txt", path.LastItem);
			}

			{
				var path = Filepath.Parse("dir/file.txt");
				Assert.AreEqual("file.txt", path.LastItem);
			}

			{
				var path = Filepath.Parse("dir1/dir2/file.txt");
				Assert.AreEqual("file.txt", path.LastItem);
			}
		}


		[TestMethod]
		public void Test_Extension()
		{
			{
				var path = Filepath.Parse("a/b/c");
				Assert.IsFalse(path.HasExtension);
				Assert.AreEqual("", path.Extension);
			}

			{
				var path = Filepath.Parse("a/b/c.tar.gz");
				Assert.IsTrue(path.HasExtension);
				Assert.AreEqual(".gz", path.Extension);
			}

			{
				var path = Filepath.Parse("a/b/.git");
				Assert.IsTrue(path.HasExtension);
				Assert.AreEqual(".git", path.Extension);
			}

			{
				var path = Filepath.Parse("a/b/.");
				Assert.IsFalse(path.HasExtension);
				Assert.AreEqual("", path.Extension);
			}

			{
				var path = Filepath.Parse("a/b/..");
				Assert.IsFalse(path.HasExtension);
				Assert.AreEqual("", path.Extension);
			}

			{
				var path = Filepath.Parse("a/b/...");
				Assert.IsFalse(path.HasExtension);
				Assert.AreEqual("", path.Extension);
			}
		}


		[TestMethod]
		public void Test_LastItemWithoutExtension()
		{
			{
				var path = Filepath.Parse("");
				Assert.AreEqual(string.Empty, path.LastItemWithoutExtension);
			}

			{
				var path = Filepath.Parse("/");
				Assert.AreEqual(string.Empty, path.LastItemWithoutExtension);
			}

			{
				var path = Filepath.Parse(".svn");
				Assert.AreEqual(string.Empty, path.LastItemWithoutExtension);
			}

			{
				var path = Filepath.Parse("c:/dir/.git");
				Assert.AreEqual(string.Empty, path.LastItemWithoutExtension);
			}

			{
				var path = Filepath.Parse("/dir/.tar.gz");
				Assert.AreEqual(".tar", path.LastItemWithoutExtension);
			}

			{
				var path = Filepath.Parse("file");
				Assert.AreEqual("file", path.LastItemWithoutExtension);
			}

			{
				var path = Filepath.Parse("file.txt");
				Assert.AreEqual("file", path.LastItemWithoutExtension);
			}

			{
				var path = Filepath.Parse("/usr/bin/file.txt");
				Assert.AreEqual("file", path.LastItemWithoutExtension);
			}
		}


		[TestMethod]
		public void Test_Combine()
		{
			var basepath = Filepath.Parse(@"c:\dir1/dir2");

			{
				var path = basepath.Combine("dir3");

				Assert.AreEqual(@"c:\dir1\dir2\dir3", path.ToString("\\"));

				#region detail

				// 以下、詳細にチェック
				var prefix = basepath.Prefix as Dos;
				Assert.AreEqual("c", prefix!.DriveLetter);

				Assert.AreEqual(3, path.Items.Length);
				Assert.AreEqual("dir1", path.Items[0]);
				Assert.AreEqual("dir2", path.Items[1]);
				Assert.AreEqual("dir3", path.Items[2]);

				#endregion
			}

			{
				var path = basepath.Combine("dir3/file.txt");
				Assert.AreEqual(4, path.Items.Length);
				Assert.AreEqual("file.txt", path.Items[3]);
			}

			{
				var path = basepath.Combine(@"d:\");
				Assert.AreEqual("d", (path.Prefix as Dos)!.DriveLetter);
				Assert.AreEqual(0, path.Items.Length);
			}

			{
				var path = basepath.Combine(@"d:\dirX\dirY");

				Assert.AreEqual(@"d:\dirX\dirY", path.ToString("\\"));

				#region detail
				Assert.AreEqual("d", (path.Prefix as Dos)!.DriveLetter);
				Assert.AreEqual(2, path.Items.Length);
				Assert.AreEqual("dirX", path.Items[0]);
				Assert.AreEqual("dirY", path.Items[1]);
				#endregion
			}

			{ // ちょっと珍しい形式 1
				var path = basepath.Combine(@"d:");

				Assert.AreEqual(@"d:\dir1\dir2", path.ToString("\\"));

				#region detail
				Assert.AreEqual("d", (path.Prefix as Dos)!.DriveLetter);
				Assert.AreEqual(2, path.Items.Length);
				Assert.AreEqual("dir1", path.Items[0]);
				Assert.AreEqual("dir2", path.Items[1]);
				#endregion
			}

			{ // ちょっと珍しい形式 2
				var path = basepath.Combine(@"d:dir3\dir4");

				Assert.AreEqual(@"d:\dir1\dir2\dir3\dir4", path.ToString("\\"));

				#region detail
				Assert.AreEqual("d", (path.Prefix as Dos)!.DriveLetter);
				Assert.AreEqual(4, path.Items.Length);
				Assert.AreEqual("dir3", path.Items[2]);
				Assert.AreEqual("dir4", path.Items[3]);
				#endregion
			}
		}


		[TestMethod]
		public void Test_Slice_1()
		{
			var basepath = Filepath.Parse("/home/kuma/foo/bar/baz.txt");

			// 色々なパラメータを入力しても
			// へんな例外が発生しなければOK
			{
				for (var c = -20; c < 20; c++)
				{
					for (var s = -20; s < 20; s++)
					{
						var path = basepath.Slice(s, c);
						Assert.IsTrue(true);
					}
				}
			}

			{
				var path = basepath.Slice(0);
				Assert.IsTrue(path.Absolute);
				Assert.AreEqual("/home/kuma/foo/bar/baz.txt", path.ToString("/"));
			}

			{
				var path = basepath.Slice(1);
				Assert.IsFalse(path.Absolute);
				Assert.AreEqual("kuma/foo/bar/baz.txt", path.ToString("/"));
			}

			{
				var path = basepath.Slice(-1);
				Assert.IsFalse(path.Absolute);
				Assert.AreEqual(1, path.Items.Length);
				Assert.AreEqual("baz.txt", path.ToString("/"));
			}

			{
				var path = basepath.Slice(-2);
				Assert.IsFalse(path.Absolute);
				Assert.AreEqual(2, path.Items.Length);
				Assert.AreEqual("bar/baz.txt", path.ToString("/"));
			}

			// 2引数
			{
				var path = basepath.Slice(0, 0);
				Assert.IsTrue(path.Absolute);
				Assert.AreEqual("/", path.ToString("/"));
			}

			{
				var path = basepath.Slice(0, 1);
				Assert.IsTrue(path.Absolute);
				Assert.AreEqual("/home", path.ToString("/"));
			}

			{
				var path = basepath.Slice(0, 2);
				Assert.IsTrue(path.Absolute);
				Assert.AreEqual("/home/kuma", path.ToString("/"));
			}

			{
				var path = basepath.Slice(1, 2);
				Assert.IsFalse(path.Absolute);
				Assert.AreEqual("kuma/foo", path.ToString("/"));
			}

			{
				var path = basepath.Slice(2, 2);
				Assert.IsFalse(path.Absolute);
				Assert.AreEqual("foo/bar", path.ToString("/"));
			}

			{
				var path = basepath.Slice(0, -1);
				Assert.IsTrue(path.Absolute);
				Assert.AreEqual("/home/kuma/foo/bar", path.ToString("/"));
			}

			{
				var path = basepath.Slice(0, -2);
				Assert.IsTrue(path.Absolute);
				Assert.AreEqual("/home/kuma/foo", path.ToString("/"));
			}

			{
				var path = basepath.Slice(1, -2);
				Assert.IsFalse(path.Absolute);
				Assert.AreEqual("kuma/foo", path.ToString("/"));
			}

			{
				var path = basepath.Slice(2, -2);
				Assert.IsFalse(path.Absolute);
				Assert.AreEqual("foo", path.ToString("/"));
			}
		}


#if false
		[DllImport("shlwapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern bool PathCanonicalize(
				[Out] StringBuilder lpszDest,
				string lpszSrc);

		[TestMethod]
		public void Try_PathCanonicalize()
		{
			var buf = new StringBuilder(256);
			PathCanonicalize(buf, @".\..\dir\...\xxx\..\file.txt");
			Assert.AreEqual(@"dir\...\file.txt", buf.ToString());
		}
#endif


		[TestMethod]
		public void Test_Canonicalize()
		{
			{
				var path = Filepath.Parse("./../dir/.../xxx/../file.txt");
				Assert.AreEqual("dir/.../file.txt", path.Canonicalize().ToString("/"));
			}
		}


		[TestMethod]
		public void Test_PrefixOfDosDevice_Regex()
		{
			{
				var src1 = FilepathScanner._Prepare(@"\\.\foo");
				var m1 = DosDevice.PREFIX_PATTERN.Match(src1);
				Assert.IsTrue(m1.Success);
				Assert.AreEqual(@".", m1.Groups[1].Value);
				Assert.AreEqual(@"/foo", src1.Substring(m1.Length));
			}

			{
				var src2 = FilepathScanner._Prepare(@"\\?\foo");
				var m2 = DosDevice.PREFIX_PATTERN.Match(src2);
				Assert.IsTrue(m2.Success);
				Assert.AreEqual(@"?", m2.Groups[1].Value);
				Assert.AreEqual(@"/foo", src2.Substring(m2.Length));
			}
		}


		[TestMethod]
		public void Test_PrefixDosDevice_1()
		{
			var src = @"\\.\";
			var scan = new FilepathScanner(src);

			Assert.IsTrue(DosDevice.TryParse(scan, out var prefix));
			Assert.IsFalse(prefix!.IsUnc);
		}


		[TestMethod]
		public void Test_PrefixDosDevice_UNC1()
		{
			var src = @"\\.\UNC";
			var scan = new FilepathScanner(src);

			Assert.IsTrue(DosDevice.TryParse(scan, out var prefix));
			Assert.IsTrue(prefix!.IsUnc);
			//Assert.AreEqual(@"127.0.0.1", prefix.Server);
			//Assert.IsTrue(string.IsNullOrEmpty(prefix.Share));
			//Assert.AreEqual(@"127.0.0.1", prefix.Volume);
		}


		[TestMethod]
		public void Test_PrefixDosDevice_UNC2()
		{
			var src = @"\\.\UNC\127.0.0.1";
			var scan = new FilepathScanner(src);

			Assert.IsTrue(DosDevice.TryParse(scan, out var prefix));
			Assert.IsTrue(prefix!.IsUnc);
			Assert.AreEqual(@"127.0.0.1", prefix!.Server);
			Assert.IsTrue(string.IsNullOrEmpty(prefix!.Share));
			Assert.AreEqual(@"127.0.0.1", prefix!.Volume);
		}


		[TestMethod]
		public void Test_PrefixDosDevice_UNC3()
		{
			var src = @"\\?\UNC\127.0.0.1\share-name";
			var scan = new FilepathScanner(src);

			Assert.IsTrue(DosDevice.TryParse(scan, out var prefix));
			Assert.IsTrue(prefix!.IsUnc);
			Assert.AreEqual(@"127.0.0.1", prefix!.Server);
			Assert.AreEqual(@"share-name", prefix!.Share);
			Assert.AreEqual(@"127.0.0.1\share-name", prefix!.Volume);
		}


		[TestMethod]
		public void Test_PrefixDosDevice_Normal1()
		{
			var src = @"\\.\C:\dir\file.txt";
			var scan = new FilepathScanner(src);

			Assert.IsTrue(DosDevice.TryParse(scan, out var prefix));
			Assert.IsFalse(prefix!.IsUnc);
			Assert.AreEqual(@"C:", prefix!.Volume);
		}


		[TestMethod]
		public void Test_PrefixDosDevice_Normal2()
		{
			var src = @"\\.\Volume{xxx-xxx-xxx}\dir\file.txt";
			var scan = new FilepathScanner(src);

			Assert.IsTrue(DosDevice.TryParse(scan, out var prefix));
			Assert.IsFalse(prefix!.IsUnc);
			Assert.AreEqual(@"Volume{xxx-xxx-xxx}", prefix!.Volume);
		}


		[TestMethod]
		public void Test_PrefixJustUnc_1()
		{
			var src = @"\\server\C$";
			var scan = new FilepathScanner(src);

			Assert.IsTrue(Unc.TryParse(scan, out var prefix));
			Assert.AreEqual("server", prefix!.Server);
			Assert.AreEqual("C$", prefix!.Share);
		}


		[TestMethod]
		public void Test_PrefixJustUnc_2()
		{
			var src = @"\\server\share-name\dir\file.txt";
			var scan = new FilepathScanner(src);

			Assert.IsTrue(Unc.TryParse(scan, out var prefix));
			Assert.AreEqual("server", prefix!.Server);
			Assert.AreEqual("share-name", prefix!.Share);
		}
	}
}
