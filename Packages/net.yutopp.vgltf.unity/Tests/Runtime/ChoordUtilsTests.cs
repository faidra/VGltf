﻿//
// Copyright (c) 2019- yutopp (yutopp@gmail.com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at  https://www.boost.org/LICENSE_1_0.txt)
//

using NUnit.Framework;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.IO;
using System;

namespace VGltf.Unity.UnitTests
{
    public class ChordUtilsTests
    {
        [Test]
        public void UVTest()
        {
            var srcUV = new Vector2(0.2f, 0.8f);
            var conv = CoordUtils.ConvertUV(srcUV);
            Assert.That(Asserts.EqualsWithDelta(srcUV, conv), Is.EqualTo(false));

            var dstUV = CoordUtils.ConvertUV(conv);
            Assert.That(Asserts.EqualsWithDelta(srcUV, dstUV), Is.EqualTo(true));
        }

        [Test]
        public void SpaceVector3Test()
        {
            var srcPos = new Vector3(0.2f, 0.8f, -0.1f);
            var conv = CoordUtils.ConvertSpace(srcPos);
            //Assert.False(EqualsWithDelta(srcPos, conv));

            var dstPos = CoordUtils.ConvertSpace(conv);
            Assert.IsTrue(Asserts.EqualsWithDelta(srcPos, dstPos));
        }

        [Test]
        public void SpaceQuaternionTest()
        {
            var srcRot = Quaternion.AngleAxis(30, new Vector3(1, 2, 3));
            var conv = CoordUtils.ConvertSpace(srcRot);
            Assert.That(Asserts.EqualsWithDelta(srcRot, conv), Is.EqualTo(false));

            var dstRot = CoordUtils.ConvertSpace(conv);
            Assert.That(Asserts.EqualsWithDelta(srcRot, dstRot), Is.EqualTo(true));
        }

        [Test]
        [TestCaseSource("TRSElems")]
        public void Matrix4x4Test(Vector3 srcT, Quaternion srcR, Vector3 srcS)
        {
            var srcM = new Matrix4x4();
            srcM.SetTRS(srcT, srcR, srcS);
            Assert.IsTrue(Asserts.EqualsWithDelta(srcM, srcM));

            var conv = CoordUtils.ConvertSpace(srcM);
            var convT = CoordUtils.GetTranslate(conv);
            var convR = CoordUtils.GetRotation(conv);
            var convS = CoordUtils.GetScale(conv);
            Assert.IsTrue(Asserts.EqualsWithDelta(CoordUtils.ConvertSpace(srcT), convT),
                string.Format("Expect = {0}, Actual = {1}", CoordUtils.ConvertSpace(srcT), convT));
            Assert.IsTrue(Asserts.EqualsWithDelta(CoordUtils.ConvertSpace(srcR), convR),
                string.Format("Expect = {0}, Actual = {1}", CoordUtils.ConvertSpace(srcR), convR));
            Assert.IsTrue(Asserts.EqualsWithDelta(srcS, convS),
                string.Format("Expect = {0}, Actual = {1}", srcS, convS));

            var dstM = CoordUtils.ConvertSpace(conv);
            Assert.IsTrue(Asserts.EqualsWithDelta(dstM, srcM));

            var dstT = CoordUtils.GetTranslate(dstM);
            var dstR = CoordUtils.GetRotation(dstM);
            var dstS = CoordUtils.GetScale(dstM);
            Assert.IsTrue(Asserts.EqualsWithDelta(srcT, dstT),
                string.Format("Expect = {0}, Actual = {1}", srcT, dstT));
            Assert.IsTrue(Asserts.EqualsWithDelta(srcR, dstR),
                string.Format("Expect = {0}, Actual = {1}", srcR, dstR));
            Assert.IsTrue(Asserts.EqualsWithDelta(srcS, dstS),
                string.Format("Expect = {0}, Actual = {1}", srcS, dstS));
        }

        public static object[] TRSElems = {
            new object[] {
                new Vector3(1f, 2f, 3f),
                Quaternion.AngleAxis(30, new Vector3(1, 2, 3)),
                Vector3.one,
            },
            new object[] {
                new Vector3(-1f, -2f, -3f),
                Quaternion.AngleAxis(30, new Vector3(1, 2, 3)),
                Vector3.one,
            },
            new object[] {
                new Vector3(1f, 2f, 3f),
                Quaternion.AngleAxis(-30, new Vector3(-1, -2, -3)),
                Vector3.one,
            },
            new object[] {
                new Vector3(-1f, -2f, -3f),
                Quaternion.AngleAxis(-30, new Vector3(-1, -2, -3)),
                Vector3.one,
            },
        };
    }

    public class Asserts
    {
        class AssertionFailedException : Exception
        {
            public AssertionFailedException(string message) : base(message)
            {
            }
        }

        public static bool EqualsWithDelta(Vector2 a, Vector2 b)
        {
            return Mathf.Approximately(a[0], b[0])
                && Mathf.Approximately(a[1], b[1]);
        }

        public static bool EqualsWithDelta(Vector3 a, Vector3 b)
        {
            return Mathf.Approximately(a[0], b[0])
                && Mathf.Approximately(a[1], b[1])
                && Mathf.Approximately(a[2], b[2]);
        }

        public static bool EqualsWithDelta(Vector4 a, Vector4 b)
        {
            return Mathf.Approximately(a[0], b[0])
                && Mathf.Approximately(a[1], b[1])
                && Mathf.Approximately(a[2], b[2])
                && Mathf.Approximately(a[3], b[3]);
        }

        public static bool EqualsWithDelta(Quaternion a, Quaternion b)
        {
            return Mathf.Approximately(a[0], b[0])
                && Mathf.Approximately(a[1], b[1])
                && Mathf.Approximately(a[2], b[2])
                && Mathf.Approximately(a[3], b[3]);
        }

        public static bool EqualsWithDelta(Matrix4x4 a, Matrix4x4 b)
        {
            return Mathf.Approximately(a[0, 0], b[0, 0])
                && Mathf.Approximately(a[0, 1], b[0, 1])
                && Mathf.Approximately(a[0, 2], b[0, 2])
                && Mathf.Approximately(a[0, 3], b[0, 3])

                && Mathf.Approximately(a[1, 0], b[1, 0])
                && Mathf.Approximately(a[1, 1], b[1, 1])
                && Mathf.Approximately(a[1, 2], b[1, 2])
                && Mathf.Approximately(a[1, 3], b[1, 3])

                && Mathf.Approximately(a[2, 0], b[2, 0])
                && Mathf.Approximately(a[2, 1], b[2, 1])
                && Mathf.Approximately(a[2, 2], b[2, 2])
                && Mathf.Approximately(a[2, 3], b[2, 3])

                && Mathf.Approximately(a[3, 0], b[3, 0])
                && Mathf.Approximately(a[3, 1], b[3, 1])
                && Mathf.Approximately(a[3, 2], b[3, 2])
                && Mathf.Approximately(a[3, 3], b[3, 3]);
        }

        public static bool EqualsWithDelta(BoneWeight a, BoneWeight b)
        {
            if (a.boneIndex0 != b.boneIndex0) return false;
            if (a.boneIndex1 != b.boneIndex1) return false;
            if (a.boneIndex2 != b.boneIndex2) return false;
            if (a.boneIndex3 != b.boneIndex3) return false;

            if (!EqualsWithDelta(
                new Vector4(a.weight0, a.weight1, a.weight2, a.weight3),
                new Vector4(b.weight0, b.weight1, b.weight2, b.weight3)))
            {
                return false;
            }

            return true;
        }

        public static bool EqualsWithDelta(Transform a, Transform b)
        {
            if (!EqualsWithDelta(a.position, b.position))
            {
                return false;
            }

            if (!EqualsWithDelta(a.rotation, b.rotation))
            {
                return false;
            }

            if (!EqualsWithDelta(a.lossyScale, b.lossyScale))
            {
                return false;
            }

            return true;
        }

        public static void AssertEqualsWithDelta(Transform[] a, Transform[] b)
        {
            if (a.Length != b.Length)
            {
                // TODO: fix
                throw new NotImplementedException();
            }

            for (int i = 0; i < a.Length; ++i)
            {
                var av = a[i];
                var bv = b[i];
                if (!EqualsWithDelta(av, bv))
                {
                    throw new NotImplementedException($"Index {i}: {av} != {bv}");
                }
            }
        }

        public static void AssertEqualsWithDelta(Vector3[] a, Vector3[] b)
        {
            if (a.Length != b.Length)
            {
                throw new AssertionFailedException(
                    string.Format("Length not matched: {0} != {1}", a.Length, b.Length)
                    );
            }

            for (int i = 0; i < a.Length; ++i)
            {
                var av = a[i];
                var bv = b[i];
                if (!EqualsWithDelta(av, bv))
                {
                    throw new AssertionFailedException(
                        string.Format("Index {0}: {1} != {2}", i, av, bv)
                        );
                }
            }
        }

        public static void AssertEqualsWithDelta(BoneWeight[] a, BoneWeight[] b)
        {
            if (a.Length != b.Length)
            {
                throw new AssertionFailedException(
                    string.Format("Length not matched: {0} != {1}", a.Length, b.Length)
                    );
            }

            for (int i = 0; i < a.Length; ++i)
            {
                var av = a[i];
                var bv = b[i];
                if (!EqualsWithDelta(av, bv))
                {
                    throw new AssertionFailedException(
                        string.Format("Index {0}: {1} != {2}", i, av, bv)
                        );
                }
            }
        }

        public static void AssertEqualsWithDelta(Matrix4x4[] a, Matrix4x4[] b)
        {
            if (a.Length != b.Length)
            {
                throw new AssertionFailedException(
                    string.Format("Length not matched: {0} != {1}", a.Length, b.Length)
                    );
            }

            for (int i = 0; i < a.Length; ++i)
            {
                var av = a[i];
                var bv = b[i];
                if (!EqualsWithDelta(av, bv))
                {
                    throw new AssertionFailedException(
                        string.Format("Index {0}: {1} != {2}", i, av, bv)
                        );
                }
            }
        }
    }
}
