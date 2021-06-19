# xperience-mvc-testing ![License](https://img.shields.io/github/license/BizStream/xperience-mvc-testing)

Support for writing Isolated tests for Xperience Mvc applications, based on the [Microsoft.AspNetCore.Mvc.Testing](https://www.nuget.org/packages/Microsoft.AspNetCore.Mvc.Testing) & [Kentico.Xperience.Libraries.Tests](https://www.nuget.org/packages/Kentico.Xperience.Libraries.Tests/) frameworks.

For more information on Mvc Integration testing and automated testing in Kentico, see:

- [Integration tests in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [Writing automated tests in Kentico](https://docs.xperience.io/custom-development/writing-automated-tests)

## Usage

- Install the package into your Xperience Mvc Test project:

> _Kentico versions `<13.0.19` are not supported when targeting `net5.0`, due to a [bug](https://devnet.kentico.com/download/hotfixes#bug-62788) within `Kentico.Xperience.Libraries.Tests`._

```bash
dotnet add package BizStream.Kentico.Xperience.AspNetCore.Mvc.Testing
```

OR

```csproj
<PackageReference Include="BizStream.Kentico.Xperience.AspNetCore.Mvc.Testing" Version="x.x.x" />
```

- Implement a `TestFixture` derived from `AutomatedTestWithIsolatedWebApplication<TStartup>`:

```csharp
[TestFixture( Category = "IsolatedMvc" )]
public class ExampleIsolatedWebTests : AutomatedTestWithIsolatedWebApplication<ExampleApp.Startup>
{

    [Test]
    public async Task Get_Root_ShouldReturnSuccessAndContent(  )
    {
        var response = await Client.GetAsync( "/" );
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Assert.IsFalse( string.IsNullOrWhiteSpace( content ) );
    }

}
```
