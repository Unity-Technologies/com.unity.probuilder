# Quality Report

Test strategy & current info for ProBuilder.

- QA Owner: [gabrielw](gabrielw@unity3d.com)
- UX Owner: [gabrielw](gabrielw@unity3d.com), [karlh](karlh@unity3d.com)

## Package Status

| | |
|--|--|
|Test Rail | [ProBuilder for Unity 2018.x (Package Manager)](https://qatestrail.hq.unity3d.com/index.php?/projects/overview/32) |
| QA Test Results | [ProBuilder Test Rail Results](https://qatestrail.hq.unity3d.com/index.php?/runs/view/11092&group_by=cases:section_id&group_order=asc) |
| Known Bugs | [JIRA - World Building](https://unity3d.atlassian.net/secure/RapidBoard.jspa?rapidView=73&projectKey=WB&view=planning&selectedIssue=WB-1106&epics=visible) |
| Planning | [JIRA - World Building Epics](https://unity3d.atlassian.net/secure/RapidBoard.jspa?rapidView=73&projectKey=WB&view=planning&selectedIssue=WB-1106&epics=visible) |

## Test Strategy

**Manual Testing**

1. Run test via TestRails: [ProBuilder for Unity 2018.x (Package Manager)](https://qatestrail.hq.unity3d.com/index.php?/projects/overview/32)

**Unit Tests**

On [Katana](https://katana.bf.unity3d.com/projects/com.unity.probuilder/builders?automation-tools_branch=master&comunityprobuilder_branch=master&package-validation-suite_branch=master&unity_branch=trunk)

Unit tests are distributed as a separate package, named `com.unity.probuilder.tests`.

To run locally, include the test package in your Packages manifest, and make sure `com.unity.probuilder.tests` is included in the `testables` array.
