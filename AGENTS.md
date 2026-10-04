# Overview

ByteAether.Ulid is a multi-target .NET ULID library with companion packages. Use shared build settings and package versions; keep package dependencies compatible with declared target frameworks. NuGet releases pack from `v`-prefixed tags.

## Structure

`src/ByteAether.Ulid.slnx` is the solution. Shared settings are in `src/Directory.Build.{props,targets}` and `src/Directory.Packages.props`. Core code/tests: `src/Ulid/`, `src/Ulid.Tests/`; companion packages have separate projects and tests, reference core by project reference, and reuse shared code where appropriate. Package docs are `PACKAGE.md`; shared docs are in root `README.md`. Core library AOT smoke app: `src/Ulid.Tests.AotConsole/`; Core library benchmarks: `src/Ulid.Benchmarks/`.

## Build and test

Use .NET 10. From the repository root, CI build commands are:

```sh
dotnet restore src/ByteAether.Ulid.slnx -p:Configuration=CI-Debug
dotnet build src/ByteAether.Ulid.slnx --configuration CI-Debug --no-restore
```

Test hosts generally target `net10.0`; `ImplTargetFramework` selects the referenced implementation TFM, not the test host, and shared targets set matching compile constants. Each test project's `SupportedImplFrameworks` must match supported implementation `TargetFrameworks`; unsupported selections are skipped. Keep lists and `.github/workflows/build-and-test.yml` aligned. CI tests `net10.0;net9.0;net8.0;net7.0;net6.0;net5.0;netstandard2.1;netstandard2.0`. For one TFM:

```sh
dotnet test src/ByteAether.Ulid.slnx --configuration CI-Debug -p:ImplTargetFramework=net8.0 --results-directory TestResults
```

TRX output is under `TestResults/`; unconditional test projects may run on every matrix pass. Shared test setup uses xUnit v3 and Microsoft.Testing.Platform TRX reporting; follow existing dependency patterns.

Native AOT (`.github/workflows/build-and-test-aot.yml`) is validated on Linux and Windows with warnings as errors:

```sh
dotnet restore src/Ulid.Tests.AotConsole/Tests.AotConsole.csproj -p:Configuration=CI-Release --use-current-runtime -p:SelfContained=true
dotnet publish src/Ulid.Tests.AotConsole/Tests.AotConsole.csproj --configuration CI-Release --no-restore --use-current-runtime --self-contained true -o ./publish
./publish/Tests.AotConsole
```

## C# conventions

- Always use file-scoped namespaces. Follow `src/.editorconfig`: tabs, CRLF, braces, naming. Nullable and implicit usings are enabled.
- For multiline arguments/conditions, put each item on a new line, indented one tab; align the closing `)` with the construct:
  ```csharp
  Call(
      firstArgument,
      secondArgument
  );
  ```
- Preserve all target-framework compatibility; use `src/Compatibility/` where needed. Add/update tests for behavior changes and cover relevant `SupportedImplFrameworks`.
- Omit contextually implied domain terms from identifier and file names (e.g., `Transformer` over `UlidTransformer`), reserving specific prefixes solely for disambiguating multiple types in the same scope (e.g., `UlidConstraint` vs. `GuidConstraint`).
- Add/update tests for behavior changes and cover relevant `SupportedImplFrameworks`.
