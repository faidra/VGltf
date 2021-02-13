//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace VGltf.UnitTests
{
    using VJson.Schema;

    public class GltfContainerTests
    {
        [Test]
        [TestCaseSource("GltfArgs")]
        public void FromGltfTest(string[] modelPath, ModelTester.IModelTester tester)
        {
            var path = modelPath.Aggregate("SampleModels", (b, p) => Path.Combine(b, p));
            using (var fs = new FileStream(path, FileMode.Open))
            {
                var c = GltfContainer.FromGltf(fs);

                var schema = VJson.Schema.JsonSchemaAttribute.CreateFromClass<Types.Gltf>();
                var ex = schema.Validate(c.Gltf);
                Assert.Null(ex);

                var storageDir = Directory.GetParent(path).ToString();
                var loader = new ResourceLoaderFromFileStorage(storageDir);

                var store = new ResourcesStore(c.Gltf, c.Buffer, loader);
                tester.TestModel(store);
            }
        }

        [Test]
        [TestCaseSource("GlbArgs")]
        public void FromGlbTest(string[] modelPath, ModelTester.IModelTester tester)
        {
            var path = modelPath.Aggregate("SampleModels", (b, p) => Path.Combine(b, p));
            using (var fs = new FileStream(path, FileMode.Open))
            {
                var c = GltfContainer.FromGlb(fs);

                var schema = VJson.Schema.JsonSchemaAttribute.CreateFromClass<Types.Gltf>();
                var ex = schema.Validate(c.Gltf);
                Assert.Null(ex);

                var loader = new ResourceLoaderFromEmbedOnly(); // Glb files should be packed.

                var store = new ResourcesStore(c.Gltf, c.Buffer, loader);
                tester.TestModel(store);
            }
        }

        public static object[] GltfArgs = {
            new object[] {
                new string[] {"SimpleSparseAccessor", "glTF-Embedded", "SimpleSparseAccessor.gltf"},
                new ModelTester.SimpleSparseAccessorTester(),
            },

            new object[] {
                new string[] {"BoxTextured", "glTF-Embedded", "BoxTextured.gltf"},
                new ModelTester.BoxTexturedTester(),
            },

            new object[] {
                new string[] {"BoxTextured", "glTF", "BoxTextured.gltf"},
                new ModelTester.BoxTexturedTester(),
            },

            new object[] {
                new string[] {"RiggedSimple", "glTF-Embedded", "RiggedSimple.gltf"},
                new ModelTester.RiggedSimpleTester(),
            },
        };

        public static object[] GlbArgs = {
            new object[] {
                new string[] {"Alicia", "VRM", "AliciaSolid.vrm"},
                new ModelTester.AliciaSolidTester(),
            },

            new object[] {
                new string[] {"ShinchokuRobo", "shinchoku_robo.vrm"},
                new ModelTester.ShinchokuRoboTester(),
            },
        };
    }
}
