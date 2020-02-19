/*Summary
 *  TwinCamのデータ関係をやり取りするスクリプトのひな型
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Reflection;

public class UserTwinCamWheelChairManager_old : MonoBehaviour {

    [SerializeField] private float _fixedDeltaTime = 0.01f;//FixedUpdateの時間 100fps
    [SerializeField] private int _fps = 0;//HMDの描画速度より大きくなればいい

    private SerialEsp32 _serialEsp32;
    private SkywayDataConnect _skywayDataConnectRight;

    private TwinCamHmdController _twinCamHmdController;
    [SerializeField] private bool _isAngleZero = false; //角度を0にするか
    [SerializeField] private bool _isReversedAngle = false;  //送る角度データを反転するか HmdAngleを使うときはtrueじゃないと反転

    [SerializeField] private bool _isMoveWheelChair = true;   //座席を動作させるかどうか
    [SerializeField] private GameObject _rsDeviceObj;
    private RealsenseAngleCanceller _realsenseAngleCanceller;

    [SerializeField] private GameObject _leftBrowserObj;
    private SkywayDataConnect _skywayDataConnectLeft;

    public short AccelR;
    public short AccelL;

    private float countTime = 0;    //timer

    void Awake() {
        Time.fixedDeltaTime = _fixedDeltaTime;
        _fps = (int)(1 / _fixedDeltaTime);
    }

    // Use this for initialization
    void Start () {
        _serialEsp32 = GetComponent<SerialEsp32>();
        _skywayDataConnectRight = GetComponent<SkywayDataConnect>();
        _skywayDataConnectLeft = _leftBrowserObj.GetComponent<SkywayDataConnect>();

        _twinCamHmdController = GetComponent<TwinCamHmdController>();

        _realsenseAngleCanceller = _rsDeviceObj.GetComponent<RealsenseAngleCanceller>();
    }
	
	// Update is called once per frame
	void Update () {
        #region データの構造確認用
        //var data = {送りたいデータ};
        //Debug.Log("data: " + data + ", type: " + data.GetType() + ", length: " + BitConverter.GetBytes(data).Length);
        #endregion

	    //timer
	    countTime += Time.deltaTime;
    }

    void FixedUpdate() {
        getSerial();
        setSkywayData();

        getSkywayData();
        setSerial();
    }

    //skyway送信データ
    private void setSkywayData() {
        //角度を強制的に0に(角度取得一時ストップ)
        if (Input.GetKeyDown(KeyCode.S)) {
            _isAngleZero = !_isAngleZero;   //boolを反転
            Debug.Log("<color=#0000ffff>角度</color>取得停止が<b><color=#0000ffff>" + _isAngleZero + "</color></b>になりました");    //color=青
        }
        //Right
        try {
            /*_skywayDataConnect.SendData[0] = {hogehoge}.ToString(); と書く*/
            _skywayDataConnectRight.SendData[0] = countTime.ToString();
            if (_isAngleZero) {
                _skywayDataConnectRight.SendData[1] = "0";  //0を直接送る
            } else {
                if (_isMoveWheelChair) {    //座席あり
                    if (_isReversedAngle) {
                        _skywayDataConnectRight.SendData[1] = (-_realsenseAngleCanceller.HmdOffsetInfAngle).ToString();
                    } else {
                        _skywayDataConnectRight.SendData[1] = _realsenseAngleCanceller.HmdOffsetInfAngle.ToString();
                    }
                } else {    //座席なし
                    if (_isReversedAngle) {
                        _skywayDataConnectRight.SendData[1] = (-_twinCamHmdController.HmdInfAngle).ToString();
                    } else {
                        _skywayDataConnectRight.SendData[1] = _twinCamHmdController.HmdInfAngle.ToString();
                    }

                }
            }
        }
        catch (Exception e) {
            Debug.LogError(e.Message);
        }
        //Left
        try {
            /*_skywayDataConnect.SendData[0] = {hogehoge}.ToString(); と書く*/
            _skywayDataConnectLeft.SendData[0] = countTime.ToString();
        }
        catch (Exception e) {
            Debug.LogError(e.Message);
        }
    }

    //skyway受信データ
    private void getSkywayData() {
        try {
            /*{hogehoge} = ({キャスト}){受け取った値の型}.Parse(_skywayDataConnect.RecieveData[0]); と書く*/
            if (_isMoveWheelChair) {
                AccelR = short.Parse(_skywayDataConnectRight.RecieveData[1]);  //Right
                AccelL = short.Parse(_skywayDataConnectLeft.RecieveData[1]);   //Left
            }
        }
        catch (Exception e) {
            Debug.LogWarning(e.Message);
        }
    }

    //serial送信データ
    private void setSerial() {
        /*
         あらかじめ送るデータのbyte数を確認しておくこと
         SerialEsp32のSendBytesCountで簡単に確認できる
         確認したらSerialEsp32のSendBytesのサイズをInspectorで確認した値にする
         _serialEsp32.SetByteData(BitConverter.GetBytes({送るデータ}));
         ...
         */

        //_serialEsp32.SetByteData(BitConverter.GetBytes(sendsend));
        //_serialEsp32.SetByteData(BitConverter.GetBytes(rrr));

        //_serialEsp32.ResetByteDataCount();  //最後に必ずリセット
    }

    //serial受信データ
    private void getSerial() {
        /*
         受け取ったデータ処理
                //{代入先} = (キャスト)(
             (_serialEsp32.RecieveBytes[{1か3など}] << 8) & 0xFF00 |  //データ型の大きさに合わせて列を増やす
             (_serialEsp32.RecieveBytes[{0か2など}] << 0) & 0x00FF
         );
         */

        //short aaa = (short)(
        //    (_serialEsp32.RecieveBytes[1] << 8) & 0xFF00 |
        //    (_serialEsp32.RecieveBytes[0] << 0) & 0x00FF
        //);

        //AccelVehicle = (short)(-100 + _recieveBytes[0] + (_recieveBytes[1] << 8));
        //GyroVehicle = (short)(_recieveBytes[2] + (_recieveBytes[3] << 8));
        //MagnetVehicle = (short)(_recieveBytes[4] + (_recieveBytes[5] << 8));
    }
}
