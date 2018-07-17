// ----------------------------------------------------------------------------
// The MIT License
// Unity integration https://github.com/Leopotam/ecs-unityintegration
// for ECS framework https://github.com/Leopotam/ecs
// Copyright (c) 2018 Leopotam <leopotam@gmail.com>
// ----------------------------------------------------------------------------

using UnityEditor;
using UnityEngine;

namespace LeopotamGroup.Ecs.UnityIntegration.Editor {
    [CustomEditor (typeof (EcsSystemsObserver))]
    sealed class EcsSystemsObserverInspector : UnityEditor.Editor {
        static IEcsInitSystem[] _initList = new IEcsInitSystem[32];

        static IEcsRunSystem[] _runList = new IEcsRunSystem[32];

        public override void OnInspectorGUI () {
            var savedState = GUI.enabled;
            GUI.enabled = true;
            var observer = target as EcsSystemsObserver;
            var systems = observer.GetSystems ();
            int count;
            count = systems.GetInitSystems (ref _initList);
            if (count > 0) {
                GUILayout.BeginVertical (GUI.skin.box);
                EditorGUILayout.LabelField ("Initialize systems", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                for (var i = 0; i < count; i++) {
                    EditorGUILayout.LabelField (_initList[i].GetType ().Name);
                    _initList[i] = null;
                }
                EditorGUI.indentLevel--;
                GUILayout.EndVertical ();
            }

            count = systems.GetRunSystems (ref _runList);
            if (count > 0) {
                GUILayout.BeginVertical (GUI.skin.box);
                EditorGUILayout.LabelField ("Run systems", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                for (var i = 0; i < count; i++) {
                    if (systems.DisabledInDebugSystems != null) {
                        systems.DisabledInDebugSystems[i] = !EditorGUILayout.Toggle (_runList[i].GetType ().Name, !systems.DisabledInDebugSystems[i]);
                    } else {
                        EditorGUILayout.LabelField (_runList[i].GetType ().Name);
                    }
                    _runList[i] = null;
                }
                EditorGUI.indentLevel--;
                GUILayout.EndVertical ();
            }
            GUI.enabled = savedState;
        }
    }
}