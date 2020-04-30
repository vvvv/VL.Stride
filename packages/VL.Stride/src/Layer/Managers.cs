using System;
using VL.Lib.Collections;
using Stride.Core.Collections;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace VL.Stride.Layer
{
    /// <summary>
    /// Manages the children of an <see cref="Entity"/>.
    /// </summary>
    public sealed class EntityChildrenManager : IDisposable
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
    public sealed class EntityComponentsManager : IDisposable
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

    /// <summary>
    /// Manages the entities of a <see cref="Scene"/>.
    /// </summary>
    public sealed class SceneEntitiesManager : IDisposable
    {
        readonly Scene scene;
        readonly FastList<EntitySceneLink> links = new FastList<EntitySceneLink>();
        Spread<Entity> entities;

        public SceneEntitiesManager(Scene scene)
        {
            this.scene = scene;
        }

        public Scene Update(Spread<Entity> entities)
        {
            // Quick change check
            if (entities != this.entities)
                this.entities = entities;
            else
                return scene;

            // Synchronize our entity links
            var @array = entities._array;
            for (int i = 0; i < array.Length; i++)
            {
                var link = links.ElementAtOrDefault(i);
                if (link == null)
                {
                    link = new EntitySceneLink(scene);
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
            return scene;
        }

        public void Dispose()
        {
            foreach (var link in links)
                link.Dispose();
        }
    }

    /// <summary>
    /// Manages the child scenes of a <see cref="Scene"/>.
    /// </summary>
    public sealed class SceneChildrenManager : IDisposable
    {
        readonly Scene scene;
        readonly FastList<SceneLink> links = new FastList<SceneLink>();
        Spread<Scene> children;

        public SceneChildrenManager(Scene scene)
        {
            this.scene = scene;
        }

        public Scene Update(Spread<Scene> children)
        {
            // Quick change check
            if (children != this.children)
                this.children = children;
            else
                return scene;

            // Synchronize our entity links
            var @array = children._array;
            for (int i = 0; i < array.Length; i++)
            {
                var link = links.ElementAtOrDefault(i);
                if (link == null)
                {
                    link = new SceneLink(scene);
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
            return scene;
        }

        public void Dispose()
        {
            foreach (var link in links)
                link.Dispose();
        }
    }
}
