using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;


public class SensorY : MonoBehaviour
{
    public int partsCount;


    public upd_angleSend udp;///////////////////////////////////////////////////
 
    #region 変数定義

    #region Avator 位置・回転変数
    public static Vector3[] ro;
    public static Vector3[] po;
    public static Vector3[] initvec;
    public static Quaternion[] quat;
    public static Quaternion[] initquat;
    public static Quaternion q0;
    public static Quaternion q1;
    public static Quaternion q2;
    public static Quaternion q3;
    public static Quaternion q4;
    public static Quaternion q5;
    public static Quaternion q6;
    public static Quaternion Q0;
    public static Quaternion Q1;
    public static Quaternion Q2;
    public static Quaternion Q3;
    public static Quaternion Q4;
    public static Quaternion Q5;
    public static Quaternion Q6;
    public static Quaternion preQ0;
    public static Quaternion preQ1;
    public static Quaternion preQ2;
    public static Quaternion preQ3;
    public static Quaternion preQ4;
    public static Quaternion preQ5;
    public static Quaternion preQ6;
    public static Quaternion preQ02;
    public static Quaternion preQ12;
    public static Quaternion preQ22;
    public static Quaternion preQ32;
    public static Quaternion preQ42;
    public static Quaternion preQ52;
    public static Quaternion preQ62;

    public static bool initrotation;
    #endregion

    #region 通信関連変数
    public static bool start;
    private string hostname;
    private static TcpClient[] client;
    private static NetworkStream[] networkstream;
    public bool[] PartsOn = new bool[7];
    public Thread[] thread = new Thread[7];

    #endregion

    #endregion

    // Use this for initialization
    void Start()
    {
        #region 配列Partsの数だけ用意
        
        hostname = Dns.GetHostName();
        networkstream = new NetworkStream[partsCount];
        client = new TcpClient[partsCount];

        po = new Vector3[partsCount];
        ro = new Vector3[partsCount];
        initvec = new Vector3[partsCount];
        quat = new Quaternion[partsCount];
        initquat = new Quaternion[partsCount];
        thread = new Thread[partsCount];
        for (int i = 0; i < partsCount; i++)
        {
            initquat[i] = Quaternion.identity;
        }
        q0 = Quaternion.identity;
        q1 = Quaternion.identity;
        q2 = Quaternion.identity;
        q3 = Quaternion.identity;
        q4 = Quaternion.identity;
        q5 = Quaternion.identity;
        q6 = Quaternion.identity;

        initrotation = false;
        #endregion
        start = false;
        //thread[0] = new Thread(new ThreadStart(ThreadMethot0));
        //thread[1] = new Thread(new ThreadStart(ThreadMethot1));
        //thread[2] = new Thread(new ThreadStart(ThreadMethot2));
        //thread[3] = new Thread(new ThreadStart(ThreadMethot3));
        //thread[4] = new Thread(new ThreadStart(ThreadMethot4));
        //thread[5] = new Thread(new ThreadStart(ThreadMethot5));
        //thread[6] = new Thread(new ThreadStart(ThreadMethot6));

        #region PartOn→trueの部位のみ接続
        for (int i = 0; i < partsCount; i++)
        {
            if (PartsOn[i])
            {
                int portnum = i * 5000 + 10000;
                TCPConnect(i, portnum);
                //thread[i].Start();
            }
        }
        #endregion

    }

    // Update is called once per frame
    void Update()
    {
        #region 角度を変換してudpプログラムに送信
        this.transform.rotation = Q0;
        Vector3 s_ang = Q0.eulerAngles;
        Debug.Log(s_ang);
        int sensorAng = (int)s_ang.z;
        if (sensorAng > 180)
            sensorAng -= 360;
        udp.angle = sensorAng;
        #endregion

        #region ボタン設定

        #region フェードイン・アウト(Fボタン)
        //fadesc.csに記述
        #endregion

        #region HMDリセット(Rボタン)
        //Cam.csに記述
        #endregion

        #region ジャイロセンサ オフセット設定(Gボタン)
        if (Input.GetKeyDown(KeyCode.G))
        {
            Byte[] sendBytes = Encoding.ASCII.GetBytes("setgyroffset 1 1 1 0 0 0\n");
            for (int i = 0; i < partsCount; i++)
            {
                if (PartsOn[i])
                {
                    networkstream[i].Write(sendBytes, 0, sendBytes.Length);
                }
            }
            Debug.Log("Set Gyro Offset");
        }
        #endregion

        #region 計測開始(Sボタン)
        if (Input.GetKeyDown(KeyCode.S))
        {
            Byte[] sendBytes = Encoding.ASCII.GetBytes("start\n"); for (int i = 0; i < partsCount; i++)
            {
                if (PartsOn[i])
                {
                    networkstream[i].Write(sendBytes, 0, sendBytes.Length);
                }
            }
            Debug.Log("Start");
            start = false;
            Thread.Sleep(100);
            start = true;
            thread[0] = new Thread(new ThreadStart(ThreadMethot0));
            thread[0].Start();
        }
        #endregion

        #region 計測停止(Spaceボタン)
        if (Input.GetKeyDown(KeyCode.Space))
        {

            Byte[] sendBytes = Encoding.ASCII.GetBytes("stop\n"); for (int i = 0; i < partsCount; i++)
            {
                if (PartsOn[i])
                {
                    networkstream[i].Write(sendBytes, 0, sendBytes.Length);
                }
            }
            Debug.Log("Finish");
            start = false;
            Thread.Sleep(100);
            //TCPClose();
            for (int i = 0; i < partsCount; i++)
            {
                if (PartsOn[i])
                {
                    //thread[i].Abort();
                }
            }

        }
        #endregion

        #region 回転の初期化(Iボタン)
        if (Input.GetKeyDown(KeyCode.I))
        {
            for (int i = 0; i < partsCount; i++)
            {
                if (PartsOn[i])
                {
                    initvec[i] = ro[i];
                }
            }
            initquat[0] = q0;
            initquat[1] = q1;
            initquat[2] = q2;
            initquat[3] = q3;
            initquat[4] = q4;
            initquat[5] = q5;
            initquat[6] = q6;
            Debug.Log("init");
        }
        if (initrotation)
        {
            initquat[3] = q3;
            initquat[4] = q4;
            initrotation = false;
        }
        #endregion

        #region スレッド状態取得(Cボタン)
        if (Input.GetKeyDown(KeyCode.C))
        {
            for (int i = 0; i < partsCount; i++)
            {
                if (thread[i].IsAlive)
                {
                    Debug.Log("Thread" + i + ": Success");
                }
                else
                {
                    Debug.Log("Thread" + i + ": faild");
                }
            }
        }
        #endregion

        #region CSV書き込み開始(Tボタン)
        //CreateCSV内に記述
        #endregion

        #region CSV書き込み終了(Yボタン)
        //CreateCSV内に記述
        #endregion

        #endregion
    }
    
    void OnDisable()
    {
        Byte[] sendBytes = Encoding.ASCII.GetBytes("stop\n"); for (int i = 0; i < partsCount; i++)
        {
            if (PartsOn[i])
            {
                networkstream[i].Write(sendBytes, 0, sendBytes.Length);
            }
        }
        Debug.Log("Finish");
        start = false;
        Thread.Sleep(100);
        TCPClose();
        
    }

    private void TCPConnect(int part, int portnum)
    {
        client[part] = new TcpClient(hostname, portnum);
        networkstream[part] = client[part].GetStream();
        Debug.Log("TCP Connect  PortNum : " + portnum);
    }

    private void TCPClose()
    {
        for (int i = 0; i < partsCount; i++)
        {
            if (PartsOn[i])
            {
                client[i].Close();
                networkstream[i].Close();
            }
        }
        Debug.Log("TCP Close");
    }

    private static void ThreadMethot0()
    {
        while (start)
        {
            //if (start)
            {
                #region 変数の初期化＆データの読み取り
                byte[] b = new byte[client[0].ReceiveBufferSize];
                int read = networkstream[0].Read(b, 0, (int)client[0].ReceiveBufferSize);
                char[] ch = new char[read];

                int charCount = 0;

                #endregion

                #region センサデータの格納
                string[][] sensordata = new string[100][];
                string[][] geodata = new string[100][];
                for (int i = 0; i < 100; i++)
                {
                    sensordata[i] = new string[12];
                    geodata[i] = new string[5];
                }

                for (int j = 0; j < read; j++)
                {
                    if (b[j].ToString() != "0")
                    {
                        ch[charCount] = (char)b[j];
                        charCount++;
                    }
                }

                int div = 0;
                int lineCount = 0;
                int geoCount = 0;
                for (int i = 0; i < charCount; i++)
                {
                    if ((byte)ch[i] == 10)
                    {
                        string str = new string(ch, div, div + 4);
                        if (str == "qags")
                        {
                            sensordata[lineCount] = new string(ch, div, i - 1).Split(',');
                            lineCount++;
                        }
                        else if (str == "geo,")
                        {
                            geodata[geoCount] = new string(ch, div, i - 1).Split(',');
                            geoCount++;
                        }
                        div = i + 1;
                    }
                }
                #endregion
                for (int i = 0; i < lineCount; i++)
                {
                    preQ02 = preQ0;
                    preQ0 = Q0;

                    Q0 = new Quaternion(0.0001f * float.Parse(sensordata[i][3]),
                                                         -0.0001f * float.Parse(sensordata[i][4]),
                                                         0.0001f * float.Parse(sensordata[i][5]),
                                                         0.0001f * float.Parse(sensordata[i][2]));
                    float ang1 = Quaternion.Angle(preQ02, preQ0);
                    float ang2 = Quaternion.Angle(preQ02, Q0);
                    float ang3 = Quaternion.Angle(preQ0, Q0);
                    if (Mathf.Min((ang1 + ang2), (ang1 + ang3), (ang2 + ang3)) == (ang1 + ang2))
                    {
                        q0 = preQ02;
                    }
                    else if (Mathf.Min((ang1 + ang2), (ang1 + ang3), (ang2 + ang3)) == (ang1 + ang3))
                    {
                        q0 = preQ0;
                    }
                    else
                    {
                        q0 = Q0;
                    }
                }
            }
        }
    }

    private static void ThreadMethot1()
    {
        while (true)
        {
            if (start)
            {
                #region 変数の初期化＆データの読み取り
                byte[] b = new byte[client[1].ReceiveBufferSize];
                int read = networkstream[1].Read(b, 0, (int)client[1].ReceiveBufferSize);
                char[] ch = new char[read];

                int charCount = 0;

                #endregion

                #region センサデータの格納
                string[][] sensordata = new string[100][];
                string[][] geodata = new string[100][];
                for (int i = 0; i < 100; i++)
                {
                    sensordata[i] = new string[12];
                    geodata[i] = new string[5];
                }

                for (int j = 0; j < read; j++)
                {
                    if (b[j].ToString() != "0")
                    {
                        ch[charCount] = (char)b[j];
                        charCount++;
                    }
                }

                int div = 0;
                int lineCount = 0;
                int geoCount = 0;
                for (int i = 0; i < charCount; i++)
                {
                    if ((byte)ch[i] == 10)
                    {
                        string str = new string(ch, div, div + 4);
                        if (str == "qags")
                        {
                            sensordata[lineCount] = new string(ch, div, i - 1).Split(',');
                            lineCount++;
                        }
                        else if (str == "geo,")
                        {
                            geodata[geoCount] = new string(ch, div, i - 1).Split(',');
                            geoCount++;
                        }
                        div = i + 1;
                    }
                }
                #endregion

                for (int i = 0; i < lineCount; i++)
                {
                    preQ12 = preQ1;
                    preQ1 = Q1;

                    Q1 = new Quaternion(0.0001f * float.Parse(sensordata[i][3]),
                                                         -0.0001f * float.Parse(sensordata[i][4]),
                                                         0.0001f * float.Parse(sensordata[i][5]),
                                                         0.0001f * float.Parse(sensordata[i][2]));
                    float ang1 = Quaternion.Angle(preQ12, preQ1);
                    float ang2 = Quaternion.Angle(preQ12, Q1);
                    float ang3 = Quaternion.Angle(preQ1, Q1);
                    if (Mathf.Min((ang1 + ang2), (ang1 + ang3), (ang2 + ang3)) == (ang1 + ang2))
                    {
                        q1 = preQ12;
                    }
                    else if (Mathf.Min((ang1 + ang2), (ang1 + ang3), (ang2 + ang3)) == (ang1 + ang3))
                    {
                        q1 = preQ1;
                    }
                    else
                    {
                        q1 = Q1;
                    }
                }

            }
        }
    }

    private static void ThreadMethot2()
    {
        while (true)
        {
            if (start)
            {
                #region 変数の初期化＆データの読み取り
                byte[] b = new byte[client[2].ReceiveBufferSize];
                int read = networkstream[2].Read(b, 0, (int)client[2].ReceiveBufferSize);
                char[] ch = new char[read];

                int charCount = 0;

                #endregion

                #region センサデータの格納
                string[][] sensordata = new string[100][];
                string[][] geodata = new string[100][];
                for (int i = 0; i < 100; i++)
                {
                    sensordata[i] = new string[12];
                    geodata[i] = new string[5];
                }

                for (int j = 0; j < read; j++)
                {
                    if (b[j].ToString() != "0")
                    {
                        ch[charCount] = (char)b[j];
                        charCount++;
                    }
                }

                int div = 0;
                int lineCount = 0;
                int geoCount = 0;
                for (int i = 0; i < charCount; i++)
                {
                    if ((byte)ch[i] == 10)
                    {
                        string str = new string(ch, div, div + 4);
                        if (str == "qags")
                        {
                            sensordata[lineCount] = new string(ch, div, i - 1).Split(',');
                            lineCount++;
                        }
                        else if (str == "geo,")
                        {
                            geodata[geoCount] = new string(ch, div, i - 1).Split(',');
                            geoCount++;
                        }
                        div = i + 1;
                    }
                }
                #endregion

                for (int i = 0; i < lineCount; i++)
                {
                    preQ22 = preQ2;
                    preQ2 = Q2;

                    Q2 = new Quaternion(0.0001f * float.Parse(sensordata[i][3]),
                                                         -0.0001f * float.Parse(sensordata[i][4]),
                                                         0.0001f * float.Parse(sensordata[i][5]),
                                                         0.0001f * float.Parse(sensordata[i][2]));
                    float ang1 = Quaternion.Angle(preQ22, preQ2);
                    float ang2 = Quaternion.Angle(preQ22, Q2);
                    float ang3 = Quaternion.Angle(preQ2, Q2);
                    if (Mathf.Min((ang1 + ang2), (ang1 + ang3), (ang2 + ang3)) == (ang1 + ang2))
                    {
                        q2 = preQ22;
                    }
                    else if (Mathf.Min((ang1 + ang2), (ang1 + ang3), (ang2 + ang3)) == (ang1 + ang3))
                    {
                        q2 = preQ2;
                    }
                    else
                    {
                        q2 = Q2;
                    }
                }

            }
        }
    }

    private static void ThreadMethot3()
    {
        while (true)
        {
            if (start)
            {
                #region 変数の初期化＆データの読み取り
                byte[] b = new byte[client[3].ReceiveBufferSize];
                int read = networkstream[3].Read(b, 0, (int)client[3].ReceiveBufferSize);
                char[] ch = new char[read];

                int charCount = 0;

                #endregion

                #region センサデータの格納
                string[][] sensordata = new string[100][];
                string[][] geodata = new string[100][];
                for (int i = 0; i < 100; i++)
                {
                    sensordata[i] = new string[12];
                    geodata[i] = new string[5];
                }

                for (int j = 0; j < read; j++)
                {
                    if (b[j].ToString() != "0")
                    {
                        ch[charCount] = (char)b[j];
                        charCount++;
                    }
                }

                int div = 0;
                int lineCount = 0;
                int geoCount = 0;
                for (int i = 0; i < charCount; i++)
                {
                    if ((byte)ch[i] == 10)
                    {
                        string str = new string(ch, div, div + 4);
                        if (str == "qags")
                        {
                            sensordata[lineCount] = new string(ch, div, i - 1).Split(',');
                            lineCount++;
                        }
                        else if (str == "geo,")
                        {
                            geodata[geoCount] = new string(ch, div, i - 1).Split(',');
                            geoCount++;
                        }
                        div = i + 1;
                    }
                }
                #endregion

                for (int i = 0; i < lineCount; i++)
                {
                    preQ32 = preQ3;
                    preQ3 = Q3;

                    Q3 = new Quaternion(0.0001f * float.Parse(sensordata[i][3]),
                                                         -0.0001f * float.Parse(sensordata[i][4]),
                                                         0.0001f * float.Parse(sensordata[i][5]),
                                                         0.0001f * float.Parse(sensordata[i][2]));
                    float ang1 = Quaternion.Angle(preQ32, preQ3);
                    float ang2 = Quaternion.Angle(preQ32, Q3);
                    float ang3 = Quaternion.Angle(preQ3, Q3);
                    if (Mathf.Min((ang1 + ang2), (ang1 + ang3), (ang2 + ang3)) == (ang1 + ang2))
                    {
                        q3 = preQ32;
                    }
                    else if (Mathf.Min((ang1 + ang2), (ang1 + ang3), (ang2 + ang3)) == (ang1 + ang3))
                    {
                        q3 = preQ3;
                    }
                    else
                    {
                        q3 = Q3;
                    }
                }

            }
        }
    }

    private static void ThreadMethot4()
    {
        while (true)
        {
            if (start)
            {
                #region 変数の初期化＆データの読み取り
                byte[] b = new byte[client[4].ReceiveBufferSize];
                int read = networkstream[4].Read(b, 0, (int)client[4].ReceiveBufferSize);
                char[] ch = new char[read];

                int charCount = 0;

                #endregion

                #region センサデータの格納
                string[][] sensordata = new string[100][];
                string[][] geodata = new string[100][];
                for (int i = 0; i < 100; i++)
                {
                    sensordata[i] = new string[12];
                    geodata[i] = new string[5];
                }

                for (int j = 0; j < read; j++)
                {
                    if (b[j].ToString() != "0")
                    {
                        ch[charCount] = (char)b[j];
                        charCount++;
                    }
                }

                int div = 0;
                int lineCount = 0;
                int geoCount = 0;
                for (int i = 0; i < charCount; i++)
                {
                    if ((byte)ch[i] == 10)
                    {
                        string str = new string(ch, div, div + 4);
                        if (str == "qags")
                        {
                            sensordata[lineCount] = new string(ch, div, i - 1).Split(',');
                            lineCount++;
                        }
                        else if (str == "geo,")
                        {
                            geodata[geoCount] = new string(ch, div, i - 1).Split(',');
                            geoCount++;
                        }
                        div = i + 1;
                    }
                }
                #endregion

                for (int i = 0; i < lineCount; i++)
                {
                    preQ42 = preQ4;
                    preQ4 = Q4;

                    Q4 = new Quaternion(0.0001f * float.Parse(sensordata[i][3]),
                                                         -0.0001f * float.Parse(sensordata[i][4]),
                                                         0.0001f * float.Parse(sensordata[i][5]),
                                                         0.0001f * float.Parse(sensordata[i][2]));
                    float ang1 = Quaternion.Angle(preQ42, preQ4);
                    float ang2 = Quaternion.Angle(preQ42, Q4);
                    float ang3 = Quaternion.Angle(preQ4, Q4);
                    if (Mathf.Min((ang1 + ang2), (ang1 + ang3), (ang2 + ang3)) == (ang1 + ang2))
                    {
                        q4 = preQ42;
                    }
                    else if (Mathf.Min((ang1 + ang2), (ang1 + ang3), (ang2 + ang3)) == (ang1 + ang3))
                    {
                        q4 = preQ4;
                    }
                    else
                    {
                        q4 = Q4;
                    }
                }

            }
        }
    }

    private static void ThreadMethot5()
    {
        while (true)
        {
            if (start)
            {
                #region 変数の初期化＆データの読み取り
                byte[] b = new byte[client[5].ReceiveBufferSize];
                int read = networkstream[5].Read(b, 0, (int)client[5].ReceiveBufferSize);
                char[] ch = new char[read];

                int charCount = 0;

                #endregion

                #region センサデータの格納
                string[][] sensordata = new string[100][];
                string[][] geodata = new string[100][];
                for (int i = 0; i < 100; i++)
                {
                    sensordata[i] = new string[12];
                    geodata[i] = new string[5];
                }

                for (int j = 0; j < read; j++)
                {
                    if (b[j].ToString() != "0")
                    {
                        ch[charCount] = (char)b[j];
                        charCount++;
                    }
                }

                int div = 0;
                int lineCount = 0;
                int geoCount = 0;
                for (int i = 0; i < charCount; i++)
                {
                    if ((byte)ch[i] == 10)
                    {
                        string str = new string(ch, div, div + 4);
                        if (str == "qags")
                        {
                            sensordata[lineCount] = new string(ch, div, i - 1).Split(',');
                            lineCount++;
                        }
                        else if (str == "geo,")
                        {
                            geodata[geoCount] = new string(ch, div, i - 1).Split(',');
                            geoCount++;
                        }
                        div = i + 1;
                    }
                }
                #endregion

                for (int i = 0; i < lineCount; i++)
                {
                    preQ52 = preQ5;
                    preQ5 = Q5;

                    Q5 = new Quaternion(0.0001f * float.Parse(sensordata[i][3]),
                                                         -0.0001f * float.Parse(sensordata[i][4]),
                                                         0.0001f * float.Parse(sensordata[i][5]),
                                                         0.0001f * float.Parse(sensordata[i][2]));
                    float ang1 = Quaternion.Angle(preQ52, preQ5);
                    float ang2 = Quaternion.Angle(preQ52, Q5);
                    float ang3 = Quaternion.Angle(preQ5, Q5);
                    if (Mathf.Min((ang1 + ang2), (ang1 + ang3), (ang2 + ang3)) == (ang1 + ang2))
                    {
                        q5 = preQ52;
                    }
                    else if (Mathf.Min((ang1 + ang2), (ang1 + ang3), (ang2 + ang3)) == (ang1 + ang3))
                    {
                        q5 = preQ5;
                    }
                    else
                    {
                        q5 = Q5;
                    }
                }


            }
        }
    }

    private static void ThreadMethot6()
    {
        while (true)
        {
            if (start)
            {
                #region 変数の初期化＆データの読み取り
                byte[] b = new byte[client[6].ReceiveBufferSize];
                int read = networkstream[6].Read(b, 0, (int)client[6].ReceiveBufferSize);
                char[] ch = new char[read];

                int charCount = 0;

                #endregion

                #region センサデータの格納
                string[][] sensordata = new string[100][];
                string[][] geodata = new string[100][];
                for (int i = 0; i < 100; i++)
                {
                    sensordata[i] = new string[12];
                    geodata[i] = new string[5];
                }

                for (int j = 0; j < read; j++)
                {
                    if (b[j].ToString() != "0")
                    {
                        ch[charCount] = (char)b[j];
                        charCount++;
                    }
                }

                int div = 0;
                int lineCount = 0;
                int geoCount = 0;
                for (int i = 0; i < charCount; i++)
                {
                    if ((byte)ch[i] == 10)
                    {
                        string str = new string(ch, div, div + 4);
                        if (str == "qags")
                        {
                            sensordata[lineCount] = new string(ch, div, i - 1).Split(',');
                            lineCount++;
                        }
                        else if (str == "geo,")
                        {
                            geodata[geoCount] = new string(ch, div, i - 1).Split(',');
                            geoCount++;
                        }
                        div = i + 1;
                    }
                }
                #endregion
                for (int i = 0; i < lineCount; i++)
                {
                    preQ62 = preQ6;
                    preQ6 = Q6;

                    Q6 = new Quaternion(0.0001f * float.Parse(sensordata[i][3]),
                                                         -0.0001f * float.Parse(sensordata[i][4]),
                                                         0.0001f * float.Parse(sensordata[i][5]),
                                                         0.0001f * float.Parse(sensordata[i][2]));
                    float ang1 = Quaternion.Angle(preQ62, preQ6);
                    float ang2 = Quaternion.Angle(preQ62, Q6);
                    float ang3 = Quaternion.Angle(preQ6, Q6);
                    if (Mathf.Min((ang1 + ang2), (ang1 + ang3), (ang2 + ang3)) == (ang1 + ang2))
                    {
                        q6 = preQ62;
                    }
                    else if (Mathf.Min((ang1 + ang2), (ang1 + ang3), (ang2 + ang3)) == (ang1 + ang3))
                    {
                        q6 = preQ6;
                    }
                    else
                    {
                        q6 = Q6;
                    }
                }
            }
        }
    }

}
