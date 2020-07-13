using UnityEngine;
using System.Collections.Generic;
using UnityEngine.ProBuilder.MeshOperations;
using System;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Describes the type of pivot ProBuilder would automatically assign on primitive creation.
    /// </summary>
    public enum PivotLocation
    {
        Center,
        FirstVertex
    }

    public static class ShapeGenerator
    {
        /// <summary>
        /// Create a shape with default parameters.
        /// </summary>
        /// <param name="shape">The ShapeType to create.</param>
        /// <param name="pivotType">Where the shape's pivot will be.</param>
        /// <returns>A new GameObject with the ProBuilderMesh initialized to the primitve shape.</returns>
        public static ProBuilderMesh CreateShape<T>(PivotLocation pivotType = PivotLocation.Center) where T : Shape, new()
        {
            return CreateShape(typeof(T));
        }

        /// <summary>
        /// Create a shape with default parameters.
        /// </summary>
        /// <param name="shape">The ShapeType to create.</param>
        /// <param name="pivotType">Where the shape's pivot will be.</param>
        /// <returns>A new GameObject with the ProBuilderMesh initialized to the primitve shape.</returns>
        public static ProBuilderMesh CreateShape(Type shapeType, PivotLocation pivotType = PivotLocation.Center)
        {
            if (shapeType.IsAssignableFrom(typeof(Shape)))
                throw new ArgumentException("Type needs to derive from Shape");

            var shape = Activator.CreateInstance(shapeType) as Shape;
            return CreateShape(shape, pivotType);
        }

        /// <summary>
        /// Create a shape with default parameters.
        /// </summary>
        /// <param name="shape">The ShapeType to create.</param>
        /// <param name="pivotType">Where the shape's pivot will be.</param>
        /// <returns>A new GameObject with the ProBuilderMesh initialized to the primitve shape.</returns>
        public static ProBuilderMesh CreateShape(Shape shape, PivotLocation pivotType = PivotLocation.Center)
        {
            ProBuilderMesh pb = null;
            var obj = new GameObject("Shape").AddComponent<ShapeComponent>();
            obj.SetShape(shape);
            pb = obj.mesh;

            if (pb == null)
            {
#if DEBUG
                Log.Error(shape.ToString() + " type has no default!");
#endif
                return null;
            }

            if (shape.GetType() == typeof(Torus))
            {
                UVEditing.ProjectFacesBox(pb, pb.facesInternal);
            }

            pb.gameObject.name = shape.ToString();
            pb.renderer.sharedMaterial = BuiltinMaterials.defaultMaterial;

            return pb;
        }
    }
}
