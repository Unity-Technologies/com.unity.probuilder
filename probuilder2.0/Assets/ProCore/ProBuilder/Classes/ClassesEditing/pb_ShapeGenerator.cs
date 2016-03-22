
using UnityEngine;
using System.Collections.Generic;
using ProBuilder2.Math;

#if PB_DEBUG
using Parabox.Debug;
#endif

namespace ProBuilder2.Common
{

/**
 *	\brief Static utility class for generating pb_Object geometry.
 */
public class pb_ShapeGenerator
{

	public static pb_Object StairGenerator(Vector3 size, int steps, bool buildSides)
	{
		/// 4 vertices per quad, 2 quads per step.
		Vector3[] vertices = new Vector3[4 * steps * 2];
		pb_Face[] faces = new pb_Face[steps * 2];

		/// vertex index, face index
		int v = 0, t = 0;

		for(int i = 0; i < steps; i++)
		{
			float inc0 = i / (float) steps;
			float inc1 = (i + 1) / (float) steps;

			float x0 = size.x;
			float x1 = 0;
			float y0 = size.y * inc0;
			float y1 = size.y * inc1;
			float z0 = size.z * inc0;
			float z1 = size.z * inc1;

			vertices[v+0] = new Vector3(x0, y0, z0);
			vertices[v+1] = new Vector3(x1, y0, z0);
			vertices[v+2] = new Vector3(x0, y1, z0);
			vertices[v+3] = new Vector3(x1, y1, z0);

			vertices[v+4] = new Vector3(x0, y1, z0);
			vertices[v+5] = new Vector3(x1, y1, z0);
			vertices[v+6] = new Vector3(x0, y1, z1);
			vertices[v+7] = new Vector3(x1, y1, z1);

			faces[t+0] = new pb_Face( new int[] { 	v + 0,
													v + 1,
													v + 2,
													v + 1,
													v + 3,
													v + 2 });

			faces[t+1] = new pb_Face( new int[] { 	v + 4,
													v + 5,
													v + 6,
													v + 5,
													v + 7,
													v + 6 });

			v += 8;
			t += 2;
		}

		/// sides
		if(buildSides)
		{
			/// first step is special case - only needs a quad, but all other steps need
			/// a quad and tri.
			float x = 0f;

			for(int side = 0; side < 2; side++)
			{
				Vector3[] sides_v = new Vector3[ steps * 4 + (steps - 1) * 3 ];
				pb_Face[] sides_f = new pb_Face[ steps + steps-1 ];

				int sv = 0, st = 0;

				for(int i = 0; i < steps; i++)
				{
					float y0 = (Mathf.Max(i, 1) / (float) steps) * size.y;
					float y1 = ((i+1) / (float) steps) * size.y;

					float z0 = (i / (float)steps) * size.z;
					float z1 = ((i+1) / (float) steps) * size.z;

					sides_v[sv+0] = new Vector3(x, 0f, z0);
					sides_v[sv+1] = new Vector3(x, 0f, z1);
					sides_v[sv+2] = new Vector3(x, y0, z0);
					sides_v[sv+3] = new Vector3(x, y1, z1);

					sides_f[st++] = new pb_Face( side % 2 == 0 ?
						new int[] { v+0, v+1, v+2, v+1, v+3, v+2 } :
						new int[] { v+2, v+1, v+0, v+2, v+3, v+1 } );

					sides_f[st-1].textureGroup = side + 1;

					v += 4;
					sv += 4;

					/// that connecting triangle
					if(i > 0)
					{
						sides_v[sv+0] = new Vector3(x, y0, z0);
						sides_v[sv+1] = new Vector3(x, y1, z0);
						sides_v[sv+2] = new Vector3(x, y1, z1);

						sides_f[st++] = new pb_Face( side % 2 == 0 ? 
							new int[] { v+2, v+1, v+0 } :
							new int[] { v+0, v+1, v+2 } );

						sides_f[st-1].textureGroup = side + 1;

						v += 3;
						sv += 3;
					}
				}

				vertices = vertices.Concat(sides_v);
				faces = faces.Concat(sides_f);

				x += size.x;
			}

			// add that last back face
			vertices = vertices.Concat(new Vector3[] {
				new Vector3(0f, 0f, size.z),
				new Vector3(size.x, 0f, size.z),
				new Vector3(0f, size.y, size.z),
				new Vector3(size.x, size.y, size.z)
				});

			faces = faces.Add(new pb_Face(new int[] {v+0, v+1, v+2, v+1, v+3, v+2}));
		}

		pb_Object pb = pb_Object.CreateInstanceWithVerticesFaces(vertices, faces);
		pb.gameObject.name = "Stairs"; 

		return pb;	
	}

	public static pb_Object CurvedStairGenerator(float stairWidth, float height, float innerRadius, float circumference, int steps, bool buildSides)
	{
		bool noInnerSide = innerRadius < Mathf.Epsilon;

		/// 4 vertices per quad, vertical step first, then floor step can be 3 or 4 verts depending on
		/// if the inner radius is 0 or not.
		Vector3[] vertices = new Vector3[(4 * steps) + ((noInnerSide ? 3 : 4) * steps)];
		pb_Face[] faces = new pb_Face[steps * 2];

		/// vertex index, face index
		int v = 0, t = 0;

		float cir = Mathf.Abs(circumference) * Mathf.Deg2Rad;
		float outerRadius = innerRadius + stairWidth;

		for(int i = 0; i < steps; i++)
		{
			float inc0 = (i / (float) steps) * cir;
			float inc1 = ((i + 1) / (float) steps) * cir;

			float h0 = ((i / (float) steps) * height);
			float h1 = (((i+1) / (float) steps) * height);

			Vector3 v0 = new Vector3(-Mathf.Cos(inc0), 0f, Mathf.Sin(inc0) );
			Vector3 v1 = new Vector3(-Mathf.Cos(inc1), 0f, Mathf.Sin(inc1) );

			/**
			 * 
			 *		/6-----/7
			 *	   /	  /
			 *	  /5_____/4
			 *	  |3	 |2
			 *	  |		 |
			 *	  |1_____|0
			 *
			 */

			vertices[v+0] = v0 * innerRadius;
			vertices[v+1] = v0 * outerRadius;
			vertices[v+2] = v0 * innerRadius;
			vertices[v+3] = v0 * outerRadius;

			vertices[v+0].y = h0;
			vertices[v+1].y = h0;
			vertices[v+2].y = h1;
			vertices[v+3].y = h1;

			vertices[v+4] = vertices[v+2];
			vertices[v+5] = vertices[v+3];

			vertices[v+6] = v1 * outerRadius;
			vertices[v+6].y = h1;

			if(!noInnerSide)
			{
				vertices[v+7] = v1 * innerRadius;
				vertices[v+7].y = h1;
			}
			
			faces[t+0] = new pb_Face( new int[] { 	
				v + 0,
				v + 1,
				v + 2,
				v + 1,
				v + 3,
				v + 2 });

			if(noInnerSide)
			{
				faces[t+1] = new pb_Face( new int[] {
					v + 4,
					v + 5,
					v + 6 });
			}
			else
			{
				faces[t+1] = new pb_Face( new int[] {
					v + 4,
					v + 5,
					v + 6,
					v + 4,
					v + 6,
					v + 7 });
			}

			float uvRotation = ((inc1 + inc0) * -.5f) * Mathf.Rad2Deg;
			uvRotation %= 360f;
			if(uvRotation < 0f)
				uvRotation = 360f + uvRotation;
			faces[t+1].uv.rotation = uvRotation;

			v += noInnerSide ? 7 : 8;
			t += 2;
		}

		/// sides
		if(buildSides)
		{
			/// first step is special case - only needs a quad, but all other steps need
			/// a quad and tri.
			float x = noInnerSide ? innerRadius + stairWidth : innerRadius;;

			for(int side = (noInnerSide ? 1 : 0); side < 2; side++)
			{
				Vector3[] sides_v = new Vector3[ steps * 4 + (steps - 1) * 3 ];
				pb_Face[] sides_f = new pb_Face[ steps + steps-1 ];

				int sv = 0, st = 0;

				for(int i = 0; i < steps; i++)
				{
					float inc0 = (i / (float) steps) * cir;
					float inc1 = ((i + 1) / (float) steps) * cir;

					float h0 = ((Mathf.Max(i, 1) / (float) steps) * height);
					float h1 = (((i+1) / (float) steps) * height);

					Vector3 v0 = new Vector3(-Mathf.Cos(inc0), 0f, Mathf.Sin(inc0) ) * x;
					Vector3 v1 = new Vector3(-Mathf.Cos(inc1), 0f, Mathf.Sin(inc1) ) * x;	

					sides_v[sv+0] = v0;
					sides_v[sv+1] = v1;
					sides_v[sv+2] = v0;
					sides_v[sv+3] = v1;

					sides_v[sv+0].y = 0f;
					sides_v[sv+1].y = 0f;
					sides_v[sv+2].y = h0;
					sides_v[sv+3].y = h1;

					sides_f[st++] = new pb_Face( side % 2 == 0 ?
						new int[] { v+2, v+1, v+0, v+2, v+3, v+1 } :
						new int[] { v+0, v+1, v+2, v+1, v+3, v+2 } );
					sides_f[st-1].smoothingGroup = side + 1;

					v += 4;
					sv += 4;

					/// that connecting triangle
					if(i > 0)
					{
						sides_f[st-1].textureGroup = (side * steps) + i;

						sides_v[sv+0] = v0;
						sides_v[sv+1] = v1;
						sides_v[sv+2] = v0;
						sides_v[sv+0].y = h0;
						sides_v[sv+1].y = h1;
						sides_v[sv+2].y = h1;

						sides_f[st++] = new pb_Face( side % 2 == 0 ? 
							new int[] { v+2, v+1, v+0 } :
							new int[] { v+0, v+1, v+2 } );

						sides_f[st-1].textureGroup = (side * steps) + i;
						sides_f[st-1].smoothingGroup = side + 1;

						v += 3;
						sv += 3;
					}
				}

				vertices = vertices.Concat(sides_v);
				faces = faces.Concat(sides_f);

				x += stairWidth;
			}

			// // add that last back face
			float cos = -Mathf.Cos(cir), sin = Mathf.Sin(cir);

			vertices = vertices.Concat(new Vector3[] 
			{
				new Vector3(cos, 0f, sin) * innerRadius,
				new Vector3(cos, 0f, sin) * outerRadius,
				new Vector3(cos * innerRadius, height, sin * innerRadius),
				new Vector3(cos * outerRadius, height, sin * outerRadius)
				});

			faces = faces.Add(new pb_Face(new int[] {v+2, v+1, v+0, v+2, v+3, v+1}));
		}

		if(circumference < 0f)
		{
			Vector3 flip = new Vector3(-1f, 1f, 1f);

			for(int i = 0; i < vertices.Length; i++)
			{
				vertices[i].Scale(flip);
			}

			foreach(pb_Face f in faces)
				f.ReverseIndices();
		}

		pb_Object pb = pb_Object.CreateInstanceWithVerticesFaces(vertices, faces);

		pb.gameObject.name = "Stairs"; 

		return pb;	
	}

	/**
	 *	\brief Creates a stair set with the given parameters.
	 *	@param steps How many steps should this stairwell have?
	 *	@param width How wide (in meters) should this stairset be?
	 *	@param height How tall (in meters) should this stairset be?
	 *	@param depth How deep (in meters) should this stairset be?
	 *	@param sidesGoToFloor If true, stair step sides will extend to the floor.  If false, sides will only extend as low as the stair is high.
	 *	@param generateBack If true, a back face to the stairwell will be appended.
	 *	@param platformsOnly If true, only the front face and tops of the stairwell will be built.  Nice for when a staircase is embedded between geometry.
	 *	\returns A pb_Object reference to the created stairset.
	 */
	public static pb_Object StairGenerator(int steps, float width, float height, float depth, bool sidesGoToFloor, bool generateBack, bool platformsOnly)
	{
		int i = 0;
		
		List<Vector3> verts = new List<Vector3>();
		Vector3[] v = (platformsOnly) ? new Vector3[8] : new Vector3[16];

		float stepWidth = width;
		float stepHeight = height/steps;
		float stepDepth = depth/steps;
		float yMax = stepHeight; // used when stair sides extend to floor

		// platforms
		for(i = 0; i < steps; i++)
		{
			float x = stepWidth/2f, y = i*stepHeight, z = i*stepDepth;
			
			if(sidesGoToFloor)
				y = 0;

			yMax = i * stepHeight + stepHeight;

			// Front
			v[0]  = new Vector3( x, i*stepHeight, 	z);
			v[1]  = new Vector3(-x,	i*stepHeight, 	z);
			v[2] = new Vector3( x, yMax,			z);
			v[3] = new Vector3(-x, yMax, 			z);

			// Platform
			v[4] = new Vector3(  x, yMax, z);
			v[5] = new Vector3(- x, yMax, z);
			v[6] = new Vector3(  x, yMax, z + stepDepth);
			v[7] = new Vector3(- x, yMax, z + stepDepth);

			if(!platformsOnly)
			{
				// Left side
				v[8 ] = new Vector3(x, y, 		z + stepDepth);
				v[9 ] = new Vector3(x, y, 		z);
				v[10] = new Vector3(x, yMax, 	z + stepDepth);
				v[11] = new Vector3(x, yMax, 	z);

				// Right side
				v[12] = new Vector3(- x, y, 	z);
				v[13] = new Vector3(- x, y, 	z + stepDepth);
				v[14] = new Vector3(- x, yMax, 	z);
				v[15] = new Vector3(- x, yMax, 	z + stepDepth);
			}

			verts.AddRange(v);
		}

		if(generateBack) {
			verts.Add(new Vector3(-stepWidth/2f, 0f, depth) );
			verts.Add(new Vector3(stepWidth/2f, 0f, depth) );
			verts.Add(new Vector3(-stepWidth/2f, height, depth) );
			verts.Add(new Vector3(stepWidth/2f, height, depth) );
		}

		pb_Object pb = pb_Object.CreateInstanceWithPoints(verts.ToArray());
		pb.gameObject.name = "Stairs"; 
		return pb;
	}

	/**
	 * Create a new cube with the specified size.  Size is baked (eg, not applied as a scale value in the transform).
	 */
	public static pb_Object CubeGenerator(Vector3 size)
	{
		Vector3[] points = new Vector3[pb_Constant.TRIANGLES_CUBE.Length];
		for(int i = 0; i < pb_Constant.TRIANGLES_CUBE.Length; i++)
			points[i] = Vector3.Scale(pb_Constant.VERTICES_CUBE[pb_Constant.TRIANGLES_CUBE[i]], size);

		pb_Object pb = pb_Object.CreateInstanceWithPoints(points);

		pb.gameObject.name = "Cube";
		return pb;
	}

	/**
	 *	\brief Creates a cylinder #pb_Object with the supplied parameters.
	 *	@param axisDivisions How many divisions to create on the vertical axis.  Larger values = smoother surface.
	 *	@param radius The radius in world units.
	 *	@param height The height of this object in world units.
	 *	@param heightCuts The amount of divisions to create on the horizontal axis.
	 *	\returns The newly generated #pb_Object.
	 */
	public static pb_Object CylinderGenerator(int axisDivisions, float radius, float height, int heightCuts)
	{
		if(axisDivisions % 2 != 0)
			axisDivisions++;
		
		if(axisDivisions > 64)
			axisDivisions = 64;

		float stepAngle = 360f/axisDivisions;
		float heightStep = height/(heightCuts+1);

		Vector3[] circle = new Vector3[axisDivisions];

		// get a circle
		for(int i = 0; i < axisDivisions; i++)
		{
			float angle0 = stepAngle * i * Mathf.Deg2Rad;

			float x = Mathf.Cos(angle0) * radius;
			float z = Mathf.Sin(angle0) * radius;

			circle[i] = new Vector3(x, 0f, z);
		}

		// add two because end caps
		Vector3[] verts = new Vector3[(axisDivisions*(heightCuts+1)*4) + (axisDivisions*6)];
		pb_Face[] faces = new pb_Face[axisDivisions*(heightCuts+1)   + (axisDivisions*2)];

		// build vertex array
		int it = 0;
		// +1 to account for 0 height cuts
		for(int i = 0; i < heightCuts+1; i++)
		{
			float Y = i*heightStep;
			float Y2 = (i+1)*heightStep;

			for(int n = 0; n < axisDivisions; n++)
			{
				verts[it+0] = new Vector3(circle[n+0].x, Y, circle[n+0].z);			
				verts[it+1] = new Vector3(circle[n+0].x, Y2, circle[n+0].z);

				if(n != axisDivisions-1) {
					verts[it+2] = new Vector3(circle[n+1].x, Y, circle[n+1].z);
					verts[it+3] = new Vector3(circle[n+1].x, Y2, circle[n+1].z);
				} else {
					verts[it+2] = new Vector3(circle[0].x, Y, circle[0].z);
					verts[it+3] = new Vector3(circle[0].x, Y2, circle[0].z);
				}

				it+=4;
			}
		}

		// wind side faces
		int f = 0;
		for(int i = 0; i < heightCuts+1; i++)
		{
			for(int n = 0; n < axisDivisions*4; n+=4)
			{
				int index = (i*(axisDivisions*4))+n;
				int zero 	= index;
				int one 	= index + 1;
				int two 	= index + 2;
				int three 	= index + 3;
	
				faces[f++] = new pb_Face(new int[6]{
					zero,
					one,
					two,
					one, 
					three,
					two					
					});
			}
		}

		// construct caps seperately, cause they aren't wound the same way
		int ind = (axisDivisions*(heightCuts+1)*4);
		int f_ind = axisDivisions*(heightCuts+1);

		for(int n = 0; n < axisDivisions; n++)
		{
			// bottom faces
			verts[ind+0] = new Vector3(circle[n].x, 0f, circle[n].z);

			verts[ind+1] = Vector3.zero;

			if(n != axisDivisions-1)
				verts[ind+2] = new Vector3(circle[n+1].x, 0f, circle[n+1].z);
			else
				verts[ind+2] = new Vector3(circle[000].x, 0f, circle[000].z);
			
			faces[f_ind + n] = new pb_Face(new int[3] {ind+2, ind+1, ind+0});

			ind += 3;

			// top faces
			verts[ind+0] 	= new Vector3(circle[n].x, height, circle[n].z);
			verts[ind+1] 	= new Vector3(0f, height, 0f);
			if(n != axisDivisions-1)
				verts[ind+2] = new Vector3(circle[n+1].x, height, circle[n+1].z);
			else
				verts[ind+2] = new Vector3(circle[000].x, height, circle[000].z);
			
			faces[f_ind + (n+axisDivisions)] = new pb_Face(new int[3] {ind+0, ind+1, ind+2});

			ind += 3;
		}

		pb_Object pb = pb_Object.CreateInstanceWithVerticesFaces(verts, faces);
		pb.gameObject.name = "Cylinder";
		return pb;
	}

	/**
	 *	\brief Returns a pb_Object prism with the passed size.
	 * @param size Size to apply to generated object.
	 * \returns New #pb_Object.
	 */
	public static pb_Object PrismGenerator(Vector3 size)
	{
		Vector3[] template = new Vector3[6]
		{
			Vector3.Scale(new Vector3(-.5f, 0f, -.5f), 	size),
			Vector3.Scale(new Vector3(.5f, 0f, -.5f), 	size),
			Vector3.Scale(new Vector3(0f, .5f, -.5f), 	size),
			Vector3.Scale(new Vector3(-.5f, 0f, .5f), 	size),
			Vector3.Scale(new Vector3(0.5f, 0f, .5f), 	size),
			Vector3.Scale(new Vector3(0f, .5f, .5f), 	size)
		};

		Vector3[] v = new Vector3[18]
		{
			template[0],	// 0	front
			template[1],	// 1
			template[2],	// 2

			template[1],	// 3	right side
			template[4],	// 4
			template[2],	// 5
			template[5],	// 6

			template[4],	// 7 	back side
			template[3],	// 8
			template[5],	// 9

			template[3],	// 10 	left side
			template[0],	// 11 
			template[5],	// 12 
			template[2],	// 13 

			template[0],	// 14	// bottom
			template[1],	// 15
			template[3],	// 16
			template[4]		// 17
		};
		
		pb_Face[] f = new pb_Face[5]
		{
			new pb_Face(new int[3]{2, 1, 0}),			// x
			new pb_Face(new int[6]{5, 4, 3, 5, 6, 4}),	// x
			new pb_Face(new int[3]{9, 8, 7}),
			new pb_Face(new int[6]{12, 11, 10, 12, 13, 11}),
			new pb_Face(new int[6]{14, 15, 16, 15, 17, 16})
		};

		pb_Object pb = pb_Object.CreateInstanceWithVerticesFaces(v, f);
		pb.RebuildFaceCaches();
		pb.gameObject.name = "Prism";
		return pb;
	}

	/**
	 *	\brief Returns a pb_Object door with the passed parameters.
	 * @param totalWidth the total width of the door
	 * @param totalHeight the total height of the door
	 * @param ledgeHeight the height between the top of the door frame and top of the object
	 * @param legWidth the width of each leg on both sides of the door
	 * @param depth the distance between the front and back faces of the door object
	 * \returns New #pb_Object.
	 */
	public static pb_Object DoorGenerator(float totalWidth, float totalHeight, float ledgeHeight, float legWidth, float depth)
	{

	  float xLegCoord = totalWidth/2f;
	  legWidth = xLegCoord - legWidth;
	  ledgeHeight = totalHeight - ledgeHeight;

		Vector3[] template = new Vector3[12]	  // front verts
		{
		  
			// _ _ _ _
			new Vector3(-xLegCoord, 0f, depth),			  // 0
			new Vector3(-legWidth, 0f, depth),			  // 1
			new Vector3(legWidth, 0f, depth),			  // 2
			new Vector3(xLegCoord, 0f, depth),			  // 3
			// . . . .
			// _ _ _ _ 
			new Vector3(-xLegCoord, ledgeHeight, depth),  // 4
			new Vector3(-legWidth, ledgeHeight, depth),	  // 5
			new Vector3(legWidth, ledgeHeight, depth),	  // 6
			new Vector3(xLegCoord, ledgeHeight, depth),	  // 7
			// - - - - 
			// . . . . 
			// _ _ _ _
			new Vector3(-xLegCoord, totalHeight, depth),  // 8
			new Vector3(-legWidth, totalHeight, depth),	  // 9
			new Vector3(legWidth, totalHeight, depth),	  // 10
			new Vector3(xLegCoord, totalHeight, depth)	  // 11
			
		};

		// front face
		Vector3[] v = new Vector3[30]
		{
			template[0],	// left
			template[1],	// left
			template[4],	// left
			template[1],	// left		
			template[5],	// left		
			template[4],	// left	
			
			template[4],	// top left
			template[5],
			template[8],
			template[5],
			template[9],	
			template[8],

			template[5],	// mid center
			template[6],	// mid center
			template[9],	// mid center
			template[6],	// mid center
			template[10],	// mid center
			template[9],	// mid center

			template[6],	// right top
			template[7],
			template[10],
			template[7],
			template[11],
			template[10],

			template[2],	// right mid
			template[3],
			template[6],	
			template[3],
			template[7],
			template[6],

		};

		System.Array.Resize(ref v, 88);

		for (int i = 30; i < 60; i++) {
		  v[i] = v[i - 30];
		  v[i].z = -v[i].z;
		}

		// // build inside frame
		// left inside
		v[60+0] = template[1];
		v[60+1] = new Vector3( template[1].x, template[1].y, -template[1].z);
		v[60+2] = template[5];
		v[60+3] = new Vector3( template[5].x, template[5].y, -template[5].z);

		// top inside arch
		v[60+4] = template[5];
		v[60+5] = new Vector3( template[5].x, template[5].y, -template[5].z);
		v[60+6] = template[6];
		v[60+7] = new Vector3( template[6].x, template[6].y, -template[6].z);
		
		// right inside
		v[60+8] = template[6];
		v[60+9] = new Vector3( template[6].x, template[6].y, -template[6].z);
		v[60+10] = template[2];
		v[60+11] = new Vector3( template[2].x, template[2].y, -template[2].z);

		int[] tris = new int[30]
		{
			0, 1, 2, 3, 4, 5,
			6, 7, 8, 9, 10, 11,
			12, 13, 14, 15, 16, 17,
			18, 19, 20, 21, 22, 23,
			24, 25, 26, 27, 28, 29
		};
		
		System.Array.Resize(ref tris, 78);

		// copy and flip tris
		for(int i = 30; i < 60; i+=3)
		{
			tris[i+2] = tris[i-30] + 30;
			tris[i+1] = tris[i-29] + 30;
			tris[i+0] = tris[i-28] + 30;
		}

		int vInd = 60;
		for(int i = 60; i < 78; i+=6)
		{
			tris[i+0] = vInd+0;
			tris[i+1] = vInd+1;
			tris[i+2] = vInd+2;

			tris[i+3] = vInd+1;
			tris[i+4] = vInd+3;
			tris[i+5] = vInd+2;

			vInd+=4;
		}

		pb_Face[] f = new pb_Face[13];

		for(int i = 0; i < 13; i++)
		{
			int[] seg = new int[6];
			System.Array.Copy(tris, i*6, seg, 0, 6);
			f[i] = new pb_Face(seg);
		}

		pb_Object pb = pb_Object.CreateInstanceWithVerticesFaces(v, f);
		pb.gameObject.name = "Door";
		return pb;
	}

	/**
	 *	\brief Returns a pb_Object plane with the passed size.
	 * @param _width Plane width.
	 * @param _height Plane height.
	 * @param _widthCuts Divisions on the X axis.
	 * @param _heightCuts Divisions on the Y axis.
	 * @param axis The axis to build the plane on.  Ex: ProBuilder.Axis.Up is a plane with a normal of Vector3.up.
	 * @param smooth (Unsupported) Toggles the sharing of vertices in triangle array.  Default is false.
	 * \returns New #pb_Object.
	 */
	public static pb_Object PlaneGenerator(float _width, float _height, int widthCuts, int heightCuts, Axis axis, bool smooth)
	{
		float width = _width;
		float height = _height;

		int w = widthCuts+1;
		int h = heightCuts+1;

		Vector2[] p = (smooth) ? new Vector2[w*h] : new Vector2[ (w*h) * 4 ];
		Vector3[] v = (smooth) ? new Vector3[w*h] : new Vector3[ (w*h) * 4 ];
		pb_Face[] f = new pb_Face[w*h];

		int i = 0, j = 0;
		{
			for(int y = 0; y < h; y++)
			{
				for(int x = 0; x < w; x++)
				{
					float x0 = x*(width/w) - (width/2f) - ((width/w)/w);
					float x1 = (x+1)*(width/w) - (width/2f) - ((width/w)/w);

					float y0 = y*(height/h)  - (height/2f) - ((height/h)/h);
					float y1 = (y+1)*(height/h)  - (height/2f) - ((height/h)/h);

					p[i+0] = new Vector2(x0, 	y0);
					p[i+1] = new Vector2(x1, 	y0);
					p[i+2] = new Vector2(x0, 	y1);
					p[i+3] = new Vector2(x1, 	y1);

					f[j++] = new pb_Face( new int[6]
						{
							i+0,
							i+1,
							i+2,
							i+1,
							i+3,
							i+2
						});

					i+=4;
				}
			}
		}

		switch(axis)
		{
			case Axis.Right:
				for(i = 0; i < v.Length; i++)
					v[i] = new Vector3(0f, p[i].x, p[i].y);
				break;
			case Axis.Left:
				for(i = 0; i < v.Length; i++)
					v[i] = new Vector3(0f, p[i].y, p[i].x);
				break;
			case Axis.Up:
				for(i = 0; i < v.Length; i++)
					v[i] = new Vector3(p[i].y, 0f, p[i].x);
				break;
			case Axis.Down:
				for(i = 0; i < v.Length; i++)
					v[i] = new Vector3(p[i].x, 0f, p[i].y);
				break;				
			case Axis.Forward:
				for(i = 0; i < v.Length; i++)
					v[i] = new Vector3(p[i].x, p[i].y, 0f);
				break;
			case Axis.Backward:
				for(i = 0; i < v.Length; i++)
					v[i] = new Vector3(p[i].y, p[i].x, 0f);
				break;				
		}
		pb_Object pb;

		pb = pb_Object.CreateInstanceWithVerticesFaces(v, f);
		
		pb.gameObject.name = "Plane";

		Vector3 center = Vector3.zero;
		Vector3[] verts = pb.VerticesInWorldSpace();
		foreach (Vector3 vector in verts)
			center += vector;
	
		center /= verts.Length;

		Vector3 dir = (pb.transform.position - center);

		pb.transform.position = center;

		pb.TranslateVertices_World(pb.msh.triangles, dir);

		return pb;
	}

	/**
	 *	\brief Returns a pb_Object pipe with the passed size.
	 * @param radius Radius of the generated pipe.
	 * @param height Height of the generated pipe.
	 * @param thickness How thick the walls will be.
	 * @param subdivAxis How many subdivisions on the axis.
	 * @param subdivHeight How many subdivisions on the Y axis.
	 * \returns New #pb_Object.
	 */
	public static pb_Object PipeGenerator(
		float radius, float height, float thickness, int subdivAxis, int subdivHeight) 
	{

		// template is outer ring - radius refers to outer ring always
		Vector2[] templateOut = new Vector2[subdivAxis];
		Vector2[] templateIn = new Vector2[subdivAxis];

		for(int i = 0; i < subdivAxis; i++)
		{
			templateOut[i] = pb_Math.PointInCircumference(radius, i*(360f/subdivAxis), Vector2.zero);
			templateIn[i] = pb_Math.PointInCircumference(radius-thickness, i*(360f/subdivAxis), Vector2.zero);
		}

		List<Vector3> v = new List<Vector3>();

		subdivHeight += 1;

		// build out sides
		Vector2 tmp, tmp2, tmp3, tmp4;
		for(int i = 0; i < subdivHeight; i++)
		{
			// height subdivisions
			float y = i*(height/subdivHeight );
			float y2 = (i+1)*(height/subdivHeight );

			for(int n = 0; n < subdivAxis; n++)
			{
				tmp = templateOut[n];
				tmp2 = n < (subdivAxis-1) ? templateOut[n+1] : templateOut[0];

				// outside quads
				Vector3[] qvo = new Vector3[4]
				{
					new Vector3(tmp2.x, y, tmp2.y),
					new Vector3(tmp.x, y, tmp.y),
					new Vector3(tmp2.x, y2, tmp2.y),
					new Vector3(tmp.x, y2, tmp.y)
				};

				// inside quad
				tmp = templateIn[n];
				tmp2 = n < (subdivAxis-1) ? templateIn[n+1] : templateIn[0];
				Vector3[] qvi = new Vector3[4]
				{
					new Vector3(tmp.x, y, tmp.y),
					new Vector3(tmp2.x, y, tmp2.y),
					new Vector3(tmp.x, y2, tmp.y),
					new Vector3(tmp2.x, y2, tmp2.y)
				};

				v.AddRange(qvo);
				v.AddRange(qvi);
			}
		}

		// build top and bottom
		for(int i = 0; i < subdivAxis; i++)
		{
			tmp = templateOut[i];
			tmp2 = (i < subdivAxis-1) ? templateOut[i+1] : templateOut[0];
			tmp3 = templateIn[i];
			tmp4 = (i < subdivAxis-1) ? templateIn[i+1] : templateIn[0];
			
			// top
			Vector3[] tpt = new Vector3[4]
			{
				new Vector3(tmp2.x, height, tmp2.y),
				new Vector3(tmp.x,  height, tmp.y),
				new Vector3(tmp4.x, height, tmp4.y),
				new Vector3(tmp3.x, height, tmp3.y)
			};

			// top
			Vector3[] tpb = new Vector3[4]
			{
				new Vector3(tmp.x, 0f, tmp.y),
				new Vector3(tmp2.x, 0f, tmp2.y),
				new Vector3(tmp3.x, 0f, tmp3.y),
				new Vector3(tmp4.x, 0f, tmp4.y),
			};

			v.AddRange(tpb);		
			v.AddRange(tpt);		
		}

		pb_Object pb = pb_Object.CreateInstanceWithPoints(v.ToArray());

		pb.gameObject.name = "Pipe";

		return pb;
	}

	/**
	 * \brief Returns a pb_Object cone with the passed size.
	 * @param radius Radius of the generated cone.
	 * @param height How tall the cone will be.
	 * @param subdivAxis How many subdivisions on the axis.
	 * \returns New #pb_Object.
	 */
	public static pb_Object ConeGenerator(
		float radius, float height, int subdivAxis)
	{
		// template is outer ring - radius refers to outer ring always
		Vector3[] template = new Vector3[subdivAxis];

		for(int i = 0; i < subdivAxis; i++)
		{
			Vector2 ct = pb_Math.PointInCircumference(radius, i*(360f/subdivAxis), Vector2.zero);
			template[i] = new Vector3(ct.x, 0f, ct.y);
		}
			
		List<Vector3> v = new List<Vector3>();
		List<pb_Face> f = new List<pb_Face>();

		// build sides
		for(int i = 0; i < subdivAxis; i++)
		{
			// side face
			v.Add(template[i]);
			v.Add((i < subdivAxis-1) ? template[i+1] : template[0]);
			v.Add(Vector3.up * height);

			// bottom face
			v.Add(template[i]);
			v.Add((i < subdivAxis-1) ? template[i+1] : template[0]);
			v.Add(Vector3.zero);
		}

		for(int i = 0; i < subdivAxis*6; i+=6)
		{
			f.Add( new pb_Face( new int[3] {i+2,i+1,i+0} ) );
			f.Add( new pb_Face( new int[3] {i+3,i+4,i+5} ) );
		}

		pb_Object pb = pb_Object.CreateInstanceWithVerticesFaces(v.ToArray(), f.ToArray());
		pb.gameObject.name = "Cone";
		return pb;
	}

	/**
	  * \brief Returns a pb_Object cone with the passed size.
	  * @param angle amount of a circle the arch takes up
	  * @param radius distance from origin to furthest extent of geometry
	  * @param width distance from arch top to inner radius
	  * @param depth depth of arch blocks
	  * @param radialCuts how many blocks compose the arch
	  * @param insideFaces render inside faces toggle
	  * @param outsideFaces render outside faces toggle
	  * @param frontFaces render front faces toggle
	  * @param backFaces render back faces toggle
	  * \returns New #pb_Object.
	  */
	public static pb_Object ArchGenerator(
		float angle, float radius, float width, float depth, int radialCuts, bool insideFaces, bool outsideFaces, bool frontFaces, bool backFaces, bool endCaps)
	{
		Vector2[] templateOut = new Vector2[radialCuts];
		Vector2[] templateIn = new Vector2[radialCuts];

		for (int i = 0; i < radialCuts; i++)
		{
			templateOut[i] = pb_Math.PointInCircumference(radius, i * (angle / (radialCuts-1)), Vector2.zero);
			templateIn[i] = pb_Math.PointInCircumference(radius - width, i * (angle / (radialCuts-1)), Vector2.zero);
		}

		List<Vector3> v = new List<Vector3>();

		Vector2 tmp, tmp2, tmp3, tmp4;

		float y = 0;

		for (int n = 0; n < radialCuts-1; n++)
		{
			// outside faces
			tmp = templateOut[n];
			tmp2 = n < (radialCuts - 1) ? templateOut[n + 1] : templateOut[n];

			Vector3[] qvo = new Vector3[4]
			{
				new Vector3(tmp.x, tmp.y, y),
				new Vector3(tmp2.x, tmp2.y, y),
				new Vector3(tmp.x, tmp.y, depth),
				new Vector3(tmp2.x, tmp2.y, depth)
			};

			// inside faces
			tmp = templateIn[n];
			tmp2 = n < (radialCuts - 1) ? templateIn[n + 1] : templateIn[n];

			Vector3[] qvi = new Vector3[4]
			{
				new Vector3(tmp2.x, tmp2.y, y),
				new Vector3(tmp.x, tmp.y, y),
				new Vector3(tmp2.x, tmp2.y, depth),
				new Vector3(tmp.x, tmp.y, depth)
			};

			if(outsideFaces)
				v.AddRange(qvo);

			if(n != radialCuts-1 && insideFaces)
				v.AddRange(qvi);

			// left side bottom face
			if( angle < 360f && endCaps)
			{
				if(n == 0)
				{
					v.AddRange(
						new Vector3[4]
						{
							new Vector3(templateOut[n].x, templateOut[n].y, depth),
							new Vector3(templateIn[n].x, templateIn[n].y, depth),
							new Vector3(templateOut[n].x, templateOut[n].y, y),
							new Vector3(templateIn[n].x, templateIn[n].y, y)
						});
				}
				
				// ride side bottom face
				if(n == radialCuts-2)
				{
					v.AddRange(
						new Vector3[4]
						{
							new Vector3(templateIn[n+1].x, templateIn[n+1].y, depth),
							new Vector3(templateOut[n+1].x, templateOut[n+1].y, depth),
							new Vector3(templateIn[n+1].x, templateIn[n+1].y, y),
							new Vector3(templateOut[n+1].x, templateOut[n+1].y, y)
						});
				}
			}
		}

		// build front and back faces
		for (int i = 0; i < radialCuts-1; i++)
		{
			tmp = templateOut[i];
			tmp2 = (i < radialCuts - 1) ? templateOut[i + 1] : templateOut[i];
			tmp3 = templateIn[i];
			tmp4 = (i < radialCuts - 1) ? templateIn[i + 1] : templateIn[i];

			// front
			Vector3[] tpb = new Vector3[4]
			{
				new Vector3(tmp.x, tmp.y, depth),
				new Vector3(tmp2.x, tmp2.y, depth),
				new Vector3(tmp3.x, tmp3.y, depth),
				new Vector3(tmp4.x, tmp4.y, depth),
			};

			// back
			Vector3[] tpt = new Vector3[4]
			{
				new Vector3(tmp2.x, tmp2.y, 0f),
				new Vector3(tmp.x,  tmp.y, 0f),
				new Vector3(tmp4.x, tmp4.y, 0f),
				new Vector3(tmp3.x, tmp3.y, 0f)
			};

			if(frontFaces)
				v.AddRange(tpb);
			if(backFaces)
				v.AddRange(tpt);
		}

		pb_Object pb = pb_Object.CreateInstanceWithPoints(v.ToArray());

		pb.gameObject.name = "Arch";
		return pb;
	}
	
	static readonly Vector3[] ICOSAHEDRON_VERTICES = new Vector3[12]
	{
		new Vector3(-1f,  pb_Math.PHI,  0f),
		new Vector3( 1f,  pb_Math.PHI,  0f),
		new Vector3(-1f, -pb_Math.PHI,  0f),
		new Vector3( 1f, -pb_Math.PHI,  0f),

		new Vector3( 0f, -1f,  pb_Math.PHI),
		new Vector3( 0f,  1f,  pb_Math.PHI),
		new Vector3( 0f, -1f, -pb_Math.PHI),
		new Vector3( 0f,  1f, -pb_Math.PHI),

		new Vector3(  pb_Math.PHI, 0f, -1f),
		new Vector3(  pb_Math.PHI, 0f,  1f),
		new Vector3( -pb_Math.PHI, 0f, -1f),
		new Vector3( -pb_Math.PHI, 0f,  1f)
	};

	static readonly int[] ICOSAHEDRON_TRIANGLES = new int[60]
	{
		0, 11, 5,
		0, 5, 1,
		0, 1, 7,
		0, 7, 10,
		0, 10, 11,

		1, 5, 9,
		5, 11, 4,
		11, 10, 2,
		10, 7, 6,
		7, 1, 8,
		 
		3, 9, 4,
		3, 4, 2,
		3, 2, 6,
		3, 6, 8,
		3, 8, 9,

		4, 9, 5,
		2, 4, 11,
		6, 2, 10,
		8, 6, 7,
		9, 8, 1
	};

  	/**
	  * \brief Creates an icosphere from a radius and subdivision count.
	  * This method does not extract shared indices, so after generating
	  * make sure to use pbVertexOps.Weldvertices() to generate them.
	  */
	public static pb_Object IcosahedronGenerator(float radius, int subdivisions)
	{
		// http://blog.andreaskahler.com/2009/06/creating-icosphere-mesh-in-code.html

		Vector3[] v = new Vector3[ICOSAHEDRON_TRIANGLES.Length];

		/**
		 * Regular Icosahedron - 12 vertices, 20 faces.
		 */
		for(int i = 0; i < ICOSAHEDRON_TRIANGLES.Length; i+=3)
		{
			v[i+0] = ICOSAHEDRON_VERTICES[ ICOSAHEDRON_TRIANGLES[i+0] ].normalized * radius;
			v[i+1] = ICOSAHEDRON_VERTICES[ ICOSAHEDRON_TRIANGLES[i+1] ].normalized * radius;
			v[i+2] = ICOSAHEDRON_VERTICES[ ICOSAHEDRON_TRIANGLES[i+2] ].normalized * radius;
		}

		/**
		 * Subdivide
		 */
		for(int i = 0; i < subdivisions; i++) {
			v = SubdivideIcosahedron(v, radius);
		}

		/**
		 * Wind faces
		 */
		pb_Face[] f = new pb_Face[v.Length/3];
		for(int i = 0; i < v.Length; i+=3) {
			f[i/3] = new pb_Face( new int[3] { i, i+1, i+2 } );
			f[i/3].manualUV = true;
		}

		GameObject _gameObject = new GameObject();	
		pb_Object pb = _gameObject.AddComponent<pb_Object>();

		pb.SetVertices(v);
		pb.SetUV(new Vector2[v.Length]);
		pb.SetFaces(f);

		pb_IntArray[] si = new pb_IntArray[v.Length];
		for(int i = 0; i < si.Length; i++)
			si[i] = new pb_IntArray(new int[] { i });

		pb.SetSharedIndices(si);

		pb.ToMesh();
		pb.Refresh();

		pb.gameObject.name = "Icosphere";

		return pb;
	}

	/**
	 * Subdivides a set of vertices (wound as individual triangles) on an icosphere.
	 *
	 *	 /\			 /\
	 * 	/  \	-> 	/--\
	 * /____\	   /_\/_\
	 *
	 */
	static Vector3[] SubdivideIcosahedron(Vector3[] vertices, float radius)
	{
		Vector3[] v = new Vector3[vertices.Length * 4];

		int index = 0;

		Vector3 p0 = Vector3.zero,	//	    5
				p1 = Vector3.zero,	//    3   4
				p2 = Vector3.zero,	//	0,  1,  2
				p3 = Vector3.zero,
				p4 = Vector3.zero,
				p5 = Vector3.zero;

		for(int i = 0; i < vertices.Length; i+=3)
		{
			p0 = vertices[i+0];
			p2 = vertices[i+1];
			p5 = vertices[i+2];
			p1 = ((p0 + p2) * .5f).normalized * radius;
			p3 = ((p0 + p5) * .5f).normalized * radius;
			p4 = ((p2 + p5) * .5f).normalized * radius;

			v[index++] = p0;
			v[index++] = p1;
			v[index++] = p3;

			v[index++] = p1;
			v[index++] = p2;
			v[index++] = p4;

			v[index++] = p1;
			v[index++] = p4;
			v[index++] = p3;

			v[index++] = p3;
			v[index++] = p4;
			v[index++] = p5;
		}

		return v;
	}

	static Vector3[] CircleVertices(int segments, float radius, float circumference, Quaternion rotation, float offset)
	{
		float seg = (float)segments-1;

		Vector3[] v = new Vector3[ (segments -1 ) * 2];
		v[0] = new Vector3(Mathf.Cos( ((0f/seg) * circumference) * Mathf.Deg2Rad ) * radius, Mathf.Sin(((0f/seg) * circumference) * Mathf.Deg2Rad) * radius, 0f);
		v[1] = new Vector3(Mathf.Cos( ((1f/seg) * circumference) * Mathf.Deg2Rad ) * radius, Mathf.Sin(((1f/seg) * circumference) * Mathf.Deg2Rad) * radius, 0f);

		v[0] = rotation * ((v[0] + Vector3.right * offset));
		v[1] = rotation * ((v[1] + Vector3.right * offset));

		int n = 2;

		System.Text.StringBuilder sb = new System.Text.StringBuilder();

		for(int i = 2; i < segments; i++)
		{
			float rad = ((i/seg) * circumference) * Mathf.Deg2Rad;
			sb.AppendLine(rad.ToString());

			v[n+0] = v[n-1];
			v[n+1] = rotation * (new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0f) + Vector3.right * offset);

			n += 2;
		}

		return v;
	}

	/**
	 * Create a torus mesh.
	 */
	public static pb_Object TorusGenerator(int InRows, int InColumns, float InRadius, float InTubeRadius, bool InSmooth, float InHorizontalCircumference, float InVerticalCircumference)
	{
		int rows 	= (int) Mathf.Clamp( InRows + 1, 4, 128 );
		int columns = (int) Mathf.Clamp( InColumns + 1, 4, 128 );
		float radius = Mathf.Clamp(InRadius, .01f, 2048f);
		float tubeRadius = Mathf.Clamp(InTubeRadius, .01f, radius);
		radius -= tubeRadius;
		float horizontalCircumference = Mathf.Clamp(InHorizontalCircumference, .01f, 360f);
		float verticalCircumference = Mathf.Clamp(InVerticalCircumference, .01f, 360f);

		List<Vector3> vertices = new List<Vector3>();

		int col = columns - 1;

		Vector3[] cir = CircleVertices(rows, tubeRadius, verticalCircumference, Quaternion.Euler(Vector3.up * 0f * horizontalCircumference), radius);

		for(int i = 1; i < columns; i++)
		{
			vertices.AddRange(cir);
			Quaternion rotation = Quaternion.Euler(Vector3.up * ((i/(float)col) * horizontalCircumference));
			cir = CircleVertices(rows, tubeRadius, verticalCircumference, rotation, radius);
			vertices.AddRange(cir);
		}

		// List<int> ind = new List<int>();
		List<pb_Face> faces = new List<pb_Face>();
		int fc = 0;

		// faces
		for(int i = 0; i < (columns-1) * 2; i += 2)
		{
			for(int n = 0; n < rows-1; n++)
			{
				int a = (i+0) * ((rows-1) * 2) + (n * 2);
				int b = (i+1) * ((rows-1) * 2) + (n * 2);

				int c = (i+0) * ((rows-1) * 2) + (n * 2) + 1;
				int d = (i+1) * ((rows-1) * 2) + (n * 2) + 1;

				faces.Add( new pb_Face(new int[] { a, b, c, b, d, c } ) );
				faces[fc].SetSmoothingGroup(InSmooth ? 1 : 0);
				faces[fc++].manualUV = true;
			}
		}

		pb_Object pb = pb_Object.CreateInstanceWithVerticesFaces(vertices.ToArray(), faces.ToArray());
		pb.gameObject.name = "Torus";

		return pb;
	}
}
}
