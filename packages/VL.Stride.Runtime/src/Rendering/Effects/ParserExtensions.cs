using Stride.Core.Shaders.Ast.Hlsl;
using System;
using System.Linq;
using VL.Lang;
using Stride.Core.Mathematics;
using Stride.Core.Shaders.Ast.Stride;
using Stride.Core.Shaders.Ast;

namespace VL.Stride.Rendering
{
    public static class ParserExtensions
    {
        public static ClassType GetFirstClassDecl(this Shader shader)
        {

            var result = shader.Declarations.OfType<ClassType>().FirstOrDefault();

            if (result == null)
            {
                var nameSpace = shader.Declarations.OfType<NamespaceBlock>().FirstOrDefault();
                if (nameSpace != null)
                {
                    result = nameSpace.Body.OfType<ClassType>().FirstOrDefault();
                }

            }

            return result;
        }

        public static bool TryGetAttribute(this Variable v, string attrName, out AttributeDeclaration attribute)
        {
            attribute = v.Attributes.OfType<AttributeDeclaration>().Where(a => a.Name.Text == ShaderMetadata.DefaultName).FirstOrDefault();
            return attribute != null;
        }

        public static string ParseString(this AttributeDeclaration attr)
        {
            return attr.Parameters.FirstOrDefault()?.Value as string;
        }

        public static bool ParseBool(this AttributeDeclaration attr, int index = 0)
        {
            if (attr.Parameters.Count > index)
            {
                if (bool.TryParse(attr.Parameters[index].Text, out var value))
                {
                    return value;
                }
            }
            return default;
        }

        public static float ParseFloat(this AttributeDeclaration attr, int index = 0)
        {
            if (attr.Parameters.Count > index)
            {
                if (UserInputParsing.TryParseFloat(attr.Parameters[index].Text, out var value))
                {
                    return value;
                }
            }
            return default;
        }

        public static int ParseInt(this AttributeDeclaration attr, int index = 0)
        {
            if (attr.Parameters.Count > index)
            {
                if (UserInputParsing.TryParseInt(attr.Parameters[index].Text, out var value))
                {
                    return value;
                }
            }
            return default;
        }

        public static Int2 ParseInt2(this AttributeDeclaration attr)
        {
            return new Int2(attr.ParseInt(0), attr.ParseInt(1));
        }

        public static Int3 ParseInt3(this AttributeDeclaration attr)
        {
            return new Int3(attr.ParseInt(0), attr.ParseInt(1), attr.ParseInt(2));
        }

        public static Int4 ParseInt4(this AttributeDeclaration attr)
        {
            return new Int4(attr.ParseInt(0), attr.ParseInt(1), attr.ParseInt(2), attr.ParseInt(3));
        }

        public static uint ParseUInt(this AttributeDeclaration attr, int index = 0)
        {
            if (attr.Parameters.Count > index)
            {
                if (UserInputParsing.TryParseUInt(attr.Parameters[index].Text, out var value))
                {
                    return value;
                }
            }
            return default;
        }

        public static Vector2 ParseVector2(this AttributeDeclaration attr)
        {
            return new Vector2(attr.ParseFloat(0), attr.ParseFloat(1));
        }

        public static Vector3 ParseVector3(this AttributeDeclaration attr)
        {
            return new Vector3(attr.ParseFloat(0), attr.ParseFloat(1), attr.ParseFloat(2));
        }

        public static Vector4 ParseVector4(this AttributeDeclaration attr)
        {
            return new Vector4(attr.ParseFloat(0), attr.ParseFloat(1), attr.ParseFloat(2), attr.ParseFloat(3));
        }

        public static object ParseBoxed(this AttributeDeclaration attr, Type type, object defaultVlaue = null)
        {
            if (type == typeof(float))
                return attr.ParseFloat();

            if (type == typeof(Vector2))
                return attr.ParseVector2();

            if (type == typeof(Vector3))
                return attr.ParseVector3();

            if (type == typeof(Vector4))
                return attr.ParseVector4();

            if (type == typeof(bool))
                return attr.ParseBool();

            if (type == typeof(int))
                return attr.ParseInt();

            if (type == typeof(Int2))
                return attr.ParseInt2();

            if (type == typeof(Int3))
                return attr.ParseInt3();

            if (type == typeof(Int4))
                return attr.ParseInt4();

            if (type == typeof(uint))
                return attr.ParseUInt();

            if (type == typeof(string))
                return attr.ParseString();

            return defaultVlaue ?? Activator.CreateInstance(type);
        }
    }
}
