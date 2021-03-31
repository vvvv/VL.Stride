using Stride.Core.Shaders.Ast;
using Stride.Core.Shaders.Ast.Stride;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VL.Stride.Rendering
{
    public class EffectParserResult
    {
        public readonly Shader Shader;

        // base shaders
        public IReadOnlyList<EffectParserResult> BaseShaders => baseShaders;
        private readonly List<EffectParserResult> baseShaders = new List<EffectParserResult>();

        // compositions
        public IReadOnlyList<CompositionInput> Compositions => compositions;
        private readonly List<CompositionInput> compositions;

        public EffectParserResult(Shader shader)
        {
            Shader = shader;
            var s = Shader.GetFirstClassDecl();
            var compositions = s.Members
                .OfType<Variable>()
                .Select((v, i) => (v, i))
                .Where(v => v.v.Qualifiers.Contains(StrideStorageQualifier.Compose))
                .Select(v => new CompositionInput(v.v, v.i))
                .ToList();
        }

        public void AddBaseShader(EffectParserResult baseShader)
            => baseShaders.Add(baseShader);
    }

    public class EffectParserResultRef
    {
        public EffectParserResult result;
    }

    public class CompositionInput
    {
        public readonly string Name;
        public readonly string TypeName;

        /// <summary>
        /// The local index in the shader file.
        /// </summary>
        public readonly int LocalIndex;

        public readonly Variable Variable;

        public CompositionInput(Variable v, int localIndex)
        {
            LocalIndex = localIndex;
            Name = v.Name.Text;
            TypeName = v.Type.Name.Text;
            Variable = v;
        }
    }
}
