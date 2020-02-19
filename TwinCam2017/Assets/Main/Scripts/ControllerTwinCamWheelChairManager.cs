/*Summary
 *  TwinCamのデータ関係をやり取りするスクリプト
 *  Controller側，車椅子使用
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Reflection;
using System.Threading;

public class ControllerTwinCamWheelChairManager : MonoBehaviour {

    #region Thread
    private Thread _thread;
    private static int FinalThreadSleepTime = 200;   //終了時にThreadが停止する時間
    private bool _isStartThread = false;
    #endregion

    #region FPS
    [SerializeField] private float _fixedDeltaTime = 0.01f;//FixedUpdateの時間 100fps
    [SerializeField] private int _fps = 0;//HMDの描画速度より大きくなればいい
    #endregion

    #region Serial
    private SerialEsp32 _serialEsp32;
    #endregion

    #region SkyWay
    private SkywayDataConnect _skywayDataConnect;

    [SerializeField] private bool _isLeft = true;
    //IDをあらかじめ決めておくため
    [SerializeField] private string _leftYourId = "twincamL";
    [SerializeField] private string _leftCallToId = "userL";
    [SerializeField] private string _rightYourId = "twincamR";
    [SerializeField] private string _rightCallToId = "userR";
    #endregion

    private short _twinCamAngle = 0;
    private short _accelVehicle = 0;

    private float _countTime = 0;    //timer

    // Use this for initialization
    void Awake() {
        #region FPS
        Time.fixedDeltaTime = _fixedDeltaTime;
        _fps = (int)(1 / _fixedDeltaTime);
        #endregion

        _skywayDataConnect = GetComponent<SkywayDataConnect>();
        if (_isLeft) {
            _skywayDataConnect.YourId = _leftYourId;
            _skywayDataConnect.CallToId = _leftCallToId;
        } else {
            _skywayDataConnect.YourId = _rightYourId;
            _skywayDataConnect.CallToId = _rightCallToId;
        }
    }

    void Start () {
        _serialEsp32 = GetComponent<SerialEsp32>();

        #region Thread
        if (!_isStartThread) {
            startThread();
        }
        #endregion
    }

    //Thereadを開始
    private void startThread() {
        _isStartThread = true;
        _thread = new Thread(threadUpdate);
        _thread.Start();
    }

    void OnDestroy() {
        if (_isStartThread) {
            _isStartThread = false;
            Thread.Sleep(FinalThreadSleepTime);
        }
    }

    // Update is called once per frame
    void Update () {
        #region データの構造確認用
        //var data = {送りたいデータ};
        //Debug.Log("data: " + data + ", type: " + data.GetType() + ", length: " + BitConverter.GetBytes(data).Length);
        #endregion

	    //timer
	    _countTime += Time.deltaTime;
    }

    void FixedUpdate() {
        setSkywayData();
        getSkywayData();
    }

    private void threadUpdate() {
        while (_isStartThread) {
            getSerial();
            setSerial();
        }
    }

    //skyway送信データ
    private void setSkywayData() {
        try {
            /*_skywayDataConnect.SendData[0] = {hogehoge}.ToString(); と書く*/
            _skywayDataConnect.SendData[0] = _countTime.ToString();
            _skywayDataConnect.SendData[1] = _accelVehicle.ToString();
        }
        catch (Exception e) {
            Debug.LogError(e.Message);
        }
    }

    //skyway受信データ
    private void getSkywayData() {
        try {
            /*{hogehoge} = ({キャスト}){受け取った値の型}.Parse(_skywayDataConnect.RecieveData[0]); と書く*/
            //_twinCamAngle = _converterAngleToPulse.Convert(int.Parse(_skywayDataConnect.RecieveData[1]));
            _twinCamAngle = (short)float.Parse(_skywayDataConnect.RecieveData[1]);
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
        _serialEsp32.SetByteData(BitConverter.GetBytes(_twinCamAngle));
        _serialEsp32.ResetByteDataCount();  //最後に必ずリセット
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

        _accelVehicle = (short)(
            (_serialEsp32.RecieveBytes[1] << 8) & 0xFF00 |
            (_serialEsp32.RecieveBytes[0] << 0) & 0x00FF
        );
    }
}
