using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using NativeWebSocket;

public class ConnectionWS : MonoBehaviour
{
  WebSocket websocket;
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
  string text="";




// Start is called before the first frame update
  async void Start()
  {
     //websocket = new WebSocket("ws://echo.websocket.org");
    websocket = new WebSocket("ws://10.138.226.85:65321");

    websocket.OnOpen += () =>
    {
      Debug.Log("Connection open!");
    };

    websocket.OnError += (e) =>
    {
      Debug.Log("Error! " + e);
    };

    websocket.OnClose += (e) =>
    {
      Debug.Log("Connection closed!");
    };

    websocket.OnMessage += (bytes) =>
    {
      // Reading a plain text message
      var message = System.Text.Encoding.UTF8.GetString(bytes);
      Debug.Log("Received OnMessage! (" + bytes.Length + " bytes) " + message);
    };

    // Keep sending messages at every 0.1s
    InvokeRepeating("SendWebSocketMessage", 0.0f, 0.1f);

    await websocket.Connect();

  }

  void Update()
  {
    #if !UNITY_WEBGL || UNITY_EDITOR
      websocket.DispatchMessageQueue();
    #endif

    if (transmission.transform.position.x < neutral-0.0003)
    {
      speed = 50;
      //Debug.Log(1);
    }
    else if (transmission.transform.position.x > neutral+0.00023)
    {
      speed = -50;
      //Debug.Log(-1);
        }
    else 
        {
      speed = 0;
      //Debug.Log(0);
        }
    speedSign = speed >= 0;
    speedVal = Math.Abs(speed);
    scaledAngle = ((wheel.transform.rotation.z + 0.75) / (1.5)) * Math.PI ;
    left_vel = (Math.Min(speedVal, (1 - Math.Cos(scaledAngle)) * speedVal))/2;
    right_vel = (Math.Min(speedVal, (1 + Math.Cos(scaledAngle)) * speedVal))/2;
    l_vel = Convert.ToInt32(speedSign ? left_vel : -left_vel);
    r_vel = Convert.ToInt32(speedSign ? right_vel : -right_vel);
    //Debug.Log("SCA"+scaledAngle);
    //Debug.Log("speed          "+ transmission.transform.position.x);
    //Debug.Log("left_vel      " + left_vel);
    //Debug.Log("right_vel     " + right_vel);
    text= "100,"+l_vel.ToString()+","+r_vel.ToString() + "\n";
    Debug.Log(text);


    Debug.Log("Left" + left_vel + "           Right" + right_vel);

  }

  async void SendWebSocketMessage()
  {
    if (websocket.State == WebSocketState.Open)
    {

      // Sending plain text
      Debug.Log("Sending...");
      Debug.Log(text);
      await websocket.SendText(text);
    }
  }

  

  private async void OnApplicationQuit()
  {
    await websocket.Close();
  }
}
