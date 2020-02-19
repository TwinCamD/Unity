using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using UnityEngine.VR;
using ZenFulcrum.EmbeddedBrowser;

[RequireComponent(typeof(Browser))]

public class TwincamDataConnection : MonoBehaviour {

    public bool isUser;    //ユーザ側？ (HMD繋ぐほう)
    //private static SerialPort _serialPort = new SerialPort("COM3", 115200);

    private SerialPort _serialPort;
    public string portName;    //ポートの名前 windows(COM) mac(/dev/)
   
    private Browser browser;
	public float time = 0.0f;

    private bool _isRunning = false;

    //HMD関連
    public bool isReversed;    //反転する？
    public int hmdTwincamAngle = 450;    //HMD,Twincamの角度

    private string[] _sendDataArr = new string[2]; //string型でデータを格納[送りたいデータの要素数] 

    //private int sendSTART = 0;
    //private int recieveSTART = 0;
    private bool _connectStart = false;    //通信スタートしてる？

    //private string lastrcvd = "";
    //[SerializeField]
    //public int currentAngle;

    public upd_angleSend udp;
    public SensorY sensor;
    public int seatAngle;    //座席の角度
    private int _seatAngleOfset = 0;    //座席の角度のオフセット値
    private int _gearRatio = 2;    //ギア比
    

    // Use this for initialization
    void Start () {
        if (!isUser)
        {
            //Create SerialPort Instance
            _serialPort = new SerialPort(portName, 115200);

            //Open Serial Port
            if (_serialPort != null)
            {
                _serialPort.Open();
                _serialPort.ReadTimeout = 50;
                Debug.Log("Open Serial port");
            }
            else
            {
                _serialPort.Close();
                Debug.LogError("Failed to open Serial Port, already open!");
            }
        }
        
        //test
        browser = GetComponent<Browser>();
		//browser.CallFunction("Unitykiteru", "kiteru?", "pass").Then(ret => Debug.Log("SndResult: " + ret));
		//browser.CallFunction("Unitykiteruyo", "kiteru?", "pass").Then(ret => Debug.Log("RcvResult: " + ret)).Then(ret => hmdTwincamAngle);
	}
	
	// Update is called once per frame
	void Update () {
        if (isUser)
        {
            //HMDの角度リセット
            if (Input.GetKeyDown(KeyCode.Space))
            {
                UnityEngine.XR.InputTracking.Recenter();
                Debug.Log("Reset");
            }

            //HMD角度取得
            Quaternion rotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.CenterEye);
            hmdTwincamAngle = (int)(10.0f * 120.0f * rotation.y);
            if (isReversed)
            {
                hmdTwincamAngle *= -1;
            }
        } else
        {
            //センサの角度オフセット取得
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _seatAngleOfset = udp.angle / _gearRatio;
                Debug.Log("Reset");
            }
            //座席角度取得
            //seatAngle = udp.angle;
            //書いてない！ーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーー
        }

        //通信スタート
        if(Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.LeftControl))
        {
            _connectStart = true;
            Debug.Log("Start Connecting");
        }

        //通信
        if (_connectStart)
        {
            //送信
            StartCoroutine(SendWebRTC());

            //受信
            StartCoroutine(RecieveWebRTC());
            StartCoroutine(SerialWebRTC());
        }
        
        time += Time.deltaTime;
    }

    private IEnumerator SendWebRTC()
    {
        if (_isRunning)
            yield break;
        _isRunning = true;
        //////////////////////////////////////

        //送りたいデータをStringに変換してまとめる
        _sendDataArr[0] = hmdTwincamAngle.ToString();
        _sendDataArr[1] = seatAngle.ToString();

        //_sendData = hmdTwincamAngle.ToString();

        //配列を一つの文にする
        string _sendData = string.Join(",", _sendDataArr);

        browser.CallFunction("unityDataSend", _sendData);

        Debug.Log("SendData :"+ _sendData);
        //////////////////////////////////////
        _isRunning = false;
    }

    private IEnumerator RecieveWebRTC()
    {
        if (_isRunning)
            yield break;
        _isRunning = true;
        //////////////////////////////////////

        browser.RegisterFunction("unityDataRecieve", args => SplitData(args));
        //browser.CallFunction("unityDataRecieve", "kitekiteru?", "pass").Then(ret => DAINYU(ret));//.Then(ret => Debug.Log("RcvResult: " + ret));

        //////////////////////////////////////
        //yield return new WaitForSeconds(0.001f);
        _isRunning = false;
    }

    private void SplitData(JSONNode data)
    {
        //JSONにSplitメソッドがないため
        string _data = data;
        string[] _recievedDataArr;
        _recievedDataArr = _data.Split(',');

        Debug.Log("RecieveData :" + _data);

        //Debug.Log("recieveINFOis" + hensu);
        //Debug.Log(hmdTwincamAngle);


        Assign(_recievedDataArr);
    }

    private void Assign(string[] _recievedDataArr)
    {
        //string型をint型に変換
        int.TryParse(_recievedDataArr[0], out hmdTwincamAngle);
        int.TryParse(_recievedDataArr[1], out seatAngle);

        if (isReversed)
        {
            hmdTwincamAngle += (int)(udp.angle * 10);
        } else
        {
            hmdTwincamAngle -= (int)(udp.angle * 10);
        }

    }

    private IEnumerator SerialWebRTC()
    {
        if (_isRunning)
            yield break;
        _isRunning = true;
        //////////////////////////////////////


        ////Quaternion rotation = InputTracking.GetLocalRotation(VRNode.CenterEye);
        //Unity上に位置と角度記録
        //Debug.Log((hmdTwincamAngle));
        //Debug.Log((GetComponent<upd_angleSend>().receiveAngle));
        //hmdTwincamAngle -= (int)(GetComponent<upd_angleSend>().receiveAngle / 0.2f); 
        //hmdTwincamAngle -= (int)((GetComponent<upd_angleSend>().angle - 180) * 10);

        _serialPort.Write(hmdTwincamAngle.ToString() + "\0");

        byte[] rcv = new byte[13]; ;
        try
        {
            for (int x = 1; x <= 12; ++x)
            {
                rcv[x] = (byte)_serialPort.ReadByte();
                //Debug.Log(rcv);
            }

        }
        catch (System.Exception)
        {
            Debug.Log("something happened!");
        }


        //////////////////////////////////////
        //yield return new WaitForSeconds(0.00f);
        _isRunning = false;
    }



}
