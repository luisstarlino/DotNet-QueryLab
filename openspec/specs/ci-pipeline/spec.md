# ci-pipeline Specification

## Purpose

Defines the GitHub Actions continuous integration pipeline that builds the solution and runs the xUnit test suite, exporting results in JUnit format and publishing them to commit/PR checks without requiring an external Docker daemon.

## Requirements

### Requirement: GitHub Actions workflow for tests with JUnit export
A workflow file `.github/workflows/tests.yml` SHALL run on every push and pull_request to `main`. It SHALL: install .NET 10, restore dependencies, build in Release, run `dotnet test` with JUnit XML logger, and upload test results as a GitHub Actions artifact. The `dorny/test-reporter` action SHALL publish results to the commit/PR check.

#### Scenario: Workflow triggers on push to main
- **WHEN** a commit is pushed to the `main` branch
- **THEN** the `Tests` workflow starts and runs `dotnet test`

#### Scenario: Test results are exported as artifact
- **WHEN** the workflow completes (pass or fail)
- **THEN** a JUnit XML file is uploaded as a GitHub Actions artifact named `test-results`

#### Scenario: Test reporter publishes results to PR
- **WHEN** a pull request is opened or updated
- **THEN** `dorny/test-reporter` posts a test summary with pass/fail counts to the PR checks

### Requirement: Workflow does not require external Docker daemon
Tests SHALL use Testcontainers which manages its own Docker-in-Docker setup via `testcontainers-dotnet`. The CI runner SHALL use `ubuntu-latest` which has Docker available. No manual `docker-compose up` step is needed in CI.

#### Scenario: Tests run without manual Docker Compose in CI
- **WHEN** the workflow runs on `ubuntu-latest`
- **THEN** Testcontainers starts PostgreSQL automatically and tests pass without a `docker-compose up` step
