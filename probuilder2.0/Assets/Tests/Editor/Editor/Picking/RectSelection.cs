using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using ProBuilder.Core;
using ProBuilder.EditorCore;
using UnityEditor;
using UnityEngine.TestTools;

namespace ProBuilder.EditorTests.Picking
{
	public class RectSelection
	{
		pb_Object[] selectables;
		Camera camera;

		void Setup()
		{
			camera = new GameObject("Camera", typeof(Camera)).GetComponent<Camera>();
			camera.transform.position = new Vector3(.3f, 2.2f, -3f);

			pb_Object shape = pb_ShapeGenerator.CreateShape(pb_ShapeType.Torus);
			shape.transform.position = Vector3.zero - shape.GetComponent<MeshRenderer>().bounds.center;

			camera.transform.LookAt(shape.transform);

			selectables = new pb_Object[]
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

		Dictionary<pb_Object, HashSet<int>> TestVertexPick(pb_PickerOptions options)
		{
			try
			{
				Rect selectionRect = new Rect(camera.pixelRect);
				selectionRect.width /= EditorGUIUtility.pixelsPerPoint;
				selectionRect.height /= EditorGUIUtility.pixelsPerPoint;

				var vertices = pb_Picking.PickVerticesInRect(
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

		Dictionary<pb_Object, HashSet<pb_Edge>> TestEdgePick(pb_PickerOptions options)
		{
			try
			{
				Rect selectionRect = new Rect(camera.pixelRect);
				selectionRect.width /= EditorGUIUtility.pixelsPerPoint;
				selectionRect.height /= EditorGUIUtility.pixelsPerPoint;

				var edges = pb_Picking.PickEdgesInRect(
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

		Dictionary<pb_Object, HashSet<pb_Face>> TestFacePick(pb_PickerOptions options)
		{
			try
			{
				Rect selectionRect = new Rect(camera.pixelRect);
				selectionRect.width /= EditorGUIUtility.pixelsPerPoint;
				selectionRect.height /= EditorGUIUtility.pixelsPerPoint;

				var faces = pb_Picking.PickFacesInRect(
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
			var vertices = TestVertexPick(new pb_PickerOptions() { depthTest = true });
			var selection = vertices.FirstOrDefault();
			Assert.IsNotNull(selection);
			HashSet<int> selectedElements = selection.Value;
			Assert.Less(selectedElements.Count, selection.Key.sharedIndices.Length);
			Assert.Greater(selectedElements.Count, 0);
			Cleanup();
		}

		[Test]
		public void PickVertices_DepthTestOff()
		{
			Setup();
			var vertices = TestVertexPick(new pb_PickerOptions() { depthTest = false });
			var selection = vertices.FirstOrDefault();
			Assert.IsNotNull(selection);
			HashSet<int> selectedElements = selection.Value;
			Assert.AreEqual(selectedElements.Count, selection.Key.sharedIndices.Length);
			Cleanup();
		}

		[Test]
		public void PickEdges_DepthTestOff_RectSelectPartial()
		{
			Setup();
			var edges = TestEdgePick(new pb_PickerOptions() { depthTest = false, rectSelectMode = pb_RectSelectMode.Partial });
			Assert.IsNotNull(edges, "Selection is null");
			var selection = edges.FirstOrDefault();
			Assert.IsNotNull(selection, "Selection is null");
			HashSet<pb_Edge> selectedElements = selection.Value;
			Assert.Greater(selectedElements.Count, 0);

			Dictionary<int, int> commonLookup = selection.Key.sharedIndices.ToDictionary();
			var allEdges = pb_EdgeLookup.GetEdgeLookupHashSet(selection.Key.faces.SelectMany(x => x.edges), commonLookup);
			var selectedEdges = pb_EdgeLookup.GetEdgeLookupHashSet(selectedElements, commonLookup);
			Assert.AreEqual(allEdges.Count, selectedEdges.Count);

			Cleanup();
		}

		[Test]
		public void PickEdges_DepthTestOn_RectSelectPartial()
		{
			Setup();
			var edges = TestEdgePick(new pb_PickerOptions() { depthTest = true, rectSelectMode = pb_RectSelectMode.Partial });
			var selection = edges.FirstOrDefault();
			Assert.IsNotNull(selection);
			HashSet<pb_Edge> selectedElements = selection.Value;
			Assert.Greater(selectedElements.Count, 0);
			Assert.Less(selectedElements.Count, selection.Key.faces.Sum(x=>x.edges.Length));

			Cleanup();
		}

		[Test]
		public void PickEdges_DepthTestOff_RectSelectComplete()
		{
			Setup();
			var edges = TestEdgePick(new pb_PickerOptions() { depthTest = false, rectSelectMode = pb_RectSelectMode.Complete });
			Assert.IsNotNull(edges, "Selection is null");
			var selection = edges.FirstOrDefault();
			Assert.IsNotNull(selection, "Selection is null");
			HashSet<pb_Edge> selectedElements = selection.Value;
			Assert.Greater(selectedElements.Count, 0);

			Dictionary<int, int> commonLookup = selection.Key.sharedIndices.ToDictionary();
			var allEdges = pb_EdgeLookup.GetEdgeLookupHashSet(selection.Key.faces.SelectMany(x => x.edges), commonLookup);
			var selectedEdges = pb_EdgeLookup.GetEdgeLookupHashSet(selectedElements, commonLookup);
			Assert.AreEqual(allEdges.Count, selectedEdges.Count);

			Cleanup();
		}

		[Test]
		public void PickEdges_DepthTestOn_RectSelectComplete()
		{
			Setup();
			var edges = TestEdgePick(new pb_PickerOptions() { depthTest = true, rectSelectMode = pb_RectSelectMode.Complete });
			var selection = edges.FirstOrDefault();
			Assert.IsNotNull(selection);
			HashSet<pb_Edge> selectedElements = selection.Value;
			Assert.Greater(selectedElements.Count, 0);
			Assert.Less(selectedElements.Count, selection.Key.faces.Sum(x=>x.edges.Length));

			Cleanup();
		}

		[Test]
		public void PickFaces_DepthTestOff_RectSelectPartial()
		{
			Setup();
			var faces = TestFacePick(new pb_PickerOptions() { depthTest = false, rectSelectMode = pb_RectSelectMode.Partial });
			Assert.IsNotNull(faces, "Selection is null");
			var selection = faces.FirstOrDefault();
			Assert.IsNotNull(selection, "Selection is null");
			HashSet<pb_Face> selectedElements = selection.Value;
			Assert.Greater(selectedElements.Count, 0);
			Assert.AreEqual(selection.Key.faceCount, selectedElements.Count);
			Cleanup();
		}

		[Test]
		public void PickFaces_DepthTestOn_RectSelectPartial()
		{
			Setup();
			var faces = TestFacePick(new pb_PickerOptions() { depthTest = true, rectSelectMode = pb_RectSelectMode.Partial });
			Assert.IsNotNull(faces, "Face pick returned null");
			var selection = faces.FirstOrDefault();
			Assert.IsNotNull(selection);
			HashSet<pb_Face> selectedElements = selection.Value;
			Assert.Greater(selectedElements.Count, 0);
			Assert.Less(selectedElements.Count, selection.Key.faceCount);

			Cleanup();
		}

		[Test]
		public void PickFaces_DepthTestOff_RectSelectComplete()
		{
			Setup();
			var faces = TestFacePick(new pb_PickerOptions() { depthTest = false, rectSelectMode = pb_RectSelectMode.Complete });
			Assert.IsNotNull(faces, "Selection is null");
			var selection = faces.FirstOrDefault();
			Assert.IsNotNull(selection, "Selection is null");
			HashSet<pb_Face> selectedElements = selection.Value;
			Assert.Greater(selectedElements.Count, 0);
			Assert.AreEqual(selection.Key.faceCount, selectedElements.Count);
			Cleanup();
		}

		[Test]
		public void PickFaces_DepthTestOn_RectSelectComplete()
		{
			Setup();
			var faces = TestFacePick(new pb_PickerOptions() { depthTest = true, rectSelectMode = pb_RectSelectMode.Complete });
			var selection = faces.FirstOrDefault();
			Assert.IsNotNull(selection);
			HashSet<pb_Face> selectedElements = selection.Value;
			Assert.Greater(selectedElements.Count, 0);
			Assert.Less(selectedElements.Count, selection.Key.faceCount);

			Cleanup();
		}
	}
}
