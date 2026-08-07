# Concordia / Synaptrix — repo guidance for Claude

Synaptrix is a standalone, open-source .NET mediator library published to NuGet
(`Synaptrix`, `Synaptrix.Core`, `Synaptrix.Generator`). It has its own public users,
its own issue tracker, its own README/CHANGELOG. Treat it as a real open-source
project you're a contributor to, not as a private tool for any one consumer.

## Comments, docs, tests, commit messages

- **Never name a specific consumer project** (e.g. a private/internal codebase you
  also happen to work on) in source comments, XML docs, README/CHANGELOG prose, test
  names, test doc comments, or commit messages in this repo. Describe bugs and shapes
  in general, technical terms only — "a handler shape where X" instead of
  "observed in <project>'s FooHandler". A reader of this repo has no context on your
  other projects and shouldn't need any to understand why a fix exists.
- If a real-world case motivated a fix, that motivation belongs in your own
  conversation/notes, not in this repo. What belongs here is the general shape of the
  problem, why it's wrong, and how the fix addresses it — written so it stands on its
  own for anyone hitting the same shape, regardless of what project they're in.
- Example naming in tests/docs should read as plausible-but-generic (e.g.
  `FindEntitiesCommand<TId, TEntity>`, `FetchTabularCommand<TTabular>`) — fine as
  long as it doesn't disclose which real project it came from.
- Hold this repo to the same bar as any other public open-source library you'd
  contribute to under review: precise, professional, no internal shorthand, no
  leaked context about unrelated codebases.

## Workflow already established in this repo

- Commit convention: [versionize](https://github.com/versionize/versionize)
  conventional commits (`fix:`, `feat:`, `chore(release): X.Y.Z`). Run
  `dotnet versionize` after a `fix:`/`feat:` commit to bump the version, update
  `CHANGELOG.md`, and tag the release — don't hand-edit version numbers.
- Tests live in `tests/Synaptrix.Generator.Tests`; run
  `dotnet test tests/Synaptrix.Generator.Tests/Synaptrix.Generator.Tests.csproj`
  before committing a generator change.
- `examples/Synaptrix.Examples.MultiAssembly.*` covers cross-assembly handler composition with
  real, separately-compiled projects (Catalog referenced by nothing, Shipping referenced by
  Catalog, Host referencing only Catalog) - a class of regression the in-memory generator tests
  can't catch, since those run the generator against one fake compilation rather than real
  assemblies discovering each other. Run
  `dotnet test examples/Synaptrix.Examples.MultiAssembly.Host/Synaptrix.Examples.MultiAssembly.Host.csproj`
  before committing any change to handler discovery, chain-registration, or `GeneratedMediator`
  dispatch/fallback behavior.
- Do not push or publish to NuGet from here — tagging locally is fine, pushing the
  tag/branch and running the publish pipeline is the repo owner's call.
