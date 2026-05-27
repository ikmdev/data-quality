using Microsoft.AspNetCore.Mvc.Testing;
using System.Runtime.CompilerServices;
//This is here for code generation only, not used by the app itself
[assembly: InternalsVisibleTo("PIQI.Service.Test")]
class PIQIEngineService : WebApplicationFactory<PIQI_Engine.Server.Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        // shared extra set up goes here
        return base.CreateHost(builder);
    }
}