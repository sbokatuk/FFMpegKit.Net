---
applyTo: ".github/workflows/*.yml"
---

# Workflow rules

- The default branch is **`master`**, not `main` (`auto-release.yml`'s branch filter). The sibling
  platform repositories use `main`; never copy a branch filter or ref between them unadjusted.
- Publishing to nuget.org uses **trusted publishing**: `NuGet/login@v1` with `user: ${{ secrets.NUGET_USER }}`,
  the job on `environment: nuget.org` and `permissions: id-token: write`. `NUGET_USER` is the only
  publishing secret — never add an API key, and never move a push step out of that environment, because the
  environment name is part of the nuget.org policy that is matched **per workflow file**.
- Keep the login step immediately before the push: the issued key lasts an hour and each OIDC token is
  exchangeable exactly once.
- `build.yml` is reusable (`workflow_call`). Its `verify` input gates package validation, the sample builds
  and the three e2e suites: pull requests leave it `true`, releases pass `false`. Keep the name and the
  meaning identical to the other repositories in the family.
- Keep `release.yml`'s `guard` job. It proves the tagged commit is an ancestor of the default branch, which is
  the only reason a release may skip re-verification. Never make it conditional or `continue-on-error`.
- `release.yml` pushes to nuget.org **before** creating the GitHub release, and needs both its
  `workflow_dispatch` and `push: tags` triggers — a tag pushed by `auto-release.yml` with `GITHUB_TOKEN` does
  not trigger the tag trigger.
- `auto-release.yml` tags only release notes **added** (`--diff-filter=A`) under `docs/release-notes/` with a
  four-part version. Editing an existing note must never tag anything.
- Everything that packs runs on macOS: both packages multi-target Android, iOS and macOS together. There is no
  Android-only Linux pack leg.
- Keep the forked-pull-request guard on the beta publish job (`github.event.pull_request.head.repo.full_name ==
  github.repository`) — forks get no OIDC token.
- Explain non-obvious steps in a comment, as the existing workflows do; these files are read far more often
  than they are edited.
