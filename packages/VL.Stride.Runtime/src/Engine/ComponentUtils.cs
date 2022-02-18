using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using VL.Core;
using VL.Lib.Collections.TreePatching;
using VL.Lib.Experimental;

namespace VL.Stride.Engine
{
    public static class ComponentUtils
    {
        /// <summary>
        /// Installs a <see cref="TreeNodeParentManager{TParent, TNode}"/> for the given component.
        /// The manager will be used by the Entity node to ensure that the Component is attached to one entity only.
        /// </summary>
        /// <param name="component">The component to manage.</param>
        /// <param name="nodeContext">The path of the node will be used to generate the warning.</param>
        /// <param name="container">The container to which the lifetime of the generated objects will be tied to.</param>
        /// <returns>The component itself.</returns>
        public static TComponent WithParentManager<TComponent>(this TComponent component, NodeContext nodeContext, CompositeDisposable container)
            where TComponent : EntityComponent
        {
            var manager = new TreeNodeParentManager<Entity, EntityComponent>(component, (e, c) => e.Add(c), (e, c) => e.Remove(c))
                .DisposeBy(container);

            var sender = new Sender<object, object>(nodeContext, component, manager)
                .DisposeBy(container);

            // Subscribe to warnings of the manager and make them visible in the patch
            var cachedMessages = default(List<VL.Lang.Message>);
            manager.ToggleWarning.Subscribe(v => ToggleMessages(v))
                .DisposeBy(container);

            // Ensure warning gets removed on dispose
            Disposable.Create(() => ToggleMessages(false))
                .DisposeBy(container);

            return component;

            void ToggleMessages(bool on)
            {
                var messages = cachedMessages ?? (cachedMessages = nodeContext.Path.Stack
                    .Select(id => new VL.Lang.Message(id, Lang.MessageSeverity.Warning, "Component should only be connected to one Entity."))
                    .ToList());
                foreach (var m in messages)
                    VL.Lang.PublicAPI.Session.ToggleMessage(m, on);
            }
        }
    }
}
