using System;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Apply this attribute to any class derived from MenuAction that you want to appear in the <see cref="ProBuilderEditor"/>
    /// window.
    ///
    /// There are several working examples bundled with the ProBuilder package that demonstrate how to use the ProBuilder API to
    /// add custom actions to the ProBuilder toolbar. To import them into your Unity project, click the **Import** button under the
    /// **Samples** section on the [Package Manager window](https://docs.unity3d.com/Manual/upm-ui-details.html).
    /// </summary>
    /// <example>
    /// ```
    /// using UnityEngine;
    /// using UnityEditor;
    /// using UnityEngine.ProBuilder;
    /// using UnityEditor.ProBuilder;
    /// using UnityEngine.ProBuilder.MeshOperations;
    ///
    /// namespace ProBuilder.ExampleActions
    /// {
    ///     [ProBuilderMenuAction]
    ///     public class MyCustomProBuilderMenuAction : MenuAction
    ///     {
    ///         // @todo Write your plugin
    ///     }
    /// }
    /// ```
    /// </example>
    /// <seealso cref="MenuAction"/>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ProBuilderMenuActionAttribute : Attribute
    {
    }
}
