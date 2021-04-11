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
using System.ComponentModel;

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

        public bool IsTextureSource { get; private set; }

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

        Dictionary<string, EnumMetadata> pinEnumTypes = new Dictionary<string, EnumMetadata>();
        HashSet<string> optionalPins = new HashSet<string>();

        private void AddEnumTypePinAttribute(string name, string enumTypeName, Expression initialValue)
        {
            var type = Type.GetType(enumTypeName);
            if (type != null && type.IsEnum)
            {
                object initalVal = Activator.CreateInstance(type);
                if (initialValue is LiteralExpression literal)
                {
                    var defaultText = literal.Text;
                    var converter = TypeDescriptor.GetConverter(Enum.GetUnderlyingType(type));

                    if (converter != null && converter.IsValid(defaultText))
                    {
                        var underVal = converter.ConvertFromString(defaultText);
                        initalVal = Enum.ToObject(type, underVal);
                    }
                }

                pinEnumTypes[name] = new EnumMetadata(type, initalVal);
            }
        }

        private void AddOptionalPinAttribute(string name)
        {
            optionalPins.Add(name);
        }

        /// <summary>
        /// Gets the type of the pin, if overwritten by an attribute, e.g. int -> enum.
        /// </summary>
        public Type GetPinType(ParameterKey key, out object boxedDefaultValue)
        {
            boxedDefaultValue = null;
            if (pinEnumTypes.TryGetValue(key.Name, out var enumTypeName))
            {
                boxedDefaultValue = enumTypeName.defaultValue;
                return enumTypeName.typeName;
            }

            if (key.PropertyType == typeof(ShaderSource) && ParsedShader != null)
            {
                if (ParsedShader.CompositionsWithBaseShaders.TryGetValue(key.GetVariableName(), out var composition))
                {
                    boxedDefaultValue = composition.GetDefaultComputeNode();
                    if (knownShaderFXTypes.TryGetValue(composition.TypeName, out var type))
                    {
                        return type;
                    }
                }

                return typeof(IComputeNode);
            }

            return null;
        }

        public Type GetShaderFXOutputType(out Type innerType)
        {
            innerType = null;
            foreach (var baseShader in ParsedShader?.BaseShaders)
            {
                var baseName = baseShader?.ShaderClass?.Name;
                if (!string.IsNullOrWhiteSpace(baseName))
                {
                    if (knownShaderFXTypes.TryGetValue(baseName, out var type))
                    {
                        if (type.IsGenericType)
                            innerType = type.GetGenericArguments()[0];

                        return type;
                    }
                }
            }

            return typeof(IComputeNode);
        }


        static Dictionary<string, Type> knownShaderFXTypes = new Dictionary<string, Type>()
        {
            { "ComputeVoid", typeof(ComputeVoid) },
            { "ComputeFloat", typeof(SetVar<float>) },
            { "ComputeFloat2", typeof(SetVar<Vector2>) },
            { "ComputeFloat3", typeof(SetVar<Vector3>) },
            { "ComputeFloat4", typeof(SetVar<Vector4>) },
            { "ComputeMatrix", typeof(SetVar<Matrix>) },
            { "ComputeBool", typeof(SetVar<bool>) },
            { "ComputeInt", typeof(SetVar<int>) },
            { "ComputeInt2", typeof(SetVar<Int2>) },
            { "ComputeInt3", typeof(SetVar<Int3>) },
            { "ComputeInt4", typeof(SetVar<Int4>) },
            { "ComputeUInt", typeof(SetVar<uint>) },
        };

        /// <summary>
        /// Determines whether the specified pin with the given key is optional.
        /// </summary>
        public bool IsOptional(ParameterKey key)
        {
            return optionalPins.Contains(key.Name);
        }

        //shader
        public const string CategoryName = "Category";
        public const string SummaryName = "Summary";
        public const string RemarksName = "Remarks";
        public const string TagsName = "Tags";
        public const string OutputFormatName = "OutputFormat";
        public const string TextureSourceName = "TextureSource";

        //pin
        public const string EnumTypeName = "EnumType";
        public const string OptionalName = "Optional";
        public const string DefaultName = "Default";

        /// <summary>
        /// Registers the additional stride variable attributes. Avoids writing them to the final shader, which would create an error in the native platform compiler.
        /// </summary>
        public static void RegisterAdditionalShaderAttributes()
        {
            StrideAttributes.AvailableAttributes.Add(EnumTypeName);
            StrideAttributes.AvailableAttributes.Add(OptionalName);
            StrideAttributes.AvailableAttributes.Add(DefaultName);
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
                                shaderMetadata.Category = attr.ParseString();
                                break;
                            case SummaryName:
                                shaderMetadata.Summary = attr.ParseString();
                                break;
                            case RemarksName:
                                shaderMetadata.Remarks = attr.ParseString();
                                break;
                            case TagsName:
                                shaderMetadata.Tags = attr.ParseString();
                                break;
                            case OutputFormatName:
                                if (Enum.TryParse<PixelFormat>(attr.ParseString(), true, out var pixelFormat))
                                    shaderMetadata.OutputFormat = pixelFormat;
                                break;
                            case TextureSourceName:
                                shaderMetadata.IsTextureSource = true;
                                break;
                            default:
                                break;
                        }
                    }

                    //pins
                    foreach (var pinDecl in shaderDecl.Members.OfType<Variable>().Where(v => !v.Qualifiers.Contains(StrideStorageQualifier.Compose)))
                    {
                        foreach (var attr in pinDecl.Attributes.OfType<AttributeDeclaration>())
                        {
                            switch (attr.Name)
                            {
                                case EnumTypeName:
                                    shaderMetadata.AddEnumTypePinAttribute(shaderDecl.Name.Text + "." + pinDecl.Name.Text, attr.ParseString(), pinDecl.InitialValue);
                                    break;
                                case OptionalName:
                                    shaderMetadata.AddOptionalPinAttribute(shaderDecl.Name.Text + "." + pinDecl.Name.Text);
                                    break;
                                case DefaultName:
                                    // handled in composition parsing in ParseShader.cs
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

        class EnumMetadata
        {
            public readonly Type typeName;
            public readonly object defaultValue;

            public EnumMetadata(Type enumType, object boxedDefaultValue)
            {
                typeName = enumType;
                defaultValue = boxedDefaultValue;
            }
        }
    }
}
