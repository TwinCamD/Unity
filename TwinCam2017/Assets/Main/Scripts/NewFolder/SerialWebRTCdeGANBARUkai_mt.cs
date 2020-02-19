using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using UnityEngine.VR;
using ZenFulcrum.EmbeddedBrowser;

/**
 * A very simple controller for a Browser.
 * Call GoToURLInput() to go to the URL typed in urlInput
 */
[RequireComponent(typeof(Browser))]

public class SerialWebRTCdeGANBARUkai_mt : MonoBehaviour {


    //private static SerialPort sp = new SerialPort("COM3", 115200);
    private SerialPort sp;
    public int comN =3;
    private string COMNUMBER;

    private string message_;
    bool isRunning = false;

    private Browser browser;
	public float time = 0.0f;
	public int kakudo = 1801;
    private int countAngle = 0;

	private int sendSTART = 0;
	private int recieveSTART = 0;
    
    private string lastrcvd = "";
    //[SerializeField]
    //public int currentAngle;

    public bool dk2;
    public int HMDkakudo = 0;

    // Use this for initialization
    void Start () {
        //Create SerialPort Instance
        COMNUMBER = "COM" + comN.ToString();
        sp = new SerialPort(COMNUMBER, 115200);

        //Open Serial Port
        if (sp != null)
        {
            if (sp.IsOpen)
            {
                sp.Close();
                Debug.LogError("Failed to open Serial Port, already open!");
            }
            else
            {
                sp.Open();
                sp.ReadTimeout = 50;
                Debug.Log("Open Serial port");
            }
        }
        //test
        browser = GetComponent<Browser>();
		browser.CallFunction("Unitykiteru", "kiteru?", "pass").Then(ret => Debug.Log("SndResult: " + ret));
		browser.CallFunction("Unitykiteruyo", "kiteru?", "pass").Then(ret => Debug.Log("RcvResult: " + ret)).Then(ret => kakudo);
	}
	
	// Update is called once per frame
	void Update () {
        //角度リセット
        if (Input.GetKeyDown(KeyCode.Space))
        {
            UnityEngine.XR.InputTracking.Recenter();
        }


        //角度取得
        Quaternion rotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.CenterEye);
        Vector3 a = rotation.eulerAngles;
        //if(a.y > 359.9)
        //{
        //    countAngle++;
        //}
        //if (a.y < 0.1)
        //{
        //    countAngle--;
        //}
        //HMDkakudo = (int)(a.y + countAngle * 360);
        HMDkakudo = (int)(a.y);
        if (!dk2)
        {
            HMDkakudo = 360 - HMDkakudo;
        }

        
        //送信
        if (Input.GetKey(KeyCode.A))
        {
            sendSTART = 1;
            //recieveSTART = 0;
            Debug.Log("Send Start");
        }
        if (sendSTART == 1)
        {
			StartCoroutine(sendWebRTC());
			//browser = GetComponent<Browser>();
            //browser.CallFunction("Unitykiteru", HMDkakudo, "pass").Then(ret => Debug.Log("SndResult: " + ret));
        }
        
        //受信        
        if (Input.GetKey(KeyCode.L))
        {
            recieveSTART = 1;
            //sendSTART = 0;
            Debug.Log("Recieve Start");
        }
        if (recieveSTART == 1)
        {
			StartCoroutine(recieveWebRTC());
			//browser = GetComponent<Browser>();
            //browser.CallFunction("Unitykiteruyo", "kitekiteru?", "pass").Then(ret => DAINYU(ret));
            StartCoroutine(SerialWebRTC());
            //Debug.Log(time);
        }
        
        time = time + Time.deltaTime;
    }

    public void DAINYU(JSONNode hensu)
    {
        kakudo = JSONNode.Parse(hensu);
        //Debug.Log("recieveINFOis" + hensu);
        //Debug.Log(kakudo);
    }

    private IEnumerator SerialWebRTC()
    {
        if (isRunning)
            yield break;
        isRunning = true;
        //////////////////////////////////////

        //Quaternion rotation = InputTracking.GetLocalRotation(VRNode.CenterEye);
        //Unity上に位置と角度記録
        Debug.Log((int)(kakudo));
        sp.Write(((int)(kakudo)).ToString() + "\0");

        byte[] rcv = new byte[13]; ;
        char tmp;
        try
        {
            for (int x = 1; x <= 12; ++x)
            {
                rcv[x] = (byte)sp.ReadByte();
                //Debug.Log(rcv);
            }

        }
        catch (System.Exception)
        {
            Debug.Log("something happened!");
        }
        //////////////////////////////////////
        //yield return new WaitForSeconds(0.00f);
        isRunning = false;
    }

	private IEnumerator sendWebRTC()
	{
		if (isRunning)
			yield break;
		isRunning = true;
		//////////////////////////////////////

		browser = GetComponent<Browser>();
		browser.CallFunction("Unitykiteru", HMDkakudo, "pass")/*.Then(ret => Debug.Log("SndResult: " + ret))*/;

		//////////////////////////////////////
		//yield return new WaitForSeconds(0.005f);
		isRunning = false;
	}

	private IEnumerator recieveWebRTC()
	{
		if (isRunning)
			yield break;
		isRunning = true;
		//////////////////////////////////////

		browser = GetComponent<Browser>();
		//browser.CallFunction("IDsend", "kirin", "pass").Then(ret => Debug.Log("Result: " + ret));
		browser.CallFunction("Unitykiteruyo", "kitekiteru?", "pass").Then(ret => DAINYU(ret));//.Then(ret => Debug.Log("RcvResult: " + ret));

		//////////////////////////////////////
		//yield return new WaitForSeconds(0.001f);
		isRunning = false;
	}
}
