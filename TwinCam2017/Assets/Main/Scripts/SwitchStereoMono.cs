/*Summary
 *  単眼視と両眼立体視を切り替える
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class SwitchStereoMono : MonoBehaviour {

    public Camera LeftCamera;
    public Camera RightCamera;
    public enum TargetEye {
        Both,
        Left,
        Right
    }
    private Vector3 _tmpLeftCamPos = Vector3.zero;
    private Vector3 _tmpRightCamPos = Vector3.zero;
    [SerializeField] private bool _isDebugState = true;

    private TwinCamHmdController _twinCamHmdController;

    #region FPS
    private float _fixedDeltaTime;//FixedUpdateの時間 100fps
    #endregion

    #region DelayAccel
    private const float _maximumDelayTime = 3f;//遅延できる秒数の最大
    /*[SerializeField]*/ [Range(0f, _maximumDelayTime)] private float _delayTime = 0.4f;//遅延時間
    private int _maximumBufferSize = 0;
    private int _index = 2;//遅延させる値の数
    private short[,] _accelBuffer;//加速度を一時保存するためのバッファ
    private int _recordCount = 0;//値を保存するため
    private int _playCount = 0;//値を再生するため
    #endregion

    // Use this for initialization
    void Start () {
        //重くなりそうだからHierarchyから直接アタッチ
        //LeftCamera = GameObject.Find("/Left Sphere/Main Camera").GetComponent<Camera>();
        //RightCamera = GameObject.Find("/Right Sphere/Main Camera").GetComponent<Camera>();

        //実行時にカメラがsphereの中心からずれるので位置を取っておく
        _tmpLeftCamPos = LeftCamera.transform.position;
        _tmpRightCamPos = RightCamera.transform.position;

        #region Delay
        _maximumBufferSize = (int)(_maximumDelayTime / _fixedDeltaTime);
        #endregion
    }

    // Update is called once per frame
    void Update () {
	    if (Input.GetKeyDown(KeyCode.DownArrow)) {
	        ChangeTargetEye(TargetEye.Both);
	    }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
	        ChangeTargetEye(TargetEye.Left);
        }
	    if (Input.GetKeyDown(KeyCode.RightArrow)) {
	        ChangeTargetEye(TargetEye.Right);
	    }
	}

    void FixedUpdate() {
        _recordCount = (_recordCount + 1) % _maximumBufferSize;
        _playCount = (_recordCount - (int)(_delayTime / _fixedDeltaTime) + _maximumBufferSize) % _maximumBufferSize;
    }

    public void ChangeTargetEye(TargetEye state) {
        switch (state) {
            case TargetEye.Both:
                LeftCamera.stereoTargetEye = StereoTargetEyeMask.Left;
                RightCamera.stereoTargetEye = StereoTargetEyeMask.Right;
                if (_isDebugState) {
                    Debug.Log(state);
                }
                break;
            case TargetEye.Left:
                RightCamera.stereoTargetEye = StereoTargetEyeMask.None;
                RightCamera.transform.position = _tmpRightCamPos;
                LeftCamera.stereoTargetEye = StereoTargetEyeMask.Both;
                if (_isDebugState) {
                    Debug.Log(state);
                }
                break;
            case TargetEye.Right:
                LeftCamera.stereoTargetEye = StereoTargetEyeMask.None;
                LeftCamera.transform.position = _tmpLeftCamPos;
                RightCamera.stereoTargetEye = StereoTargetEyeMask.Both;
                if (_isDebugState) {
                    Debug.Log(state);
                }
                break;
        }
    }
}
