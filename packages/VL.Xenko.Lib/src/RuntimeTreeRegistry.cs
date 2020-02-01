using System;
using System.Collections.Generic;
using VL.Core;

namespace VL.Xenko
{
    public static class RuntimeTreeRegistry<T>
    {
        static Dictionary<uint, T> map = new Dictionary<uint, T>();

        public static void AddItem(NodeContext nodeContext, T item)
        {
            var id = nodeContext.Path.Stack.Pop().Peek();
            //if (map.ContainsKey(id))
            //    throw new NotSupportedException("A VL patch can only contain one Game directly. Try moving the game one level deeper, i.e. into it's own process node.");
            map[id] = item;
        }

        public static bool RemoveItem(NodeContext nodeContext)
        {
            return map.Remove(nodeContext.Path.Stack.Pop().Peek());
        }

        public static bool FindClosestParent(IEnumerable<uint> nodePath, out T item)
        {
            foreach(var nodeId in nodePath)
            {
                if (map.TryGetValue(nodeId, out item))
                    return true;
            }

            item = default;
            return false;
        }
    }
}
