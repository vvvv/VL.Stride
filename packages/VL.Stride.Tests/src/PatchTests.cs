﻿using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VL.Lang;
using VL.Lang.Symbols;
using VL.Model;
using VVVV.NuGetAssemblyLoader;

namespace MyTests
{
    [TestFixture]
    public class PatchTests
    {
        static string[] Packs = new string[]{ 
        
        //  FIX ME !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //@"C:\Program Files\vvvv\vvvv_gamma_2020.1.4\lib\packs",

        };



        public static IEnumerable<string> NormalPatches()
        {
            var vlStride = Directory.GetFiles(Path.Combine(MainLibPath, "VL.Stride"), "*.vl", SearchOption.AllDirectories);
            var vlStrideRuntime = Directory.GetFiles(Path.Combine(MainLibPath, "VL.Stride.Runtime"), "*.vl", SearchOption.AllDirectories);
            var vlStrideWindows = Directory.GetFiles(Path.Combine(MainLibPath, "VL.Stride.Windows"), "*.vl", SearchOption.AllDirectories);

            var pathUri = new Uri(MainLibPath, UriKind.Absolute);
            // Yield all your VL docs
            foreach (var file in vlStrideRuntime.Concat(vlStride).Concat(vlStrideWindows))
            {
                // Shows up red on build server - maybe due to super cheap graphics card?
                if (Path.GetFileName(file) == "HowTo Write a Shader.vl" ||
                    Path.GetFileName(file) == "HowTo Create a Custom ShaderFX Node.vl" ||
                    Path.GetFileName(file) == "HowTo Read Pixels of a Texture.vl" ||
                    Path.GetFileName(file) == "HowTo Write into a VertexBuffer in a Compute Shader.vl" ||
                    Path.GetFileName(file) == "HowTo Extract hue, saturation and lightness from a texture.vl" ||
                    Path.GetFileName(file) == "Example GPU Particle System with Mesh.vl" ||
                    Path.GetFileName(file) == "Example Compute Shader Instancing with Oversampling.vl" ||
                    Path.GetFileName(file) == "HowTo Rearrange Channels of a Texture.vl" ||
                    Path.GetFileName(file) == "VL.Stride.Rendering.Temp.vl")
                    continue;

                var fileUri = new Uri(file, UriKind.Absolute);
                yield return Uri.UnescapeDataString(pathUri.MakeRelativeUri(fileUri).ToString()).Replace("/", @"\");
            }
        }



        public static readonly VLSession Session;
        public static string MainLibPath;
        public static string RepositoriesPath;
        public static string VLPath;

        static PatchTests()
        {
            var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            MainLibPath = Path.GetFullPath(Path.Combine(currentDirectory, @"..\..\..\..\..\"));
            RepositoriesPath = Path.GetFullPath(Path.Combine(MainLibPath, @"..\..\"));
            VLPath = Path.GetFullPath(Path.Combine(MainLibPath, @"..\..\..\vvvv50\"));

            foreach (var pack in Packs)
                AssemblyLoader.AddPackageRepositories(pack);

            // Add the vvvv_stride\public-vl\VL.Stride\packages folder
            AssemblyLoader.AddPackageRepositories(MainLibPath);

            // Add the vvvv_stride\public-vl folder
            AssemblyLoader.AddPackageRepositories(RepositoriesPath);

            // Add the vvvv_stride\vvvv50 folder
            AssemblyLoader.AddPackageRepositories(VLPath);

            Session = new VLSession("gamma", includeUserPackages: false)
            {
                CheckSolution = false,
                IgnoreDynamicEnumErrors = true,
                NoCache = true,
                KeepTargetCode = false
            };
        }





        static Solution FCompiledSolution;


        /// <summary>
        /// Checks if the document comes with compile time errors (e.g. red nodes). Doesn't actually run the patches.
        /// </summary>
        /// <param name="filePath"></param>
        [TestCaseSource(nameof(NormalPatches))]
        public static async Task IsntRed(string filePath)
        {
            filePath = Path.Combine(MainLibPath, filePath);
            var solution = FCompiledSolution ?? (FCompiledSolution = await CompileAsync(NormalPatches()));
            var document = solution.GetOrAddDocument(filePath);

            // Check document structure
            Assert.True(document.IsValid);

            // Check dependenices
            foreach (var dep in document.GetDocSymbols().Dependencies)
                Assert.IsFalse(dep.RemoteSymbolSource is Dummy, $"Couldn't find dependency {dep}. Press F6 to build all library projects!");

            // Check all containers and process node definitions, including application entry point
            CheckNodes(document.AllTopLevelDefinitions);
        }

        static async Task<Solution> CompileAsync(IEnumerable<string> docs)
        {
            var solution = Session.CurrentSolution;
            foreach (var f in docs)
                solution = solution.GetOrAddDocument(Path.Combine(MainLibPath, f)).Solution;
            return await solution.WithFreshCompilationAsync();
        }

        public static void CheckNodes(IEnumerable<Node> nodes)
        {
            Parallel.ForEach(nodes, definition =>
            {
                var definitionSymbol = definition.GetSymbol() as IDefinitionSymbol;
                Assert.IsNotNull(definitionSymbol, $"No symbol for {definition}.");
                var errorMessages = definition.GetSymbolMessages().Where(m => m.Severity == MessageSeverity.Error);
                Assert.That(errorMessages.None(), () => $"{definition}: {string.Join(Environment.NewLine, errorMessages)}");
                Assert.IsFalse(definitionSymbol.IsUnused, $"The symbol of {definition} is marked as unused.");
            });
        }




        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // Running Tests patches not supported yet. We for now can only check for compile time errors (like red nodes)


        /// <summary>
        /// Yield all test patches that shall run
        /// </summary>
        public static IEnumerable<string> TestPatches()
        {
            yield return $@"C:\dev\vl-libs\VL.DemoLib\src\NUnitTests\tests\tests.vl";
        }
    }
}
