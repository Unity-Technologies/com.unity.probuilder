# Quality Report

Test strategy & current info for ProBuilder.

- QA Owner: [joelf](joelf@unity3d.com)
- UX Owner: [gabrielw](gabrielw@unity3d.com), [karlh](karlh@unity3d.com)

## Package Status

| | |
|--|--|
|Test Rail | [ProBuilder Test Rail - Master](https://qatestrail.hq.unity3d.com/index.php?/projects/overview/32) |
|Latest Test Rail Result - Unity 2018.3 | [PB 4.0.4p11, Unity 2018.3.9f1](https://qatestrail.hq.unity3d.com/index.php?/runs/view/12271) |
|Latest Test Rail Result - Unity 2019.1 | [PB 4.0.4p11, Unity 2019.1.0b7](https://qatestrail.hq.unity3d.com/index.php?/runs/view/12090) |
|Latest Test Rail Result - Unity 2019.2 | [PB 4.0.4p11, Unity 2019.2.0a8](https://qatestrail.hq.unity3d.com/index.php?/runs/view/12266) |
|Latest Test Rail Result - Unity 2019.3 | [PB 4.1.0p6, Unity 2019.3.0a8](https://qatestrail.hq.unity3d.com/index.php?/runs/view/13888) |
| Known Bugs | [JIRA - World Building](https://unity3d.atlassian.net/secure/RapidBoard.jspa?rapidView=73&projectKey=WB&view=planning&selectedIssue=WB-1106&epics=visible) |
| Planning | [JIRA - World Building Epics](https://unity3d.atlassian.net/secure/RapidBoard.jspa?rapidView=73&projectKey=WB&view=planning&selectedIssue=WB-1106&epics=visible) |

## Test Strategy

**Manual Testing**

1. Run test via [TestRails](https://qatestrail.hq.unity3d.com/index.php?/projects/overview/32)

**Unit Tests**

On [Katana](https://katana.bf.unity3d.com/projects/com.unity.probuilder/builders?automation-tools_branch=master&comunityprobuilder_branch=master&package-validation-suite_branch=master&unity_branch=trunk)

Unit tests are distributed as a separate package, named `com.unity.probuilder.tests`.

To run locally, include the test package in your Packages manifest, and make sure `com.unity.probuilder.tests` is included in the `testables` array.
