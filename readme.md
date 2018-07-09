## quickstart

ProBuilder is split into two packages; the source code, and unit tests. To work with source, add a reference to the `com.unity.probuilder` directory in a Unity project's manifest. The same applies for the unit tests package, though for a package to be visible to the test runner it also needs to be registered with the testables array.

```
{
  "dependencies": {
    "com.unity.probuilder": "file:../../probuilder/com.unity.probuilder",
    "com.unity.probuilder.tests": "file:../../probuilder/com.unity.probuilder.tests",
  },
  "testables":[
  	"com.unity.probuilder.tests"
  ]
}
```

Additionally, there are example projects included for testing upgrading between versions in the `UpgradeProjects` directory. Each project is named according to the version of ProBuilder it was built with. To test the upgrade process from a version, open the example project in Unity, then use Package Manager to install the new version of ProBuilder. To test a new version locally, simply edit the manifest entry for ProBuilder to point at the local copy.

See the readme in `com.unity.probuilder` for more information about working with source.
