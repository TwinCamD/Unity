/*Summary
 *  SkyWayのDataChannelを用いてUnityからデータ通信を行う
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using ZenFulcrum.EmbeddedBrowser;

[RequireComponent(typeof(Browser))]    //Browserをアタッチすること前提
public class SkywayDataConnect : MonoBehaviour {

    private Browser _browser;

    //PeerID
    public string YourId = "twincam";
    public string CallToId = "user";

    #region 送信用変数
    [SerializeField] private static int SendDataCount = 2;    //送るデータの数
    public string[] SendData = new string[SendDataCount];
    #endregion

    #region 受信用変数
    [SerializeField] private static int RecieveDataCount = 2;    //受け取るデータの数
    //プロパティ
    [SerializeField] private string[] _recieveData = new string[RecieveDataCount];
    public string[] RecieveData {
        get { return _recieveData; }
        private set { _recieveData = value; }
    }

    [SerializeField] private bool _isDebugRecieveData = true;    //Debug用
    #endregion

    void Awake() {
        _browser = GetComponent<Browser>();
    }

	// Use this for initialization
	void Start () {
	    GetPeerId();
	}

	// Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.C)) {
            Connect();
        }
        if (Input.GetKeyDown(KeyCode.X)) {
            Close();
        }
    }

    void FixedUpdate() {
        dataSend();
        dataRecieve();
    }

    //PeerIdの取得
    public void GetPeerId() {
        _browser.CallFunction("UnityGetPeerId", YourId, CallToId);
    }

    //Connect処理
    public void Connect() {
        _browser.CallFunction("UnityConnect", CallToId);
    }

    //Close処理
    public void Close() {
        _browser.CallFunction("UnityClose");
    }

    //送信
    private void dataSend() {
        try {
            /*
             他のScriptからstringにしてデータを送る ※ここに書かないこと
             例
             private SkywayDataConnect _skywaydataconnect;
             _skywaydataconnect.SendData[0] = _twinCamHmd.HmdAngle.ToString();
            */

            //Webページのjs関数を呼ぶ
            _browser.CallFunction("DataSend", string.Join(",", SendData)).Done();
        }
        catch (Exception e) {
            Debug.LogWarning(e.Message);
        }
    }

    //受信
    private void dataRecieve() {
        try {
            //Webページのjs関数が呼ばれたら
            _browser.RegisterFunction("DataRecieve", result => {
                RecieveData[0] = result[0];               //とりあえずstringを受け取って
                RecieveData = RecieveData[0].Split(',');//分配する

                /*
                 あとは他のScriptから呼び出すだけ(Stringなのに注意) ※ここに書かないこと
                 例
                 private SkywayDataConnect _skywaydataconnect;
                 {送り先} = ({キャスト}){送った型}.Parse(_skywayDataConnect.RecieveData[0]);
                */

                //debug用
                if (_isDebugRecieveData) {
                    for (int i = 0; i < RecieveDataCount; i++) {
                        Debug.Log("RecieveData " + i + " :" + RecieveData[i]);
                    }
                }
            });
        }
        catch (Exception e) {
            Debug.LogWarning(e.Message);
        }
    }
}
