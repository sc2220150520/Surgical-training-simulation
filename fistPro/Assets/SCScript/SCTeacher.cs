using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpeechLib;
using Mono.Data.Sqlite;
using System;
using System.Data;
using System.Threading;
using UnityEngine.UI;

public class SCTeacher : MonoBehaviour {

    public List<Operation.Order> ordersArry;
    //定时器
    private System.Timers.Timer myTimer;
    //当前提示的操作在数组中的索引
    private int index = 0;
    private SpVoice voice;
    private Thread th;
    public GameObject teacherText;
    public GameObject teacherImage;
    //主线程要做的事情
    private List<Action> action;
    // Use this for initialization
    void Start () {
       teacherImage.active = true;
       
        ordersArry = new List<Operation.Order>();
        getDataFromDataBase();

        myTimer = new System.Timers.Timer(10000);
        myTimer.Elapsed += myTimer_Elapsed;//到2秒了做的事件
        myTimer.AutoReset = true;//是否不断重复定时器操作
        myTimer.Enabled = true; //定时器开始用
                                //Control.CheckForIllegalCrossThreadCalls = false;

        action = new List<Action>();
    }
//得到数据从数据库
    void getDataFromDataBase()
    {
        DbAccess db = new DbAccess("data source = " + Application.dataPath + "/ModelData/NDtreatmentnew.db");
        string sqlstr = "select * from NT_processView order by NC_order";
        SqliteDataReader result = db.ExecuteQuery(sqlstr);
        while (result.Read())
        {
            string tempOrder = result.GetString(result.GetOrdinal("NC_order"));
            string tempOperationId = result.GetString(result.GetOrdinal("NC_operationid"));
            string tempOperationName = result.GetString(result.GetOrdinal("NC_name"));
            if (tempOperationName == "注射" || tempOperationName == "静点")
            {
                string tempDrug = result.GetString(result.GetOrdinal("NC_drug"));
                string[] sArray = tempDrug.Split('|');
                foreach (string i in sArray)
                {
                    string sql = "select * from NT_drug where NC_id = '" + i + "'";
                    SqliteDataReader temp = db.ExecuteQuery(sql);
                    if (!temp.HasRows)
                        temp.Close();
                    else
                    {
                        temp.Read();
                        tempOperationName = tempOperationName + temp.GetString(temp.GetOrdinal("NC_concentration")) + temp.GetString(temp.GetOrdinal("NC_name")) + temp.GetString(temp.GetOrdinal("NC_dose"))+",";
                        temp.Close();
                    }
                }

                ordersArry.Add(new Operation.Order(int.Parse(tempOrder), tempOperationId, tempOperationName));
            }
            else
            {
                ordersArry.Add(new Operation.Order(int.Parse(tempOrder), tempOperationId, tempOperationName));
            }
        }
        result.Close();
        db.CloseSqlConnection();
    }
    //计时器的操作
    void myTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        action.Add(actionEvent);
        if (th != null)
        {
            th.Abort();
            th = null;
        }
        th = new Thread(new ThreadStart(ThreadMethod)); //也可简写为new Thread(ThreadMethod);                
        th.Start(); //启动线程  
    }

    void ThreadMethod()
    {
        voice = null;
        voice = new SpVoice();
        //获取语音库
        voice.Voice = voice.GetVoices(string.Empty, string.Empty).Item(0);
        voice.Speak("请" + ordersArry[index].o_handleName);   
    }

    private void actionEvent()
    {
        teacherText.active = true;
        teacherText.transform.FindChild("Text").gameObject.GetComponent<Text>().text = "请" + ordersArry[index].o_handleName;
    }

    //释放定时器
    public void disposeTime()
    {
        myTimer.Close(); //释放Timer占用的资源
        myTimer.Dispose();
        if(th!=null)
            th.Abort();
        th = null;
    }

    //判断用户的点击是否是当前教程提示的步骤如果是则index++
    public void isAccord(Operation.SChandle customOperation)
    {
        if (customOperation.h_id == ordersArry[index].o_handleId)
        {
            object lockThis = new object();
            lock (lockThis)
            {
                index++;         
            }

            if (voice != null)
            {
                Debug.Log("声音暂停");
                voice.Pause();
                teacherText.active = false;
            }
            //Stop();   
        }
           
    }
    //语音停止
    public void Stop()
    {
        voice.Speak(string.Empty, SpeechVoiceSpeakFlags.SVSFPurgeBeforeSpeak);
    }

    void Update () {

        lock (action)
        {
            if (action.Count != 0)
            {
                foreach (var it in action)
                    it();
            }
            action.Clear();
        }

        if (voice != null)
        {
            if (voice.Status.RunningState == SpeechRunState.SRSEDone)
            {
                if (teacherText.active)
                    teacherText.active = false;
            }
        }

    }
    private void FixedUpdate()
    {
        if (index > ordersArry.Count-1)
            if(myTimer!=null)
                disposeTime();
    }

    private void OnDestroy()
    {
        if (myTimer != null)
            disposeTime();
    }
}
