using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Shapes;
using System;
using System.Globalization;
using System.Reflection;

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
            if (shapeType == null)
                throw new ArgumentNullException("shapeType", "Cannot instantiate a null shape.");

            if (shapeType.IsAssignableFrom(typeof(Shape)))
                throw new ArgumentException("Type needs to derive from Shape");

            try
            {
                var shape = Activator.CreateInstance(shapeType,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance,
                    null, null, null, null) as Shape;
                return CreateShape(shape, pivotType);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed creating shape \"{shapeType}\". Shapes must contain an empty constructor.\n{e}");
            }

            return null;
        }

        /// <summary>
        /// Create a shape with default parameters.
        /// </summary>
        /// <param name="shape">The ShapeType to create.</param>
        /// <param name="pivotType">Where the shape's pivot will be.</param>
        /// <returns>A new GameObject with the ProBuilderMesh initialized to the primitve shape.</returns>
        public static ProBuilderMesh CreateShape(Shape shape, PivotLocation pivotType = PivotLocation.Center)
        {
            if (shape == null)
                throw new ArgumentNullException("shape", "Cannot instantiate a null shape.");

            var shapeComponent = new GameObject("Shape").AddComponent<ShapeComponent>();
            shapeComponent.size = Vector3.one;
            shapeComponent.SetShape(shape);
            ProBuilderMesh pb = shapeComponent.mesh;
            pb.renderer.sharedMaterial = BuiltinMaterials.defaultMaterial;

            // Torus shape should implement this itself
            if (shape.GetType() == typeof(Torus))
                UVEditing.ProjectFacesBox(pb, pb.facesInternal);

            var attribute = Attribute.GetCustomAttribute(shape.GetType(), typeof(ShapeAttribute));

            if(attribute is ShapeAttribute shapeAttrib)
                pb.gameObject.name = shapeAttrib.name;

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
