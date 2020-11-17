using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Shapes;
using System;
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

    public static class ShapeFactory
    {
        /// <summary>
        /// Create a shape with default parameters.
        /// </summary>
        /// <param name="shape">The ShapeType to create.</param>
        /// <param name="pivotType">Where the shape's pivot will be.</param>
        /// <returns>A new GameObject with the ProBuilderMesh initialized to the primitve shape.</returns>
        public static ProBuilderMesh Instantiate<T>(PivotLocation pivotType = PivotLocation.Center) where T : Shape, new()
        {
            return Instantiate(typeof(T));
        }

        /// <summary>
        /// Create a shape with default parameters.
        /// </summary>
        /// <param name="shape">The ShapeType to create.</param>
        /// <param name="pivotType">Where the shape's pivot will be.</param>
        /// <returns>A new GameObject with the ProBuilderMesh initialized to the primitve shape.</returns>
        public static ProBuilderMesh Instantiate(Type shapeType, PivotLocation pivotType = PivotLocation.Center)
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
                return Instantiate(shape, pivotType);
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
        public static ProBuilderMesh Instantiate(Shape shape, PivotLocation pivotType = PivotLocation.Center)
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
    }
}
