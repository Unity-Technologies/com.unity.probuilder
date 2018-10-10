# Quality Report

Test strategy & current info for ProBuilder.

- QA Owner: [gabrielw](gabrielw@unity3d.com)
- UX Owner: [gabrielw](gabrielw@unity3d.com), [karlh](karlh@unity3d.com)

## Package Status

| | |
|--|--|
|Test Rail | [ProBuilder for Unity 2018.x (Package Manager)](https://qatestrail.hq.unity3d.com/index.php?/suites/view/2498) |
| QA Test Results | [ProBuilder Test Rail Results](https://qatestrail.hq.unity3d.com/index.php?/runs/overview/32) |
| Known Bugs | [Favro - World Building Collection](https://fogbugz.unity3d.com/f/filters/?ixPersonAssignedTo=1667) |
| Planning | [Favro - World Building Collection](https://favro.com/organization/c564ede4ed3337f7b17986b6/5458f34f10ce252532bf6d1e) |

## Test Strategy

**Manual Testing**

1. Run test via TestRails: [ProBuilder for Unity 2018.x (Package Manager)](https://qatestrail.hq.unity3d.com/index.php?/suites/view/2498)
2. If QA passed, submit and request publishing via the [Package Publishing Form](https://docs.google.com/forms/d/e/1FAIpQLSdSIRO6s6_gM-BxXbDtdzIej-Hhk-3n68xSyC2sM8tp7413mw/viewform)

**Unit Tests**

On [Katana](https://katana.bf.unity3d.com/projects/com.unity.probuilder/builders?automation-tools_branch=master&comunityprobuilder_branch=master&package-validation-suite_branch=master&unity_branch=trunk)

Unit tests are distributed as a separate package, named `com.unity.probuilder.tests`.

To run locally, include the test package in your Packages manifest, and make sure `com.unity.probuilder.tests` is included in the `testables` array.


