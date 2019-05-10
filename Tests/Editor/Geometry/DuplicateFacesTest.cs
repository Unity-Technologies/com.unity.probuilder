using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEditor;
using UnityEngine.TestTools;


namespace UnityEngine.ProBuilder.EditorTests.Geometry
{
    class DuplicateFacesTest
    {
        ProBuilderMesh[] selectables;

        [SetUp]
        public void Setup()
        {
            ProBuilderMesh shape1 = ShapeGenerator.CreateShape(ShapeType.Cube);
            shape1.transform.position = Vector3.zero - shape1.GetComponent<MeshRenderer>().bounds.center;

            selectables = new ProBuilderMesh[]
            {
                shape1
            };
        }

        [TearDown]
        public void Cleanup()
        {
            for (int i = 0; i < selectables.Length; i++)
            {
                UObject.DestroyImmediate(selectables[i].gameObject);
            }
        }

        [Test]
        public void DuplicateFaces_ToObject()
        {
            //Make first faces selected
            ProBuilderMesh mesh = selectables[0];
            Assert.IsNotNull(mesh.faces);
            Face face = selectables[0].faces[0];
            List<Face> selectedFaces = new List<Face>();
            selectedFaces.Add(face);
            mesh.SetSelectedFaces(selectedFaces);
            Assert.AreEqual(mesh.selectedFaceCount, 1);
            MeshSelection.SetSelection(mesh.gameObject);
            MeshSelection.OnObjectSelectionChanged();

            
            UnityEditor.ProBuilder.Actions.DuplicateFaces duplicateFaces = new UnityEditor.ProBuilder.Actions.DuplicateFaces();
            ProBuilderSettings.Set<UnityEditor.ProBuilder.Actions.DuplicateFaces.DuplicateFaceSetting>("DuplicateFaces.target", UnityEditor.ProBuilder.Actions.DuplicateFaces.DuplicateFaceSetting.GameObject);
            duplicateFaces.DoAction();

            //selectable object should keep all faces selected
            Assert.AreEqual(selectables[0].faces.Count, 6);

            Assert.AreEqual(MeshSelection.selectedObjectCount, 1);
            Assert.AreNotEqual(UnityEditor.Selection.objects[0], mesh.gameObject);

            //This needs to be called explicitly in the case of unit test so that the internal representation of ProBuilder MeshSelection
            //gets updated prior to accessing it
            MeshSelection.OnObjectSelectionChanged();
            ProBuilderMesh newMesh = MeshSelection.activeMesh;
            Assert.AreEqual(newMesh.faces.Count, 1);
        }

        [Test]
        public void DuplicateFaces_ToSubmesh()
        {
            //Make first faces selected
            ProBuilderMesh mesh = selectables[0];
            Assert.IsNotNull(mesh.faces);
            Face face = selectables[0].faces[0];
            List<Face> selectedFaces = new List<Face>();
            selectedFaces.Add(face);
            mesh.SetSelectedFaces(selectedFaces);
            Assert.AreEqual(mesh.selectedFaceCount, 1);
            MeshSelection.SetSelection(mesh.gameObject);
            MeshSelection.OnObjectSelectionChanged();


            UnityEditor.ProBuilder.Actions.DuplicateFaces duplicateFaces = new UnityEditor.ProBuilder.Actions.DuplicateFaces();
            ProBuilderSettings.Set<UnityEditor.ProBuilder.Actions.DuplicateFaces.DuplicateFaceSetting>("DuplicateFaces.target", UnityEditor.ProBuilder.Actions.DuplicateFaces.DuplicateFaceSetting.Submesh);
            duplicateFaces.DoAction();

            //All selectable object should have all faces selected
            Assert.AreEqual(selectables[0].faces.Count, 7);

            Assert.AreEqual(MeshSelection.selectedObjectCount, 1);
            Assert.AreEqual(UnityEditor.Selection.objects[0], mesh.gameObject);
        }
    }
}

