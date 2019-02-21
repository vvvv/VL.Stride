using VL.Lib.Collections;
using Xenko.Core.Collections;
using Xenko.Engine;

namespace VL.Xenko.Layer
{
    /// <summary>
    /// Manages the children of an <see cref="Entity"/>.
    /// </summary>
    public sealed class EntityChildrenManager
    {
        readonly Entity entity;
        readonly FastList<EntityLink> links = new FastList<EntityLink>();
        Spread<Entity> children;

        public EntityChildrenManager(Entity entity)
        {
            this.entity = entity;
        }

        public Entity Update(Spread<Entity> children)
        {
            // Quick change check
            if (children != this.children)
                this.children = children;
            else
                return entity;

            // Synchronize our entity links
            var @array = children._array;
            for (int i = 0; i < array.Length; i++)
            {
                var link = links.ElementAtOrDefault(i);
                if (link == null)
                {
                    link = new EntityLink(entity);
                    links.Add(link);
                }
                link.Child = array[i];
            }
            for (int i = links.Count - 1; i >= array.Length; i--)
            {
                var link = links[i];
                link.Dispose();
                links.RemoveAt(i);
            }

            return entity;
        }

        public void Dispose()
        {
            foreach (var link in links)
                link.Dispose();
        }
    }

    /// <summary>
    /// Manages the components of an <see cref="Entity"/>.
    /// </summary>
    public sealed class EntityComponentsManager
    {
        readonly Entity entity;
        readonly FastList<ComponentLink> links = new FastList<ComponentLink>();
        Spread<EntityComponent> components;

        public EntityComponentsManager(Entity entity)
        {
            this.entity = entity;
        }

        public Entity Update(Spread<EntityComponent> components)
        {
            // Quick change check
            if (components != this.components)
                this.components = components;
            else
                return entity;

            // Synchronize our entity links
            var @array = components._array;
            for (int i = 0; i < array.Length; i++)
            {
                var link = links.ElementAtOrDefault(i);
                if (link == null)
                {
                    link = new ComponentLink(entity);
                    links.Add(link);
                }
                link.Component = array[i];
            }
            for (int i = links.Count - 1; i >= array.Length; i--)
            {
                var link = links[i];
                link.Dispose();
                links.RemoveAt(i);
            }

            return entity;
        }

        public void Dispose()
        {
            foreach (var link in links)
                link.Dispose();
        }
    }
}
