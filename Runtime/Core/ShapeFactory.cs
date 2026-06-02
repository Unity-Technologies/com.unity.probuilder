using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Shapes;
using System;
using System.Reflection;
using UnityEditor;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Describes the type of pivot ProBuilder assigns by default when it creates new primitives.
    /// </summary>
    public enum PivotLocation
    {
        /// <summary>Place the pivot in the middle of the bounding box.</summary>
        Center,
        /// <summary>Place the pivot at the vertex defined as the first in the shape.</summary>
        FirstVertex
    }

    /// <summary>
    /// Provides methods to instantiate GameObjects in the Editor using ProBuilder shapes.
    /// </summary>
    public static class ShapeFactory
    {
        /// <summary>
        /// Creates a default shape with default parameters.
        /// </summary>
        /// <typeparam name="T">The Shape to instantiate</typeparam>
        /// <param name="pivotType">By default, new shapes pivot around the center of the bounding box but you can specify <see cref="PivotLocation.FirstCorner" /> instead. </param>
        /// <returns>A new GameObject with the ProBuilderMesh initialized to the default primitive shape.</returns>
        public static ProBuilderMesh Instantiate<T>(PivotLocation pivotType = PivotLocation.Center) where T : Shape, new()
        {
            return Instantiate(typeof(T));
        }

        /// <summary>
        /// Creates a specific shape with default parameters.
        /// </summary>
        /// <param name="shapeType">The <see cref="ShapeType" /> to create.</param>
        /// <param name="pivotType">By default, new shapes pivot around the center of the bounding box but you can specify <see cref="PivotLocation.FirstCorner" /> instead. </param>
        /// <returns>A new GameObject with the ProBuilderMesh initialized to the specified primitive <see cref="ShapeType" />.</returns>
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
                return Instantiate(shape);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed creating shape \"{shapeType}\". Shapes must contain an empty constructor.\n{e}");
            }

            return null;
        }

        /// <summary>
        /// Creates a specific shape with default parameters.
        /// </summary>
        /// <param name="shape">The <see cref="Shape" /> to create.</param>
        /// <returns>A new GameObject with the ProBuilderMesh initialized to the primitive <see cref="Shape" />.</returns>
        public static ProBuilderMesh Instantiate(Shape shape)
        {
            if (shape == null)
                throw new ArgumentNullException("shape", "Cannot instantiate a null shape.");

            var shapeComponent = new GameObject("Shape").AddComponent<ProBuilderShape>();
            shapeComponent.SetShape(shape);
            ProBuilderMesh pb = shapeComponent.mesh;
            pb.renderer.sharedMaterial = BuiltinMaterials.defaultMaterial;

            var attribute = Attribute.GetCustomAttribute(shape.GetType(), typeof(ShapeAttribute));

            if(attribute is ShapeAttribute shapeAttrib)
                pb.gameObject.name = shapeAttrib.name;

            return pb;
        }
    }
}
