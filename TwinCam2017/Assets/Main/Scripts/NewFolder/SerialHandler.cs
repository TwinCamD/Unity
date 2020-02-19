/*Summary
 *  Unityを使ってシリアル通信する
 *
 *Log
 *  2018.8.10 森田 unity2017.4.8f1
 *      実装
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO.Ports;
using System.Threading;

public class SerialHandler : MonoBehaviour {
    public delegate void SerialDataReceivedEventHandler(string message);
    public event SerialDataReceivedEventHandler OnDataReceived;

    private SerialPort _serialPort;
    [SerializeField] private string _portName = "COM";    //ポートの名前 windows(COM) mac(/dev/)
    [SerializeField] private int _baudRate = 115200;

    private Thread _thread;

    private bool _isRunning = false;

    private string _message;
    private bool _isNewMessageRecieved = false;

    void Awake() {
        //Open Serial Port
        _serialPort = new SerialPort(_portName, _baudRate);
        _serialPort.Open();

        _isRunning = true;

        _thread = new Thread(Read);
        _thread.Start();
    }

    void OnDestroy() {
        _isNewMessageRecieved = false;
        _isRunning = false;

        if (_thread != null && _thread.IsAlive) {
            _thread.Join();
        }

        if (_serialPort != null && _serialPort.IsOpen) {
            _serialPort.Close();
            _serialPort.Dispose();
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	    if (_isNewMessageRecieved) {
	        OnDataReceived(_message);
	    }
	    _isNewMessageRecieved = false;
    }

    private void Read() {
        while (_isRunning && _serialPort != null && _serialPort.IsOpen) {
            try {
                _message = _serialPort.ReadLine();
                _isNewMessageRecieved = true;
            }
            catch (System.Exception e) {
                Debug.LogWarning(e.Message);
            }
        }
    }

    public void Write(string message) {
        try {
            _serialPort.Write(message);
        }
        catch (System.Exception e) {
            Debug.LogWarning(e.Message);
        }
    }
}
