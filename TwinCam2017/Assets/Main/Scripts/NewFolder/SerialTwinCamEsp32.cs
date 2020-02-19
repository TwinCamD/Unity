/*Summary
 *  Unityを使ってEsp32とTwinCamの角度をシリアル通信する
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading;

public class SerialTwinCamEsp32 : MonoBehaviour {

    #region SerialPort関連
    private SerialPort _serialPort;
    [SerializeField] private string _portName = "/dev/cu.SLAB_USBtoUART";    //ポートの名前 windows(COM) mac(/dev/) 初期値はMacのESP32
    [SerializeField] private int _baudRate = 115200;
	private bool _isPortOpen = false;
    #endregion

    private Thread _thread;

    private static int RECIEVE_BYTE_COUNT = 6;
    private byte[] _rcvall = new byte[RECIEVE_BYTE_COUNT];

    public int TwinCamAngle = 0;    //送信する角度
    //プロパティ
    [SerializeField] private short _accelVehicle;   //乗り物の加速度
    public short AccelVehicle {
        get { return _accelVehicle; }
        private set { _accelVehicle = value; }
    }
    [SerializeField] private short _gyroVehicle;    //乗り物の角度
    public short GyroVehicle {
        get { return _gyroVehicle; }
        private set { _gyroVehicle = value; }
    }
    [SerializeField] private short _magnetVehicle;  //乗り物の磁力
    public short MagnetVehicle {
        get { return _magnetVehicle; }
        private set { _magnetVehicle = value; }
    }

    // Use this for initialization
    void Start () {		
        _serialPort = new SerialPort(_portName, _baudRate, Parity.None, 8, StopBits.One);
        _serialPort.ReadTimeout = 1;
        if (_serialPort.IsOpen) {
            _serialPort.Close();
        }
        _serialPort.Open();
		if (_serialPort.IsOpen) {
			_isPortOpen = true;
			Debug.Log ("Esp Open");

		}
		if (_isPortOpen) {
			_thread = new Thread(Communicate);
		    _thread.Start();
		    _serialPort.Write(new byte[] { 255 }, 0, 1);    //通信を受けたら開始するため最初に送信が必要
        }
    }

    void OnDestroy() {
        if (_isPortOpen) {
			_isPortOpen = false;
			Thread.Sleep (200);
            _serialPort.Close();
            //_serialPort.Dispose();
        }
    }

    private void Communicate() {
        int cnt = 0;   //受け取るbyteのためのカウンタ
        byte[] rcv = new byte[1];
        while (_isPortOpen) {
			try {
				_serialPort.Read (rcv, 0, 1);
			    //送信だけしてた時の名残
                //byte[] espKakudo = System.BitConverter.GetBytes(twinCamAngle);
                //_serialPort.Write(espKakudo, 0, 4);
                _rcvall[cnt] = rcv[0];
			    cnt++;
                //countが行きすぎたら...
                if (cnt >= RECIEVE_BYTE_COUNT) {
					byte[] espKakudo = System.BitConverter.GetBytes (TwinCamAngle);
					_serialPort.Write (espKakudo, 0, 4);
				    //6byte 3data 順番が怪しい？
                    AccelVehicle = (short)(-100 + _rcvall[0] + (_rcvall[1] << 8));
				    GyroVehicle = (short)(_rcvall[2] + (_rcvall[3] << 8));
				    MagnetVehicle = (short)(_rcvall[4] + (_rcvall[5] << 8));
				    cnt = 0;   //カウンタを初期化
                }
			} catch (System.Exception e) {
				Debug.LogError(e.Message);
			}
            Thread.Sleep(1);
        }
    }
}
