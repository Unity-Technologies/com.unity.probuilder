using System;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Classes inheriting MenuAction and tagged with this attribute will be displayed in the ProBuilderEditor window.
    /// See [ProBuilder API Examples](https://github.com/Unity-Technologies/ProBuilder-API-Examples) for sample code.
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
    ///                 // @todo Write your plugin
    ///         }
    /// }
    /// ```
    /// </example>
    /// <seealso cref="MenuAction"/>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ProBuilderMenuActionAttribute : Attribute
    {
    }
}
