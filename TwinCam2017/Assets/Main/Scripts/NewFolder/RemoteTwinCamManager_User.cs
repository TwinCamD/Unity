/*Summary
 *  Remote版のData通信のScriptを管理する
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

[RequireComponent(typeof(SkywayDataConnect))]    //SkywayDataConnectをアタッチすること前提
public class RemoteTwinCamManager_User : MonoBehaviour {

    private SkywayDataConnect _skywayDataConnect;

    private TwinCamHmdController _twinCamHmdController;
    [SerializeField] private bool _isMoveSeat = true;

    private SerialChairEsp32 _serialChairEsp32;

    //timer
    private float countTime = 0;

    // Use this for initialization
    void Start () {
        _skywayDataConnect = GetComponent<SkywayDataConnect>();

        _twinCamHmdController = GetComponent<TwinCamHmdController>();
        _serialChairEsp32 = GetComponent<SerialChairEsp32>();
    }
	
	// Update is called once per frame
	void Update () {
        setSendData();
	    getRecieveData();

	    //timer
        countTime += Time.deltaTime;
	}

    //送信データ整理
    private void setSendData() {
        try {
            //_skywayDataConnect.SendData[0] = {hogehoge}.ToString(); と書く
            _skywayDataConnect.SendData[0] = countTime.ToString();
            if (_isMoveSeat) {
                _skywayDataConnect.SendData[1] = _serialChairEsp32.HmdOffsetAngle.ToString(); //座席あり
            }
            else {
                _skywayDataConnect.SendData[1] = _twinCamHmdController.GetHmdAngle(true).ToString(); //座席なし
            }
        }
        catch (Exception e) {
            Debug.LogWarning(e.Message);
        }
    }

    //受信データ整理
    private void getRecieveData() {
        try {
            //{hogehoge} = ({キャスト}){受け取った値の型}.Parse(_skywayDataConnect.RecieveData[0]); と書く
            _serialChairEsp32.AccelChair = short.Parse(_skywayDataConnect.RecieveData[1]);
            _serialChairEsp32.GyroChair = short.Parse(_skywayDataConnect.RecieveData[2]);
            _serialChairEsp32.GyroChair_X = short.Parse(_skywayDataConnect.RecieveData[3]);
        }
        catch (Exception e) {
            Debug.LogWarning(e.Message);
        }
    }
}
