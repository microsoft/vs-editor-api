//********************************************************************************
// Copyright (c) Microsoft Corporation Inc. All rights reserved
//********************************************************************************
using System;
using System.Globalization;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Collections;
using System.Linq;

namespace Microsoft.VisualStudio.Utilities
{
    /// <summary>
    /// Performs a topological sort of orderable extension parts.
    /// </summary>
    public static class Orderer
    {
        /// <summary>
        /// Orders a list of items that are all orderable, that is, items that implement the IOrderable interface. 
        /// </summary>
        /// <param name="itemsToOrder">The list of items to sort.</param>
        /// <returns>The list of sorted items.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="itemsToOrder"/> is null.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006")]
        public static IList<Lazy<TValue, TMetadata>> Order<TValue, TMetadata>(IEnumerable<Lazy<TValue, TMetadata>> itemsToOrder)
            where TValue : class
            where TMetadata : IOrderable
        {
            if (itemsToOrder == null)
            {
                throw new ArgumentNullException("itemsToOrder");
            }

#if false && DEBUG
            Debug.WriteLine("Before ordering");
            DumpGraph(itemsToOrder);
#endif
            var roots = new Queue<Node<TValue, TMetadata>>();
            var unsortedItems = new List<Node<TValue, TMetadata>>();

            Orderer.PrepareGraph(itemsToOrder, roots, unsortedItems);
            IList<Lazy<TValue, TMetadata>> sortedItems = Orderer.TopologicalSort(roots, unsortedItems);

#if false && DEBUG
            Debug.WriteLine("After ordering");
            DumpGraph(sortedItems);
#endif

            return sortedItems;
        }

        private static void PrepareGraph<TValue, TMetadata>(IEnumerable<Lazy<TValue, TMetadata>> items, Queue<Node<TValue, TMetadata>> roots, List<Node<TValue, TMetadata>> unsortedItems)
            where TValue : class
            where TMetadata : IOrderable
        {
            Dictionary<string, Node<TValue, TMetadata>> map = new Dictionary<string, Node<TValue, TMetadata>>();
            foreach (Lazy<TValue, TMetadata> item in items)
            {
                if ((item != null) && (item.Metadata != null))
                {
                    var node = new Node<TValue, TMetadata>(item);

                    if (node.Name != string.Empty)
                    {
                        if (map.ContainsKey(node.Name))
                        {
                            //Nodes with duplicate names are ignored.
#if DEBUG
                            Debug.WriteLine("Duplicate name in Orderer.Order: " + node.Name);
#endif
                        }
                        else
                        {
                            map.Add(node.Name, node);
                            unsortedItems.Add(node);
                        }
                    }
                    else
                    {
                        //Even unnamed item are added to the unsortedItems. They can't be referred to but they still exist (and can affect ordering).
                        unsortedItems.Add(node);
                    }
                }
            }

            for (int i = unsortedItems.Count - 1; (i >= 0); --i)
            {
                unsortedItems[i].Resolve(map, unsortedItems);   //Placeholders are added to the end of unsorted items (and do not need to be resolved).
            }

            List<Node<TValue, TMetadata>> initialRoots = new List<Node<TValue, TMetadata>>();
            foreach (Node<TValue, TMetadata> node in unsortedItems)
            {
                if (node.After.Count == 0)
                {
                    initialRoots.Add(node);
                }
            }

            AddToRoots(roots, initialRoots);
        }

        private static IList<Lazy<TValue, TMetadata>> TopologicalSort<TValue, TMetadata>(Queue<Node<TValue, TMetadata>> roots, List<Node<TValue, TMetadata>> unsortedItems)
            where TValue : class
            where TMetadata : IOrderable
        {
            List<Lazy<TValue, TMetadata>> sortedItems = new List<Lazy<TValue, TMetadata>>();

            while (unsortedItems.Count > 0)
            {
                Node<TValue, TMetadata> node = (roots.Count == 0) ? Orderer.BreakCircularReference(unsortedItems) : roots.Dequeue();

                Debug.Assert(node.After.Count == 0);

                if (node.Item != null)
                {
                    sortedItems.Add(node.Item);
                }

             	unsortedItems.Remove(node);
                node.ClearBefore(roots);
            }

            return sortedItems;
        }

        private static void AddToRoots<TValue, TMetadata>(Queue<Node<TValue, TMetadata>> roots, List<Node<TValue, TMetadata>> newRoots)
            where TValue : class
            where TMetadata : IOrderable
        {
            newRoots.Sort((l, r) => l.Name.CompareTo(r.Name));
            foreach (Node<TValue, TMetadata> n in newRoots)
            {
                roots.Enqueue(n);
            }
        }

        private static Node<TValue, TMetadata> BreakCircularReference<TValue, TMetadata>(List<Node<TValue, TMetadata>> unsortedItems)
            where TValue : class
            where TMetadata : IOrderable
        {
            //We have a circular reference in the unsortedItems.
            //This is an error in the definition that we need to handle gracefully.

            //Find & report the cycle.
            List<List<Node<TValue, TMetadata>>> cycles = Orderer.FindCycles(unsortedItems);
            Debug.Assert(cycles.Count > 0);

#if DEBUG
            Debug.WriteLine("Orderer found cycles:");
            foreach (List<Node<TValue, TMetadata>> cycle in cycles)
            {
                foreach (Node<TValue, TMetadata> node in cycle)
                {
                    Debug.Write("\t" + node.Name);
                }
                Debug.WriteLine("");
            }
#endif

            //Find the cycle with the fewest inbound links from other cycles.
            int bestInwardLinkCount = int.MaxValue;
            List<Node<TValue, TMetadata>> bestCycle = null;
            foreach (List<Node<TValue, TMetadata>> cycle in cycles)
            {
                int inwardLinkCount = 0;
                foreach (Node<TValue, TMetadata> node in cycle)
                {
                    foreach (Node<TValue, TMetadata> child in node.After)
                    {
                        if (child.LowIndex != node.LowIndex)
                        {
                            ++inwardLinkCount;
                            break;
                        }
                    }
                }

                if (inwardLinkCount < bestInwardLinkCount)
                {
                    bestCycle = cycle;
                    bestInwardLinkCount = inwardLinkCount;
                }
            }

            //Given the best cycle we can find, pick the node that would break the smallest number of "after" constraints. 
            Node<TValue, TMetadata> bestNode;
            if (bestCycle == null)
            {
                //Odd, no cycles were found so we need to guess at random.
                bestNode = unsortedItems[0];
                Debug.Fail("Orderer was unable to find a cycle to break");
            }
            else
            {
                bestNode = bestCycle[0];
                for (int i = 1; (i < bestCycle.Count); ++i)
                {
                    Node<TValue, TMetadata> node = bestCycle[i];

                    if (node.After.Count < bestNode.After.Count)
                    {
                        bestNode = node;
                    }
                }
            }

            foreach (Node<TValue, TMetadata> a in bestNode.After)
            {
                a.Before.Remove(bestNode);
            }
            bestNode.After.Clear();

            return bestNode;
        }

        private static List<List<Node<TValue, TMetadata>>> FindCycles<TValue, TMetadata>(List<Node<TValue, TMetadata>> unsortedItems)
            where TValue : class
            where TMetadata : IOrderable
        {
            foreach (Node<TValue, TMetadata> n in unsortedItems)
            {
                n.Index = -1;
                n.LowIndex = -1;
                n.ContainedInKnownCycle = false;
            }

            List<List<Node<TValue, TMetadata>>> cycles = new List<List<Node<TValue, TMetadata>>>();

            Stack<Node<TValue, TMetadata>> stack = new Stack<Node<TValue, TMetadata>>(unsortedItems.Count);
            int index = 0;
            foreach (Node<TValue, TMetadata> node in unsortedItems)
            {
                if (node.Index == -1)
                {
                    Orderer.FindCycles(node, stack, ref index, cycles);
                    Debug.Assert(stack.Count == 0);
                }
            }

            return cycles;
        }

        private static void FindCycles<TValue, TMetadata>(Node<TValue, TMetadata> node, Stack<Node<TValue, TMetadata>> stack, ref int index, List<List<Node<TValue, TMetadata>>> cycles)
            where TValue : class
            where TMetadata : IOrderable
        {
            node.Index = index;
            node.LowIndex = index;
            ++index;

            stack.Push(node);

            foreach (Node<TValue, TMetadata> child in node.Before)
            {
                if (child.Index == -1)
                {
                    Orderer.FindCycles(child, stack, ref index, cycles);
                    node.LowIndex = Math.Min(node.LowIndex, child.LowIndex);
                }
                else if (!child.ContainedInKnownCycle)
                {
                    node.LowIndex = Math.Min(node.LowIndex, child.Index);
                }
            }

            if (node.Index == node.LowIndex)
            {
                List<Node<TValue, TMetadata>> cycle = new List<Node<TValue, TMetadata>>();
                while (stack.Count > 0)
                {
                    Node<TValue, TMetadata> child = stack.Pop();
                    cycle.Add(child);
                    child.ContainedInKnownCycle = true;

                    if (child == node)
                    {
                        //Single unit cycles aren't interesting (since we are preventing node from linking to themselves in the Resolve code below).
                        if (cycle.Count > 1)
                        {
                            cycles.Add(cycle);
                        }
                        break;
                    }

                    Debug.Assert(stack.Count > 0);
                }
            }
        }

#if DEBUG
        private static void DumpGraph<TValue, TMetadata>(IEnumerable<Lazy<TValue, TMetadata>> items)
            where TValue : class
            where TMetadata : IOrderable
        {
            int index = 0;
            foreach (Lazy<TValue, TMetadata> i in items)
            {
                if ((i != null) && (i.Metadata != null))
                {
                    Debug.WriteLine("\t{0}:{1}", ++index, i.Metadata.Name);
                    if (i.Metadata.After != null)
                    {
                        Debug.WriteLine("\t\tAfter:");
                        foreach (string a in i.Metadata.After)
                            if (!string.IsNullOrWhiteSpace(a))
                                Debug.WriteLine("\t\t\t" + a);
                    }

                    if (i.Metadata.Before != null)
                    {
                        Debug.WriteLine("\t\tBefore:");
                        foreach (string a in i.Metadata.Before)
                            if (!string.IsNullOrWhiteSpace(a))
                                Debug.WriteLine("\t\t\t" + a);
                    }
                }
            }
        }
#endif

        class Node<TValue, TMetadata>
            where TValue : class
            where TMetadata : IOrderable
        {
            public readonly string Name;
            public readonly Lazy<TValue, TMetadata> Item;

            private HashSet<Node<TValue, TMetadata>> _after = new HashSet<Node<TValue, TMetadata>>();
            public HashSet<Node<TValue, TMetadata>> After { get { return _after; } }

            private HashSet<Node<TValue, TMetadata>> _before = new HashSet<Node<TValue, TMetadata>>();
            public HashSet<Node<TValue, TMetadata>> Before { get { return _before; } }

            //Used to identify cycles
            public int Index = -1;
            public int LowIndex = -1;
            public bool ContainedInKnownCycle = false;

            public Node(Lazy<TValue, TMetadata> item)
            {
                string name = item.Metadata.Name;

                this.Name = string.IsNullOrEmpty(name) ? string.Empty : name.ToUpperInvariant();
                this.Item = item;
            }

            public Node(string name)
            {
                Debug.Assert(!string.IsNullOrEmpty(name));
                this.Name = name;
            }

            public void Resolve(Dictionary<string, Node<TValue, TMetadata>> map, List<Node<TValue, TMetadata>> unsortedItems)
            {
                this.Resolve(map, this.Item.Metadata.After, _after, unsortedItems);
                this.Resolve(map, this.Item.Metadata.Before, _before, unsortedItems);

                foreach (Node<TValue, TMetadata> b in _before)
                {
                    b._after.Add(this);
                }

                foreach (Node<TValue, TMetadata> a in _after)
                {
                    a._before.Add(this);
                }
            }

            public void ClearBefore(Queue<Node<TValue, TMetadata>> roots)
            {
                List<Node<TValue, TMetadata>> newRoots = new List<Node<TValue, TMetadata>>();
                foreach (Node<TValue, TMetadata> child in this.Before)
                {
                    child.After.Remove(this);

                    if (child.After.Count == 0)
                    {
                        newRoots.Add(child);
                    }
                }
                this.Before.Clear();

                Orderer.AddToRoots(roots, newRoots);
            }

            public override string ToString()
            {
                return this.Name;
            }

            private void Resolve(Dictionary<string, Node<TValue, TMetadata>> map, IEnumerable<string> links, HashSet<Node<TValue, TMetadata>> results, List<Node<TValue, TMetadata>> unsortedItems)
            {
                if (links != null)
                {
                    foreach (string link in links)
                    {
                        if (!string.IsNullOrEmpty(link))
                        {
                            string name = link.ToUpperInvariant();

                            Node<TValue, TMetadata> node;
                            if (!map.TryGetValue(name, out node))
                            {
                                //We need place-holder  to handle the case where A comes before B and C comes after B but B is never defined.
                                //We still want C to come after A though so we need to create a "B".
                                //
                                //B doesn't show up in the output.
                                node = new Node<TValue, TMetadata>(name);

                                map.Add(name, node);
                                unsortedItems.Add(node);
                            }

                            //Ignore links directly back to itself
                            if (node != this)
                            {
                                results.Add(node);
                            }
                            else
                            {
                                Debug.WriteLine("Orderer.Node links to itself: " + node.Name);
                            }
                        }
                    }
                }
            }
        }
    }
}
