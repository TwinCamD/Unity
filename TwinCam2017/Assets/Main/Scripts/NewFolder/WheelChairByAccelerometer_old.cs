using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using System.IO.Ports;
using System.Threading;
using System;
using UnityEngine.UI;

public class WheelChairByAccelerometer_old : MonoBehaviour {
    public string portName = "COM7";
    int baudRate = 115200;
    System.IO.Ports.SerialPort serialPort_;
    private Thread thread_;
    bool serialOpended = false;
    //public GameObject AccelerationLeft, AccelerationRight;
    public int rcvControlWord, Encoder1, Encoder2, MotorLoop, dataLoop;
    public int HomePoint = 0;
    public short AccelLeft, AccelRight, OffsetAccelLeft, OffsetAccelRight;
    int MaxSpeed = 500;////加速センサから車椅子移動量の変換の時の最大速度
    int backHomeSpeed = 10;//減点に戻る速度
    int factor = 20;//加速センサから車椅子移動量の変換の係数
    
    bool startDriving = false;
    string drivingState = "Accelerometer OFF";

    private UserTwinCamWheelChairManager_old _userTwinCamWheelChairManager;

    // Use this for initialization
    void Start () {
        
        serialPort_ = new System.IO.Ports.SerialPort(portName, baudRate, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
        serialPort_.ReadTimeout = 1;
        serialPort_.ReadBufferSize = 8;
        try
        {
            serialPort_.Open();
            serialOpended = true;
            thread_ = new Thread(dataRead_Write);
            serialPort_.Write(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0, 9);
            thread_.Start();

            Debug.Log("Wheel_Chair Serial Open");
        }
        catch
        {

        }

        _userTwinCamWheelChairManager = GetComponent<UserTwinCamWheelChairManager_old>();
    }

    public void OnOffDriving()
    {
        startDriving = !startDriving;
        drivingState = (drivingState == "Accelerometer OFF") ? "Accelerometer ON" : "Accelerometer OFF";
    }

    // Update is called once per frame
    void Update () {
        sendData[0] = 3;//header to control wheelchair with accelerometers
        if (startDriving)
        {
            AccelLeft = (short)(-_userTwinCamWheelChairManager.AccelL + OffsetAccelLeft);
            AccelRight = (short)(-_userTwinCamWheelChairManager.AccelR + OffsetAccelRight);
        }
        else
        {
            AccelLeft = AccelRight = 0;
            OffsetAccelLeft = _userTwinCamWheelChairManager.AccelL;
            OffsetAccelRight = _userTwinCamWheelChairManager.AccelR;
        }
    }

    bool keydown = false;
    void OnGUI()
    {
        if (Input.GetKeyDown(KeyCode.O)) keydown = true;
        if (Input.GetKeyUp(KeyCode.O) && keydown)
        {
            OnOffDriving();
            keydown = false;
        }
        GUILayout.BeginArea(new Rect(10, 10, 500, 200));
        //GUILayout.BeginHorizontal();
        GUILayout.Label(rcvControlWord + "\t motorLoop:" + MotorLoop + "\t SerialLoop" + dataLoop + "\n" + Encoder1 + "\t" + Encoder2);
        //GUILayout.EndHorizontal();0
        if (HomePoint == 1)
            GUILayout.Label("Closed to home point.");
        else if (HomePoint == 0)
            GUILayout.Label("Far from home");

        GUILayout.Label(drivingState);

        GUILayout.EndArea();
    }

    private void OnDestroy()
    {
        serialOpended = false;
        serialPort_.Close();
    }

    [SerializeField] int readRepeatCnt = 0;
    int getByteRead = 0;
    byte[] receiveData = new byte[255];
    byte[] sendData = new byte[255];
    int numberToRead = 5;
    int numberToSend = 9;
    private void dataRead_Write()
    {

        while (serialOpended)
        {

            //receive 6 bytes
            byte[] tmpData = new byte[2];
            //while (getByteRead < numberToRead)//data read
            {
                try
                {
                    serialPort_.Read(tmpData, 0, 1);
                    receiveData[getByteRead] = tmpData[0];
                    getByteRead++;
                }
                catch
                {
                    readRepeatCnt++;
                }
            }

            if (getByteRead >= numberToRead)//5 bytes
            {
                rcvControlWord = receiveData[0];
                //Debug.Log(RcvControlWord);
                if (rcvControlWord == 100)
                {
                    Encoder1 = receiveData[1] + (receiveData[2] << 8) + (receiveData[3] << 16) + (receiveData[4] << 24);

                }
                else if (rcvControlWord == 101)
                {

                    Encoder2 = receiveData[1] + (receiveData[2] << 8) + (receiveData[3] << 16) + (receiveData[4] << 24);
                }
                else
                {
                    MotorLoop = receiveData[1];
                    dataLoop = receiveData[2] + (receiveData[3] << 8);
                    HomePoint = receiveData[4];
                }
                getByteRead = 0;
                readRepeatCnt = 0;
                
                byte[] acL = BitConverter.GetBytes(AccelLeft);
                sendData[1] = acL[0];
                sendData[2] = acL[1];
                byte[] acR = BitConverter.GetBytes(AccelRight);
                sendData[3] = acR[0];
                sendData[4] = acR[1];
                byte[] mSpd = BitConverter.GetBytes(MaxSpeed);
                sendData[5] = mSpd[0];
                sendData[6] = mSpd[1];
                byte[] backhomeSpd = BitConverter.GetBytes(backHomeSpeed);
                sendData[7] = backhomeSpd[0];
                byte[] fct = BitConverter.GetBytes(factor);
                sendData[8] = fct[0];
                serialPort_.Write(sendData, 0, numberToSend);//send

            }


        }
    }
}
