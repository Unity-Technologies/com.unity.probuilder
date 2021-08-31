using System.Collections;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.ProBuilder.Csg;

namespace UnityEngine.ProBuilder.Tests.Framework
{
    class CsgTests : TemporaryAssetTest
    {
        static Scene s_Scene;

        GameObject m_InputA, m_InputB;

        [OneTimeSetUp]
        public void PrepareSceneView()
        {
            s_Scene = OpenScene($"{TestUtility.testsRootDirectory}/Scenes/CsgTest.unity");
            var root = s_Scene.GetRootGameObjects();

            m_InputA = root[0];
            m_InputB = root[1];
            Assume.That(m_InputA.name, Is.EqualTo("A"));
            Assume.That(m_InputB.name, Is.EqualTo("B"));
        }

        static IEnumerable BooleanOps
        {
            get
            {
                yield return CSG.BooleanOp.Intersection; 
                yield return CSG.BooleanOp.Union;
                yield return CSG.BooleanOp.Subtraction;
            }
        }

        [Test]
        public void BooleanOp_WithReallySmallEpsilon_DoesNotCrash([ValueSource(nameof(BooleanOps))] CSG.BooleanOp op)
        {
            CSG.epsilon = 0.00000001f;
            Assert.DoesNotThrow(() => { CSG.Intersect(m_InputA, m_InputA); });
        }
    }
}