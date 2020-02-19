/*Summary
 *  TwinCamのデータ関係をやり取りするスクリプト
 *  Controller側
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Reflection;
using System.Threading;


public class ControllerTwinCamServoManager_old : MonoBehaviour {

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
    private SerialServo _serialServo;
    #endregion

    #region SkyWay
    private SkywayDataConnect _skywayDataConnect;
    #endregion

    private float _twinCamAngle = 0;

    private float countTime = 0;    //timer

    // Use this for initialization
    void Awake() {
        #region FPS
        Time.fixedDeltaTime = _fixedDeltaTime;
        _fps = (int) (1 / _fixedDeltaTime);
        #endregion
    }

    void Start () {
        _serialServo = GetComponent<SerialServo>();
        _skywayDataConnect = GetComponent<SkywayDataConnect>();

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
    void Update() {
        #region データの構造確認用
        //var data = {送りたいデータ};
        //Debug.Log("data: " + data + ", type: " + data.GetType() + ", length: " + BitConverter.GetBytes(data).Length);
        #endregion

        //timer
        countTime += Time.deltaTime;
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
            _skywayDataConnect.SendData[0] = countTime.ToString();
        }
        catch (Exception e) {
            Debug.LogError(e.Message);
        }
    }

    //skyway受信データ
    private void getSkywayData() {
        try {
            /*{hogehoge} = ({キャスト}){受け取った値の型}.Parse(_skywayDataConnect.RecieveData[0]); と書く*/
            _twinCamAngle = float.Parse(_skywayDataConnect.RecieveData[1]);
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

        _serialServo.ServoAngle = _twinCamAngle;
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
    }
}
