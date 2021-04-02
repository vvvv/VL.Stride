using System;
using Stride.Graphics;
using Stride.Core.Shaders.Ast;
using System.Linq;
using Stride.Core.Shaders.Ast.Hlsl;
using System.Collections.Generic;
using Stride.Core.IO;
using Stride.Rendering;
using Stride.Core.Shaders.Ast.Stride;
using Stride.Shaders;
using VL.Stride.Shaders.ShaderFX;
using Stride.Core.Mathematics;
using Stride.Rendering.Materials;

namespace VL.Stride.Rendering
{
    public class ShaderMetadata
    {
        public PixelFormat OutputFormat { get; private set; } = PixelFormat.None;

        public string Category { get; private set; }

        public string Summary { get; private set; }

        public string Remarks { get; private set; }

        public string Tags { get; private set; }

        public ParsedShader ParsedShader { get; private set; }

        public PixelFormat GetPixelFormat(bool isFilter)
        {
            if (!isFilter && OutputFormat == PixelFormat.None)
                return PixelFormat.R8G8B8A8_UNorm_SRgb;
            else 
                return OutputFormat;
        }

        public string GetCategory(string prefix)
        {
            var result = prefix;

            if (string.IsNullOrWhiteSpace(Category))
                return result;

            if (!Category.StartsWith(prefix))
                return prefix + "." + Category;

            return Category;
        }

        Dictionary<string, string> pinEnumTypes = new Dictionary<string, string>();
        HashSet<string> optionalPins = new HashSet<string>();

        private void AddEnumTypePinAttribute(string name, string enumTypeName)
        {
            pinEnumTypes[name] = enumTypeName;
        }

        private void AddOptionalPinAttribute(string name)
        {
            optionalPins.Add(name);
        }

        /// <summary>
        /// Gets the type of the pin, if overwritten by an attribute, e.g. int -> enum.
        /// </summary>
        public Type GetPinType(ParameterKey key)
        {
            if (pinEnumTypes.TryGetValue(key.Name, out var enumTypeName))
            {
                try
                {
                    var type = Type.GetType(enumTypeName);
                    if (type != null && type.IsEnum)
                        return type;
                }
                catch (Exception)
                {

                }
            }

            if (key.PropertyType == typeof(ShaderSource))
            {
                if (ParsedShader.CompositionsWithBaseShaders.TryGetValue(key.GetVariableName(), out var composition))
                {
                    if (knownShaderFXTypes.TryGetValue(composition.TypeName, out var type))
                    {
                        return type;
                    }
                }

                return typeof(IComputeNode);
            }

            return null;
        }

        static Dictionary<string, Type> knownShaderFXTypes = new Dictionary<string, Type>()
        {
            { "ComputeVoid", typeof(ComputeVoid) },
            { "ComputeFloat", typeof(SetVar<float>) },
            { "ComputeFloat2", typeof(SetVar<Vector2>) },
            { "ComputeFloat3", typeof(SetVar<Vector3>) },
            { "ComputeFloat4", typeof(SetVar<Vector4>) },
            { "ComputeMatrix", typeof(SetVar<Matrix>) },
            { "ComputeInt", typeof(SetVar<int>) },
            { "ComputeInt2", typeof(SetVar<Int2>) },
            { "ComputeInt3", typeof(SetVar<Int3>) },
            { "ComputeInt4", typeof(SetVar<Int4>) },
        };

        /// <summary>
        /// Determines whether the specified pin with the given key is optional.
        /// </summary>
        public bool IsOptional(ParameterKey key)
        {
            return optionalPins.Contains(key.Name);
        }

        //shader
        const string CategoryName = "Category";
        const string SummaryName = "Summary";
        const string RemarksName = "Remarks";
        const string TagsName = "Tags";
        const string OutputFormatName = "OutputFormat";

        //pin
        const string EnumTypeName = "EnumType";
        const string OptionalName = "Optional";

        /// <summary>
        /// Registers the additional stride shader attributes. Avoids writing them to the final shader, which would create an error in the native platform compiler.
        /// </summary>
        public static void RegisterAdditionalShaderAttributes()
        {
            StrideAttributes.AvailableAttributes.Add(EnumTypeName);
            StrideAttributes.AvailableAttributes.Add(OptionalName);
        }

        public static ShaderMetadata CreateMetadata(string effectName, IVirtualFileProvider fileProvider)
        {
            //create metadata with default values
            var shaderMetadata = new ShaderMetadata();

            //try to populate metdata with information form the shader
            if (fileProvider.TryParseEffect(effectName, out var result))
            {
                shaderMetadata.ParsedShader = result;
                var shaderDecl = result.ShaderClass;

                if (shaderDecl != null)
                {
                    //shader 
                    foreach (var attr in shaderDecl.Attributes.OfType<AttributeDeclaration>())
                    {
                        switch (attr.Name)
                        {
                            case CategoryName:
                                shaderMetadata.Category = FirstParamAsString(attr);
                                break;
                            case SummaryName:
                                shaderMetadata.Summary = FirstParamAsString(attr);
                                break;
                            case RemarksName:
                                shaderMetadata.Remarks = FirstParamAsString(attr);
                                break;
                            case TagsName:
                                shaderMetadata.Tags = FirstParamAsString(attr);
                                break;
                            case OutputFormatName:
                                if (Enum.TryParse<PixelFormat>(FirstParamAsString(attr), true, out var pixelFormat))
                                    shaderMetadata.OutputFormat = pixelFormat;
                                break;
                            default:
                                break;
                        }
                    }

                    //pins
                    foreach (var pinDecl in shaderDecl.Members.OfType<Variable>())
                    {
                        foreach (var attr in pinDecl.Attributes.OfType<AttributeDeclaration>())
                        {
                            switch (attr.Name)
                            {
                                case EnumTypeName:
                                    var enumTypeName = FirstParamAsString(attr);
                                    shaderMetadata.AddEnumTypePinAttribute(shaderDecl.Name.Text + "." + pinDecl.Name.Text, enumTypeName);
                                    break;
                                case OptionalName:
                                    shaderMetadata.AddOptionalPinAttribute(shaderDecl.Name.Text + "." + pinDecl.Name.Text);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }

            return shaderMetadata;
        }

        private static string FirstParamAsString(AttributeDeclaration attr)
        {
            return attr.Parameters.FirstOrDefault()?.Value as string;
        }
    }
}
