using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ClipController : MonoBehaviour
{
    [Header("AnimationClip")]
    [SerializeField] AnimationClip leftAttack;
    [SerializeField] AnimationClip rightAttack;

    Character character;

    private void Awake()
    {
        character = Character.Instance;
    }

    public void SetKeyframesRotationZ(float zStart, float zEnd)
    {
        EditorCurveBinding[] bindings;
        AnimationClip targetClip;

        if (character.IsFlip)
            targetClip = rightAttack;
        else
            targetClip = leftAttack;

        bindings = AnimationUtility.GetCurveBindings(targetClip);

        foreach (var binding in bindings)
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(targetClip, binding);

            if (binding.propertyName.Contains("localEulerAnglesRaw.z"))
            {
                // Debug.Log(binding.propertyName);

                if (curve.length > 0)
                {
                    Keyframe firstKey = curve.keys[0];
                    Keyframe lastKey = curve.keys[curve.length - 1];

                    if (character.IsFlip)
                    {
                        firstKey.value = zEnd * 360;
                        lastKey.value = zStart * 360;
                        if (firstKey.value < 0) firstKey.value %= 360;
                        if (lastKey.value < 0) lastKey.value %= 360;
                    }
                    else
                    {
                        firstKey.value = zStart * 360;
                        lastKey.value = zEnd * 360;
                        if (firstKey.value < 0) firstKey.value %= 360;
                        if (lastKey.value < 0) lastKey.value %= 360;
                    }

                    curve.MoveKey(0, firstKey);
                    curve.MoveKey(curve.length - 1, lastKey);

                    // Debug.Log("firstKey.z: " + firstKey.value);
                    // Debug.Log("lastKey.z: " + lastKey.value);
                    
                    AnimationUtility.SetEditorCurve(targetClip, binding, curve);
                }

                break;
            }

        }

        foreach (var binding in bindings)
        {
            Debug.Log(binding.propertyName);
            AnimationCurve curve = AnimationUtility.GetEditorCurve(targetClip, binding);

            if (binding.propertyName.Contains("localEulerAnglesRaw"))
            {
                Keyframe firstKey = curve.keys[0];
                Keyframe lastKey = curve.keys[curve.length - 1];

                Debug.Log(binding.propertyName);
                Debug.Log("firstKey: " + firstKey.value + " / lastKey: " + lastKey.value);
            }
        }

    }
}
