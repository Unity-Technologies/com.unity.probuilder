using System;
using System.Collections;
using System.Collections.Generic;
using ProBuilder.Core;
using UnityEngine;
using UnityEditor;

namespace ProBuilder.Test
{
	public class CreateBasicShapeTemplatea
	{
		[MenuItem("Tools/Create Shape Templates")]
		static void Init()
		{
			string[] primitiveNames = Enum.GetNames(typeof(pb_ShapeType));
			pb_ShapeType[] primitiveTypes = (pb_ShapeType[]) Enum.GetValues(typeof(pb_ShapeType));

			for (int i = 0, c = primitiveNames.Length; i < c; i++)
			{
				var pb = pb_ShapeGenerator.CreateShape(primitiveTypes[i]);
				if (pb == null)
					continue;
				pb.gameObject.name = primitiveNames[i];
				AssetDatabase.CreateAsset(
					pb.GetComponent<MeshFilter>().sharedMesh,
					"Assets/Tests/Editor/Runtime/Shape/Mesh Templates/" + primitiveNames[i] + "_Default.asset");
			}
		}
	}
}
