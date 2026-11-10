using System;
using System.Collections.Generic;

namespace Specht.Mcp.Features.PinnedTool;

/// <summary>The order a cancelled run's descendants are killed in, found in one walk from a process to its children.</summary>
/// <implements epic="0132" feature="F3" claim="B-041" />
public sealed class ProcessTree(Func<int, IEnumerable<int>> childrenOf)
{
    /// <summary>Lists every descendant of <paramref name="root"/> once, the root excluded, each before its ancestors.</summary>
    /// <implements epic="0132" feature="F3" claim="B-041" />
    public IReadOnlyList<int> KillOrder(int root)
    {
        var found = new List<int>();
        var seen = new HashSet<int> { root };
        var parents = new Queue<int>([root]);
        while (parents.TryDequeue(out var parent))
        {
            foreach (var child in childrenOf(parent))
            {
                if (seen.Add(child))
                {
                    found.Add(child);
                    parents.Enqueue(child);
                }
            }
        }

        found.Reverse();
        return found;
    }
}
