using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public class upd_angleSend : MonoBehaviour {
    const int esp32Port = 3333;
    const int myPort = 3345;
    public int angle = 0;
    public int receiveAngle = 0;
    UdpClient udpServer;
    // Use this for initialization
    void Start () {
        udpServer = new UdpClient(myPort);
    }
	
	// FixedUpdate is called once per frame
	void Update () {
        try
        {
            var remoteEP = new IPEndPoint(IPAddress.Any, esp32Port);
            //if (remoteEP != null)
            {
                var data = udpServer.Receive(ref remoteEP); // listen on port 11000
                receiveAngle = data[0];// + (data[1] << 8);
                //for (int i = 0; i < data.Length; i++)
                //    Debug.Log("esp32 " + i + ": " + data[i]);
                //Debug.Log("receive data from IP " + remoteEP.Address.ToString() + " port " + remoteEP.Port);
                byte[] ang = BitConverter.GetBytes(angle);
                udpServer.Send(ang, 4, remoteEP); // reply back
            }
        }
        catch
        {

        }
    }
}
