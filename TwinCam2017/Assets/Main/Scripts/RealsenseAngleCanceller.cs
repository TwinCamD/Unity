/*Summary
 *  RealSenseを使って車椅子の角度を取得する
 *  それに合わせてHMDの回転量を相殺する
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public class RealsenseAngleCanceller : MonoBehaviour {

    #region RealSense
    private Transform _realSenseTransform;
    [SerializeField] private float _realSenseAngle; //RealSenseで取得した角度 表示用

    [SerializeField] private float _realSenseInfAngle; //プロパティ用
    public float RealSenseInfAngle {   //RealSenseで取得した無限角度
        get { return _realSenseInfAngle; }
        private set { _realSenseInfAngle = value; }
    }
    private int _rotationTimes = 0;    //回転数
    private float _lastDegree = 0;     //前回の角度     

    private float _realSenseOffset = 0; //resetしたときのoffset
    #endregion

    #region HMD
    [SerializeField] private GameObject _browserObj;  //BrowserについているHmdのスクリプトを取得するため
    private TwinCamHmdController _twinCamHmdController;
    [SerializeField] private float _hmdOffsetInfAngle; //プロパティ用
    public float HmdOffsetInfAngle {   //車椅子の回転量を相殺したHMDの角度
        get { return _hmdOffsetInfAngle; }
        private set { _hmdOffsetInfAngle = value; }
    }
    #endregion

    #region LPF
    private float _lastLpf = 0; //前回のローパスフィルタの値
    [SerializeField] private float _a = 0;   //係数
    [SerializeField] private float _aBorder = 2f; //係数変更の境目 角度 
    [SerializeField] private float _aFast = 0.8f;//値の差が大きい時のA valueの割合 大きいほど感度がいい
    [SerializeField] private float _aSlow = 0.01f;//値の差が小さい時のA
    #endregion

    #region Sphere
    [SerializeField] private GameObject _leftSphereObj, _rightSphereObj;  //Sphereオブジェクトを格納（映像を流しているスクリプト)
    private Transform _leftSphTransform, _rightSphTransform;
    private float _lastSphereAngle = 0;
    #endregion

    // Use this for initialization
    void Start() {
        _realSenseTransform = GetComponent<Transform>();
        _twinCamHmdController = _browserObj.GetComponent<TwinCamHmdController>();

        //SphereのTransformを取得
        _leftSphTransform = _leftSphereObj.GetComponent<Transform>();
        _rightSphTransform = _rightSphereObj.GetComponent<Transform>();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            reset();
        }

        rotateSphere();
    }

    void FixedUpdate() {
        _realSenseAngle = GetRealSenseAngle(false);   //RealSenseの角度を取得
        RealSenseInfAngle = lowPassFilter(getInfiniteRealSenseAngle());    //RealSenseの無限角度を取得

        HmdOffsetInfAngle = getHmdOffsetInfAngle();
    }

    //realsenseのYow角度を取得
    public float GetRealSenseAngle(bool isReversed) {
        //Quaternionで返ってくるRealSenseの回転角をeulerAngleに変換して、y軸を取得する
        float degree = _realSenseTransform.rotation.eulerAngles.y;  //0～360の7桁

        if (isReversed) {   //反転
            return 360f - degree;   //360 - (0～360) する
        }
        else {
            return degree;
        }
    }

    //瞬間的に180度以上回転しないこと前提
    private float getInfiniteRealSenseAngle() {
        float degree = GetRealSenseAngle(false);
        float difference = degree - _lastDegree;

        //0を超えるときの処理
        if (difference < -180f) {
            _rotationTimes++;
        }
        else if (180f < difference) {
            _rotationTimes--;
        }
        _lastDegree = degree;

        return degree + 360f * _rotationTimes;  //時計回りが正
    }

    //角度リセット
    private void reset() {
        _realSenseOffset = RealSenseInfAngle;
    }

    //車椅子の回転量を相殺したHMDの角度を取得
    private float getHmdOffsetInfAngle() {
        return _twinCamHmdController.HmdInfAngle - (RealSenseInfAngle - _realSenseOffset);
    }

    //ローパスフィルタ
    private float lowPassFilter(float value) {
        if (Mathf.Abs(value - _lastLpf) > _aBorder) {
            _a = _aFast;
        }
        else {
            _a = _aSlow;
        }
        _lastLpf += _a * (value - _lastLpf); //_lastLpf = (1 - a) * _lastLpf + a * value;
        return _lastLpf;

    }

    //Sphereを回転させることにより，車椅子の回転とHMDの回転を相殺する
    private void rotateSphere() {
        //前回の角度の変化量の分だけ回転(Rotate)
        _leftSphTransform.Rotate(0, RealSenseInfAngle - _lastSphereAngle, 0);
        _rightSphTransform.Rotate(0, RealSenseInfAngle - _lastSphereAngle, 0);
        _lastSphereAngle = RealSenseInfAngle;  //前回の角度を保存
    }
}