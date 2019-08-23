using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using VL.Core;
using VL.Core.Diagnostics;
using Xenko.Core.Diagnostics;
using Xenko.Core.Mathematics;
using Xenko.Rendering;

namespace VL.Xenko.EffectLib
{
    static class EffectUtils
    {
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

        public static MessageType ToMessageType(this LogMessageType type)
        {
            switch (type)
            {
                case LogMessageType.Debug:
                    return MessageType.Debug;
                case LogMessageType.Verbose:
                case LogMessageType.Info:
                    return MessageType.Info;
                case LogMessageType.Warning:
                    return MessageType.Warning;
                case LogMessageType.Error:
                case LogMessageType.Fatal:
                    return MessageType.Error;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    static class WellKnownParameters
    {
        public static readonly Dictionary<string, PerFrameParameters> PerFrameMap = BuildParameterMap<PerFrameParameters>("Global");
        public static readonly Dictionary<string, PerViewParameters> PerViewMap = BuildParameterMap<PerViewParameters>("Transformation");
        public static readonly Dictionary<string, PerDrawParameters> PerDrawMap = BuildParameterMap<PerDrawParameters>("Transformation");

        public static IEnumerable<T> GetWellKnownParameters<T>(this ParameterCollection parameters, Dictionary<string, T> map)
        {
            if (parameters != null)
            {
                foreach (var p in parameters.Layout.LayoutParameterKeyInfos)
                {
                    if (map.TryGetValue(p.Key.Name, out T entry))
                        yield return entry;
                }
            }
        }

        public static void SetPerDrawParameters(this ParameterCollection parameters, PerDrawParameters[] perDrawParams, RenderView renderView, ref Matrix world)
        {
            var worldInverse = world;
            worldInverse.Invert();
            Matrix.Multiply(ref world, ref renderView.View, out Matrix worldView);
            foreach (var perDraw in perDrawParams)
            {
                switch (perDraw)
                {
                    case PerDrawParameters.World:
                        // Already handled
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
                        worldInverse.Invert();
                        parameters.Set(TransformationKeys.WorldViewInverse, ref worldViewInverse);
                        break;
                    case PerDrawParameters.WorldViewProjection:
                        var worldViewProjection = worldView * renderView.Projection;
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
                        // TODO: This is how Xenko does it - differs from patched version
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
                        // TODO: Differs from Xenko
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

        static Dictionary<string, T> BuildParameterMap<T>(string effectName)
        {
            var map = new Dictionary<string, T>();
            foreach (var entry in Enum.GetValues(typeof(T)))
                map.Add($"{effectName}.{entry.ToString()}", (T)entry);
            return map;
        }
    }

    delegate ref TOut ValueConverter<TIn, TOut>(ref TIn value);

    static class TypeConversions
    {
        public static Dictionary<Type, Type> ShaderToPinTypeMap = new Dictionary<Type, Type>()
        {
            //{ typeof(Matrix), typeof(SharpDX.Matrix) },
            //{ typeof(Vector2), typeof(SharpDX.Vector2) },
            //{ typeof(Vector3), typeof(SharpDX.Vector3) },
            //{ typeof(Vector4), typeof(SharpDX.Vector4) },
        };

        static readonly MethodInfo ConvertMethod = typeof(TypeConversions).GetMethod(nameof(Convert), BindingFlags.Static | BindingFlags.Public);

        public static object ConvertShaderToPin(object shaderValue, Type pinType)
        {
            var convertMethod = ConvertMethod.MakeGenericMethod(shaderValue.GetType(), pinType);
            return convertMethod.Invoke(null, new object[] { shaderValue });
        }

        public static TOut Convert<TIn, TOut>(TIn value)
        {
            var converter = GetConverter<TIn, TOut>();
            return converter(ref value);
        }

        public static ValueConverter<TIn, TOut> GetConverter<TIn, TOut>()
        {
            if (typeof(TIn) == typeof(Matrix) || typeof(TOut) == typeof(Matrix))
                return ConvertMatrix<TIn, TOut>;
            return Unsafe.As<TIn, TOut>;
        }

        public static ref TOut ConvertMatrix<TIn, TOut>(ref TIn value)
        {
            ref var m = ref Unsafe.As<TIn, Matrix>(ref value);
            m.Transpose();
            return ref Unsafe.As<Matrix, TOut>(ref m);
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
}
