using System.Linq;
using UObject = UnityEngine.Object;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.ProBuilder;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;


namespace UnityEngine.ProBuilder.EditorTests.Editor
{
    public class UVEditorTest
    {
        bool m_OpenedWindow = false;        
        ProBuilderMesh m_cube;

        [SetUp]
        public void Setup()
        {
            // make sure the ProBuilder window is open
            if (ProBuilderEditor.instance == null)
            {
                ProBuilderEditor.MenuOpenWindow();
                m_OpenedWindow = true;
            }

            UVEditor.MenuOpenUVEditor();

            m_cube = ShapeGenerator.CreateShape(ShapeType.Cube);
            UnityEditor.ProBuilder.EditorUtility.InitObject(m_cube);
        }

        [TearDown]
        public void Cleanup()
        {
            // close editor window if we had to open it
            if (m_OpenedWindow && ProBuilderEditor.instance != null)
            {
                ProBuilderEditor.instance.Close();
            }

            UObject.DestroyImmediate(m_cube.gameObject);
        }

        [Test]
        public void UVEditor_Manual_BoxProjection()
        {
            //Select faces
            
            List<Face> selectedFaces = new List<Face>();
            selectedFaces.Add(m_cube.faces[2]);
            selectedFaces.Add(m_cube.faces[4]);
            selectedFaces.Add(m_cube.faces[5]);
            m_cube.SetSelectedFaces(selectedFaces);
            MeshSelection.SetSelection(m_cube.gameObject);
            MeshSelection.OnObjectSelectionChanged();

            foreach(Face f in selectedFaces)
            {
                Assert.That(f.manualUV, Is.EqualTo(false));
            }

            
            //Select faces
            UVEditor.instance.Menu_SetManualUV();

            foreach(Face f in selectedFaces)
            {
                Assert.That(f.manualUV, Is.EqualTo(true));
            }

            //Modify those faces
            Vector2  lowerLeftUV = UVEditor.instance.UVSelectionLowerLeftUV();
            Assert.That(lowerLeftUV, !Is.EqualTo(UVEditor.LowerLeft));

            UVEditor.instance.Menu_BoxProject();
            lowerLeftUV = UVEditor.instance.UVSelectionLowerLeftUV();
            Assert.That(lowerLeftUV, Is.EqualTo(UVEditor.LowerLeft));
        }

        [Test]
        public void UVEditor_Manual_PlanarProjection()
        {
            //Select faces

            List<Face> selectedFaces = new List<Face>();
            selectedFaces.Add(m_cube.faces[2]);
            selectedFaces.Add(m_cube.faces[4]);
            selectedFaces.Add(m_cube.faces[5]);
            m_cube.SetSelectedFaces(selectedFaces);
            MeshSelection.SetSelection(m_cube.gameObject);
            MeshSelection.OnObjectSelectionChanged();

            foreach (Face f in selectedFaces)
            {
                Assert.That(f.manualUV, Is.EqualTo(false));
            }


            //Select faces
            UVEditor.instance.Menu_SetManualUV();

            foreach (Face f in selectedFaces)
            {
                Assert.That(f.manualUV, Is.EqualTo(true));
            }

            //Modify those faces
            Vector2 lowerLeftUV = UVEditor.instance.UVSelectionLowerLeftUV();
            Assert.That(lowerLeftUV, !Is.EqualTo(UVEditor.LowerLeft));

            UVEditor.instance.Menu_PlanarProject();
            lowerLeftUV = UVEditor.instance.UVSelectionLowerLeftUV();
            Assert.That(lowerLeftUV, Is.EqualTo(UVEditor.LowerLeft));
        }
    }
}
