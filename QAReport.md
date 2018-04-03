# Quality Report

Test strategy & current info for ProBuilder.

- QA Owner: [gabrielw](gabrielw@unity3d.com)
- UX Owner: [gabrielw](gabrielw@unity3d.com), [karlh](karlh@unity3d.com)

## Package Status

| | |
|--|--|
| QA Test Results | [ProBuilder QA Sheet](https://docs.google.com/spreadsheets/d/1B4cszFPbVvRvUDm2hqoyWaf98MHkB_TLQhCYtM2E9Bs/edit#gid=1764739309) |
| Known Bugs | [ProBuilder Issue Tracker on Github](https://github.com/procore3d/probuilder2/issues) |
| Planning | [Favro - World Building Collection](https://favro.com/organization/c564ede4ed3337f7b17986b6/5458f34f10ce252532bf6d1e) |

## Test Strategy

**Manual Testing**

1. Create a new Unity project, install ProBuilder (see readme for instructions).
1. Test any items that are new or modified in this release (`Tools/ProBuilder/About` will list new features).
1. Open the [ProBuilder QA Sheet](https://docs.google.com/a/unity3d.com/spreadsheets/d/1B4cszFPbVvRvUDm2hqoyWaf98MHkB_TLQhCYtM2E9Bs/edit?usp=sharing), and create a new duplicate named "ProBuilder 3.0.0-f.0" (substituting for the current version).
1. Rename "Trunk" column to the Unity version tested against (or if testing a backport use of the existing columns).
1. Test each item in the "Test in a New Project" section, and mark results (see "Legend" notes in the QA Sheet).

**Unit Tests**

Unit tests are currently only available in the development branch, [here](https://github.com/procore3d/probuilder2). To run Unit Tests, open the "Window > Test Runner" and run all "probuilder2.0" tests.