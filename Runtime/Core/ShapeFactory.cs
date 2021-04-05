using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Shapes;
using System;
using System.Reflection;
using UnityEditor;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Describes the type of pivot ProBuilder would automatically assign on primitive creation.
    /// </summary>
    public enum PivotLocation
    {
        Center,
        FirstCorner
    }

    public static class ShapeFactory
    {
        /// <summary>
        /// Create a shape with default parameters.
        /// </summary>
        /// <param name="pivotType">Where the shape's pivot will be.</param>
        /// <returns>A new GameObject with the ProBuilderMesh initialized to the primitive shape.</returns>
        public static ProBuilderMesh Instantiate<T>(PivotLocation pivotType = PivotLocation.Center) where T : Shape, new()
        {
            return Instantiate(typeof(T));
        }

        /// <summary>
        /// Create a shape with default parameters.
        /// </summary>
        /// <param name="shapeType">The ShapeType to create.</param>
        /// <param name="pivotType">Where the shape's pivot will be.</param>
        /// <returns>A new GameObject with the ProBuilderMesh initialized to the primitive shape.</returns>
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
        /// <returns>A new GameObject with the ProBuilderMesh initialized to the primitive shape.</returns>
        public static ProBuilderMesh Instantiate(Shape shape, PivotLocation pivotType = PivotLocation.Center)
        {
            if (shape == null)
                throw new ArgumentNullException("shape", "Cannot instantiate a null shape.");

            var shapeComponent = new GameObject("Shape").AddComponent<ProBuilderShape>();
            shapeComponent.SetShape(shape, pivotType);
            ProBuilderMesh pb = shapeComponent.mesh;
            pb.renderer.sharedMaterial = BuiltinMaterials.defaultMaterial;

            var attribute = Attribute.GetCustomAttribute(shape.GetType(), typeof(ShapeAttribute));

            if(attribute is ShapeAttribute shapeAttrib)
                pb.gameObject.name = shapeAttrib.name;

            return pb;
        }
    }
}
