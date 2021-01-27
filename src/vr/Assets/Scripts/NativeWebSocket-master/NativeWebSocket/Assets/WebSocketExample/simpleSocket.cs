using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
public class simpleSocket : MonoBehaviour
{
  public String Host = "10.138.226.85";
  public Int32 Port = 65321;

  TcpClient mySocket = null;
  NetworkStream theStream = null;
  StreamWriter theWriter = null;


  public GameObject wheel;
  public GameObject transmission;
  int speed = 50;
  double left_vel = 0;
  int l_vel = 0;
  bool speedSign = false;
  double speedVal;
  double scaledAngle = 0;
  double right_vel = 0;
  int r_vel = 0;
  double neutral = 0.1382;
  double smooth_angle = 0;
  string text = "";


  // Start is called before the first frame update
  void Start()
  {
    mySocket = new TcpClient();

    if (SetupSocket())
    {
      Debug.Log("socket is set up");
    }

    InvokeRepeating("SendSocketMessage", 0.0f, 0.1f);
  }

  // Update is called once per frame
  void Update()
  {
    if (!mySocket.Connected)
    {
      SetupSocket();
    }

    if (transmission.transform.position.x < neutral - 0.0003)
    {
      speed = 200;
      //Debug.Log(1);
    }
    else if (transmission.transform.position.x > neutral + 0.00023)
    {
      speed = -200;
      //Debug.Log(-1);
    }
    else
    {
      speed = 0;
      //Debug.Log(0);
    }
    speedSign = speed >= 0;
    speedVal = Math.Abs(speed);
    scaledAngle = ((wheel.transform.rotation.z + 0.75) / (1.5)) * Math.PI;
    scaledAngle = 2*Math.PI * (1 / (1 + Math.Exp(-(1 * (scaledAngle - 0)))))- Math.PI;
    Debug.Log("SCA" + scaledAngle);
    left_vel = Math.Min(speedVal, (1 - Math.Cos(scaledAngle)) * speedVal);
    right_vel = Math.Min(speedVal, (1 + Math.Cos(scaledAngle)) * speedVal);
    l_vel = Convert.ToInt32(speedSign ? left_vel : -left_vel);
    r_vel = Convert.ToInt32(speedSign ? right_vel : -right_vel);
    //Debug.Log("SCA"+scaledAngle);
    //Debug.Log("speed          "+ speed);
    //Debug.Log("left_vel      " + left_vel);
    //Debug.Log("right_vel     " + right_vel);
    text = "100," + l_vel.ToString() + "," + r_vel.ToString() + "\n";
    //Debug.Log(text);


    //Debug.Log("Left" + left_vel + "           Right" + right_vel);
  }

  public bool SetupSocket()
  {
    try
    {
      mySocket.Connect(Host, Port);
      theStream = mySocket.GetStream();
      theWriter = new StreamWriter(theStream);
      Byte[] sendBytes = System.Text.Encoding.UTF8.GetBytes(text);
      mySocket.GetStream().Write(sendBytes, 0, sendBytes.Length);
      Debug.Log("socket is sent");
      return true;
    }
    catch (Exception e)
    {
      Debug.Log("Socket error: " + e);
      return false;
    }
  }

  async void SendSocketMessage()
  {
    

      // Sending plain text
      Debug.Log("Sending...");
      Debug.Log(text);
      Byte[] sendBytes = System.Text.Encoding.UTF8.GetBytes(text);
      mySocket.GetStream().Write(sendBytes, 0, sendBytes.Length);
    
  }



  private void OnApplicationQuit()
  {
    if (mySocket != null && mySocket.Connected)
      mySocket.Close();
  }

} // end class s_TCP  