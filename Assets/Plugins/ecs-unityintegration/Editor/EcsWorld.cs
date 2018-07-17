// ----------------------------------------------------------------------------
// The MIT License
// Unity integration https://github.com/Leopotam/ecs-unityintegration
// for ECS framework https://github.com/Leopotam/ecs
// Copyright (c) 2018 Leopotam <leopotam@gmail.com>
// ----------------------------------------------------------------------------

using UnityEditor;
using UnityEngine;

namespace LeopotamGroup.Ecs.UnityIntegration.Editor {
    [CustomEditor (typeof (EcsWorldObserver))]
    sealed class EcsWorldObserverInspector : UnityEditor.Editor {
        public override void OnInspectorGUI () {
            var observer = target as EcsWorldObserver;
            var stats = observer.GetStats ();
            var guiEnabled = GUI.enabled;
            GUI.enabled = true;
            GUILayout.BeginVertical (GUI.skin.box);
            EditorGUILayout.LabelField ("Components", stats.Components.ToString ());
            EditorGUILayout.LabelField ("Filters", stats.Filters.ToString ());
            EditorGUILayout.LabelField ("Active entities", stats.ActiveEntities.ToString ());
            EditorGUILayout.LabelField ("Reserved entities", stats.ReservedEntities.ToString ());
            GUILayout.EndVertical ();
            GUI.enabled = guiEnabled;
        }
    }
}