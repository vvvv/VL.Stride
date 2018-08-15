using Xenko.Core.Collections;
using Xenko.Core.Mathematics;
using Xenko.Engine;
using Xenko.Rendering;
using Xenko.Rendering.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VL.Xenko.Utils
{
    public static class CollectionUtils
    {
        public static void AddEnity(TrackingCollection<Entity> collection, Entity item, out TrackingCollection<Entity> collectionOut, out Entity itemOut)
        {
            collection.Add(item);
            collectionOut = collection;
            itemOut = item;
        }

        public static void RemoveEnity(TrackingCollection<Entity> collection, Entity item, out TrackingCollection<Entity> collectionOut, out Entity itemOut)
        {
            collection.Remove(item);
            collectionOut = collection;
            itemOut = item;
        }

        public static void AddTracking<T>(TrackingCollection<T> collection, T item, out TrackingCollection<T> collectionOut, out T itemOut)
        {
            collection.Add(item);
            collectionOut = collection;
            itemOut = item;
        }

        public static void RemoveTracking<T>(TrackingCollection<T> collection, T item, out TrackingCollection<T> collectionOut, out T itemOut)
        {
            collection.Remove(item);
            collectionOut = collection;
            itemOut = item;
        }

        public static void AddFastTracking<T>(FastTrackingCollection<T> collection, T item, out FastTrackingCollection<T> collectionOut, out T itemOut)
        {
            collection.Add(item);
            collectionOut = collection;
            itemOut = item;
        }

        public static void RemoveFastTracking<T>(FastTrackingCollection<T> collection, T item, out FastTrackingCollection<T> collectionOut, out T itemOut)
        {
            collection.Remove(item);
            collectionOut = collection;
            itemOut = item;
        }
    }
}
