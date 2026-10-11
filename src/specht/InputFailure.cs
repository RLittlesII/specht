using Dunet;
using Specht.Manifest;

namespace Specht;

/// <summary>
/// An input a check cannot run on, returned as a value: a root or manifest that is missing (<c>0001-F2</c> B-005, B-006), a
/// manifest that does not parse (B-007), or one the engine rejects (<c>0001-F5</c> B-012). No message holds an absolute path.
/// </summary>
[Union(EnableImplicitConversions = false)]
public partial record InputFailure
{
    /// <summary>
    /// A root that is not a directory. It carries no path: the engine holds only the resolved root, and the message names the
    /// root as it was typed, which only the caller has.
    /// </summary>
    public partial record RootNotFound;

    /// <summary>A root with no manifest at <see cref="SpecManifest.RelativePath"/>.</summary>
    public partial record ManifestNotFound
    {
        /// <summary>Gets what is missing, naming the manifest path.</summary>
        public string Message => $"{SpecManifest.RelativePath}: there is no manifest at the manifest path.";
    }

    /// <summary>A manifest that is not well-formed JSON or does not parse into the shape the engine reads.</summary>
    public partial record ManifestUnreadable
    {
        /// <summary>Gets what does not parse, naming the manifest path.</summary>
        public string Message => $"{SpecManifest.RelativePath}: the manifest does not parse into the manifest's shape.";
    }

    /// <summary>A well-formed manifest the engine rejects.</summary>
    /// <param name="Message">What is wrong with the manifest, naming the offending key.</param>
    public partial record ManifestRejected(string Message);
}
