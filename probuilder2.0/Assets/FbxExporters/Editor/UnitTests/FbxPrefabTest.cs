using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace FbxExporters.UnitTests
{
    public class FbxPrefabTest : ExporterTestBase
    {
        GameObject m_original; // stored for testing
        FbxPrefab.FbxRepresentation m_originalRep;

        GameObject m_source; // the fbx model
        GameObject m_autoPrefab; // prefab that auto-updates
        GameObject m_manualPrefab; // prefab that doesn't auto-update

        public static void AssertAreIdentical(
                FbxPrefab.FbxRepresentation a,
                FbxPrefab.FbxRepresentation b) {
            Assert.AreEqual(a.ToJson(), b.ToJson());
        }

        public static void AssertAreDifferent(
                FbxPrefab.FbxRepresentation a,
                FbxPrefab.FbxRepresentation b) {
            Assert.AreNotEqual(a.ToJson(), b.ToJson());
        }

        [SetUp]
        public void Init() {
            // Create a test hierarchy. It has unique names.
            m_original = CreateHierarchy("FbxPrefabTestRoot");
            m_originalRep = Rep(m_original);

            // Convert it to FBX. The asset file will be deleted automatically
            // on termination.
            var fbxAsset = FbxExporters.Editor.ModelExporter.ExportObject(
                    GetRandomFbxFilePath(), m_original);
            m_source = AssetDatabase.LoadMainAssetAtPath(fbxAsset) as GameObject;
            Assert.IsTrue(m_source);

            // Create an FbxPrefab linked to the Fbx file. Make it auto-update.
            {
                var prefabInstance = GameObject.Instantiate(m_original);
                var fbxPrefab = prefabInstance.AddComponent<FbxPrefab>();
                fbxPrefab.SetSourceModel(m_source);
                fbxPrefab.SetAutoUpdate(true);
                m_autoPrefab = PrefabUtility.CreatePrefab(
                        GetRandomPrefabAssetPath(),
                        prefabInstance);
            }

            // Create an FbxPrefab linked to the same Fbx file. Make it NOT auto-update.
            {
                var prefabInstance = GameObject.Instantiate(m_original);
                var fbxPrefab = prefabInstance.AddComponent<FbxPrefab>();
                fbxPrefab.SetSourceModel(m_source);
                fbxPrefab.SetAutoUpdate(false);
                m_manualPrefab = PrefabUtility.CreatePrefab(
                        GetRandomPrefabAssetPath(),
                        prefabInstance);
            }
        }

        FbxPrefab.FbxRepresentation Rep(GameObject go) {
            return FbxPrefab.FbxRepresentation.FromTransform(go.transform);
        }

        FbxPrefab.FbxRepresentation History(GameObject go) {
            return go.GetComponent<FbxPrefab>().GetFbxHistory();
        }

        GameObject ModifySourceFbx()
        {

            // Modify the source fbx file:
            // - delete parent1
            // - add parent3
            var newModel = PrefabUtility.InstantiatePrefab(m_source) as GameObject;
            GameObject.DestroyImmediate(newModel.transform.Find("Parent1").gameObject);
            CreateGameObject("Parent3", newModel.transform);

            // Export it to clobber the old FBX file.
            // Sleep one second first to make sure the timestamp differs
            // enough, so the asset database knows to reload it. I was getting
            // test failures otherwise.
            System.Threading.Thread.Sleep(1000);
            FbxExporters.Editor.ModelExporter.ExportObjects (
                    AssetDatabase.GetAssetPath(m_source),
                    new Object[] { newModel } );
            AssetDatabase.Refresh();

            return newModel;
        }

        [Test]
        public void BasicTest() {
            // Check the history is good at the start
            AssertAreIdentical(m_originalRep, Rep(m_manualPrefab));
            AssertAreIdentical(m_originalRep, Rep(m_autoPrefab));
            AssertAreIdentical(m_originalRep, History(m_manualPrefab));
            AssertAreIdentical(m_originalRep, History(m_autoPrefab));

            var newHierarchy = Rep(ModifySourceFbx());
            AssertAreDifferent(m_originalRep, newHierarchy);

            // Make sure the fbx source changed.
            AssertAreDifferent(m_originalRep, Rep(m_source));
            AssertAreIdentical(newHierarchy, Rep(m_source));

            // Make sure the auto-update prefab changed.
            AssertAreIdentical(newHierarchy, Rep(m_autoPrefab));
            AssertAreIdentical(newHierarchy, History(m_autoPrefab));

            // Make sure the manual-update prefab didn't.
            AssertAreIdentical(m_originalRep, Rep(m_manualPrefab));
            AssertAreIdentical(m_originalRep, History(m_manualPrefab));

            // Manual update, make sure it updated.
            var manualPrefabComponent = m_manualPrefab.GetComponent<FbxPrefab>();
            manualPrefabComponent.SyncPrefab();
            AssertAreIdentical(newHierarchy, Rep(m_manualPrefab));
            AssertAreIdentical(newHierarchy, History(m_manualPrefab));

            // Check some corner cases.
            Assert.AreEqual(m_source, manualPrefabComponent.GetFbxAsset());

            // Illegal to set the source model to something that isn't an
            // asset.
            var go = CreateGameObject("foo");
            Assert.That( () => manualPrefabComponent.SetSourceModel(go), Throws.Exception );

            // Illegal to set the source model to something that isn't an fbx
            // asset (it's a prefab).
            Assert.That( () => manualPrefabComponent.SetSourceModel(m_autoPrefab), Throws.Exception );

            // Legal to set the source model to null. It doesn't change the
            // hierarchy or anything.
            Assert.That( () => manualPrefabComponent.SetSourceModel(null), Throws.Nothing );
            Assert.IsNull(manualPrefabComponent.GetFbxAsset());
            AssertAreIdentical(newHierarchy, Rep(m_manualPrefab));
            AssertAreIdentical(newHierarchy, History(m_manualPrefab));
            Assert.That( () => manualPrefabComponent.SyncPrefab(), Throws.Nothing );
            AssertAreIdentical(newHierarchy, Rep(m_manualPrefab));
            AssertAreIdentical(newHierarchy, History(m_manualPrefab));

            // Switch to some other model, which looks like the original model
            // (but is a totally different file). This will cause an update
            // immediately.
            var fbxAsset = FbxExporters.Editor.ModelExporter.ExportObject(
                    GetRandomFbxFilePath(), m_original);
            var newSource = AssetDatabase.LoadMainAssetAtPath(fbxAsset) as GameObject;
            Assert.IsTrue(newSource);
            manualPrefabComponent.SetSourceModel(newSource);
            AssertAreIdentical(m_originalRep, Rep(m_manualPrefab));
            AssertAreIdentical(m_originalRep, History(m_manualPrefab));
        }

        [Test]
        public void ManualToAuto() {
            // Check what happens when we go from manual to auto-update.
            var newHierarchy = Rep(ModifySourceFbx());
            AssertAreIdentical(m_originalRep, Rep(m_manualPrefab));
            AssertAreIdentical(m_originalRep, History(m_manualPrefab));

            m_manualPrefab.GetComponent<FbxPrefab>().SetAutoUpdate(false);
            AssertAreIdentical(m_originalRep, Rep(m_manualPrefab));
            AssertAreIdentical(m_originalRep, History(m_manualPrefab));

            m_manualPrefab.GetComponent<FbxPrefab>().SetAutoUpdate(true);
            AssertAreIdentical(newHierarchy, Rep(m_manualPrefab));
            AssertAreIdentical(newHierarchy, History(m_manualPrefab));
        }
    }
}
