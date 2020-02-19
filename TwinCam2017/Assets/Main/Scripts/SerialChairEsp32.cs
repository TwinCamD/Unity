/*Summary
 *  Unityを使ってEsp32と座席の角度をシリアル通信する
 *  修正の必要あり
 */


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading;

public class SerialChairEsp32 : MonoBehaviour {

    #region SerialPort関連
    private SerialPort _serialPort;
    [SerializeField] private string _portName = "COM3";    //ポートの名前 windows(COM) mac(/dev/)
    [SerializeField] private int _baudRate = 115200;
	bool isportOpen = false;
    #endregion

    public GameObject leftsphere, rightspere;

    //椅子の加速度
    [SerializeField] private short _offsetAcc = -100;
    [SerializeField] private short _accelChair;
    public short AccelChair {
        get { return _accelChair; }
        set { _accelChair = (short)(_offsetAcc + value); }
    }
    private short lastAccelChair = 0;
    int accelCheckCnt = 0;
    public short GyroChair = 0;    //椅子の角度
    private int GyroChairFilter_X = 0;
    private short lastGyro_X = 0;
    public short GyroChair_X = 0;

    public short AccelFeedback = 0;
    
    public short AccelOffset = -125;   //椅子の加速度のオフセット
    
    [SerializeField] private float _gainAccel = 0.0f;  //加速度のgain
    [SerializeField] private float _gainGyro = 0.0f;   //ジャイロのgain
    [SerializeField] private float _gainGyro_X = 0.0f;   //ジャイロのgain

    public int encoderValue = 0;
    private int lastEncoderValue = 0;
    public float chairRotationSpdFilter = 0f;//椅子回転速度 °/s
    private float lastChairRotationSpd = 0f;
    public float BACK_HOME_SPD = 3.5f;//
    public float BackhomeSPDcheck = 10f;
    public float Kp = 0.5f;
    public float Ki = 0.1f;
    private const int middleValue = 127;
    Thread RWthread;

    
    [SerializeField] private float _defaultGainAccel = 1.0f;//4f;
    [SerializeField] private float _defaultGainGyro = -1.0f;//-3.0f;
    [SerializeField] private float _defaultGainGyro_X = 8.0f;//-3.0f;

    public int sendToChairMotor = 0;

    //private rotation_along_chair _leftSphRotAng;
    //private rotation_along_chair _rightSphRotAng;
    private Transform _leftSphTransform;
    private Transform _rightSphTransform;
    private float lastRotSphereAngle = 0;

    #region HMD
    private TwinCamHmdController _twinCamHmdController;
    private float _encoderOffset = 0;
    private float lastangle = 0;
    private static float ENC_PER_DEG = 50000f / 360f; //  エンコーダの一回転の値 / 一回転の角度
    [SerializeField] private float _hmdOffsetAngle;
    public float HmdOffsetAngle {
        get { return _hmdOffsetAngle; }
        private set { _hmdOffsetAngle = value; }
    }
    [SerializeField] private bool _isReversed = true;       //反転するか trueにしないと現状反転する
    #endregion

    // Use this for initialization
    void Start () {
        _serialPort = new SerialPort(_portName, _baudRate, Parity.None, 8, StopBits.One);
		_serialPort.ReadTimeout = 1;
		if ( _serialPort.IsOpen)
			_serialPort.Close();

        _serialPort.Open();
		if (_serialPort.IsOpen) {
			isportOpen = true;
			Debug.Log ("Esp Open");

		}
		if (isportOpen) {
			RWthread = new Thread (Write);
			RWthread.Start();
			_serialPort.Write (new byte[]{ middleValue }, 0, 1);
		}

        //_leftSphRotAng = LeftSphereObj.GetComponent<rotation_along_chair>();
        //_rightSphRotAng = RightSphereObj.GetComponent<rotation_along_chair>();

        _leftSphTransform = leftsphere.GetComponent<Transform>();
        _rightSphTransform = rightspere.GetComponent<Transform>();

        _twinCamHmdController = GetComponent<TwinCamHmdController>();
    }

    void OnDestroy() {
        if (isportOpen) {
			isportOpen = false;
			Thread.Sleep (200);
            _serialPort.Write(new byte[] { middleValue }, 0, 1);
            _serialPort.Close();
            _serialPort.Dispose();
        }
    }

    void Update() {
        hmd();

        _leftSphTransform.Rotate((Vector3.up * (-rotationangle + lastRotSphereAngle)));
        _rightSphTransform.Rotate((Vector3.up * (-rotationangle + lastRotSphereAngle)));
        lastRotSphereAngle = rotationangle;

        //_leftSphRotAng.rotationAngle = rotationangle;
        //_rightSphRotAng.rotationAngle = rotationangle;

        if (Input.GetKeyDown(KeyCode.Space)) {
            AccelOffset = AccelChair;
        }

        if (Input.GetKeyDown(KeyCode.G) || Input.GetKeyDown(KeyCode.B)) {
            _gainAccel = _defaultGainAccel;
            _gainGyro = _defaultGainGyro;
            _gainGyro_X = _defaultGainGyro_X;
            Debug.Log("Gain Set");
        }

        if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.V)) {
            _gainAccel = 0f;
            _gainGyro = 0f;
            _gainGyro_X = 0f;
            Debug.Log("Gain Off");
        }
    }

    private void hmd() {
        //座席用の相殺
        float nowAngle = (encoderValue - _encoderOffset) / ENC_PER_DEG;
        float chairAngle = lastangle + 0.8f * (nowAngle - lastangle);
        lastangle = chairAngle;

        HmdOffsetAngle = _twinCamHmdController.GetHmdAngle(false) + chairAngle;

        //フィルタ
        while (HmdOffsetAngle < 0) {
            HmdOffsetAngle += 360;
        }

        while (360 < HmdOffsetAngle) {
            HmdOffsetAngle -= 360;
        }

        //反転
        if (_isReversed) {
            HmdOffsetAngle = 360f - HmdOffsetAngle;
        }

        //角度リセット
        if (Input.GetKeyDown(KeyCode.Space)) {
            _encoderOffset = encoderValue;
        }
    }

    void FixedUpdate() {
        //1秒間で加速度が変化なければ、その加速度の値がoffset
        if (Math.Abs(AccelChair - lastAccelChair) < 3 )
            accelCheckCnt++;
        else
            accelCheckCnt = 0;
        if (accelCheckCnt > 50)//20ms x 50
        {
            AccelOffset = AccelChair;
            accelCheckCnt = 0;
        }
        lastAccelChair = AccelChair;
       
        /*
        rotationangle = encoderValue * 360.0f / 50000.0f;
        LeftSphereObj.GetComponent<rotation_along_chair>().rotationAngle = rotationangle;
        RightSphereObj.GetComponent<rotation_along_chair>().rotationAngle = rotationangle;
        float chairSpd = (rotationangle - lastRotationAngle) / 0.02f;
        chairRotationSpdFilter = lastChairRotationSpd + 0.75f * (chairSpd - lastChairRotationSpd);
        lastRotationAngle = rotationangle;
        //lastChairRotationSpd = chairSpd;
        */
    }
    float rotationangle = 0f;
    float lastRotationAngle = 0f;

    int cnt = 0;
    const int rcvLength = 4;
	byte[] rcval = new byte[rcvLength];
    private long lastTicks = 0;
    [SerializeField] float I_Component = 0f;
    float accelToSpd = 0f;
    [SerializeField] float targSpd;

    private void Write() {
        Thread.Sleep(1000);
		while (isportOpen) {
            //long nowTicks = DateTime.Now.Ticks;
            //float dTime = (nowTicks - lastTicks) / 10000000f;//秒
            //lastTicks = nowTicks;
            

            byte[] rcv = new byte[1];
			try {
				_serialPort.Read (rcv, 0, 1);
				rcval [cnt] = rcv [0];
			    cnt++;
                if (cnt >= rcvLength) {
                    encoderValue = rcval[0] + (rcval[1] << 8) + (rcval[2] << 16) + (rcval[3] << 24);

                    rotationangle = encoderValue * 360.0f / 50000.0f;
                    //LeftSphereObj.GetComponent<rotation_along_chair>().rotationAngle = rotationangle;
                    //RightSphereObj.GetComponent<rotation_along_chair>().rotationAngle = rotationangle;
                    float chairSpd = (rotationangle - lastRotationAngle) / 0.02f;
                    chairRotationSpdFilter = lastChairRotationSpd + 0.75f * (chairSpd - lastChairRotationSpd);
                    lastRotationAngle = rotationangle;
                    //lastChairRotationSpd = chairSpd;


                    //accelToSpd = (AccelChair - AccelOffset);// += dTime * (AccelChair - AccelOffset);
                    AccelFeedback = (short)(_gainAccel * (AccelChair - AccelOffset));

                    GyroChairFilter_X = GyroChair_X;//;(int)((lastGyro_X + 0.5f * (GyroChair_X - lastGyro_X)));
                    lastGyro_X = (short)GyroChairFilter_X;

                    targSpd = _gainGyro * GyroChair;
                    if (Math.Abs(GyroChair) < BackhomeSPDcheck)//Segwayからの回転速度がほぼゼロ,ホームに戻る
                    {
                        targSpd = (-encoderValue / 1500f) * BACK_HOME_SPD;//ホームに近づくと速度を遅くする
                        if (Math.Abs(targSpd) > BACK_HOME_SPD) targSpd = Math.Sign(-encoderValue) * BACK_HOME_SPD;
                        if (Math.Abs(encoderValue) < 500) I_Component = 0;
                    }

                    targSpd = targSpd + _gainGyro_X * GyroChairFilter_X;
                    float dSPD = targSpd - chairRotationSpdFilter;
                    I_Component += Ki * dSPD;
                    if (Math.Abs(I_Component) > 100) I_Component = Math.Sign(I_Component) * 100f;
                    int spdControlValue = (int)(Kp * dSPD + I_Component);

                    //int spdControlValue = (int)targSpd;
                    sendToChairMotor = middleValue + spdControlValue;
                    //if (Math.Abs(spdControlValue)<10) {
                    //    sendToChairMotor = middleValue;
                    //}
                    if (sendToChairMotor < 0) sendToChairMotor = 0;
                    else if (sendToChairMotor > 255) sendToChairMotor = 255;
                    byte[] MotorControlValue = System.BitConverter.GetBytes (sendToChairMotor);
					_serialPort.Write (MotorControlValue, 0, 1);
					cnt = 0;
					
				}
				
			} catch (System.Exception e) {
				Debug.LogError(e.Message);
			}
			Thread.Sleep (1);
		}
        _serialPort.Write(new byte[] { middleValue }, 0, 1);
    }
}
