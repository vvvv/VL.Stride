using Stride.Core.Shaders.Ast;
using Stride.Core.Shaders.Ast.Hlsl;
using Stride.Core.Shaders.Ast.Stride;
using Stride.Rendering;
using Stride.Shaders;
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
        public readonly ClassType ShaderClass;

        // base shaders
        public IReadOnlyList<ParsedShader> BaseShaders => baseShaders;
        private readonly List<ParsedShader> baseShaders = new List<ParsedShader>();

        // compositions
        public IReadOnlyDictionary<string, CompositionInput> Compositions => compositions;
        private readonly Dictionary<string, CompositionInput> compositions;

        public IReadOnlyDictionary<string, CompositionInput> CompositionsWithBaseShaders => compositionsWithBaseShaders.Value;

        Lazy<IReadOnlyDictionary<string, CompositionInput>> compositionsWithBaseShaders;

        private IEnumerable<CompositionInput> GetCompositionsWithBaseShaders()
        {
            foreach (var comp in Compositions)
            {
                yield return comp.Value;
            }

            foreach (var baseClass in BaseShaders)
            {
                foreach (var baseComp in baseClass.Compositions)
                {
                    yield return baseComp.Value;
                }
            }
        }

        public ParsedShader(Shader shader)
        {
            Shader = shader;
            ShaderClass = Shader.GetFirstClassDecl();
            compositions = ShaderClass.Members
                .OfType<Variable>()
                .Select((v, i) => (v, i))
                .Where(v => v.v.Qualifiers.Contains(StrideStorageQualifier.Compose))
                .Select(v => new CompositionInput(v.v, v.i))
                .ToDictionary(v => v.Name);

            compositionsWithBaseShaders = new Lazy<IReadOnlyDictionary<string, CompositionInput>>(() => GetCompositionsWithBaseShaders().ToDictionary(c => c.Name));
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
        public readonly PermutationParameterKey<ShaderSource> Key;
        public readonly Lazy<ShaderSource> DefaultShaderSource;

        /// <summary>
        /// The local index of this variable in the shader file.
        /// </summary>
        public readonly int LocalIndex;

        public readonly Variable Variable;

        public CompositionInput(Variable v, int localIndex)
        {
            Name = v.Name.Text;
            TypeName = v.Type.Name.Text;
            Key = new PermutationParameterKey<ShaderSource>(Name);
            LocalIndex = localIndex;
            Variable = v;
            DefaultShaderSource = new Lazy<ShaderSource>(() => new ShaderClassSource(TypeName));
        }
    }
}
