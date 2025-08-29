using System.Reflection;
using System.Runtime.Loader;

namespace Scrubbler.Host.Helper;

internal class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public PluginLoadContext(string mainAssemblyPath)
        : base(isCollectible: true) // collectible = unloadable
    {
        _resolver = new AssemblyDependencyResolver(mainAssemblyPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (SharedAssemblyList.IsShared(assemblyName.Name!))
        {
            return Default.Assemblies.FirstOrDefault(a => a.GetName().Name == assemblyName.Name);
        }

        var path = _resolver.ResolveAssemblyToPath(assemblyName);
        return path != null ? LoadFromAssemblyPath(path) : null;
    }
}
