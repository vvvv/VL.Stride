using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VL.Xenko.Lib
{

    public class RuntimeTreeNode<T>
    {
        public uint NodeId;
        public T Value;
        List<RuntimeTreeNode<T>> Children = new List<RuntimeTreeNode<T>>();
        public RuntimeTreeNode(uint nodeId, T value = default)
        {
            NodeId = nodeId;
            Value = value;
        }

        public RuntimeTreeNode<T> AddOrGetChild(uint id)
        {
            var child = Children.SingleOrDefault(c => c.NodeId == id);
            if (child != null)
            {
                return child;
            }
            else
            {
                var newNode = new RuntimeTreeNode<T>(id);
                Children.Add(newNode);
                return newNode;
            }
        }

        public bool TryGetChild(uint id, out RuntimeTreeNode<T> child)
        {
            foreach (var c in Children)
            {
                if (c.NodeId == id)
                {
                    child = c;
                    return true;
                }
            }

            child = null;
            return false;
        }
    }

    public class RuntimeTree<T>
    {

        RuntimeTreeNode<T> root = new RuntimeTreeNode<T>(0);

        public void AddNode(IEnumerable<uint> path, T value)
        {
            var currentNode = root;
            foreach (var id in path)
            {
                currentNode = currentNode.AddOrGetChild(id);
            }
            currentNode.Value = value;
        }

        public bool FindClosestParent(IEnumerable<uint> path, out T value)
        {
            var currentNode = root;
            foreach (var id in path)
            {
                if (currentNode.TryGetChild(id, out var child))
                    currentNode = child;
                else
                    break;
            }

            value = currentNode.Value;
            return currentNode != root;
        }
    }
}
