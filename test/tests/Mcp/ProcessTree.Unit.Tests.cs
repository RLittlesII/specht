using AwesomeAssertions;
using Specht.Mcp.Features.PinnedTool;

namespace Specht.Tests.Mcp;

/// <summary>
/// Holds the order a cancelled run's descendants are killed in, over trees given as a delegate from a process to its children, with
/// no process started: every program the run started, directly or through another program, taken in one walk and each before its
/// ancestors, the run's own program excluded (0132-F3 B-037).
/// </summary>
[Trait("Tier", "Unit")]
public sealed class ProcessTreeUnitTests
{
    [Fact]
    public void ATreeThreeLevelsDeep_WhenOrderedForKilling_ShouldListEveryDescendantOnceAndEachBeforeItsParent()
    {
        // Given
        var children = new Dictionary<int, int[]>
        {
            [100] = [200, 201, 202],
            [200] = [300, 301],
            [202] = [302],
            [301] = [400, 401],
            [900] = [901],
        };
        var tree = new ProcessTree(pid => children.GetValueOrDefault(pid, []));

        // When
        var order = tree.KillOrder(100).ToList();

        // Then
        order.Should().BeEquivalentTo([200, 201, 202, 300, 301, 302, 400, 401]);
        foreach (var (parent, offspring) in children.Where(static pair => pair.Key is not 100 and not 900))
        {
            foreach (var child in offspring)
            {
                order.IndexOf(child).Should().BeLessThan(order.IndexOf(parent), $"{child} is a child of {parent}");
            }
        }
    }

    [Fact]
    public void AProcessWithNoChildren_WhenOrderedForKilling_ShouldListNothing()
    {
        // Given
        var tree = new ProcessTree(static _ => []);

        // When
        var order = tree.KillOrder(100);

        // Then
        order.Should().BeEmpty();
    }

    [Fact]
    public void ATreeTwoLevelsDeep_WhenOrderedForKilling_ShouldAskForTheChildrenOfEachProcessInItOnce()
    {
        // Given
        var children = new Dictionary<int, int[]>
        {
            [1] = [2, 3],
            [2] = [4],
            [3] = [5, 6],
            [7] = [8],
        };
        var asked = new List<int>();
        var tree = new ProcessTree(pid =>
        {
            asked.Add(pid);
            return children.GetValueOrDefault(pid, []);
        });

        // When
        _ = tree.KillOrder(1);

        // Then
        asked.Should().BeEquivalentTo([1, 2, 3, 4, 5, 6]);
    }
}
