using UnityEngine;
using System;
using System.IO;
using NUnit.Framework;
using UnityEditor.SettingsManagement;
using UnityEngine.TestTools;

namespace UnityEngine.SettingsManagement.EditorTests
{
	class SettingsTests : IPrebuildSetup, IPostBuildCleanup
	{
		const string k_SettingsTestsPath = "ProjectSettings/SettingsTests.json";
		static Settings s_Settings;

		static Settings settings
		{
			get
			{
				if(s_Settings == null)
					s_Settings = new Settings(k_SettingsTestsPath);
				return s_Settings;
			}
		}

		public void Setup()
		{
		}

		public void Cleanup()
		{
			foreach (var v in s_AllPreferences)
				v.Delete();

			if (File.Exists(k_SettingsTestsPath))
				File.Delete(k_SettingsTestsPath);
		}

		[Serializable]
		struct DummyStruct : IEquatable<DummyStruct>
		{
			public string stringValue;
			public int intValue;

			public DummyStruct(string s, int i)
			{
				stringValue = s;
				intValue = i;
			}

			public static DummyStruct defaultValue
			{
				get { return new DummyStruct("I'm a string!", 42); }
			}

			public bool Equals(DummyStruct other)
			{
				return string.Equals(stringValue, other.stringValue) && intValue == other.intValue;
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj))
					return false;
				return obj is DummyStruct && Equals((DummyStruct)obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return ((stringValue != null ? stringValue.GetHashCode() : 0) * 397) ^ intValue;
				}
			}

			public static bool operator ==(DummyStruct left, DummyStruct right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(DummyStruct left, DummyStruct right)
			{
				return !left.Equals(right);
			}

			public override string ToString()
			{
				return stringValue + "  " + intValue;
			}
		}

		[Serializable]
		struct DummyClass : IEquatable<DummyClass>
		{
			public string stringValue;
			public int intValue;

			public DummyClass(string s, int i)
			{
				stringValue = s;
				intValue = i;
			}

			public static DummyClass defaultValue
			{
				get { return new DummyClass("I'm a string!", 42); }
			}

			public bool Equals(DummyClass other)
			{
				return string.Equals(stringValue, other.stringValue) && intValue == other.intValue;
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj))
					return false;
				return obj is DummyClass && Equals((DummyClass)obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return ((stringValue != null ? stringValue.GetHashCode() : 0) * 397) ^ intValue;
				}
			}

			public static bool operator ==(DummyClass left, DummyClass right)
			{
				return left.Equals(right);
			}

			public static bool operator !=(DummyClass left, DummyClass right)
			{
				return !left.Equals(right);
			}

			public override string ToString()
			{
				return stringValue + "  " + intValue;
			}
		}

		static UserSetting<bool> s_StaticBoolUser = new UserSetting<bool>(settings, "tests.user.static.bool", true, SettingScope.User);
		static UserSetting<bool> s_StaticBoolProject = new UserSetting<bool>(settings, "tests.project.static.bool", true, SettingScope.Project);

		static UserSetting<string> s_StaticStringUser = new UserSetting<string>(settings, "tests.user.static.string", "Hello, world!", SettingScope.User);
		static UserSetting<string> s_StaticStringProject = new UserSetting<string>(settings, "tests.project.static.string", "Goodbye, world!", SettingScope.Project);

		static UserSetting<DummyStruct> s_StaticStructUser = new UserSetting<DummyStruct>(settings, "tests.user.static.struct", DummyStruct.defaultValue, SettingScope.User);
		static UserSetting<DummyStruct> s_StaticStructProject = new UserSetting<DummyStruct>(settings, "tests.project.static.struct", DummyStruct.defaultValue, SettingScope.Project);

		static UserSetting<DummyClass> s_StaticClassUser = new UserSetting<DummyClass>(settings, "tests.user.static.class", DummyClass.defaultValue, SettingScope.User);
		static UserSetting<DummyClass> s_StaticClassProject = new UserSetting<DummyClass>(settings, "tests.project.static.class", DummyClass.defaultValue, SettingScope.Project);

		static IUserSetting[] s_AllPreferences = new IUserSetting[]
		{
			s_StaticBoolUser,
			s_StaticBoolProject,
			s_StaticStringUser,
			s_StaticStringProject,
			s_StaticStructUser,
			s_StaticStructProject,
			s_StaticClassUser,
			s_StaticClassProject
		};

		[Test]
		public static void DefaultsAreCorrect()
		{
			foreach (var pref in s_AllPreferences)
				pref.Reset();

			Assert.IsTrue((bool) s_StaticBoolUser, s_StaticBoolUser.ToString());
			Assert.IsTrue((bool) s_StaticBoolProject, s_StaticBoolProject.ToString());

			Assert.AreEqual("Hello, world!", (string) s_StaticStringUser, s_StaticStringUser.ToString());
			Assert.AreEqual("Goodbye, world!", (string) s_StaticStringProject, s_StaticStringProject.ToString());

			Assert.AreEqual(DummyStruct.defaultValue, (DummyStruct) s_StaticStructUser, s_StaticStructUser.ToString());
			Assert.AreEqual(DummyStruct.defaultValue, (DummyStruct) s_StaticStructProject, s_StaticStructProject.ToString());

			Assert.AreEqual(DummyClass.defaultValue, (DummyClass) s_StaticClassUser, s_StaticClassUser.ToString());
			Assert.AreEqual(DummyClass.defaultValue, (DummyClass) s_StaticClassProject, s_StaticClassProject.ToString());
		}

		[Test]
		public static void SetValue()
		{
			// BOOl
			s_StaticBoolUser.value = false;
			s_StaticBoolProject.value = false;

			Assert.IsFalse((bool) s_StaticBoolUser);
			Assert.IsFalse((bool) s_StaticBoolProject);

			// STRING
			s_StaticStringUser.value = "Some more text";
			s_StaticStringProject.value = "Some text here";

			Assert.AreEqual("Some more text", (string) s_StaticStringUser);
			Assert.AreEqual("Some text here", (string) s_StaticStringProject);

			// STRUCT
			var userStruct = new DummyStruct("Changed text", 23);
			var projectStruct = new DummyStruct("Slightly different text", -9825);

			s_StaticStructUser.SetValue(userStruct);
			s_StaticStructProject.SetValue(projectStruct);

			Assert.AreEqual(userStruct, (DummyStruct) s_StaticStructUser);
			Assert.AreEqual(projectStruct, (DummyStruct) s_StaticStructProject);

			// CLASS
			var userClass = new DummyClass("Changed text", 23);
			var projectClass = new DummyClass("Slightly different text", -9825);

			s_StaticClassUser.SetValue(userClass);
			s_StaticClassProject.SetValue(projectClass);

			Assert.AreEqual(userClass, (DummyClass) s_StaticClassUser);
			Assert.AreEqual(projectClass, (DummyClass) s_StaticClassProject);
		}

		[Test]
		public static void SetAndReset()
		{
			// BOOL
			s_StaticBoolUser.value = false;
			s_StaticBoolProject.value = false;

			// STRING
			s_StaticStringUser.value = "Some more text";
			s_StaticStringProject.value = "Some text here";

			// STRUCT
			s_StaticStructUser.SetValue(new DummyStruct("Changed text", 23));
			s_StaticStructProject.SetValue(new DummyStruct("Slightly different text", -9825));

			// CLASS
			s_StaticClassUser.SetValue(new DummyClass("Changed text", 23));
			s_StaticClassProject.SetValue(new DummyClass("Slightly different text", -9825));

			Assert.IsFalse((bool) s_StaticBoolUser);
			Assert.IsFalse((bool) s_StaticBoolProject);

			Assert.AreNotEqual("Hello, world!", (string) s_StaticStringUser);
			Assert.AreNotEqual("Goodbye, world!", (string) s_StaticStringProject);

			Assert.AreNotEqual(DummyStruct.defaultValue, (DummyStruct) s_StaticStructUser);
			Assert.AreNotEqual(DummyStruct.defaultValue, (DummyStruct) s_StaticStructProject);

			Assert.AreNotEqual(DummyClass.defaultValue, (DummyClass) s_StaticClassUser);
			Assert.AreNotEqual(DummyClass.defaultValue, (DummyClass) s_StaticClassProject);

			foreach (var pref in s_AllPreferences)
				pref.Reset();

			Assert.IsTrue((bool) s_StaticBoolUser);
			Assert.IsTrue((bool) s_StaticBoolProject);

			Assert.AreEqual("Hello, world!", (string) s_StaticStringUser);
			Assert.AreEqual("Goodbye, world!", (string) s_StaticStringProject);

			Assert.AreEqual(DummyStruct.defaultValue, (DummyStruct) s_StaticStructUser);
			Assert.AreEqual(DummyStruct.defaultValue, (DummyStruct) s_StaticStructProject);

			Assert.AreEqual(DummyClass.defaultValue, (DummyClass) s_StaticClassUser);
			Assert.AreEqual(DummyClass.defaultValue, (DummyClass) s_StaticClassProject);
		}

		[Test]
		public static void SerializeAndLoad()
		{
			foreach(var pref in s_AllPreferences)
				pref.Reset();
			settings.Save();

			var instance = new Settings(k_SettingsTestsPath);
			instance.Load();

			Assert.AreEqual((bool) s_StaticBoolUser, instance.Get<bool>(s_StaticBoolUser.key, s_StaticBoolUser.scope));
			Assert.AreEqual((bool) s_StaticBoolProject, instance.Get<bool>(s_StaticBoolProject.key, s_StaticBoolProject.scope));
			Assert.AreEqual((string) s_StaticStringUser, instance.Get<string>(s_StaticStringUser.key, s_StaticStringUser.scope));
			Assert.AreEqual((string) s_StaticStringProject, instance.Get<string>(s_StaticStringProject.key, s_StaticStringProject.scope));
			Assert.AreEqual((DummyStruct) s_StaticStructUser, instance.Get<DummyStruct>(s_StaticStructUser.key, s_StaticStructUser.scope));
			Assert.AreEqual((DummyStruct) s_StaticStructProject, instance.Get<DummyStruct>(s_StaticStructProject.key, s_StaticStructProject.scope));
			Assert.AreEqual((DummyClass) s_StaticClassUser, instance.Get<DummyClass>(s_StaticClassUser.key, s_StaticClassUser.scope));
			Assert.AreEqual((DummyClass) s_StaticClassProject, instance.Get<DummyClass>(s_StaticClassProject.key, s_StaticClassProject.scope));
		}

		[Test]
		public static void DeleteKeys()
		{
			foreach(var pref in s_AllPreferences)
				pref.Delete();

			settings.Save();
			var instance = new Settings(k_SettingsTestsPath);
			instance.Load();

			Assert.IsFalse(instance.ContainsKey<bool>("tests.user.static.bool", SettingScope.User), "tests.user.static.bool");
			Assert.IsFalse(instance.ContainsKey<bool>("tests.project.static.bool", SettingScope.Project), "tests.project.static.bool");
			Assert.IsFalse(instance.ContainsKey<string>("tests.user.static.string", SettingScope.User), "tests.user.static.string");
			Assert.IsFalse(instance.ContainsKey<string>("tests.project.static.string", SettingScope.Project), "tests.project.static.string");
			Assert.IsFalse(instance.ContainsKey<DummyStruct>("tests.user.static.struct", SettingScope.User), "tests.user.static.struct");
			Assert.IsFalse(instance.ContainsKey<DummyStruct>("tests.project.static.struct", SettingScope.Project), "tests.project.static.struct");
			Assert.IsFalse(instance.ContainsKey<DummyClass>("tests.user.static.class", SettingScope.User), "tests.user.static.class");
			Assert.IsFalse(instance.ContainsKey<DummyClass>("tests.project.static.class", SettingScope.Project), "tests.project.static.class");
		}

		[Test]
		public static void KeysExistInSettingsInstance()
		{
			foreach(var pref in s_AllPreferences)
				pref.Reset();

			settings.Save();

			Assert.IsTrue(settings.ContainsKey<bool>("tests.user.static.bool", SettingScope.User), "tests.user.static.bool");
			Assert.IsTrue(settings.ContainsKey<bool>("tests.project.static.bool", SettingScope.Project), "tests.project.static.bool");
			Assert.IsTrue(settings.ContainsKey<string>("tests.user.static.string", SettingScope.User), "tests.user.static.string");
			Assert.IsTrue(settings.ContainsKey<string>("tests.project.static.string", SettingScope.Project), "tests.project.static.string");
			Assert.IsTrue(settings.ContainsKey<DummyStruct>("tests.user.static.struct", SettingScope.User), "tests.user.static.struct");
			Assert.IsTrue(settings.ContainsKey<DummyStruct>("tests.project.static.struct", SettingScope.Project), "tests.project.static.struct");
			Assert.IsTrue(settings.ContainsKey<DummyClass>("tests.user.static.class", SettingScope.User), "tests.user.static.class");
			Assert.IsTrue(settings.ContainsKey<DummyClass>("tests.project.static.class", SettingScope.Project), "tests.project.static.class");
		}

		[Test]
		public static void KeysExistInSerializedForm()
		{
			foreach(var pref in s_AllPreferences)
				pref.Reset();

			settings.Save();

			var instance = new Settings(k_SettingsTestsPath);
			instance.Load();

			Assert.IsTrue(instance.ContainsKey<bool>("tests.user.static.bool", SettingScope.User), "tests.user.static.bool");
			Assert.IsTrue(instance.ContainsKey<bool>("tests.project.static.bool", SettingScope.Project), "tests.project.static.bool");
			Assert.IsTrue(instance.ContainsKey<string>("tests.user.static.string", SettingScope.User), "tests.user.static.string");
			Assert.IsTrue(instance.ContainsKey<string>("tests.project.static.string", SettingScope.Project), "tests.project.static.string");
			Assert.IsTrue(instance.ContainsKey<DummyStruct>("tests.user.static.struct", SettingScope.User), "tests.user.static.struct");
			Assert.IsTrue(instance.ContainsKey<DummyStruct>("tests.project.static.struct", SettingScope.Project), "tests.project.static.struct");
			Assert.IsTrue(instance.ContainsKey<DummyClass>("tests.user.static.class", SettingScope.User), "tests.user.static.class");
			Assert.IsTrue(instance.ContainsKey<DummyClass>("tests.project.static.class", SettingScope.Project), "tests.project.static.class");
		}
	}
}