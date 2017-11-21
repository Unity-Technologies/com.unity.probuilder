using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

namespace ProBuilder.Core
{
	public class SpawnAssloadOfObjects : Editor
	{
		const int OBJ_COUNT = 200;

		static System.Random r = new System.Random();

		static Vector3 RandVec(float min, float max) { return new Vector3(RandFloat(min,max), RandFloat(min,max), RandFloat(min,max)); }
		static int RandInt(int min, int max) { return r.Next(min, max); }
		static float RandFloat(float min, float max) { return (float) r.NextDouble() * (max-min) + min; }
		static bool RandBool() { return r.Next(0,100) % 2 == 0; }

		[MenuItem("Tools/Debug/ProBuilder/Spawn Random Objects")]
		static void Doit()
		{
			GameObject parent = new GameObject();

			for(int i = 0; i < OBJ_COUNT; i++)
			{
				GameObject go;
				bool cancel = false;

				switch(RandInt(0,6))
				{
					case 1:
						cancel = EditorUtility.DisplayCancelableProgressBar("Spawn Random Objects", "Prism: (" + (i + " / " + OBJ_COUNT + ")").ToString(), i / (float) OBJ_COUNT);
						go = Prism().gameObject;
						break;

					case 2:
						cancel = EditorUtility.DisplayCancelableProgressBar("Spawn Random Objects", "Stair: (" + (i + " / " + OBJ_COUNT + ")").ToString(), i / (float) OBJ_COUNT);
						go = Stair().gameObject;
						break;

					case 3:
						cancel = EditorUtility.DisplayCancelableProgressBar("Spawn Random Objects", "Cylinder: (" + (i + " / " + OBJ_COUNT + ")").ToString(), i / (float) OBJ_COUNT);
						go = Cylinder().gameObject;
						break;

					case 4:
						cancel = EditorUtility.DisplayCancelableProgressBar("Spawn Random Objects", "Torus: (" + (i + " / " + OBJ_COUNT + ")").ToString(), i / (float) OBJ_COUNT);
						go = Torus().gameObject;
						break;

					case 5:
						cancel = EditorUtility.DisplayCancelableProgressBar("Spawn Random Objects", "Icosphere: (" + (i + " / " + OBJ_COUNT + ")").ToString(), i / (float) OBJ_COUNT);
						go = Icosphere().gameObject;
						break;
		
					default:
						cancel = EditorUtility.DisplayCancelableProgressBar("Spawn Random Objects", "Cube: (" + (i + " / " + OBJ_COUNT + ")").ToString(), i / (float) OBJ_COUNT);
						go = Cube().gameObject;
						break;
				}

				if(cancel)
					break;

				go.transform.position = RandVec(-50f, 50f);
				go.transform.SetParent(parent.transform, true);
			}

			EditorUtility.ClearProgressBar();
		}

		static pb_Object Cube()
		{
			return pb_ShapeGenerator.CubeGenerator(RandVec(.2f, 10f));
		}

		static pb_Object Prism()
		{
			return pb_ShapeGenerator.PrismGenerator(RandVec(.2f, 10f));
		}

		static pb_Object Stair()
		{
			if(RandBool())
				return pb_ShapeGenerator.StairGenerator(RandVec(1f, 20f), RandInt(0, 30), RandBool());
			else
				return pb_ShapeGenerator.CurvedStairGenerator( 
						RandFloat(.4f, 3f),
						RandFloat(2f, 15f),
						RandFloat(.01f, 8f),
						RandFloat(0f, 360f),
						RandInt(0, 40),
						RandBool());
		}

		static pb_Object Cylinder()
		{
			return pb_ShapeGenerator.CylinderGenerator(RandInt(3, 30), RandFloat(.4f, 4f), RandFloat(1f, 10f), RandInt(0, 5));
		}

		static pb_Object Torus()
		{
			int 	a = RandInt(3, 10);
			int 	b = RandInt(3, 10);
			float 	c = RandFloat(.5f, 10f);
			float 	d = RandFloat(.5f, 8f);
			bool 	e = RandBool();
			float 	f = RandFloat(25f, 360f);
			float 	g = RandFloat(25f, 360f);

			pb_Object pb = pb_ShapeGenerator.TorusGenerator(a, b, c, d, e, f, g);

			pb.name = "Torus [" + a + ", " + b + ", " + c + ", " + d + ", " + e + ", " + f + ", " + g + "]"; 

			return pb;
		}

		static pb_Object Icosphere()
		{
			return pb_ShapeGenerator.IcosahedronGenerator(RandFloat(.5f, 5f), RandInt(0, 4));
		}
	}
}
