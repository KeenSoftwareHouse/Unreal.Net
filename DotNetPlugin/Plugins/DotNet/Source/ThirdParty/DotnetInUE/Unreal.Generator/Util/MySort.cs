// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Unreal.Util
{
    /// <summary>
    /// Adapter that allows any object type to participate in a topological sort.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public interface ITopologicalSortAdapter<TItem>
    {
        int GetLinkCount(in TItem item);
        TItem GetLink(in TItem item, int index);
        bool HandleCycle(IEnumerable<TItem> participants);
    }
    
    /// <summary>
    /// Provides utilities for more exotic sorting.
    /// </summary>
    public static class SortUtil
    {
        /// <summary>
        /// Taking a list as a directed graph this method produces the topological ordering of it's vertices.
        /// </summary>
        /// <remarks>
        /// This method takes an optional argument <paramref name="handleCycle"/>.
        /// 
        /// When provide this callback is invoked for every cycle found in the graph.
        /// Each time the call may return <c>true</c> to signify that the cycle should just be ignored,
        /// or <c>false</c> to mean that the sorting operation should be aborted.
        /// 
        /// If the argument is not provide cycles are always fatal.
        /// 
        /// If the sorting is aborted the original list remains unmodified.
        /// </remarks>
        /// <typeparam name="TItem">Type of the list item.</typeparam>
        /// <param name="list">The list to order.</param>
        /// <param name="adapter">Sorting adapter.</param>
        /// <returns>Whether the sorting operation was successful (there were no cycles or all off them were ignored).</returns>
        public static bool TopologicalSort<TItem>(IList<TItem> list, ITopologicalSortAdapter<TItem> adapter)
            where TItem : notnull
        {
            // Index items.
            Dictionary<TItem, int> itemIndex = new();
            for (int i = 0; i < list.Count; ++i)
            {
                itemIndex[list[i]] = i;
            }

            // We store the sorted items in a temporary list.
            List<TItem> sorted = new(list.Count);

            // The algorithm involved is a simple depth-first-search implementation of a topological sort.
            // There are alternatives but DFS does not require creating a temporary copy of the graph.
            // It also allows us to detect cycles and handle them without needing to abort.

            bool hasCycle = false;

            // Color tells weather we have visited a node and whether it is on the stack or not.
            byte[] color = new byte[list.Count];

            // node, child
            List<(int, int)> stack = new(10);

            // Loop over each item and calculate from it's depth
            for (int i = 0; i < color.Length; ++i)
            {
                // Node on the graph and child index.
                int node = i, index = 0;

                if (color[node] == 1) // Already added.
                    continue;

                if (adapter.GetLinkCount(list[node]) == 0)
                {
                    sorted.Add(list[node]);
                    color[node] = 1;
                    continue;
                }

                do
                {
                    var listItem = list[node];
                    var count = adapter.GetLinkCount(listItem);
                    
                    var last = count == 0 || index == count;

                    // This is the return from recursive call
                    // We also handle leaves here so we must check if they are visited
                    if (last)
                    {
                        if (color[node] != 1)
                        {
                            color[node] = 1;
                            sorted.Add(listItem);
                        }

                        goto recursionReturn;
                    }

                    // Check return from recursion
                    if (color[node] == 2 && index == 0)
                    {
                        hasCycle = true;

                        var cycle = stack.Select(x => list[x.Item1]);

                        if (adapter.HandleCycle(cycle))
                            goto recursionReturn;
                        else
                            goto finalize;
                    }

                    if (color[node] == 1)
                        goto recursionReturn;

                    // Mark as on stack.
                    color[node] = 2;

                    // Setup next step and save stack for recursion return.
                    if (count > 0)
                    {
                        // Stack next position, move on to child.
                        stack.Add((node, index + 1));

                        // Try to find node.
                        while (index < count && !itemIndex.TryGetValue(adapter.GetLink(listItem, index), out node))
                            index++;

                        // If found, descend.
                        if (index < count)
                        {
                            index = 0;
                            continue;
                        }

                        // Fall through as if we reached the end.
                    }

                    // Return from recursion.
                    recursionReturn:
                    if (stack.Count == 0)
                        break;

                    // Pop last.
                    var lastStackIndex = stack.Count - 1;
                    var t = stack[lastStackIndex];
                    stack.RemoveAt(lastStackIndex);
                    node = t.Item1;
                    index = t.Item2;
                } while (true);
            }

            // Copy over sorted list.
            for (int i = 0; i < color.Length; ++i)
                list[i] = sorted[i];

            finalize:
            return hasCycle;
        }
    }
}