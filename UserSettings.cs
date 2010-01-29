/*
 * Copyright (C) 2010, Henon <meinrad.recheis@gmail.com>
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or
 * without modification, are permitted provided that the following
 * conditions are met:
 *
 * - Redistributions of source code must retain the above copyright
 *   notice, this list of conditions and the following disclaimer.
 *
 * - Redistributions in binary form must reproduce the above
 *   copyright notice, this list of conditions and the following
 *   disclaimer in the documentation and/or other materials provided
 *   with the distribution.
 *
 * - Neither the name of the project nor the
 *   names of its contributors may be used to endorse or promote
 *   products derived from this software without specific prior
 *   written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;

namespace GitSharp.Demo
{
	public static class UserSettings
	{
		public static string UserSettingsDirectory
		{
			get
			{
				return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GitSharp.Demo");
			}
		}

		public static string GetString(string setting)
		{
			try
			{
				var filename = Path.Combine(UserSettingsDirectory, setting);
				if (!new DirectoryInfo(UserSettingsDirectory).Exists || !new FileInfo(filename).Exists)
					return null;
				return File.ReadAllText(filename);
			}
			catch (IOException) { }
			catch (UnauthorizedAccessException) { }
			catch (SecurityException) { }
			return null;
		}

		public static void SetValue(string setting, string value)
		{
			try
			{
				var filename = Path.Combine(UserSettingsDirectory, setting);
				if (!new DirectoryInfo(UserSettingsDirectory).Exists)
					Directory.CreateDirectory(UserSettingsDirectory);
				if (value == null)
					File.Delete(filename);
				else
					File.WriteAllText(filename, value);
			}
			catch (IOException) { }
			catch (UnauthorizedAccessException) { }
			catch (SecurityException) { }
		}
	}
}

#if DEBUG

namespace Test
{
	using GitSharp.Demo;
	using NUnit.Framework;

	[TestFixture]
	public class UserSettingsTest
	{
		[Test]
		public void ReadWrite()
		{
			Assert.IsNull(UserSettings.GetString("not a valid setting"));
			UserSettings.SetValue("test", "hello world!");
			Assert.AreEqual("hello world!", UserSettings.GetString("test"));
			UserSettings.SetValue("test", null);
			Assert.IsNull(UserSettings.GetString("test"));
		}
	}
}
#endif