using System.Linq;
using Dissonance.Audio.Playback;
using UnityEditor;
using UnityEngine;

namespace Dissonance
{
    [CustomEditor(typeof (VoicePlayback))]
    [CanEditMultipleObjects]
    public class VoicePlaybackEditor : Editor
    {
        private Texture2D _logo;
        //private bool _show3D = true;

        //private SerializedProperty _outputAudioGroup;
        //private SerializedProperty _priority;
        //private SerializedProperty _volume;
        //private SerializedProperty _spatialBlend;
        //private SerializedProperty _spread;
        //private SerializedProperty _minDistance;
        //private SerializedProperty _maxDistance;
        //private SerializedProperty _audioCurve;

        public void Awake()
        {
            _logo = Resources.Load<Texture2D>("dissonance_logo");
        }

        public void OnEnable()
        {
            //_outputAudioGroup = serializedObject.FindProperty("_output");
            //_priority = serializedObject.FindProperty("_audioPriority");
            //_volume = serializedObject.FindProperty("_volume");
            //_spatialBlend = serializedObject.FindProperty("_spatialBlend");
            //_spread = serializedObject.FindProperty("_spread");
            //_minDistance = serializedObject.FindProperty("_minDistance");
            //_maxDistance = serializedObject.FindProperty("_maxDistance");
            //_audioCurve = serializedObject.FindProperty("_audioCurve");
        }

        public override void OnInspectorGUI()
        {
            //serializedObject.Update();

            GUILayout.Label(_logo);

            //EditorGUILayout.PropertyField(_outputAudioGroup);
            //EditorGUILayout.IntSlider(_priority, 0, 256);
            //EditorGUILayout.Slider(_volume, 0, 1);
            //EditorGUILayout.Slider(_spatialBlend, 0, 1);

            //_show3D = EditorGUILayout.Foldout(_show3D, "3D Sound Settings");
            //if (_show3D)
            //{
            //    EditorGUI.indentLevel++;

            //    EditorGUILayout.Slider(_spread, 0, 360);
            //    EditorGUILayout.PropertyField(_minDistance);
            //    EditorGUILayout.PropertyField(_maxDistance);

            //    var rect = GUILayoutUtility.GetAspectRect(1);

            //    var curve = _audioCurve.animationCurveValue;
            //    if (curve.keys.Length == 0)
            //    {
            //        var keyframes = new Keyframe[101];
            //        for (var i = 0; i < 101; i++)
            //        {
            //            //This is the distance from min -> max
            //            var dBase = Mathf.Lerp(_minDistance.floatValue, _maxDistance.floatValue, i / 100f);

            //            //Let's offset the base distance so it's from 0 -> (max - min)
            //            var dOff = dBase - _minDistance.floatValue;

            //            //Finally let's normalize distance into 0 -> 1 range
            //            var d = dOff / (_maxDistance.floatValue - _minDistance.floatValue);

            //            //What's the denominator of our attenuation function?
            //            var denom = 1 + 1.25f * d + 14.5f * d * d + 5 * d * d * d;

            //            //Finally save a curve with (x = min -> max) and (y = 0 -> 1)
            //            keyframes[i] = new Keyframe(dBase, Mathf.Clamp(1 / denom, 0, 1));
            //        }

            //        curve = new AnimationCurve(keyframes);
            //        //_audioCurve.animationCurveValue = curve;
            //    }

            //    var points = Enumerable
            //        .Range(0, 101)
            //        .Select(a => a / 100f)
            //        .Select(a => new Vector3(
            //            rect.min.x + a * rect.width,
            //            rect.min.y + (1 - curve.Evaluate(Mathf.Lerp(_minDistance.floatValue, _maxDistance.floatValue, a))) * rect.height,
            //            0)
            //        )
            //        .ToArray();

            //    Handles.DrawSolidRectangleWithOutline(rect, Color.gray, Color.black);
            //    Handles.DrawAAPolyLine(3, points);

            //    EditorGUI.indentLevel--;
            //}

            //serializedObject.ApplyModifiedProperties();
        }
    }
}
