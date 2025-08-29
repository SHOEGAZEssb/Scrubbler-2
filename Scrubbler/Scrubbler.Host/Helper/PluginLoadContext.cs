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
        // if it's a shared assembly, always reuse host copy
        if (SharedAssemblyList.IsShared(assemblyName.Name!))
        {
            // Default.Assemblies is not directly enumerable, so resolve explicitly
            var asm = Default.Assemblies
                .FirstOrDefault(a => string.Equals(a.GetName().Name, assemblyName.Name, StringComparison.OrdinalIgnoreCase));

            if (asm != null)
                return asm;
        }

        // otherwise, resolve from plugin folder
        var path = _resolver.ResolveAssemblyToPath(assemblyName);
        if (path != null)
        {
            return LoadFromAssemblyPath(path);
        }

        return null;
    }
}
