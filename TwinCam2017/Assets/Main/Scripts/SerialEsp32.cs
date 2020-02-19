/*Summary
 *  Unityを使ってEsp32とシリアル通信する
 *  決まった個数のデータをやり取りすること前提
 *  最初にデータを送信するとESP32からデータが返ってくる仕様
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO.Ports;
using System.Reflection;
using System.Threading;

public class SerialEsp32 : MonoBehaviour {

    private Thread _thread;
    private static int FinalThreadSleepTime = 200;   //終了時にThreadが停止する時間

    #region SerialPort関連
    private SerialPort _serialPort;
    [SerializeField] private string _portName = "/dev/cu.SLAB_USBtoUART";    //ポートの名前 windows(COM) mac(/dev/) 初期値はMacのESP32
    [SerializeField] private int _baudRate = 115200;
	private bool _isPortOpen = false;   //ポートが開いているか
    #endregion

    #region 送信用変数
    private int _setDataCount = 0;  //送るデータをきれいに配置するためのカウント
    private bool _isCreateSendBytesArr = false;    //配列が準備されたか １度だけ呼ばれる
    private bool _isReadySendBytes = false;    //送信が可能かどうか １度だけ呼ばれる
    private int _sendBytesCount = 0; //送るデータのbyteの総数
    [SerializeField] private byte[] _sendBytes; //データ送信用の配列
    #endregion

    #region 受信用変数
    [SerializeField] private int _recieveBytesCount = 6; //受け取るデータのバイト数
    [SerializeField] private byte[] _recieveBytes;  //プロパティ用
    public byte[] RecieveBytes {    //データ受信用の配列
        get { return _recieveBytes; }
        private set { _recieveBytes = value; }
    }
    [SerializeField] private int _readBufferSize = 4096;//4096が規定値
    #endregion

    // Use this for initialization
    void Start () {
        RecieveBytes = new byte[_recieveBytesCount];
        setupSerialPort();
        if (_isPortOpen) {
            startThread();
        }
    }

    void OnDestroy() {
        if (_isPortOpen) {
            _isPortOpen = false;
            Thread.Sleep(FinalThreadSleepTime);
            _serialPort.Close();
        }
    }

    //SerialPortを準備
    private void setupSerialPort() {
        _serialPort = new SerialPort(_portName, _baudRate, Parity.None, 8, StopBits.One) {
            ReadTimeout = 1,    //読み取り待機時間を設定
            ReadBufferSize = _readBufferSize
        };
        if (_serialPort.IsOpen) {
            _serialPort.Close();
        }
        _serialPort.Open();
        if (_serialPort.IsOpen) {
            _isPortOpen = true;
            Debug.Log("Esp Open");
        }
    }

    //Thereadを開始
    private void startThread() {
        _thread = new Thread(communicate);
        _thread.Start();
    }

    //Serial通信 無限ループ
    private void communicate() {
        byte[] tmpData = new byte[1];   //配列じゃないと受け取れない
        byte[] tmpRevieveBytes = new byte[_recieveBytesCount];
        int i = _recieveBytesCount; //１回目にSendから入るため
        while (_isPortOpen) {   //whileの中でfor文ぽいことをしている
            if (_isReadySendBytes) {
                if (!(i < _recieveBytesCount)) {
                    //Read
                    RecieveBytes = tmpRevieveBytes;
                    //Write
                    _serialPort.Write(_sendBytes, 0, _sendBytesCount);
                    i = 0;
                } else {
                    //Read
                    try {
                        _serialPort.Read(tmpData, 0, 1);
                        tmpRevieveBytes[i] = tmpData[0];
                        i++;
                    }
                    catch (Exception e) {
                        //Debug.LogWarning(e.Message);    //確実に値を取りこぼすタイミングがあるためWarningに
                    }
                }
            }
        }
    }

    //データをbyteに変換して送るために保持する
    public void SetByteData(byte[] bytedata) {
        int byteLength = bytedata.Length;
        if (_isCreateSendBytesArr) {
            try {
                for (int i = 0; i < byteLength; i++) {
                    _sendBytes[_setDataCount + i] = bytedata[i];    //前からデータを詰める
                }
                if (!_isReadySendBytes) {   //一度だけ判定
                    _isReadySendBytes = true;
                }
            }
            catch (Exception e) {
                Debug.LogError(e.Message);
            }
        }
        _setDataCount += byteLength;    //詰めたデータの長さを取っておく

        //受信用配列の大きさを設定
        if (!_isCreateSendBytesArr && _sendBytesCount < _setDataCount) {
            _sendBytesCount = _setDataCount;    //最大値を保存
        }
    }

    //データを１巡セットしたらカウントをリセット
    public void ResetByteDataCount() {
        _setDataCount = 0;
        if (!_isCreateSendBytesArr) {   //一度だけ判定
            _isCreateSendBytesArr = true;
            _sendBytes = new byte[_sendBytesCount]; //受信用配列を準備
        }
    }
}
