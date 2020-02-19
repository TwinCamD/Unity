/*Summary
 *  Angleをstepに変更する
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConverterAngleToPulse : MonoBehaviour {

    [SerializeField] private int lastPulse = 0;
    [SerializeField] private int _maximumStep;
    [SerializeField] private int step;

    // Use this for initialization
    void Start() {
        float time = Time.fixedDeltaTime * 1000f;
        _maximumStep = (int) time; //ms
    }

    public byte Convert(int nowPulse) {
        step = nowPulse - lastPulse;
        if (Mathf.Abs(step) > _maximumStep) {
            step = (int) (Mathf.Sign(step) * _maximumStep);  //signは正負をつけるため
        }

        lastPulse += step;  //lastPulse = lastPulse + step

        return (byte)step;
    }
}

