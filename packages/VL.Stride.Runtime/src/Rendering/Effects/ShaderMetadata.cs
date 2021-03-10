using System;
using System.IO;
using System.Text;
using Stride.Core.Yaml.Serialization;
using Stride.Graphics;
using Stride.Core.Shaders.Parser;
using VL.Lib.Control;
using Stride.Shaders.Parser;
using Stride.Core.Shaders.Ast;
using System.Linq;
using Stride.Core.Shaders.Ast.Hlsl;
using Stride.Core.Shaders.Ast.Stride;
using System.Collections.Generic;

namespace VL.Stride.Rendering
{
    public class ShaderMetadata
    {
        public PixelFormat OutputFormat { get; set; } = PixelFormat.None;

        public PixelFormat GetPixelFormat(bool hasTextureIn)
        {
            if (!hasTextureIn && OutputFormat == PixelFormat.None)
                return PixelFormat.R8G8B8A8_UNorm;
            else 
                return OutputFormat;
        }

        const string MetadataBegin = "/*MetadataBegin";
        const string MetadataEnd = "MetadataEnd*/";
        public static bool TryParseMetadata(string filename, Serializer serializer, out ShaderMetadata shaderMetadata)
        {
            shaderMetadata = new ShaderMetadata();

            try
            {
                using (var stream = File.OpenRead(filename))
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var firstLine = reader.ReadLine();

                    if (firstLine.Contains(MetadataBegin))
                    {
                        var success = false;
                        var sb = new StringBuilder();
                        var currentLine = firstLine;
                        while (!reader.EndOfStream)
                        {
                            currentLine = reader.ReadLine();
                            if (currentLine.Contains(MetadataEnd))
                            {
                                success = true;
                                break;
                            }

                            sb.AppendLine(currentLine);
                        }

                        if (success)
                        {
                            shaderMetadata = serializer.DeserializeInto(sb.ToString(), shaderMetadata);
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Could't read shader metadata of: " + Path.GetFileName(filename));
                System.Console.WriteLine(e.InnermostException());
            }

            return false;
        }

        public static bool TryParseMetadataAttr(string inputFileName, Serializer serializer, out ShaderMetadata shaderMetadata)
        {
            shaderMetadata = new ShaderMetadata();

            if (EffectUtils.TryParseEffect(inputFileName, out var shader))
            {
                var shaderDecl = GetFistClassDecl(shader.Declarations);

                if (shaderDecl != null)
                {
                    foreach (var attr in shaderDecl.Attributes.OfType<AttributeDeclaration>())
                    {
                        switch (attr.Name)
                        {
                            case "OutputFormat":
                                if (Enum.TryParse<PixelFormat>(attr.Parameters.FirstOrDefault().Value as string, true, out var pixelFormat))
                                    shaderMetadata.OutputFormat = pixelFormat;
                                break;
                            default:
                                break;
                        }
                    }
                }

                return true;
            }

            return false;
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
