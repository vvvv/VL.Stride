using System;
using System.Collections.Generic;
using System.Linq;
using VL.Lib.Collections;
using VL.Xenko.Games;
using Xenko.Core.Collections;
using Xenko.Core.Mathematics;
using Xenko.Engine;
using Xenko.Games;

namespace VL.Xenko.Layer
{
    public class SceneGraphManager
    {
        public SceneGraphManager(VLGame vLGame)
        {
            ManagerForGame.Add(vLGame, this);
        }

        static Dictionary<VLGame, SceneGraphManager> ManagerForGame = new Dictionary<VLGame, SceneGraphManager>();
        Dictionary<object, int> ParentChangesLastFrame = new Dictionary<object, int>();
        HashSet<object> TroublesomeEntityOrComponent = new HashSet<object>();

        public void OncePerFrame(GameTime gameTime)
        {
            TroublesomeEntityOrComponent.Clear();
            var objs = ParentChangesLastFrame
                .Where(pair => pair.Value > 1)
                .Select(pair => pair.Key);
            TroublesomeEntityOrComponent.AddRange(objs);
            ParentChangesLastFrame.Clear();
        }

        /// <summary>
        /// To be called by all components and entities once per frame to inform the user of weird scene graphs.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="entityOrComponent"></param>
        public static void ComplainIfParentGotChangedMoreThanOncePerFrame(VLGame game, object entityOrComponent)
        {
            var manager = ManagerForGame[game];
            if (manager.TroublesomeEntityOrComponent.Contains(entityOrComponent))
                throw new Exception($"Many parents for {entityOrComponent} detected. Make sure you only draw one link.");
        }

        internal static void ParentGotSet(VLGame game, object entityOrComponent)
        {
            var manager = ManagerForGame[game];
            if (manager.ParentChangesLastFrame.TryGetValue(entityOrComponent, out int c))
            {
                c++;
                manager.ParentChangesLastFrame[entityOrComponent] = c;
            }
            else
                manager.ParentChangesLastFrame[entityOrComponent] = 1;
        }
    }

    /// <summary>
    /// Manages the children of an <see cref="Entity"/>.
    /// </summary>
    public sealed class EntityChildrenManager : IDisposable
    {
        readonly Entity entity;
        readonly VLGame game;
        readonly FastList<EntityLink> links = new FastList<EntityLink>();
        Spread<Entity> children;

        public EntityChildrenManager(VLGame game, Entity entity)
        {
            this.game = game;
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
                    link = new EntityLink(game, entity);
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
        readonly VLGame game;
        readonly FastList<ComponentLink> links = new FastList<ComponentLink>();
        Spread<EntityComponent> components;

        public EntityComponentsManager(VLGame game, Entity entity)
        {
            this.game = game;
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
                    link = new ComponentLink(game, entity);
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
        readonly VLGame game;
        readonly FastList<EntitySceneLink> links = new FastList<EntitySceneLink>();
        Spread<Entity> entities;

        public SceneEntitiesManager(VLGame game, Scene scene)
        {
            this.game = game;
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
                    link = new EntitySceneLink(game, scene);
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
        readonly VLGame game;
        readonly FastList<SceneLink> links = new FastList<SceneLink>();
        Spread<Scene> children;

        public SceneChildrenManager(VLGame game, Scene scene)
        {
            this.game = game;
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
                    link = new SceneLink(game, scene);
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
