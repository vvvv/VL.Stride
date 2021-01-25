using System;
using System.IO;
using System.Text;
using Stride.Core.Yaml.Serialization;
using Stride.Graphics;
using VL.Lib.Control;

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
        public static bool TryReadMetadata(string filename, Serializer serializer, out ShaderMetadata shaderMetadata)
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
    }
}
