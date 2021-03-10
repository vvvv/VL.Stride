using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VL.Core;
using Stride.Core.Mathematics;
using Stride.Rendering;
using Stride.Graphics;
using Stride.Core.IO;
using System.IO;
using Stride.Shaders.Compiler;
using Stride.Core.Shaders.Ast;
using Stride.Core.Shaders.Parser;
using Stride.Shaders.Parser;

namespace VL.Stride.Rendering
{
    static class EffectUtils
    {
        public static string GetPathOfSdslShader(string effectName, IVirtualFileProvider fileProvider)
        {
            var path = EffectCompilerBase.GetStoragePathFromShaderType(effectName);
            if (fileProvider.TryGetFileLocation(path, out var filePath, out _, out _))
                return filePath;

            var pathUrl = path + "/path";
            if (fileProvider.FileExists(pathUrl))
            {
                using (var pathStream = fileProvider.OpenStream(pathUrl, VirtualFileMode.Open, VirtualFileAccess.Read))
                using (var reader = new StreamReader(pathStream))
                {
                    return reader.ReadToEnd();
                }
            }

            return null;
        }

        static readonly Regex FCamelCasePattern = new Regex("[a-z][A-Z0-9]", RegexOptions.Compiled);

        public static void SelectPin<TPin>(this IVLPin[] pins, IVLPinDescription description, ref TPin pin) where TPin : Pin
        {
            pin = pins.OfType<TPin>().FirstOrDefault(p => p.Name == description.Name);
        }

        public static string GetPinName(this ParameterKey key, HashSet<string> usedNames)
        {
            var variableName = key.GetVariableName();
            var shaderName = key.GetShaderName();
            var name = key.Name;
            var camelCasedName = FCamelCasePattern.Replace(variableName, match => $"{match.Value[0]} {match.Value[1]}");
            var result = char.ToUpper(camelCasedName[0]) + camelCasedName.Substring(1);
            if (usedNames.Add(result))
                return result;
            return $"{shaderName} {result}";
        }

        public static string GetShaderName(this ParameterKey key)
        {
            var name = key.Name;
            var dotIndex = name.IndexOf('.');
            if (dotIndex > 0)
                return name.Substring(0, dotIndex);
            return string.Empty;
        }

        public static string GetVariableName(this ParameterKey key)
        {
            var name = key.Name;
            var dotIndex = name.IndexOf('.');
            if (dotIndex >= 0)
                return name.Substring(dotIndex + 1);
            return name;
        }

        public static bool TryParseEffect(this IVirtualFileProvider fileProvider, string effectName, out Shader shader)
        {
            shader = null;
            var fileName = GetPathOfSdslShader(effectName, fileProvider);
            if (!string.IsNullOrWhiteSpace(fileName))
                return TryParseEffect(fileName, out shader);
            else
                return false;
            
        }


        public static bool TryParseEffect(string inputFileName, out Shader shader)
        {
            shader = null;

            try
            {
                ShaderMacro[] macros;

                // Changed some keywords to avoid ambiguities with HLSL and improve consistency
                if (inputFileName != null && Path.GetExtension(inputFileName).ToLowerInvariant() == ".sdfx")
                {
                    // SDFX
                    macros = new[]
                    {
                        new ShaderMacro("shader", "effect")
                    };
                }
                else
                {
                    // SDSL
                    macros = new[]
                    {
                        new ShaderMacro("class", "shader")
                    };
                }

                var parsingResult = StrideShaderParser.TryPreProcessAndParse(inputFileName, macros);

                if (parsingResult.HasErrors)
                {
                    return false;
                }
                else //success
                {
                    shader = parsingResult.Shader;
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    static class WellKnownParameters
    {
        public static readonly Dictionary<string, PerFrameParameters> PerFrameMap = BuildParameterMap<PerFrameParameters>("Global");
        public static readonly Dictionary<string, PerViewParameters> PerViewMap = BuildParameterMap<PerViewParameters>("Transformation");
        public static readonly Dictionary<string, PerDrawParameters> PerDrawMap = BuildParameterMap<PerDrawParameters>("Transformation");
        public static readonly Dictionary<string, TexturingParameters> TexturingMap = BuildParameterMap<TexturingParameters>("Texturing");

        public static IEnumerable<T> GetWellKnownParameters<T>(this ParameterCollection parameters, Dictionary<string, T> map)
        {
            foreach (var p in parameters.Layout.LayoutParameterKeyInfos)
            {
                if (map.TryGetValue(p.Key.Name, out T entry))
                    yield return entry;
            }
        }

        public static IEnumerable<TexturingParameters> GetTexturingParameters(this ParameterCollection parameters)
        {
            foreach (var p in parameters.Layout.LayoutParameterKeyInfos)
            {
                if (p.Key == TexturingKeys.Texture0)
                    yield return TexturingParameters.Texture0TexelSize;

                if (p.Key == TexturingKeys.Texture1)
                    yield return TexturingParameters.Texture1TexelSize;

                if (p.Key == TexturingKeys.Texture2)
                    yield return TexturingParameters.Texture2TexelSize;

                if (p.Key == TexturingKeys.Texture3)
                    yield return TexturingParameters.Texture3TexelSize;

                if (p.Key == TexturingKeys.Texture4)
                    yield return TexturingParameters.Texture4TexelSize;

                if (p.Key == TexturingKeys.Texture5)
                    yield return TexturingParameters.Texture5TexelSize;

                if (p.Key == TexturingKeys.Texture6)
                    yield return TexturingParameters.Texture6TexelSize;

                if (p.Key == TexturingKeys.Texture7)
                    yield return TexturingParameters.Texture7TexelSize;

                if (p.Key == TexturingKeys.Texture8)
                    yield return TexturingParameters.Texture8TexelSize;

                if (p.Key == TexturingKeys.Texture9)
                    yield return TexturingParameters.Texture9TexelSize;
            }
        }

        public static void SetPerDrawParameters(this ParameterCollection parameters, PerDrawParameters[] perDrawParams, RenderView renderView, ref Matrix world)
        {
            var worldInverse = world;
            worldInverse.Invert();
            Matrix.Multiply(ref world, ref renderView.View, out var worldView);
            foreach (var perDraw in perDrawParams)
            {
                switch (perDraw)
                {
                    case PerDrawParameters.World:
                        // Already handled. DON'T write it again or we introduce a feedback between render calls!
                        break;
                    case PerDrawParameters.WorldInverse:
                        parameters.Set(TransformationKeys.WorldInverse, ref worldInverse);
                        break;
                    case PerDrawParameters.WorldInverseTranspose:
                        var worldInverseTranspose = worldInverse;
                        worldInverseTranspose.Transpose();
                        parameters.Set(TransformationKeys.WorldInverseTranspose, ref worldInverseTranspose);
                        break;
                    case PerDrawParameters.WorldView:
                        parameters.Set(TransformationKeys.WorldView, ref worldView);
                        break;
                    case PerDrawParameters.WorldViewInverse:
                        var worldViewInverse = worldView;
                        worldViewInverse.Invert();
                        parameters.Set(TransformationKeys.WorldViewInverse, ref worldViewInverse);
                        break;
                    case PerDrawParameters.WorldViewProjection:
                        Matrix.Multiply(ref worldView, ref renderView.Projection, out var worldViewProjection);
                        parameters.Set(TransformationKeys.WorldViewProjection, ref worldViewProjection);
                        break;
                    case PerDrawParameters.WorldScale:
                        var worldScale = new Vector3(
                            ((Vector3)world.Row1).Length(),
                            ((Vector3)world.Row2).Length(),
                            ((Vector3)world.Row3).Length());
                        parameters.Set(TransformationKeys.WorldScale, ref worldScale);
                        break;
                    case PerDrawParameters.EyeMS:
                        // TODO: This is how Stride does it - differs from patched version
                        //var eyeMS = new Vector4(worldInverse.M41, worldInverse.M42, worldInverse.M43, 1.0f);
                        var viewInverse = renderView.View;
                        viewInverse.Invert();
                        var eyeMS = Vector4.Transform(new Vector4(0, 0, -1, 0), viewInverse);
                        parameters.Set(TransformationKeys.EyeMS, ref eyeMS);
                        break;
                    default:
                        break;
                }
            }
        }

        public static void SetPerViewParameters(this ParameterCollection parameters, PerViewParameters[] perViewParams, RenderView renderView)
        {
            foreach (var perView in perViewParams)
            {
                switch (perView)
                {
                    case PerViewParameters.View:
                        parameters.Set(TransformationKeys.View, ref renderView.View);
                        break;
                    case PerViewParameters.ViewInverse:
                        var view = renderView.View;
                        view.Invert();
                        parameters.Set(TransformationKeys.ViewInverse, ref view);
                        break;
                    case PerViewParameters.Projection:
                        parameters.Set(TransformationKeys.Projection, ref renderView.Projection);
                        break;
                    case PerViewParameters.ProjectionInverse:
                        var projection = renderView.Projection;
                        projection.Invert();
                        parameters.Set(TransformationKeys.ProjectionInverse, ref projection);
                        break;
                    case PerViewParameters.ViewProjection:
                        parameters.Set(TransformationKeys.ViewProjection, ref renderView.ViewProjection);
                        break;
                    case PerViewParameters.ProjScreenRay:
                        var projScreenRay = new Vector2(-1.0f / renderView.Projection.M11, 1.0f / renderView.Projection.M22);
                        parameters.Set(TransformationKeys.ProjScreenRay, ref projScreenRay);
                        break;
                    case PerViewParameters.Eye:
                        var viewInverse = renderView.View;
                        viewInverse.Invert();
                        // TODO: Differs from Stride
                        //var eye = new Vector4(viewInverse.M41, viewInverse.M42, viewInverse.M43, 1.0f);
                        var eye = viewInverse.Row4;
                        parameters.Set(TransformationKeys.Eye, ref eye);
                        break;
                    default:
                        break;
                }
            }
        }

        public static void SetPerFrameParameters(this ParameterCollection parameters, PerFrameParameters[] perFrameParams, RenderContext renderContext)
        {
            foreach (var perFrame in perFrameParams)
            {
                switch (perFrame)
                {
                    case PerFrameParameters.Time:
                        parameters.Set(GlobalKeys.Time, (float)renderContext.Time.Total.TotalSeconds);
                        break;
                    case PerFrameParameters.TimeStep:
                        parameters.Set(GlobalKeys.TimeStep, (float)renderContext.Time.Elapsed.TotalSeconds);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public static void SetTexturingParameters(this ParameterCollection parameters, TexturingParameters[] texturingParams)
        {
            foreach (var texturingParam in texturingParams)
            {
                switch (texturingParam)
                {
                    case TexturingParameters.Texture0TexelSize:
                        SetTexelSize(parameters, 0);
                        break;
                    case TexturingParameters.Texture1TexelSize:
                        SetTexelSize(parameters, 1);
                        break;
                    case TexturingParameters.Texture2TexelSize:
                        SetTexelSize(parameters, 2);
                        break;
                    case TexturingParameters.Texture3TexelSize:
                        SetTexelSize(parameters, 3);
                        break;
                    case TexturingParameters.Texture4TexelSize:
                        SetTexelSize(parameters, 4);
                        break;
                    case TexturingParameters.Texture5TexelSize:
                        SetTexelSize(parameters, 5);
                        break;
                    case TexturingParameters.Texture6TexelSize:
                        SetTexelSize(parameters, 6);
                        break;
                    case TexturingParameters.Texture7TexelSize:
                        SetTexelSize(parameters, 7);
                        break;
                    case TexturingParameters.Texture8TexelSize:
                        SetTexelSize(parameters, 8);
                        break;
                    case TexturingParameters.Texture9TexelSize:
                        SetTexelSize(parameters, 9);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private static void SetTexelSize(ParameterCollection parameters, int i)
        {
            var tex = parameters.Get(TexturingKeys.DefaultTextures[i]);
            if (tex != null)
                parameters.Set(TexturingKeys.TexturesTexelSize[i], new Vector2(1.0f / tex.ViewWidth, 1.0f / tex.ViewHeight));
        }

        static Dictionary<string, T> BuildParameterMap<T>(string effectName)
        {
            var map = new Dictionary<string, T>();
            foreach (var entry in Enum.GetValues(typeof(T)))
                map.Add($"{effectName}.{entry.ToString()}", (T)entry);
            return map;
        }
    }

    // from Globals shader
    enum PerFrameParameters
    {
        Time,
        TimeStep,
    }

    // from Transformation shader
    enum PerViewParameters
    {
        /// <summary>
        /// View matrix. Default to Matrix.Identity.
        /// </summary>
        View,
        /// <summary>
        /// Inverse View matrix. Default to Matrix.Inverse(View)
        /// </summary>
        ViewInverse,
        /// <summary>
        /// Projection matrix. Default to Matrix.Identity.
        /// </summary>
        Projection,
        /// <summary>
        /// Projection matrix. Default to Matrix.Inverse(Projection).
        /// </summary>
        ProjectionInverse,
        /// <summary>
        /// ViewProjection matrix. Default to = View * Projection.
        /// </summary>
        ViewProjection,
        /// <summary>
        /// Screen projected ray vector.  Default to = new Vector2(-1.0f / Projection.M11, 1.0f / Projection.M22);
        /// </summary>
        ProjScreenRay,
        /// <summary>
        /// Eye vector. Default to = View^-1[M41,M42,M43,1.0]
        /// </summary>
        Eye
    }

    // from Transformation shader
    enum PerDrawParameters
    {
        /// <summary>
        /// World matrix. Default to Matrix.Identity.
        /// </summary>
        World,
        /// <summary>
        /// Inverse World matrix. Default to Matrix.Inverse(World).
        /// </summary>
        WorldInverse,
        /// <summary>
        /// Inverse Transpose World matrix. Default to Matrix.Transpose(Matrix.Inverse(World)).
        /// </summary>
        WorldInverseTranspose,
        /// <summary>
        /// WorldView matrix. Default to = World * View.
        /// </summary>
        WorldView,
        /// <summary>
        /// Inverse WorldView matrix. Default to Matrix.Inverse(WorldView)
        /// </summary>
        WorldViewInverse,
        /// <summary>
        /// WorldViewProjection matrix. Default to = World * ViewProjection.
        /// </summary>
        WorldViewProjection,
        /// <summary>
        /// The scale of the World. Default to Vector2.One.
        /// </summary>
        WorldScale,
        /// <summary>
        /// Eye vector in model space. Default to = (World*View)^-1[M41,M42,M43,1.0]
        /// </summary>
        EyeMS
    }

    enum TexturingParameters
    {
        Texture0TexelSize,
        Texture1TexelSize,
        Texture2TexelSize,
        Texture3TexelSize,
        Texture4TexelSize,
        Texture5TexelSize,
        Texture6TexelSize,
        Texture7TexelSize,
        Texture8TexelSize,
        Texture9TexelSize
    }
}
