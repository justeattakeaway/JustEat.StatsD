# Contributing

## Feature requests and bugs

Please submit any feature requests or bugs as an [issue](https://github.com/justeat/JustEat.StatsD/issues) in GitHub.

## Pull requests

The easier your PRs are to review and merge, the more likely your contribution will be accepted. :-)

If you wish to contribute code, please consider the guidelines below:

  1. Create an issue detailing the motivation for the change.
  1. Fork the repository to your GitHub account.
  1. Create a branch to work on your changes.
  1. Try to commit changes in a logical manner. Messy histories will be squashed if merged.
  1. Please follow the existing code style and [EditorConfig](http://editorconfig.org/) formatting settings so that your file touches are consistent with ours and the diff is reduced.
  1. If fixing a bug or adding new functionality, add any tests you deem appropriate.
  1. Test coverage should not go down.
  1. Note any breaking changes in your PR description.
  1. Ensure ```Build.ps1```/```build.sh``` runs with no errors or warnings and all the tests pass.
  1. Open a pull request against the ```master``` branch, referencing your issue, if appropriate.

Once your pull request is opened, the project maintainers will assess it for validity and an appropriate level of quality. For example, the Pull Request status checks should all be green.

If the project maintainers are satisfied that your contribution is appropriate it will be merged into the master branch when appropriate and it will then be released when the library is next published to [NuGet](https://www.nuget.org/profiles/JUSTEAT_OSS).

## Releases

  * AppVeyor and Travis CI should be green on the master branch.
  * Check the version number has been updated since the last release - follow [SemVer rules](http://semver.org)
    * Bump the version in `appveyor.yml` and `version.props` if neccessary.
  * Update the CHANGELOG.md
  * Create a new release in [GitHub](https://github.com/justeat/JustEat.StatsD/releases) with appropriate release notes and tagged version number (for example `v1.2.3-beta01`).
  * Wait for the tag build to complete in AppVeyor and check the metadata/version is as expected.
  * Push the tag build's package to [NuGet](https://www.nuget.org/packages/JustEat.StatsD) using an AppVeyor deployment and wait for the pushed version to be indexed.
  * Share the news! 🎉
