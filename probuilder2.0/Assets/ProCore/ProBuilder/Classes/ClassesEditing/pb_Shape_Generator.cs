using UnityEngine;
using System.Collections.Generic;
using ProBuilder2.Math;
using ProBuilder2.Common;

#if PB_DEBUG
using Parabox.Debug;
#endif

/**
 *	\brief Static utility class for generating pb_Object geometry.
 *	Generally you will not need to call this, as the #ProBuilder
 *	class contains methods for creating #pb_Objects with parameters
 *	for shape.
 */
public class pb_Shape_Generator
{
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
		pb.SetName("Stairs"); 
		return pb;
	}

	public static pb_Object CubeGenerator(Vector3 size)
	{
		Vector3[] points = new Vector3[pb_Constant.TRIANGLES_CUBE.Length];
		for(int i = 0; i < pb_Constant.TRIANGLES_CUBE.Length; i++)
			points[i] = Vector3.Scale(pb_Constant.VERTICES_CUBE[pb_Constant.TRIANGLES_CUBE[i]], size);

		pb_Object pb = pb_Object.CreateInstanceWithPoints(points);
		pb.SetName("Cube");
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
		pb.SetName("Cylinder");
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
		pb.SetName("Prism");
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
		pb.SetName("Door");
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
		
		pb.SetName("Plane");

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

		pb.SetName("Pipe");

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
		pb.SetName("Cone");
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

		pb.SetName("Arch");
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
}