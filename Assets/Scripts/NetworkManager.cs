﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager _Manager;

    class MakoServerMessage
    {
        public string type;
        public string message;
        public string exp_type;
        public string expression;
        public string direction;
        public string module_name;
    }

    [TextArea()]
    public string ttsText;

    [HideInInspector]
    public Dictionary<string, string> ttsScript;

    [HideInInspector]
    public bool isConnected = false;
    [HideInInspector]
    public bool ttsDone;
    public bool emotionDone;
    [HideInInspector]
    public string sceneToLoad = "Null";

    Queue<MakoServerMessage> jobs;



    WebSocket ws;
    

    private void Awake()
    {
        if (_Manager != null)
        {
            Destroy(_Manager);
        }
        else
        {
            _Manager = this;
            DontDestroyOnLoad(this);
        }
        jobs = new Queue<MakoServerMessage>();
        ttsScript = new Dictionary<string, string>();
    }
    
    void Start()
    {
        string[] _ttsText = ttsText.Split('\n');

        for(int i = 0; i < _ttsText.Length; i++){
            ttsScript.Add(_ttsText[i].Trim(), i.ToString());
        }

        foreach(var elem in ttsScript){
            Debug.Log(elem);
        }

        //Debug.Log(ttsScript);

        ttsDone = true;
        ws = new WebSocket("ws://192.168.1.10:9000");
        ws.OnMessage += (sender, e) =>
        {
            Debug.Log(e.Data);
            MakoServerMessage msg = JsonConvert.DeserializeObject<MakoServerMessage>(e.Data);
            jobs.Enqueue(msg);
            //Debug.Log(JsonConvert.DeserializeObject<LEDControlPacket>(e.Data).type);
            //Debug.Log("Message received from " + ((WebSocket)sender).Url + ", Data : " + e.Data);
        };
        StartCoroutine(Connect());
    }

    IEnumerator Connect()
    {
        while (!isConnected)
        {
            ws.Connect();
            yield return new WaitForSeconds(10);
        }
    }

    void Update()
    {
        while(jobs.Count > 0)
        {
            MakoServerMessage msg = jobs.Dequeue();
            if(msg.type == "welcome")
            {
                isConnected = true;
                //Debug.Log(isConnected);
            }
            if(msg.type == "module_request")
            {
                if (msg.message != "WaitMenu")
                {
                    MakoServerMessage _msg = new MakoServerMessage();
                    _msg.type = "module_response";
                    _msg.message = "Request Received";
                    _msg.module_name = msg.message;
                    ws.Send(JsonConvert.SerializeObject(_msg));
                }
                SceneManager.LoadScene(msg.message);
            }
            if(msg.type == "tts_response")
            {
                if(msg.message == "tts_complete"){
                    ttsDone = true;
                }
            }
            // if(msg.type == "led_response")
            // {
            //     if(msg.message == "led_complete"){
            //         emotionDone = true;
            //         Debug.Log(emotionDone);
            //         //Invoke("sendNeutralFace", 5);
            //     }
            // }
        }
    }

    public void sendTTS(string message){
        ttsDone = false;
        Debug.Log(message);
        MakoServerMessage msg = new MakoServerMessage();
        msg.type = "tts_request";
        msg.message = ttsScript[message];        
        ws.Send(JsonConvert.SerializeObject(msg));
    }

    public void sendExpression(string exp_type){
        emotionDone = false;
        MakoServerMessage msg = new MakoServerMessage();
        msg.type = "led_control";
        msg.exp_type = exp_type;        
        ws.Send(JsonConvert.SerializeObject(msg));
        Invoke("sendNeutralFace", 5);
    }

    private void sendNeutralFace(){
        emotionDone = false;
        MakoServerMessage msg = new MakoServerMessage();
        msg.type = "led_control";
        msg.exp_type = "nf";        
        ws.Send(JsonConvert.SerializeObject(msg));
    }

    public void sendServoExpression(string expression){
        MakoServerMessage msg = new MakoServerMessage();
        msg.type = "servo_control";
        msg.expression = expression;        
        ws.Send(JsonConvert.SerializeObject(msg));
        if(expression.StartsWith("r"))  Invoke("sendServoReset_Right", 5);
        else Invoke("sendServoReset_Left", 5);
    }   

    public void sendServoReset_Right(){
        MakoServerMessage msg = new MakoServerMessage();
        msg.type = "servo_control";
        msg.expression = "right_reset";        
        ws.Send(JsonConvert.SerializeObject(msg));
    }

    public void sendServoReset_Left(){
        MakoServerMessage msg = new MakoServerMessage();
        msg.type = "servo_control";
        msg.expression = "left_reset";        
        ws.Send(JsonConvert.SerializeObject(msg));
    }

    public void sendMotorCommand(string direction){
        MakoServerMessage msg = new MakoServerMessage();
        msg.type = "motor_control";
        msg.direction = direction;     
        ws.Send(JsonConvert.SerializeObject(msg));
    }


}
