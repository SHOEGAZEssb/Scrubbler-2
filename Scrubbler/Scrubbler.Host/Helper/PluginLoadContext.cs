using System.Reflection;
using System.Runtime.Loader;

namespace Scrubbler.Host.Helper;

internal class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;
    private readonly HashSet<string> _sharedAssemblies;

    public PluginLoadContext(string mainAssemblyPath, IEnumerable<string> sharedAssemblies)
        : base(isCollectible: true) // collectible = unloadable
    {
        _resolver = new AssemblyDependencyResolver(mainAssemblyPath);
        _sharedAssemblies = new HashSet<string>(sharedAssemblies, StringComparer.OrdinalIgnoreCase);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // if it's in the shared list → load from default (host context)
        if (_sharedAssemblies.Contains(assemblyName.Name!))
        {
            return Default.Assemblies.FirstOrDefault(a => a.GetName().Name == assemblyName.Name);
        }

        // otherwise, try resolve locally
        var path = _resolver.ResolveAssemblyToPath(assemblyName);
        if (path != null)
        {
            return LoadFromAssemblyPath(path);
        }

        return null;
    }
}
