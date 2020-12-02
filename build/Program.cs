namespace Engage.Dnn.SqlServerTypes.Build
{
    using System;
    using System.Collections.Generic;

    using Cake.Core.Configuration;
    using Cake.Frosting;
    using Cake.NuGet;

    public class Program : IFrostingStartup
    {
        public static int Main(string[] args)
        {
            // Create the host.
            var host = new CakeHostBuilder().WithArguments(args)
                .UseStartup<Program>()
                .Build();

            // Run the host.
            return host.Run();
        }

        public void Configure(ICakeServices services)
        {
            services.UseContext<Context>();
            services.UseLifetime<Lifetime>();
            services.UseWorkingDirectory("..");
            
            var module = new NuGetModule(new CakeConfiguration(new Dictionary<string, string>()));
            module.Register(services);

            services.UseTool(new Uri("nuget:?package=NuGet.CommandLine&version=5.8.0"));
        }
    }
}