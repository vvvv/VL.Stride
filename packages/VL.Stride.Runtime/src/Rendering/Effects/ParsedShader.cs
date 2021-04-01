using Stride.Core.Shaders.Ast;
using Stride.Core.Shaders.Ast.Stride;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VL.Stride.Rendering
{
    public class ParsedShader
    {
        public readonly Shader Shader;

        // base shaders
        public IReadOnlyList<ParsedShader> BaseShaders => baseShaders;
        private readonly List<ParsedShader> baseShaders = new List<ParsedShader>();

        // compositions
        public IReadOnlyList<CompositionInput> Compositions => compositions;
        private readonly List<CompositionInput> compositions;

        public ParsedShader(Shader shader)
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

        public void AddBaseShader(ParsedShader baseShader)
            => baseShaders.Add(baseShader);
    }

    public class ParsedShaderRef
    {
        public ParsedShader ParsedShader;
    }

    public class CompositionInput
    {
        public readonly string Name;
        public readonly string TypeName;

        /// <summary>
        /// The local index of this variable in the shader file.
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
