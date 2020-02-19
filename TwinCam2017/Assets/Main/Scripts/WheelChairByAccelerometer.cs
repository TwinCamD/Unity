
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading;
using UnityEngine.UI;

public class WheelChairByAccelerometer : MonoBehaviour {

    private Thread _thread;
    private static int FinalThreadSleepTime = 200;   //終了時にThreadが停止する時間
    private bool _isStartThread = false;

    [SerializeField] private bool startDriving = false;
    string drivingState = "Accelerometer OFF";

    public short RawAccelRight = 0, RawAccelLeft = 0;//加速度
    [SerializeField] private short _accelRight;//計算した右側加速度
    public short AccelRight {
        get { return _accelRight; }
        private set { _accelRight = value; }
    }
    [SerializeField] private short _accelLeft;//計算した左側加速度
    public short AccelLeft {
        get { return _accelLeft; }
        private set { _accelLeft = value; }
    }
    [SerializeField] private short _offsetAccelLeft = 0, _offsetAccelRight = 0;//加速度のオフセット値

    [SerializeField] private short _maxSpeed = 3000;   //加速センサから車椅子移動量の変換の時の最大速度
    public short MaxSpeed {
        get { return _maxSpeed; }
        private set { _maxSpeed = value; }
    }
    [SerializeField] private byte[] _backHomeSpeed = {60};   //原点に戻る速度
    public byte[] BackHomeSpeed {
        get { return _backHomeSpeed; }
        private set { _backHomeSpeed = value; }
    }
    [SerializeField] private byte[] _factor = {20};   //加速センサから車椅子移動量の変換の係数
    public byte[] Factor {
        get { return _factor; }
        private set { _factor = value; }
    }

    public byte RcvControlWord;

    public int Encoder1, Encoder2;
    public short DataLoop;
    public byte MotorLoop, HomePoint = 0;

    //MedianFilter
    private int _filterSize = 5;//フィルターの大きさ 奇数を入れること
    private static int _indexSize = 4;//保存するデータの種類の数
    private short[,] _tmpNum;

    [SerializeField] private short _worningLine = 10000;

    // Use this for initialization
    void Start () {
        _tmpNum = new short[_filterSize, _indexSize];
        if (!_isStartThread) {
            startThread();
        }
    }

    void OnDestroy() {
        if (_isStartThread) {
            _isStartThread = false;
            Thread.Sleep(FinalThreadSleepTime);
        }
    }

    private void threadUpdate() {
        while (_isStartThread) {
            if (startDriving) {
                AccelRight = filter(getMedian((short) -(RawAccelRight - _offsetAccelRight), 0));
                AccelLeft = filter(getMedian((short) (RawAccelLeft - _offsetAccelLeft), 1));
            }
            else {
                AccelRight = AccelLeft = 0;
                setOffset();
            }
        }
    }

    //Thereadを開始
    private void startThread() {
        _isStartThread = true;
        _thread = new Thread(threadUpdate);
        _thread.Start();
    }

    public void OnOffDriving() {
        startDriving = !startDriving;
        drivingState = (drivingState == "Accelerometer OFF") ? "Accelerometer ON" : "Accelerometer OFF";
    }

    private void setOffset() {
        _offsetAccelRight = getAverage(RawAccelRight, 2);
        _offsetAccelLeft = getAverage(RawAccelLeft, 3);
    }

    [SerializeField] private short _gain = 50;
    [SerializeField] private short _offset = 30;
    private short filter(short acc) {
        return (short) (Mathf.Sign(acc) * (_offset + Mathf.Sqrt(_gain * Mathf.Abs(acc))));
    }

    //メディアンフィルタ
    private short getMedian(short num, int index) {
        for (int i = 0; i < _filterSize - 1; i++) {
            _tmpNum[i, index] = _tmpNum[i + 1, index];//値を前に詰める
        }
        _tmpNum[_filterSize - 1, index] = num;//最後に値を追加

        short[] tmp = new short[_filterSize];
        for (int i = 0; i < _filterSize; i++) {
            tmp[i] = _tmpNum[i, index];//計算するために値を抽出
        }
        Array.Sort(tmp);//並べ替え
        return tmp[(int)(_filterSize / 2f)];
    }

    //移動平均フィルタ
    private short getAverage(short num, int index) {
        for (int i = 0; i < _filterSize - 1; i++) {
            _tmpNum[i, index] = _tmpNum[i + 1, index];//値を前に詰める
        }
        _tmpNum[_filterSize - 1, index] = num;//最後に値を追加

        double tmp = 0;
        for (int i = 0; i < _filterSize; i++) {
            tmp += _tmpNum[i, index];//計算するために値を抽出
        }

        return (short) (tmp / _filterSize);
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
        GUILayout.Label(RcvControlWord + "\t motorLoop:" + MotorLoop + "\t SerialLoop" + DataLoop + "\n" + Encoder1 + "\t" + Encoder2);
        //GUILayout.EndHorizontal();0
        //if (HomePoint == 1)
        //    GUILayout.Label("Closed to home point.");
        //else if (HomePoint == 0)
        //    GUILayout.Label("Far from home");

        //GUILayout.Label(drivingState);

        GUILayout.EndArea();
    }
}
