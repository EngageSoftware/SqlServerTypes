namespace Engage.Dnn.SqlServerTypes.Build
{
    using System;

    using Cake.Frosting;

    public class Program
    {
        public static int Main(string[] args)
        {
            return new CakeHost()
                .UseContext<Context>()
                .UseWorkingDirectory("..")
                .InstallTool(new Uri("nuget:?package=NuGet.CommandLine&version=5.9.1"))
                .Run(args);
        }
    }
}