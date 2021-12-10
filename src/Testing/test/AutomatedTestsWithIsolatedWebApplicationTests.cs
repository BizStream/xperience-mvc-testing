using NUnit.Framework;

namespace BizStream.Kentico.Xperience.AspNetCore.Mvc.Testing.Tests;

[TestFixture]
public class AutomatedTestsWithIsolatedWebApplicationTests : AutomatedTestsWithIsolatedWebApplication<Startup>
{
    [Test]
    [TestCase( "/" )]
    [TestCase( "/test" )]
    public async Task Get_ShouldReturnSuccess( string path )
    {
        var response = await Client.GetAsync( path );
        response.EnsureSuccessStatusCode();
    }

    [Test]
    [TestCase( "/" )]
    [TestCase( "/test" )]
    public async Task Get_ShouldReturnContent( string path )
    {
        var response = await Client.GetAsync( path );
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Assert.IsFalse( string.IsNullOrWhiteSpace( content ) );
    }
}
