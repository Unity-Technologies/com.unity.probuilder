using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    class ScriptableShape : ScriptableObject
    {
        [SerializeReference]
        public Shape Shape;
    }
}
