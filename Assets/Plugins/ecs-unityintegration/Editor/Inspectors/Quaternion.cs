// ----------------------------------------------------------------------------
// The MIT License
// Unity integration https://github.com/Leopotam/ecs-unityintegration
// for ECS framework https://github.com/Leopotam/ecs
// Copyright (c) 2018 Leopotam <leopotam@gmail.com>
// ----------------------------------------------------------------------------

using System;
using UnityEditor;
using UnityEngine;

namespace LeopotamGroup.Ecs.UnityIntegration.Editor.Inspectors {
    sealed class QuaternionInspector : IEcsComponentInspector {
        Type IEcsComponentInspector.GetFieldType () {
            return typeof (Quaternion);
        }

        void IEcsComponentInspector.OnGUI (string label, object value) {
            EditorGUILayout.Vector3Field (label, ((Quaternion) value).eulerAngles);
        }
    }
}