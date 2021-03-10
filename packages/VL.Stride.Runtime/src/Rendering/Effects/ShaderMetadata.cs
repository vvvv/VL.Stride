using System;
using Stride.Graphics;
using Stride.Core.Shaders.Ast;
using System.Linq;
using Stride.Core.Shaders.Ast.Hlsl;
using System.Collections.Generic;
using Stride.Core.IO;

namespace VL.Stride.Rendering
{
    public class ShaderMetadata
    {
        public PixelFormat OutputFormat { get; private set; } = PixelFormat.None;

        public string Category { get; private set; }

        public string Summary { get; private set; }

        public string Remarks { get; private set; }

        public string Tags { get; private set; }

        public PixelFormat GetPixelFormat(bool hasTextureIn)
        {
            if (!hasTextureIn && OutputFormat == PixelFormat.None)
                return PixelFormat.R8G8B8A8_UNorm;
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

        const string CategoryName = "Category";
        const string SummaryName = "Summary";
        const string RemarksName = "Remarks";
        const string TagsName = "Tags";
        const string OutputFormatName = "OutputFormat";

        public static ShaderMetadata CreateMetadata(string effectName, IVirtualFileProvider fileProvider)
        {
            //create metadata with default values
            var shaderMetadata = new ShaderMetadata();

            var inputFileName = EffectUtils.GetPathOfSdslShader(effectName, fileProvider);

            //try to populate metdata with information form the shader
            if (EffectUtils.TryParseEffect(inputFileName, out var shader))
            {
                var shaderDecl = GetFistClassDecl(shader.Declarations);

                if (shaderDecl != null)
                {
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
                }
            }

            return shaderMetadata;
        }

        private static string FirstParamAsString(AttributeDeclaration attr)
        {
            return attr.Parameters.FirstOrDefault()?.Value as string;
        }

        static ClassType GetFistClassDecl(List<Node> nodes)
        {
            return nodes.OfType<ClassType>().FirstOrDefault();

            //TODO: namespace could be surrounding the shader
            //else if (firstDecl is NamespaceBlock ns)
            //{
            //    return GetFistClassDecl(ns.Body);
            //}
            //
            //return null;
        }
    }
}
