using System.Collections.Generic;
using Stride.Core;
using Stride.Rendering.Materials;
using Stride.Shaders;
using static VL.Stride.Shaders.ShaderFX.ShaderFXUtils;


namespace VL.Stride.Shaders.ShaderFX
{
    /// <summary>
    /// Defines a variable and assigns a value to it. Can also re-assign an existing Var.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="ComputeNode{T}" />
    /// <seealso cref="IComputeVoid" />
    public class Var<T> : ComputeNode<T>, IComputeVoid
    {
        static readonly PropertyKey<int> VarIDCounter = new PropertyKey<int>("Var.VarIDCounter", typeof(int), DefaultValueMetadata.Static(0, keepDefaultValue: true));
        private readonly string varName;

        static int GetAndIncIDCount(ShaderGeneratorContext context)
        {
            var result = context.Tags.Get(VarIDCounter);
            context.Tags.Set(VarIDCounter, result + 1);
            return result;
        }

        public Var(IComputeValue<T> value, string name)
            : this(value, null, name)
        { }

        public Var(IComputeValue<T> value, Var<T> var)
            : this(value, var, "Var")
        { }

        public Var(IComputeValue<T> value, Var<T> var = null, string varName = "Var", string semanticName = null, bool appendID = true)
        {
            if (var != null) //re-assign existing var
            {
                appendID = false;
                SemanticName = var.SemanticName;
            }
            else
            {
                createVar = true;
                SemanticName = semanticName;
            }

            ExistingVar = var;
            AppendID = appendID;
            Value = value;
        }

        [DataMemberIgnore]
        public Var<T> ExistingVar { get; }

        public IComputeValue<T> Value { get; }

        public string VarName => ExistingVar?.VarNameWithID ?? varName;

        public string VarNameWithID
        {
            get
            {
                if (!compiled)
                    throw new System.Exception("Var not yet compiled");
                return varNameWithID;
            }

            protected set => varNameWithID = value;
        }

        public bool AppendID { get; }

        public string SemanticName { get; }

        bool createVar;
        private string varNameWithID;
        private bool compiled;

        public override IEnumerable<IComputeNode> GetChildren(object context = null)
        {
            return ReturnIfNotNull(Value);
        }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            VarNameWithID = VarName;

            if (AppendID)
                VarNameWithID = VarName + "_" + GetAndIncIDCount(context);

            compiled = true;

            var shaderSource = SemanticName != null ?
                GetShaderSourceForType<T>("AssignSemantic", VarNameWithID, SemanticName)
                : GetShaderSourceForType<T>("AssignVar", VarNameWithID);

            var mixin = shaderSource.CreateMixin();

            mixin.AddComposition(Value, "Value", context, baseKeys);

            return mixin;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", createVar ? "" : "Assign", VarName);
        }
    }
}
