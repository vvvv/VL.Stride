using System.Collections.Generic;
using VL.Core;

namespace VL.Xenko
{
    public static class RuntimeTreeRegistry<T>
    {
        static Dictionary<uint, T> map = new Dictionary<uint, T>();

        public static void AddItem(NodeContext nodeContext, T item)
        {
            map[nodeContext.Path.Stack.Pop().Peek()] = item;
        }

        public static bool RemoveItem(NodeContext nodeContext)
        {
            return map.Remove(nodeContext.Path.Stack.Pop().Peek());
        }

        public static bool FindClosestParent(NodeContext nodeContext, out T item)
        {
            foreach(var nodeId in nodeContext.Path.Stack)
            {
                if (map.TryGetValue(nodeId, out item))
                    return true;
            }

            item = default;
            return false;
        }
    }
}
