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
        /// <param name="shape">The ShapeType to create.</param>
        /// <param name="pivotType">Where the shape's pivot will be.</param>
        /// <returns>A new GameObject with the ProBuilderMesh initialized to the primitve shape.</returns>
        public static ProBuilderMesh Instantiate<T>(PivotLocation pivotType = PivotLocation.Center) where T : ShapePrimitive, new()
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

            if (shapeType.IsAssignableFrom(typeof(ShapePrimitive)))
                throw new ArgumentException("Type needs to derive from Shape");

            try
            {
                var shape = Activator.CreateInstance(shapeType,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance,
                    null, null, null, null) as ShapePrimitive;
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
        /// <param name="shapePrimitive">The ShapeType to create.</param>
        /// <param name="pivotType">Where the shape's pivot will be.</param>
        /// <returns>A new GameObject with the ProBuilderMesh initialized to the primitve shape.</returns>
        public static ProBuilderMesh Instantiate(ShapePrimitive shapePrimitive, PivotLocation pivotType = PivotLocation.Center)
        {
            if (shapePrimitive == null)
                throw new ArgumentNullException("shapePrimitive", "Cannot instantiate a null shape.");

            var shapeComponent = new GameObject("Shape").AddComponent<ProBuilderShape>();
            shapeComponent.SetShape(shapePrimitive, pivotType);
            ProBuilderMesh pb = shapeComponent.mesh;
            pb.renderer.sharedMaterial = BuiltinMaterials.defaultMaterial;

            var attribute = Attribute.GetCustomAttribute(shapePrimitive.GetType(), typeof(ShapePrimitiveAttribute));

            if(attribute is ShapePrimitiveAttribute shapeAttrib)
                pb.gameObject.name = shapeAttrib.name;

            return pb;
        }
    }
}
