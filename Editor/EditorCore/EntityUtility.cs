using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    static class EntityUtility
    {
        const StaticEditorFlags StaticEditorFlags_All =
#if UNITY_2019_2_OR_NEWER
            StaticEditorFlags.ContributeGI |
#else
            StaticEditorFlags.LightmapStatic |
#endif
            StaticEditorFlags.OccluderStatic |
            StaticEditorFlags.BatchingStatic |
            StaticEditorFlags.OccludeeStatic |
#if !UNITY_2022_2_OR_NEWER
            StaticEditorFlags.NavigationStatic |
            StaticEditorFlags.OffMeshLinkGeneration |
#endif
            StaticEditorFlags.ReflectionProbeStatic;

        /// <summary>
        /// Sets the EntityType for the passed gameObject.
        /// </summary>
        /// <param name="newEntityType">The type to set.</param>
        /// <param name="target">The gameObject to apply the EntityType to.  Must contains pb_Object and pb_Entity components.  Method does contain null checks.</param>
        [Obsolete("pb_Entity is deprecated. Manage static flags manually or use Set Trigger/Set Collider actions.")]
        public static void SetEntityType(EntityType newEntityType, GameObject target)
        {
            if (!target.TryGetComponent<Entity>(out var ent))
                ent = target.AddComponent<Entity>();

            ProBuilderMesh pb = target.GetComponent<ProBuilderMesh>();

            if (!ent || !pb)
                return;

            SetEditorFlags(StaticEditorFlags_All, target);

            switch (newEntityType)
            {
                case EntityType.Detail:
                case EntityType.Occluder:
                    SetBrush(target);
                    break;

                case EntityType.Trigger:
                    SetTrigger(target);
                    break;

                case EntityType.Collider:
                    SetCollider(target);
                    break;

                case EntityType.Mover:
                    SetDynamic(target);
                    break;
            }

            ent.SetEntity(newEntityType);
        }

        [Obsolete("pb_Entity is deprecated. Manage static flags manually or use Set Trigger/Set Collider actions.")]
        static void SetBrush(GameObject target)
        {
            EntityType et = target.GetComponent<Entity>().entityType;

            if (et == EntityType.Trigger ||
                et == EntityType.Collider)
            {
                ProBuilderMesh pb = target.GetComponent<ProBuilderMesh>();
                foreach (var face in pb.facesInternal)
                    face.material = BuiltinMaterials.defaultMaterial;
                pb.ToMesh();
                pb.Refresh();
            }
        }

        [Obsolete("pb_Entity is deprecated. Manage static flags manually or use Set Trigger/Set Collider actions.")]
        static void SetDynamic(GameObject target)
        {
            EntityType et = target.GetComponent<Entity>().entityType;

            SetEditorFlags((StaticEditorFlags)0, target);

            if (et == EntityType.Trigger ||
                et == EntityType.Collider)
            {
                ProBuilderMesh pb = target.GetComponent<ProBuilderMesh>();
                foreach (var face in pb.facesInternal)
                    face.material = BuiltinMaterials.defaultMaterial;

                pb.ToMesh();
                pb.Refresh();
            }
        }

        [Obsolete("pb_Entity is deprecated. Manage static flags manually or use Set Trigger/Set Collider actions.")]
        static void SetTrigger(GameObject target)
        {
            ProBuilderMesh pb = target.GetComponent<ProBuilderMesh>();
            foreach (var face in pb.facesInternal)
                face.material = BuiltinMaterials.triggerMaterial;
            SetIsTrigger(true, target);
            SetEditorFlags((StaticEditorFlags)0, target);

            pb.ToMesh();
            pb.Refresh();
        }

        [Obsolete("pb_Entity is deprecated. Manage static flags manually or use Set Trigger/Set Collider actions.")]
        static void SetCollider(GameObject target)
        {
            ProBuilderMesh pb = target.GetComponent<ProBuilderMesh>();
            foreach (var face in pb.facesInternal)
                face.material = BuiltinMaterials.colliderMaterial;
            pb.ToMesh();
            pb.Refresh();

            SetEditorFlags((StaticEditorFlags)(StaticEditorFlags.NavigationStatic | StaticEditorFlags.OffMeshLinkGeneration),
                target);
        }

        [Obsolete("pb_Entity is deprecated. Manage static flags manually or use Set Trigger/Set Collider actions.")]
        static void SetEditorFlags(StaticEditorFlags editorFlags, GameObject target)
        {
            GameObjectUtility.SetStaticEditorFlags(target, editorFlags);
        }

        [Obsolete("pb_Entity is deprecated. Manage static flags manually or use Set Trigger/Set Collider actions.")]
        static void SetIsTrigger(bool val, GameObject target)
        {
            Collider[] colliders = InternalUtility.GetComponents<Collider>(target);
            foreach (Collider col in colliders)
            {
                if (val && col is MeshCollider)
                    ((MeshCollider)col).convex = true;
                col.isTrigger = val;
            }
        }
    }
}
