/*Summary
 *  Remote版のData通信のScriptを管理する
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

[RequireComponent(typeof(SkywayDataConnect))]    //SkywayDataConnectをアタッチすること前提
public class RemoteTwinCamManager_Controller : MonoBehaviour {

    private SkywayDataConnect _skywayDataConnect;

	private SerialTwinCamEsp32 _serialTwinCamEsp32;

    //timer
    private float countTime = 0;

    // Use this for initialization
    void Start() {
        _skywayDataConnect = GetComponent<SkywayDataConnect>();

	_serialTwinCamEsp32 = GetComponent<SerialTwinCamEsp32>();
    }

    // Update is called once per frame
    void Update() {
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
	        _skywayDataConnect.SendData[1] = _serialTwinCamEsp32.AccelVehicle.ToString();
	        _skywayDataConnect.SendData[2] = _serialTwinCamEsp32.GyroVehicle.ToString();
	        _skywayDataConnect.SendData[3] = _serialTwinCamEsp32.MagnetVehicle.ToString();
        }
        catch (Exception e) {
            Debug.LogWarning(e.Message);
        }
    }

    //受信データ整理
    private void getRecieveData() {
        try {
            //{hogehoge} = ({キャスト}){受け取った値の型}.Parse(_skywayDataConnect.RecieveData[0]); と書く
        	_serialTwinCamEsp32.TwinCamAngle = (int)float.Parse(_skywayDataConnect.RecieveData[1]);
        }
        catch (Exception e) {
            Debug.LogWarning(e.Message);
        }
    }
}
