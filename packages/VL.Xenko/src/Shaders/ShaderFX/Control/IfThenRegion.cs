using System;
using System.Collections.Generic;
using System.Text;
using Xenko.Rendering.Materials;
using Xenko.Shaders;
using static VL.Xenko.Shaders.ShaderFX.ShaderFXUtils;

namespace VL.Xenko.Shaders.ShaderFX.Control
{
    public class IfThenRegionBuilder
    {
        public void Build()
        {
            var linksIntoRegion = new List<IComputeNode>();
            var linksToBorderCP = new List<IComputeNode>();
            var linksToInputPins = new List<IComputeNode>();

            //init accum1
            var accum1InitValue = new Constant<float>(-1);

            var getAccum1InitValue = accum1InitValue.Getter();
            var accum1 = CreateAndInitVar("Accum", getAccum1InitValue);

            //body patch begin ----

            var bodyConstant1 = new Constant<float>(1);
            var bodyConstant2 = new Constant<float>(2);

            var getPlusIn1 = bodyConstant1.Getter();
            var getPlusIn2 = bodyConstant2.Getter();
            var plusExpression = new BinaryOperation<float>("Plus", getPlusIn1, getPlusIn2);
            var plusResult = CreateAndInitVar("PlusResult", plusExpression);

            //re-assign accumulator 1
            var getPlusResult = plusResult.Getter();
            var accum1ReAssign = accum1.AssignVar(getPlusResult);

            //body patch end ----

            //build body
            var bodyStatements = ShaderGraph.BuildFinalShaderGraph(accum1ReAssign, excludes: linksIntoRegion);

            //actual if expression
            //condition
            var conditionValue = new Constant<bool>(true);

            var getConditionValue = conditionValue.Getter();
            var ifThenRegion = new IfThenRegion(bodyStatements, getConditionValue, GenerateShaderSource, GetChildren);

            //make sure accums are initialized before the region is called
            var finalStatements = new ComputeOrder(accum1, ifThenRegion);

            //outputs
            var regionBorderOutputs = new[] { accum1 };

        }

        public ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            var shaderClassSource = new ShaderClassSource("IfThen");

            var mixin = shaderClassSource.CreateMixin();

            //mixin.AddComposition(then, "Then", context, baseKeys);
            //mixin.AddComposition(condtion, "Condition", context, baseKeys);

            return mixin;
        }

        public IEnumerable<IComputeNode> GetChildren(object context = null)
        {
            return GetInputLinks();
        }

        private IEnumerable<IComputeNode> GetInputLinks()
        {
            throw new NotImplementedException();
        }
    }

    public class IfThenRegion : ComputeVoid
    {
        public IfThenRegion(IComputeVoid then, 
            IComputeValue<bool> condition,
            Func<ShaderGeneratorContext, MaterialComputeColorKeys, ShaderSource> generateShader,
            Func<object, IEnumerable<IComputeNode>> generateChildren)
        {
            Then = then;
            Condtion = condition;
            GenerateShader = generateShader;
            GenerateChildren = generateChildren;
        }

        Func<ShaderGeneratorContext, MaterialComputeColorKeys, ShaderSource> GenerateShader { get; }
        Func<object, IEnumerable<IComputeNode>> GenerateChildren { get; }

        public IComputeVoid Then { get; }
        public IComputeValue<bool> Condtion { get; }

        public override ShaderSource GenerateShaderSource(ShaderGeneratorContext context, MaterialComputeColorKeys baseKeys)
        {
            //var shaderClassSource = new ShaderClassSource("IfThen");

            //var mixin = shaderClassSource.CreateMixin();

            //mixin.AddComposition(Then, "Then", context, baseKeys);
            //mixin.AddComposition(Condtion, "Condition", context, baseKeys);

            //return mixin;
            return GenerateShader(context, baseKeys);
        }

        public override IEnumerable<IComputeNode> GetChildren(object context = null)
        {
            return GenerateChildren(context);
        }
    }
}
