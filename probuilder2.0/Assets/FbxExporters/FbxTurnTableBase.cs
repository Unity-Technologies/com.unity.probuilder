// ***********************************************************************
// Copyright (c) 2017 Unity Technologies. All rights reserved.
//
// Licensed under the ##LICENSENAME##.
// See LICENSE.md file in the project root for full license information.
// ***********************************************************************

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FbxExporters.Review
{
    public class FbxTurnTableBase : MonoBehaviour
    {

        [Tooltip ("Rotation speed in degrees/second")]
        [SerializeField]
        private float speed = 10f;

#if UNITY_EDITOR
        private float timeOfLastUpdate = float.MaxValue;
#endif

        public void Rotate()
        {
            float deltaTime = 0;
#if UNITY_EDITOR
            deltaTime = Time.realtimeSinceStartup - timeOfLastUpdate;
            if(deltaTime <= 0){
                deltaTime = 0.001f;
            }
            timeOfLastUpdate = Time.realtimeSinceStartup;
#else
            deltaTime = Time.deltaTime;
#endif
            transform.Rotate (Vector3.up, speed * deltaTime, Space.World); 
        }

        void Update () {
            Rotate ();
        }
    }
}