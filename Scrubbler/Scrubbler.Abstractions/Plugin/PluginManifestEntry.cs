using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrubbler.Abstractions.Plugin;
public record PluginManifestEntry(
    string Id,
    string Name,
    string Version,
    string Description,
    string PluginType,
    IReadOnlyList<string> SupportedPlatforms,
    Uri SourceUri,
    Uri? IconUri = null
);

