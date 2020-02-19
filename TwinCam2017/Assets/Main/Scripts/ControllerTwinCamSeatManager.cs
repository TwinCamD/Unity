/*Summary
 *  TwinCamのデータ関係をやり取りするスクリプトのひな型
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Reflection;

public class ControllerTwinCamSeatManager : MonoBehaviour {

    [SerializeField] private float _fixedDeltaTime = 0.01f;//FixedUpdateの時間 100fps
    [SerializeField] private int _fps = 0;//HMDの描画速度より大きくなればいい

    private SerialEsp32 _serialEsp32;
    private SkywayDataConnect _skywayDataConnect;

    private int _twinCamAngle = 0;

    private short _accelVehicle = 0;
    private short _gyroVehicle = 0;
    private short _magnetVehicle = 0;

    //timer
    private float countTime = 0;

    void Awake() {
        Time.fixedDeltaTime = _fixedDeltaTime;
        _fps = (int)(1 / _fixedDeltaTime);
    }

    // Use this for initialization
    void Start () {
        _serialEsp32 = GetComponent<SerialEsp32>();
        _skywayDataConnect = GetComponent<SkywayDataConnect>();
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
        getSerial();
        setSkywayData();

        getSkywayData();
        setSerial();
    }

    //skyway送信データ
    private void setSkywayData() {
        try {
            /*_skywayDataConnect.SendData[0] = {hogehoge}.ToString(); と書く*/
            _skywayDataConnect.SendData[0] = countTime.ToString();
            _skywayDataConnect.SendData[1] = _accelVehicle.ToString();
            _skywayDataConnect.SendData[2] = _gyroVehicle.ToString();
            _skywayDataConnect.SendData[3] = _magnetVehicle.ToString();
        }
        catch (Exception e) {
            Debug.LogError(e.Message);
        }
    }

    //skyway受信データ
    private void getSkywayData() {
        try {
            /*{hogehoge} = ({キャスト}){受け取った値の型}.Parse(_skywayDataConnect.RecieveData[0]); と書く*/
            _twinCamAngle = (int)float.Parse(_skywayDataConnect.RecieveData[1]);
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
        _gyroVehicle = (short)(
            (_serialEsp32.RecieveBytes[3] << 8) & 0xFF00 |
            (_serialEsp32.RecieveBytes[2] << 0) & 0x00FF
        );
        _magnetVehicle = (short)(
            (_serialEsp32.RecieveBytes[5] << 8) & 0xFF00 |
            (_serialEsp32.RecieveBytes[4] << 0) & 0x00FF
        );
    }
}
