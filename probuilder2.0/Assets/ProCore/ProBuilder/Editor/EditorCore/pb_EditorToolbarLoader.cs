using UnityEngine;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.Actions;
using System.Linq;

namespace ProBuilder2.EditorCommon
{
	public class pb_EditorToolbarLoader
	{
		public static List<pb_MenuAction> GetActions()
		{
			return new List<pb_MenuAction>()
			{
				// tools
				new OpenShapeEditor(),
				new OpenMaterialEditor(),
				new OpenUVEditor(),
				new OpenVertexColorEditor(),
				new OpenSmoothingEditor(),
				new OpenMirrorObjectsEditor(),

				new ToggleSelectBackFaces(),
				new ToggleHandleAlignment(),

				new GrowSelection(),
				new ShrinkSelection(),
				new InvertSelection(),
				new SelectEdgeLoop(),
				new SelectEdgeRing(),

				// object
				new MergeObjects(),
				new FlipObjectNormals(),
				new SubdivideObject(),
				new FreezeTransform(),
				new ConformObjectNormals(),
				new TriangulateObject(),

				// All
				new SetPivotToSelection(),

				// Faces (All)
				new DeleteFaces(),
				new DetachFaces(),
				new ExtrudeFaces(),

				// Face
				new ConformFaceNormals(),
				new FlipFaceEdge(),
				new FlipFaceNormals(),
				new MergeFaces(),
				new SubdivideFaces(),
				
				// Edge
				new BridgeEdges(),
				new ConnectEdges(),
				new ExtrudeEdges(),
				new InsertEdgeLoop(),
				new SubdivideEdges(),

				// Vertex
				new CollapseVertices(),
				new WeldVertices(),
				new ConnectVertices(),
				new SplitVertices(),
			};
		}
	}
}
