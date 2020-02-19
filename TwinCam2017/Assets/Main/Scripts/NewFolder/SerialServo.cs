/*Summary
 *  Unityを使ってEsp32とシリアル通信する
 *  決まった個数のデータをやり取りすること前提
 *  最初にデータを送信するとESP32からデータが返ってくる仕様
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Reflection;
using System.Threading;
using UnityEngine;

public class SerialServo : MonoBehaviour {

    private Thread _thread;
    private static int FinalThreadSleepTime = 200; //終了時にThreadが停止する時間

    #region SerialPort関連
    private SerialPort _serialPort;
    [SerializeField] private string _portName = "/dev/cu.SLAB_USBtoUART"; //ポートの名前 windows(COM) mac(/dev/) 初期値はMacのESP32
    [SerializeField] private int _baudRate = 115200;
    private bool _isPortOpen = false; //ポートが開いているか
    #endregion

    #region 送信用変数
    private static int _sendBytesCount = 32; //送るデータのbyteの総数
    private byte[] _sendBytes = new byte[_sendBytesCount]; //データ送信用の配列
    #endregion

    #region 受信用変数
    // [SerializeField] private int _recieveBytesCount = 6; //受け取るデータのバイト数
    // [SerializeField] private byte[] _recieveBytes;  //プロパティ用
    // public byte[] RecieveBytes {    //データ受信用の配列
    //     get { return _recieveBytes; }
    //     private set { _recieveBytes = value; }
    // }
    // [SerializeField] private int _readBufferSize = 4096;//4096が規定値
    #endregion

    public float ServoAngle = 0; //ServoMotorの角度

    // Use this for initialization
    void Start () {
        setupSerialPort ();
        if (_isPortOpen) {
            startThread ();
        }
    }

    void OnDestroy () {
        if (_isPortOpen) {
            _isPortOpen = false;
            Thread.Sleep (FinalThreadSleepTime);
            _serialPort.Close ();
        }
    }

    //SerialPortを準備
    private void setupSerialPort () {
        _serialPort = new SerialPort (_portName, _baudRate, Parity.None, 8, StopBits.One) {
            // ReadTimeout = 1,    //読み取り待機時間を設定
            // ReadBufferSize = _readBufferSize
        };
        if (_serialPort.IsOpen) {
            _serialPort.Close ();
        }
        _serialPort.Open ();
        if (_serialPort.IsOpen) {
            _isPortOpen = true;
            Debug.Log ("Serial Open");
        }
    }

    //Thereadを開始
    private void startThread () {
        _thread = new Thread (communicate);
        _thread.Start ();
    }

    //Serial通信 無限ループ
    private void communicate () {
        torque (1, true);
        Thread.Sleep (10);

        while (_isPortOpen) {
            move (1, ServoAngle);
            Thread.Sleep (10);
        }
    }

    /**
     *  サーボのトルクをON/OFFする
     *
     *  @param sId サーボID
     *  @param sMode ON/OFFフラグ trueでトルクON
     */
    private void torque (int sId, bool sMode) {
        // パケット作成
        _sendBytes[0] = (byte) (0xFA); // ヘッダー1
        _sendBytes[1] = (byte) (0xAF); // ヘッダー2
        _sendBytes[2] = (byte) (sId); // サーボID
        _sendBytes[3] = (byte) (0x00); // フラグ
        _sendBytes[4] = (byte) (0x24); // アドレス(0x24=36)
        _sendBytes[5] = (byte) (0x01); // 長さ(1byte)
        _sendBytes[6] = (byte) (0x01); // 個数
        if (sMode) {
            _sendBytes[7] = (byte) (0x01); // ON/OFFフラグ
        } else {
            _sendBytes[7] = (byte) (0x00);
        }
        // チェックサムの計算
        byte sum = _sendBytes[2]; //ヘッダーは含まない
        for (int i = 3; i < 8; i++) {
            sum = (byte) (sum ^ _sendBytes[i]);
        }
        _sendBytes[8] = sum; // チェックサム

        // 送信
        _serialPort.Write (_sendBytes, 0, 9);
    }

    /**
     *  サーボを指定角度へ動かす
     *  可動範囲は中央が0度で，サーボ上面から見て時計回りが+，反時計回りが-
     *  指定角度の単位は0.1度単位
     *
     *  @param sId サーボID
     *  @param sPos 指定角度 90度 = 900
     */
    public void move (int sId, float angle) {
        int sPos = (int)(10 * angle);

        // パケット作成
        _sendBytes[0] = (byte) 0xFA; // ヘッダー1
        _sendBytes[1] = (byte) 0xAF; // ヘッダー2
        _sendBytes[2] = (byte) sId; // サーボID
        _sendBytes[3] = (byte) 0x00; // フラグ
        _sendBytes[4] = (byte) 0x1E; // アドレス(0x1E=30)
        _sendBytes[5] = (byte) 0x02; // 長さ(2byte)
        _sendBytes[6] = (byte) 0x01; // 個数
        _sendBytes[7] = (byte) (sPos & 0x00FF); // 位置
        _sendBytes[8] = (byte) ((sPos & 0xFF00) >> 8); // 位置
        // チェックサムの計算
        byte sum = _sendBytes[2];
        for (int i = 3; i < 9; i++) {
            sum = (byte) (sum ^ _sendBytes[i]);
        }
        _sendBytes[9] = sum; // チェックサム

        // 送信
        _serialPort.Write (_sendBytes, 0, 10);
    }

}