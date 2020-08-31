using Stride.Animations;
using Stride.Core.Annotations;
using Stride.Engine;
using Stride.Engine.Design;
using Stride.Input;
using Stride.Rendering;
using VL.Stride.Shaders.ShaderFX;

namespace VL.Stride.Input
{
    [DefaultEntityComponentProcessorAttribute(typeof(InputSourceProcessor), ExecutionMode = ExecutionMode.All)]
    public class InputSourceComponent : EntityComponent
    {
        public IInputSource InputSource { get; set; }
    }

    internal class InputSourceProcessor : EntityProcessor<InputSourceComponent, InputSourceProcessor.AssociatedData>
    {
        protected override AssociatedData GenerateComponentData([NotNull] Entity entity, [NotNull] InputSourceComponent component)
        {
            return new AssociatedData(); // { InputSource = component.InputSource };
        }

        public class AssociatedData
        {
            //public IInputSource InputSource { get; set; }
        }

        public override void Draw(RenderContext context)
        {
            // fetch input source from render context (not from scene). We probably could get rid of the idea of having a input source per scene.
            context.GetWindowInputSource(out var inputSource);

            if (inputSource != null)
            {

                // Go thru components
                foreach (var entityKeyPair in ComponentDatas)
                {
                    var component = entityKeyPair.Key;
                    //var associatedData = entityKeyPair.Value;
                    component.InputSource = inputSource;
                }

                base.Draw(context);
            }
        }
    }
}
