using Stride.Core.Mathematics;
using Stride.Core.Shaders.Ast;
using Stride.Core.Shaders.Ast.Hlsl;
using Stride.Core.Shaders.Ast.Stride;
using Stride.Rendering;
using Stride.Rendering.Materials;
using Stride.Rendering.Materials.ComputeColors;
using Stride.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VL.Stride.Shaders.ShaderFX;

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
        }

        // cache
        ShaderSource defaultShaderSource;
        ComputeNode defaultComputeNode;

        public ComputeNode GetDefaultComputeNode()
        {
            if (defaultComputeNode != null)
                return defaultComputeNode;

            if (knownShaderFXTypeInputs.TryGetValue(TypeName, out var compDefault))
            {
                var boxedDefaultValue = compDefault.BoxedDefault;

                if (Variable.TryGetAttribute(ShaderMetadata.DefaultName, out var attribute))
                {
                    boxedDefaultValue = attribute.ParseBoxed(compDefault.ValueType);
                }

                defaultComputeNode = compDefault.Factory(boxedDefaultValue);
                return defaultComputeNode;
            }

            return null;        
        }

        public ShaderSource GetDefaultShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            if (defaultShaderSource != null)
                return defaultShaderSource;

            var defaultNode = GetDefaultComputeNode();

            if (defaultNode != null)
            {
                defaultShaderSource = defaultNode.GenerateShaderSource(context, baseKeys);
                return defaultShaderSource;
            }
            else
            {
                defaultShaderSource = new ShaderClassSource(TypeName);
                return defaultShaderSource;
            }
        }

        static Dictionary<string, CompDefault> knownShaderFXTypeInputs = new Dictionary<string, CompDefault>()
        {
            { "ComputeVoid", new CompDefaultVoid() },
            { "ComputeFloat", new CompDefaultValue<float>() },
            { "ComputeFloat2", new CompDefaultValue<Vector2>() },
            { "ComputeFloat3", new CompDefaultValue<Vector3>() },
            { "ComputeFloat4", new CompDefaultValue<Vector4>() },
            { "ComputeMatrix", new CompDefaultValue<Matrix>() },
            { "ComputeBool", new CompDefaultValue<bool>() },
            { "ComputeInt", new CompDefaultValue<int>() },
            { "ComputeInt2", new CompDefaultValue<Int2>() },
            { "ComputeInt3", new CompDefaultValue<Int3>() },
            { "ComputeInt4", new CompDefaultValue<Int4>() },
            { "ComputeUInt", new CompDefaultValue<uint>() },
        };

        abstract class CompDefault
        {
            public readonly object BoxedDefault;
            public readonly Func<object, ComputeNode> Factory;
            public readonly Type ValueType;

            public CompDefault(object defaultValue, Func<object, ComputeNode> factory, Type valueType)
            {
                BoxedDefault = defaultValue;
                Factory = factory;
                ValueType = valueType;
            }
        }

        class CompDefaultVoid : CompDefault
        {
            public CompDefaultVoid()
                : base(null, _ => new ComputeOrder(), null)
            {
            }
        }

        class CompDefaultValue<T> : CompDefault where T : struct
        {
            public CompDefaultValue(T defaultValue = default)
                : base(defaultValue, BuildInput, typeof(T))
            {
            }

            static ComputeNode BuildInput(object boxedDefaultValue)
            {
                var input = new InputValue<T>();
                 if (boxedDefaultValue is T defaultValue)
                    input.Input = defaultValue;
                return input;
            }
        }
    }
}
