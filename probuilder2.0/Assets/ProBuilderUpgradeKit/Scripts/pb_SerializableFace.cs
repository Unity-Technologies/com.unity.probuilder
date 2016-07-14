using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using ProBuilder2.Common;

namespace ProBuilder2.UpgradeKit
{

	/**
	 * A serializable storage class for pb_Face.
	 */
	[System.Serializable()]
	public class pb_SerializableFace : ISerializable
	{
#region PROPERTIES

		public int[] 					indices;
		public int[] 					distinctIndices;
		public pb_Edge[] 				edges;
		public int 						smoothingGroup;
		public pb_UV					uv;
		public Material 				material;
		public bool 					manualUV;
		public int 						elementGroup;
		public int 						textureGroup;
#endregion

#region INIT

		public pb_SerializableFace() {}

		public pb_SerializableFace(pb_Face face)
		{
			this.indices			= face.indices;
			this.distinctIndices	= face.distinctIndices;
			this.edges				= face.edges;
			this.smoothingGroup		= face.smoothingGroup;
			this.uv					= face.uv;
			this.material			= face.material;
			this.manualUV  			= false;
			pb_UpgradeKitUtils.TryGetField(face, "manualUV", ref this.manualUV);
			this.elementGroup		= -1;
			pb_UpgradeKitUtils.TryGetField(face, "elementGroup", ref this.elementGroup);
			this.textureGroup		= -1;
			pb_UpgradeKitUtils.TryGetField(face, "textureGroup", ref this.textureGroup);
		}

		public static explicit operator pb_Face(pb_SerializableFace serialized)
		{
			pb_Face face = new pb_Face( serialized.indices );

			face.uv = serialized.uv;
			face.material = serialized.material;
			face.smoothingGroup = serialized.smoothingGroup;
			face.RebuildCaches();	// set distinct indices and edges

			pb_UpgradeKitUtils.TrySetField(face, "manualUV", serialized.manualUV);
			pb_UpgradeKitUtils.TrySetField(face, "elementGroup", serialized.elementGroup);
			pb_UpgradeKitUtils.TrySetField(face, "textureGroup", serialized.textureGroup);

			return face;
		}
#endregion

#region SERIALIZATION

		// OnSerialize
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("indices",			indices, 					typeof(int[]));
			info.AddValue("distinctIndices", 	distinctIndices, 			typeof(int[]));
			info.AddValue("edges", 				System.Array.ConvertAll(edges, x => new pb_SerializableEdge(x)), typeof(pb_SerializableEdge[]));
			info.AddValue("smoothingGroup",	 	smoothingGroup, 			typeof(int));
			info.AddValue("uv",	 				new pb_SerializableUV(uv),	typeof(pb_SerializableUV));
			info.AddValue("material",			material.name,				typeof(string));
			info.AddValue("manualUV", 			manualUV, 					typeof(bool));
			info.AddValue("elementGroup", 		elementGroup, 				typeof(int));
			info.AddValue("textureGroup", 		textureGroup, 				typeof(int));
		}

		// The pb_SerializedMesh constructor is used to deserialize values. 
		public pb_SerializableFace(SerializationInfo info, StreamingContext context)
		{
			this.indices 			= (int[])		info.GetValue( "indices",			typeof(int[]));
			this.distinctIndices 	= (int[])		info.GetValue( "distinctIndices",	typeof(int[]));
			pb_SerializableEdge[] sedges = (pb_SerializableEdge[]) info.GetValue("edges", typeof(pb_SerializableEdge[]));
			this.edges 				= System.Array.ConvertAll(sedges, x => (pb_Edge)x);
			this.smoothingGroup 	= (int) 		info.GetValue( "smoothingGroup",	typeof(int));
			pb_SerializableUV suv = (pb_SerializableUV) info.GetValue("uv", typeof(pb_SerializableUV));
			this.uv = (pb_UV) suv;
			// this.uv 				= (pb_UV)		info.GetValue( "uv",				typeof(pb_SerializableUV));
			this.manualUV 			= (bool) 		info.GetValue( "manualUV",			typeof(bool));
			this.elementGroup 		= (int) 		info.GetValue( "elementGroup",		typeof(int));
			this.textureGroup 		= (int) 		info.GetValue( "textureGroup",		typeof(int));

			this.material = pb_UpgradeKitUtils.GetDefaultMaterial();

			string matName = (string)info.GetValue("material", typeof(string));

			foreach(Material mat in Resources.FindObjectsOfTypeAll(typeof(Material)))
			{
				if(mat.name.Equals(matName))
				{
					this.material = mat;
					break;
				}
			}
		}
#endregion
	}
}
