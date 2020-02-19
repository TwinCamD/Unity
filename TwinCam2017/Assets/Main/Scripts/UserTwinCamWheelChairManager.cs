/*Summary
 *  TwinCamのデータ関係をやり取りするスクリプト
 *  User側，車椅子使用
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Reflection;
using System.Threading;

public class UserTwinCamWheelChairManager : MonoBehaviour {

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
    private SkywayDataConnect _skywayDataConnectRight;
    [SerializeField] private GameObject _leftBrowserObj;
    private SkywayDataConnect _skywayDataConnectLeft;
    #endregion

    #region HMD
    private TwinCamHmdController _twinCamHmdController;
    [SerializeField] private bool _isAngleZero = false; //角度を0にするか
    [SerializeField] private bool _isReversedAngle = false;  //送る角度データを反転するか HmdAngleを使うときはtrueじゃないと反転
    //[SerializeField] [Range(-360f, 360f)] private float _offsetAngle = 0f;
    #endregion

    #region WheelChair
    [SerializeField] private bool _isMoveWheelChair = true;   //座席を動作させるかどうか
    private WheelChairByAccelerometer _wheelChairByAccelerometer;
    [SerializeField] private bool _isResetWheelChair = false;
    #endregion

    #region RealSense
    [SerializeField] private GameObject _rsDeviceObj;
    private RealsenseAngleCanceller _realsenseAngleCanceller;
    #endregion

    #region DelayAccel
    private const float _maximumDelayTime = 3f;//遅延できる秒数の最大
    [SerializeField] [Range(0f, _maximumDelayTime)] private float _delayTime = 0.4f;//遅延時間
    private int _maximumBufferSize = 0;
    private int _index = 2;//遅延させる値の数
    private short[,] _accelBuffer;//加速度を一時保存するためのバッファ
    private int _recordCount = 0;//値を保存するため
    private int _playCount = 0;//値を再生するため
    #endregion

    private float countTime = 0;    //timer

    // Use this for initialization
    void Awake() {
        #region FPS
        Time.fixedDeltaTime = _fixedDeltaTime;
        _fps = (int)(1 / _fixedDeltaTime);
        #endregion

        #region DelayAccel
        _maximumBufferSize = (int)(_maximumDelayTime / _fixedDeltaTime);
        #endregion
    }
    
    void Start () {
        _accelBuffer = new short[_maximumBufferSize, _index];

        _serialEsp32 = GetComponent<SerialEsp32>();
        _skywayDataConnectRight = GetComponent<SkywayDataConnect>();
        _skywayDataConnectLeft = _leftBrowserObj.GetComponent<SkywayDataConnect>();
        _twinCamHmdController = GetComponent<TwinCamHmdController>();
        _wheelChairByAccelerometer = GetComponent<WheelChairByAccelerometer>();
        _realsenseAngleCanceller = _rsDeviceObj.GetComponent<RealsenseAngleCanceller>();

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

	    //角度を強制的に0に(角度取得一時ストップ)
	    if (Input.GetKeyDown(KeyCode.S)) {
	        _isAngleZero = !_isAngleZero;   //boolを反転
	        Debug.Log("<color=#0000ffff>角度</color>取得停止が<b><color=#0000ffff>" + _isAngleZero + "</color></b>になりました");    //color=青
	    }

	    if (Input.GetKeyDown(KeyCode.I)) {
	        _isResetWheelChair = !_isResetWheelChair;
	    }

        //timer
        countTime += Time.deltaTime;
    }

    void FixedUpdate() {
        setSkywayData();
        getSkywayData();

        _wheelChairByAccelerometer.RawAccelRight = _accelBuffer[_playCount, 0]; //Right 0
        _wheelChairByAccelerometer.RawAccelLeft = _accelBuffer[_playCount, 1];  //Left 1
    }

    private void threadUpdate() {
        while (_isStartThread) {
            getSerial();
            setSerial();
        }
    }

    //skyway送信データ
    private void setSkywayData() {
        //Right
        try {
            /*_skywayDataConnect.SendData[0] = {hogehoge}.ToString(); と書く*/
            _skywayDataConnectRight.SendData[0] = countTime.ToString();
            if (_isAngleZero) {
                _skywayDataConnectRight.SendData[1] = "0";  //0を直接送る
            } else {
                if (_isMoveWheelChair) {    //座席あり
                    if (_isReversedAngle) {
                        _skywayDataConnectRight.SendData[1] =
                            (-_realsenseAngleCanceller.HmdOffsetInfAngle ).ToString();
                    } else {
                        _skywayDataConnectRight.SendData[1] =
                            (_realsenseAngleCanceller.HmdOffsetInfAngle ).ToString();
                    }
                } else {    //座席なし
                    if (_isReversedAngle) {
                        _skywayDataConnectRight.SendData[1] =
                            (-_twinCamHmdController.HmdInfAngle ).ToString();
                    } else {
                        _skywayDataConnectRight.SendData[1] =
                            (_twinCamHmdController.HmdInfAngle ).ToString();
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
        //Right
        try {
            if (_isMoveWheelChair) {
                /*{hogehoge} = ({キャスト}){受け取った値の型}.Parse(_skywayDataConnect.RecieveData[0]); と書く*/
                _accelBuffer[_recordCount, 0] = short.Parse(_skywayDataConnectRight.RecieveData[1]); //Right 0
            }
        }
        catch (Exception e) {
            Debug.LogWarning(e.Message);
        }
        //Left
        try {
            if (_isMoveWheelChair) {
                _accelBuffer[_recordCount, 1] = short.Parse(_skywayDataConnectLeft.RecieveData[1]); //Left 1
            }
        }
        catch (Exception e) {
            Debug.LogWarning(e.Message);
        }

        _recordCount = (_recordCount + 1) % _maximumBufferSize;
        _playCount = (_recordCount - (int)(_delayTime / _fixedDeltaTime) + _maximumBufferSize) % _maximumBufferSize;
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
        if (_isMoveWheelChair) {
            byte[] header = new byte[1];//header to control wheelchair with accelerometers
            if (_isResetWheelChair) {
                header[0] = (byte) 2;
            } else {
                header[0] = (byte) 6;
            }
            _serialEsp32.SetByteData(header);
            _serialEsp32.SetByteData(BitConverter.GetBytes(_wheelChairByAccelerometer.AccelLeft));
            _serialEsp32.SetByteData(BitConverter.GetBytes(_wheelChairByAccelerometer.AccelRight));
            _serialEsp32.SetByteData(BitConverter.GetBytes(_wheelChairByAccelerometer.MaxSpeed));
            _serialEsp32.SetByteData(_wheelChairByAccelerometer.BackHomeSpeed);
            _serialEsp32.SetByteData(_wheelChairByAccelerometer.Factor);

            _serialEsp32.ResetByteDataCount();  //最後に必ずリセット   
        }
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
        byte[] recieveBytes = new byte[5];
        recieveBytes = _serialEsp32.RecieveBytes;
        //Debug.Log(recieveBytes[0]+", "+ recieveBytes[1] + ", " + recieveBytes[2] + ", " + recieveBytes[3] + ", " + recieveBytes[4]);
        if (_isMoveWheelChair) {
            byte rcvControlWord = recieveBytes[0];
            if (rcvControlWord == 100) {
                _wheelChairByAccelerometer.Encoder1 = (
                    recieveBytes[1]
                    + (recieveBytes[2] << 8)
                    + (recieveBytes[3] << 16)
                    + (recieveBytes[4] << 24)
                );
            }
            else if (rcvControlWord == 101) {
                _wheelChairByAccelerometer.Encoder2 = (
                    recieveBytes[1]
                    + (recieveBytes[2] << 8)
                    + (recieveBytes[3] << 16)
                    + (recieveBytes[4] << 24)
                );
            }
            else {
                _wheelChairByAccelerometer.MotorLoop = recieveBytes[1];
                _wheelChairByAccelerometer.DataLoop = (short)(
                    recieveBytes[2]
                    + (recieveBytes[3] << 8)
                );
                _wheelChairByAccelerometer.HomePoint = recieveBytes[4];
            }
            _wheelChairByAccelerometer.RcvControlWord = rcvControlWord;
        }
    }
}
