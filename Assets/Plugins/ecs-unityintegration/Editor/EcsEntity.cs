// ----------------------------------------------------------------------------
// The MIT License
// Unity integration https://github.com/Leopotam/ecs-unityintegration
// for ECS framework https://github.com/Leopotam/ecs
// Copyright (c) 2018 Leopotam <leopotam@gmail.com>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LeopotamGroup.Ecs.UnityIntegration.Editor {
    [CustomEditor (typeof (EcsEntityObserver))]
    sealed class EcsEntityObserverInspector : UnityEditor.Editor {
        static object[] _componentsCache = new object[32];

        EcsEntityObserver _entity;

        public override void OnInspectorGUI () {
            if (_entity.World != null) {
                var guiEnabled = GUI.enabled;
                GUI.enabled = true;
                DrawComponents ();
                GUI.enabled = guiEnabled;
                EditorUtility.SetDirty (target);
            }
        }

        void OnEnable () {
            _entity = target as EcsEntityObserver;
        }

        void OnDisable () {
            _entity = null;
        }

        void DrawComponents () {
            var count = _entity.World.GetComponents (_entity.Id, ref _componentsCache);
            for (var i = 0; i < count; i++) {
                var component = _componentsCache[i];
                _componentsCache[i] = null;
                var type = component.GetType ();
                GUILayout.BeginVertical (GUI.skin.box);
                if (!EcsComponentInspectors.Render (type.Name, type, component)) {
                    EditorGUILayout.LabelField (type.Name, EditorStyles.boldLabel);
                    var indent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel++;
                    foreach (var field in type.GetFields (BindingFlags.Instance | BindingFlags.Public)) {
                        DrawTypeField (component, field);
                    }
                    EditorGUI.indentLevel = indent;
                }
                GUILayout.EndVertical ();
                EditorGUILayout.Space ();
            }
        }

        void DrawTypeField (object instance, FieldInfo field) {
            var fieldValue = field.GetValue (instance);
            var fieldType = field.FieldType;
            if (!EcsComponentInspectors.Render (field.Name, fieldType, fieldValue)) {
                if (fieldType == typeof (UnityEngine.Object) || fieldType.IsSubclassOf (typeof (UnityEngine.Object))) {
                    GUILayout.BeginHorizontal ();
                    EditorGUILayout.LabelField (field.Name, GUILayout.MaxWidth (EditorGUIUtility.labelWidth - 16));
                    var guiEnabled = GUI.enabled;
                    GUI.enabled = false;
                    EditorGUILayout.ObjectField (fieldValue as UnityEngine.Object, fieldType, false);
                    GUI.enabled = guiEnabled;
                    GUILayout.EndHorizontal ();
                    return;
                }
                var strVal = fieldValue != null ? string.Format (System.Globalization.CultureInfo.InvariantCulture, "{0}", fieldValue) : "null";
                EditorGUILayout.TextField (field.Name, strVal);
            }
        }
    }

    static class EcsComponentInspectors {
        static readonly Dictionary<Type, IEcsComponentInspector> _inspectors = new Dictionary<Type, IEcsComponentInspector> ();

        static EcsComponentInspectors () {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies ()) {
                foreach (var type in assembly.GetTypes ()) {
                    if (typeof (IEcsComponentInspector).IsAssignableFrom (type) && !type.IsInterface) {
                        var inspector = Activator.CreateInstance (type) as IEcsComponentInspector;
                        var componentType = inspector.GetFieldType ();
                        if (_inspectors.ContainsKey (componentType)) {
                            Debug.LogWarningFormat ("Inspector for \"{0}\" already exists, new inspector will be used instead.", componentType.Name);
                        }
                        _inspectors[componentType] = inspector;
                    }
                }
            }
        }

        public static bool Render (string label, Type type, object value) {
            IEcsComponentInspector inspector;
            if (_inspectors.TryGetValue (type, out inspector)) {
                inspector.OnGUI (label, value);
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Custom inspector for specified field type.
    /// </summary>
    public interface IEcsComponentInspector {
        /// <summary>
        /// Supported field type.
        /// </summary>
        Type GetFieldType ();

        /// <summary>
        /// Renders provided instance of specified type.
        /// </summary>
        /// <param name="label">Label of field.</param>
        /// <param name="value">Value of field.</param>
        void OnGUI (string label, object value);
    }
}