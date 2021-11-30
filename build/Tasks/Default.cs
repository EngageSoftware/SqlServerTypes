namespace Engage.Dnn.SqlServerTypes.Build.Tasks
{
    using Cake.Frosting;

    using JetBrains.Annotations;

    [IsDependentOn(typeof(Package))]
    [UsedImplicitly]
    public sealed class Default : FrostingTask<Context>
    {
    }
}