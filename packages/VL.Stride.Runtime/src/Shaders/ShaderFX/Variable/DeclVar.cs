using System;
using System.Collections.Generic;
using Stride.Core;
using Stride.Rendering.Materials;
using Stride.Shaders;
using static VL.Stride.Shaders.ShaderFX.ShaderFXUtils;


namespace VL.Stride.Shaders.ShaderFX
{
    /// <summary>
    /// Contains information about a stream variable and generates a unique but stable ID when the shader gets compiled.
    /// </summary>
    public class DeclVar<T>
    {
        static readonly PropertyKey<HashSet<object>> VisitedVarsKey = new PropertyKey<HashSet<object>>("Var.Visited", typeof(HashSet<object>), DefaultValueMetadata.Static<HashSet<object>>(null, keepDefaultValue: true));
        static readonly PropertyKey<int> VarIDCounterKey = new PropertyKey<int>("Var.VarIDCounter", typeof(int), DefaultValueMetadata.Static(0, keepDefaultValue: true));
        private readonly string varName;
        private string varNameWithID;

        static int GetAndIncIDCount(ShaderGeneratorContext context)
        {
            var result = context.Tags.Get(VarIDCounterKey);
            context.Tags.Set(VarIDCounterKey, result + 1);
            return result;
        }

        bool Visited(ShaderGeneratorContext context)
        {
            var visitedVars = context.Tags.Get(VisitedVarsKey);

            if (visitedVars is null)
            {
                visitedVars = new HashSet<object>();
                visitedVars.Add(this);
                context.Tags.Set(VisitedVarsKey, visitedVars);
                return false;
            }

            if (visitedVars.Contains(this))
                return true;

            visitedVars.Add(this);

            return false;
        }

        public DeclVar(string varName, bool appendID = true)
        {
            this.varName = varName;
            AppendID = appendID;
        }

        public virtual string GetNameForContext(ShaderGeneratorContext context)
        {
            if (AppendID)
            {
                if (!Visited(context))
                    varNameWithID = VarName + "_" + GetAndIncIDCount(context);
            }
            else
            {
                varNameWithID = VarName;
            }

            return varNameWithID;
        }

        public IComputeValue<T> Value { get; }

        public string VarName => varName;

        public bool AppendID { get; }

        public override string ToString()
        {
            return string.Format("Decl Var: {0}", VarName);
        }
    }
}
