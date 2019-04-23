using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UObject = UnityEngine.Object;
using NUnit.Framework;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEngine.ProBuilder.RuntimeTests.MeshOperations
{
	public class ConvertTextureUnwrappingAutoManualTests
	{
		public ProBuilderMesh mesh;
		public ProBuilder.Face face;
		public ProBuilder.Edge verticalEdge;
		public ProBuilder.Edge horizontalEdge;

		[SetUp]
		public void Setup()
		{
			mesh = ShapeGenerator.CreateShape(ShapeType.Sprite);
			face = mesh.faces.First();
			verticalEdge = face.edgesInternal[0];
			horizontalEdge = face.edgesInternal[1];

			// Verify that UVs are actually rotated
			Assume.That(face.manualUV, Is.EqualTo(false));

		}

		[TearDown]
		public void Teardown()
		{
			UObject.DestroyImmediate(mesh.gameObject);
		}

		static Vector2[] AutoUVOffsetParameters = new[]
		{
			new Vector2( 0f,  0f),
			new Vector2( 1f,  0f),
			new Vector2( 0f,  1f),
			new Vector2( 1f,  1f),
			new Vector2(-1f, 0f),
			new Vector2( 0f, -1f),
			new Vector2(-1f, -1f),
		};

		static float[] AutoUVRotationParameters = new[]
		{
			0f,
			60f,
			180f,
			200f,
			359f
		};

		static Vector2[] AutoUVScaleParameters = new[]
		{
			new Vector2(1f, 1f),
			new Vector2(2f, 2f),
			new Vector2(.5f, .5f),
			new Vector2(.2f, 1f),
			new Vector2(1f, .2f),
		};

		static float GetEdgeRotation(ProBuilderMesh mesh, ProBuilder.Edge edge)
		{
			var dir = mesh.texturesInternal[edge.b] - mesh.texturesInternal[edge.a];
			var angle = Vector2.Angle(Vector2.up, dir);
			if (Vector2.Dot(Vector2.right, dir) < 0)
				angle = 360f - angle;
			return angle;
		}

		static float GetEdgeScale(ProBuilderMesh mesh, ProBuilder.Edge edge)
		{
			return Vector2.Distance(mesh.texturesInternal[edge.b], mesh.texturesInternal[edge.a]);
		}

		[Test]
		public void SetManualFaceToAuto_MatchesOriginalUVs(
			[ValueSource("AutoUVOffsetParameters")] Vector2 offset,
			[ValueSource("AutoUVRotationParameters")] float rotation,
			[ValueSource("AutoUVScaleParameters")] Vector2 scale)
		{
			var unwrap = face.uv;
			unwrap.offset = offset;
			unwrap.rotation = rotation;
			unwrap.scale = scale;

			face.uv = unwrap;

			// Verify that UV settings have actually been applied
			Assume.That(face.uv.offset, Is.EqualTo(offset));
			Assume.That(face.uv.rotation, Is.EqualTo(rotation));
			Assume.That(face.uv.scale, Is.EqualTo(scale));
			Assume.That(face.manualUV, Is.EqualTo(false));

			mesh.Refresh(RefreshMask.UV);

			// Verify that the UVs are in the correct place
			Assume.That(GetEdgeRotation(mesh, verticalEdge), Is.EqualTo(rotation).Within(.1f));
			Assume.That(GetEdgeScale(mesh, verticalEdge), Is.EqualTo(scale.y).Within(.1f));
			Assume.That(GetEdgeScale(mesh, horizontalEdge), Is.EqualTo(scale.x).Within(.1f));
			// Offset is flipped in code for legacy reasons
			var center = Bounds2D.Center(mesh.texturesInternal, face.distinctIndexesInternal);
			Assume.That(center.x, Is.EqualTo(-offset.x).Within(.1f));
			Assume.That(center.y, Is.EqualTo(-offset.y).Within(.1f));

			face.uv = AutoUnwrapSettings.defaultAutoUnwrapSettings;
			face.manualUV = true;

			// Verify that UV settings have been reset
			Assume.That(face.uv.offset, Is.EqualTo(new Vector2(0f, 0f)));
			Assume.That(face.uv.rotation, Is.EqualTo(0f));
			Assume.That(face.uv.scale, Is.EqualTo(new Vector2(1f, 1f)));
			Assume.That(face.manualUV, Is.EqualTo(true));

			// This sets the manualFlag to false, sets the AutoUnwrap settings, and rebuilds UVs
			UVEditing.SetAutoAndAlignUnwrapParamsToUVs(mesh, new [] { face });

			Assert.That(face.uv.offset.x, Is.EqualTo(offset.x).Within(.1f));
			Assert.That(face.uv.offset.y, Is.EqualTo(offset.y).Within(.1f));
			Assert.That(face.uv.rotation, Is.EqualTo(rotation).Within(.1f));
			Assert.That(face.uv.scale.x, Is.EqualTo(scale.x).Within(.1f));
			Assert.That(face.uv.scale.y, Is.EqualTo(scale.y).Within(.1f));
			Assert.That(face.manualUV, Is.EqualTo(false));
		}
	}
}
