using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Specht.Mcp.Features.PinnedTool;

/// <summary>The children of every process on Linux, read in one scan of <c>/proc</c> when the object is made.</summary>
/// <implements epic="0132" feature="F3" claim="B-037" />
internal sealed class ProcScan
{
    public ProcScan()
    {
        foreach (var directory in Directory.EnumerateDirectories("/proc"))
        {
            if (!int.TryParse(Path.GetFileName(directory), NumberStyles.None, CultureInfo.InvariantCulture, out var pid))
            {
                continue;
            }

            string text;
            try
            {
                text = File.ReadAllText(Path.Combine(directory, "stat"));
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                continue;
            }

            if (new ProcStatLine(text).Parent is not { } parent)
            {
                continue;
            }

            if (!_children.TryGetValue(parent, out var siblings))
            {
                siblings = [];
                _children[parent] = siblings;
            }

            siblings.Add(pid);
        }
    }

    public IEnumerable<int> ChildrenOf(int parent) => _children.TryGetValue(parent, out var siblings) ? siblings : [];

    private readonly Dictionary<int, List<int>> _children = [];
}
