[assembly:System.Web.PreApplicationStartMethod(typeof(Engage.Dnn.SqlServerTypes.Startup), nameof(Engage.Dnn.SqlServerTypes.Startup.OnStartup))]
namespace Engage.Dnn.SqlServerTypes
{
    using System.Web.Hosting;

    public static class Startup
    {
        public static void OnStartup()
        { 
            Loader.LoadNativeAssemblies(HostingEnvironment.MapPath("~/bin"));
        }
    }
}
