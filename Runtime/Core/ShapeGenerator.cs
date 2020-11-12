using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Shapes;
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

            obj.size = Vector3.one;
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

            ShapeAttribute attribute = Attribute.GetCustomAttribute(shape.GetType(), typeof(ShapeAttribute)) as ShapeAttribute;
            pb.gameObject.name = attribute != null ? attribute.name : shape.GetType().ToString();

            return pb;
        }


        /// <summary>
        /// Create a new cone shape.
        /// </summary>
        /// <param name="pivotType">Where the shape's pivot will be.</param>
        /// <param name="radius">Radius of the generated cone.</param>
        /// <param name="height">How tall the cone will be.</param>
        /// <param name="subdivAxis">How many subdivisions on the axis.</param>
        /// <returns>A new GameObject with a reference to the ProBuilderMesh component.</returns>
        public static ProBuilderMesh GenerateCone(PivotLocation pivotType, float radius, float height, int subdivAxis)
        {
            ProBuilderMesh pb = CreateShape(typeof(Cone), pivotType);
            Cone cone = pb.GetComponent<ShapeComponent>().shape as Cone;
            cone.m_NumberOfSides = subdivAxis;
            cone.RebuildMesh(pb, new Vector3(radius, height, radius));

            pb.RefreshUV(pb.faces);
            return pb;
        }

    }
}
