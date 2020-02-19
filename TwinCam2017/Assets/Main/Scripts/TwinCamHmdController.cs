/*Summary
 *  HmdとTwinCam用にいろいろ設定する
 *  HMDの値を関数として返す
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class TwinCamHmdController : MonoBehaviour {

    [SerializeField] private float _hmdAngle;   //HMDの角度 表示用

    [SerializeField] private float _hmdInfAngle;   //HMDの無限角度
    public float HmdInfAngle {
        get { return _hmdInfAngle; }
        private set { _hmdInfAngle = value; }
    }
    private int _rotationTimes = 0;    //回転数
    private float _lastDegree = 0;     //前回の角度     

    void Start() {
        InputTracking.disablePositionalTracking = true;             //カメラの位置移動を無効化
        XRDevice.SetTrackingSpaceType(TrackingSpaceType.Stationary);//ルームスケールを[椅子に座るモード]に設定
        XRSettings.showDeviceView = true;                           //左目で見える映像をミラーリングする
    }

    void Update() {
        //角度リセット
        if (Input.GetKeyDown(KeyCode.Space)) {
            recenter();
        }
    }

    void FixedUpdate() {
        _hmdAngle = GetHmdAngle(true);   //角度取得

        HmdInfAngle = getInfiniteHmdAngle();   //無限角度取得
    }

    private void recenter() {
        InputTracking.Recenter();
        _rotationTimes = 0;
        _lastDegree = 0;
        Debug.Log("Recenter!");
    }

    public float GetHmdAngle(bool isReversed) {
        //Quaternionで返ってくるHMDの回転角をeulerAngleに変換して、y軸を取得する
        float degree = InputTracking.GetLocalRotation(XRNode.CenterEye).eulerAngles.y;  //0～360の7桁

        if (isReversed) {   //反転
            return 360f - degree;   //360 - (0～360) する
        } else {
            return degree;
        }
    }

    //瞬間的に180度以上回転しないこと前提
    private float getInfiniteHmdAngle() {
        float degree = GetHmdAngle(false);
        float difference = degree - _lastDegree;

        //0を超えるときの処理
        if (difference < -180f) {
            _rotationTimes++;
        } else if (180f < difference) {
            _rotationTimes--;
        }
        _lastDegree = degree;

        return degree + 360f * _rotationTimes;  //時計回りが正
    }
}