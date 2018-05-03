using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEditor;
using UnityEngine.TestTools;

namespace UnityEngine.ProBuilder.EditorTests.Picking
{
	class RectSelection
	{
		ProBuilderMesh[] selectables;
		Camera camera;

		void Setup()
		{
			camera = new GameObject("Camera", typeof(Camera)).GetComponent<Camera>();
			camera.transform.position = new Vector3(.3f, 2.2f, -3f);

			ProBuilderMesh shape = ShapeGenerator.CreateShape(ShapeType.Torus);
			shape.transform.position = Vector3.zero - shape.GetComponent<MeshRenderer>().bounds.center;

			camera.transform.LookAt(shape.transform);

			selectables = new ProBuilderMesh[]
			{
				shape
			};
		}

		void Cleanup()
		{
			for (int i = 0; i < selectables.Length; i++)
			{
				UObject.DestroyImmediate(selectables[i].gameObject);
			}

			UObject.DestroyImmediate(camera.gameObject);
		}

		Dictionary<ProBuilderMesh, HashSet<int>> TestVertexPick(PickerOptions options)
		{
			try
			{
				Rect selectionRect = new Rect(camera.pixelRect);
				selectionRect.width /= EditorGUIUtility.pixelsPerPoint;
				selectionRect.height /= EditorGUIUtility.pixelsPerPoint;

				var vertices = UnityEngine.ProBuilder.Picking.PickVerticesInRect(
					camera,
					selectionRect,
					selectables,
					options,
					EditorGUIUtility.pixelsPerPoint);

				LogAssert.NoUnexpectedReceived();

				return vertices;
			}
			catch
			{
				return null;
			}
		}

		Dictionary<ProBuilderMesh, HashSet<Edge>> TestEdgePick(PickerOptions options)
		{
			try
			{
				Rect selectionRect = new Rect(camera.pixelRect);
				selectionRect.width /= EditorGUIUtility.pixelsPerPoint;
				selectionRect.height /= EditorGUIUtility.pixelsPerPoint;

				var edges = UnityEngine.ProBuilder.Picking.PickEdgesInRect(
					camera,
					selectionRect,
					selectables,
					options,
					EditorGUIUtility.pixelsPerPoint);

				LogAssert.NoUnexpectedReceived();

				return edges;
			}
			catch(System.Exception e)
			{
				Debug.LogError(e.ToString());
				return null;
			}
		}

		Dictionary<ProBuilderMesh, HashSet<Face>> TestFacePick(PickerOptions options)
		{
			try
			{
				Rect selectionRect = new Rect(camera.pixelRect);
				selectionRect.width /= EditorGUIUtility.pixelsPerPoint;
				selectionRect.height /= EditorGUIUtility.pixelsPerPoint;

				var faces = UnityEngine.ProBuilder.Picking.PickFacesInRect(
					camera,
					selectionRect,
					selectables,
					options,
					EditorGUIUtility.pixelsPerPoint);

				LogAssert.NoUnexpectedReceived();

				return faces;
			}
			catch(System.Exception e)
			{
				Debug.LogError(e.ToString());
				return null;
			}
		}

		[Test]
		public void PickVertices_DepthTestOn()
		{
			Setup();
			var vertices = TestVertexPick(new PickerOptions() { depthTest = true });
			var selection = vertices.FirstOrDefault();
			Assert.IsNotNull(selection);
			HashSet<int> selectedElements = selection.Value;
			Assert.Less(selectedElements.Count, selection.Key.sharedIndicesInternal.Length);
			Assert.Greater(selectedElements.Count, 0);
			Cleanup();
		}

		[Test]
		public void PickVertices_DepthTestOff()
		{
			Setup();
			var vertices = TestVertexPick(new PickerOptions() { depthTest = false });
			var selection = vertices.FirstOrDefault();
			Assert.IsNotNull(selection);
			HashSet<int> selectedElements = selection.Value;
			Assert.AreEqual(selectedElements.Count, selection.Key.sharedIndicesInternal.Length);
			Cleanup();
		}

		[Test]
		public void PickEdges_DepthTestOff_RectSelectPartial()
		{
			Setup();
			var edges = TestEdgePick(new PickerOptions() { depthTest = false, rectSelectMode = RectSelectMode.Partial });
			Assert.IsNotNull(edges, "Selection is null");
			var selection = edges.FirstOrDefault();
			Assert.IsNotNull(selection, "Selection is null");
			HashSet<Edge> selectedElements = selection.Value;
			Assert.Greater(selectedElements.Count, 0);

			Dictionary<int, int> commonLookup = selection.Key.sharedIndicesInternal.ToDictionary();
			var allEdges = EdgeLookup.GetEdgeLookupHashSet(selection.Key.facesInternal.SelectMany(x => x.edgesInternal), commonLookup);
			var selectedEdges = EdgeLookup.GetEdgeLookupHashSet(selectedElements, commonLookup);
			Assert.AreEqual(allEdges.Count, selectedEdges.Count);

			Cleanup();
		}

		[Test]
		public void PickEdges_DepthTestOn_RectSelectPartial()
		{
			Setup();
			var edges = TestEdgePick(new PickerOptions() { depthTest = true, rectSelectMode = RectSelectMode.Partial });
			var selection = edges.FirstOrDefault();
			Assert.IsNotNull(selection);
			HashSet<Edge> selectedElements = selection.Value;
			Assert.Greater(selectedElements.Count, 0);
			Assert.Less(selectedElements.Count, selection.Key.facesInternal.Sum(x=>x.edgesInternal.Length));

			Cleanup();
		}

		[Test]
		public void PickEdges_DepthTestOff_RectSelectComplete()
		{
			Setup();
			var edges = TestEdgePick(new PickerOptions() { depthTest = false, rectSelectMode = RectSelectMode.Complete });
			Assert.IsNotNull(edges, "Selection is null");
			var selection = edges.FirstOrDefault();
			Assert.IsNotNull(selection, "Selection is null");
			HashSet<Edge> selectedElements = selection.Value;
			Assert.Greater(selectedElements.Count, 0);

			Dictionary<int, int> commonLookup = selection.Key.sharedIndicesInternal.ToDictionary();
			var allEdges = EdgeLookup.GetEdgeLookupHashSet(selection.Key.facesInternal.SelectMany(x => x.edgesInternal), commonLookup);
			var selectedEdges = EdgeLookup.GetEdgeLookupHashSet(selectedElements, commonLookup);
			Assert.AreEqual(allEdges.Count, selectedEdges.Count);

			Cleanup();
		}

		[Test]
		public void PickEdges_DepthTestOn_RectSelectComplete()
		{
			Setup();
			var edges = TestEdgePick(new PickerOptions() { depthTest = true, rectSelectMode = RectSelectMode.Complete });
			var selection = edges.FirstOrDefault();
			Assert.IsNotNull(selection);
			HashSet<Edge> selectedElements = selection.Value;
			Assert.Greater(selectedElements.Count, 0);
			Assert.Less(selectedElements.Count, selection.Key.facesInternal.Sum(x=>x.edgesInternal.Length));

			Cleanup();
		}

		[Test]
		public void PickFaces_DepthTestOff_RectSelectPartial()
		{
			Setup();
			var faces = TestFacePick(new PickerOptions() { depthTest = false, rectSelectMode = RectSelectMode.Partial });
			Assert.IsNotNull(faces, "Selection is null");
			var selection = faces.FirstOrDefault();
			Assert.IsNotNull(selection, "Selection is null");
			HashSet<Face> selectedElements = selection.Value;
			Assert.Greater(selectedElements.Count, 0);
			Assert.AreEqual(selection.Key.faceCount, selectedElements.Count);
			Cleanup();
		}

		[Test]
		public void PickFaces_DepthTestOn_RectSelectPartial()
		{
			Setup();
			var faces = TestFacePick(new PickerOptions() { depthTest = true, rectSelectMode = RectSelectMode.Partial });
			Assert.IsNotNull(faces, "Face pick returned null");
			var selection = faces.FirstOrDefault();
			Assert.IsNotNull(selection);
			HashSet<Face> selectedElements = selection.Value;
			Assert.Greater(selectedElements.Count, 0);
			Assert.Less(selectedElements.Count, selection.Key.faceCount);

			Cleanup();
		}

		[Test]
		public void PickFaces_DepthTestOff_RectSelectComplete()
		{
			Setup();
			var faces = TestFacePick(new PickerOptions() { depthTest = false, rectSelectMode = RectSelectMode.Complete });
			Assert.IsNotNull(faces, "Selection is null");
			var selection = faces.FirstOrDefault();
			Assert.IsNotNull(selection, "Selection is null");
			HashSet<Face> selectedElements = selection.Value;
			Assert.Greater(selectedElements.Count, 0);
			Assert.AreEqual(selection.Key.faceCount, selectedElements.Count);
			Cleanup();
		}

		[Test]
		public void PickFaces_DepthTestOn_RectSelectComplete()
		{
			Setup();
			var faces = TestFacePick(new PickerOptions() { depthTest = true, rectSelectMode = RectSelectMode.Complete });
			var selection = faces.FirstOrDefault();
			Assert.IsNotNull(selection);
			HashSet<Face> selectedElements = selection.Value;
			Assert.Greater(selectedElements.Count, 0);
			Assert.Less(selectedElements.Count, selection.Key.faceCount);

			Cleanup();
		}
	}
}
