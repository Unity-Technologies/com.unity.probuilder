# Quality Report

Test strategy & current info for ProBuilder.

- QA Owner: [joelf](joelf@unity3d.com)

## ProBuilder v4.2.0 QA Pass
​
- New added functionalities tested (from changelog list)
  - Offset Elements action
  - Subdivide Edges custom range
  - Metal shader for macOS
- Bug fixes and improvements validated (from changelog list)
  - Boolean Editor fixes and improvements
  - PolyShape Inspector glitch and ProGrids interaction
  - Various UI layout adjustments accros the tool dialogs
- Samples packages validated
  - Rename of everything mentioning LWRP for Universal RP
  - Example scene using materials problems were fixed on the spot and should be now in the final version.
​
NOTE
​
Multiple levels of subdivision that was causing lost of geometry was also fixed and should be included in this version. To be added in the changelog.

## Test Strategy

**Manual Testing**

1. Run test via [TestRails](https://qatestrail.hq.unity3d.com/index.php?/projects/overview/32)

**Unit Tests**

On [Katana](https://katana.bf.unity3d.com/projects/com.unity.probuilder/builders?automation-tools_branch=master&comunityprobuilder_branch=master&package-validation-suite_branch=master&unity_branch=trunk)

Unit tests are distributed as a separate package, named `com.unity.probuilder.tests`.

To run locally, include the test package in your Packages manifest, and make sure `com.unity.probuilder.tests` is included in the `testables` array.
