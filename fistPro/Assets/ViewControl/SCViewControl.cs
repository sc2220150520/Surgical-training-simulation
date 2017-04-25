using System.Collections;
using UnityEngine;
using Mono.Data.Sqlite;
using System;
using System.Data;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;
using SpeechLib;
using System.Threading;
using System.Linq;

public class SCViewControl : MonoBehaviour {
    public Operation.SCPersonInfo person;
    public Sprite btnImage;
    public Sprite btnImagetwo;
    public Font textFont;
    public GameObject parentGamobject;
    //操作组
    private List<Operation.SCGroup> stateGroupArry;
    public SCStateAndOpeation.SCState currentState;
    private List<CustomView.SCText> currentStateViewArry;
    public GameObject showTextParent;
    //控制对话框的隐藏与展示
    public GameObject produceDialog;
    public GameObject parentDialogBtn;
    //用于保存当点击了一个组按钮之后产生的新按钮，用于点击其他组按钮的时候对当前的进行消除
    private List<GameObject> currentDialogBtnArry;
    //保存当前选择的药品
    private List<Operation.Drugs> currentDrugArry;

    private System.Timers.Timer myTimer;
    //主线程要做的销毁定时器等事情
    private List<Action> action;

    private bool isTeacher;
    //结束时候的操作列表
    private List<string> operationList;
    //结束时的操作文本框集合
    private List<CustomView.SCText> endListView;
    public GameObject gameObjectEndTextParent;

    void Start () {
        stateGroupArry = new List<Operation.SCGroup>();
        getDataForGroup();
        initOperationView();
        if (PlayerPrefs.GetString("mode") == "教学")
            isTeacher = true;
        else
            isTeacher = false;
        //初始化开始状态
        currentState = new SCStateAndOpeation.SCState(1);
        getCurrentState(1);
        currentStateViewArry = new List<CustomView.SCText>();
        showCurrentState();
        currentDialogBtnArry = new List<GameObject>();
        currentDrugArry = new List<Operation.Drugs>();

        //设置定时器，当超过一定时间时自动结束
        myTimer = new System.Timers.Timer(600000);//600000
        myTimer.Elapsed += myTimer_Elapsed;//到2秒了做的事件
        myTimer.AutoReset = false;//是否不断重复定时器操作
        myTimer.Enabled = true; //定时器开始用

        action = new List<Action>();
        operationList = new List<string>();
        endListView = new List<CustomView.SCText>();

    }

    void myTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        action.Add(actionEvent);
       
    }

    void actionEvent()
    {
        GameObject.FindWithTag("EndDialog").GetComponent<Devdog.InventorySystem.UIWindow>().Show();
        //结束时去掉教学脚本
        if(isTeacher)
            GameObject.Destroy(gameObject.GetComponent<SCTeacher>());
        if (myTimer != null)
        {
            myTimer.Close(); //释放Timer占用的资源
            myTimer.Dispose();
        }
    }

    void initOperationView()
    {
        // CustomView.SCCanvs canvs = new CustomView.SCCanvs(5, "canvs", null);
        int j = 0;float k = 1.0f;
        while(j<stateGroupArry.Count)
        {
            float i = 0;
            while (i + 0.166f < 1.0f&&j<stateGroupArry.Count)
            {
                createBtn(new Vector2(i, k-0.08f), new Vector2(i + 0.166f, k), "btn", stateGroupArry[j]);
                i = i + 0.166f;
                j++;
            }
            k = k - 0.08f;
        }
      
       
    }
    //获取操作的组名并存放于数组中
   void getDataForGroup()
    {
        DbAccess db = new DbAccess("data source = " + Application.dataPath + "/ModelData/NDtreatmentnew.db");
        string sqlStr = "select distinct NC_groupid,NC_groupname from NT_operation order by NC_groupid";
        SqliteDataReader result = db.ExecuteQuery(sqlStr);
        while (result.Read())
        {
            string id = result.GetString(result.GetOrdinal("NC_groupid"));
            string name = result.GetString(result.GetOrdinal("NC_groupname"));
            Operation.SCGroup temp = new Operation.SCGroup(id,name);
            stateGroupArry.Add(temp);
        }
        result.Close();
        db.CloseSqlConnection();
    }
    //获取当前状态的各个变量和变量值
    void getCurrentState(int stateid)
    {
        if (currentState.stateMap.Count != 0)
            currentState.stateMap.Clear();
        DbAccess db = new DbAccess("data source = " + Application.dataPath + "/ModelData/NDtreatmentnew.db");
        string sqlstr = "select NC_stateid, NC_name,NC_stateValue from NV_stateview where NC_stateid = "+stateid;
        SqliteDataReader result = db.ExecuteQuery(sqlstr);
        currentState.stateid = stateid;
        while (result.Read())
        {
            string tempname = result.GetString(result.GetOrdinal("NC_name"));
            string tempvalue = result.GetString(result.GetOrdinal("NC_stateValue"));
            currentState.stateMap.Add(tempname,tempvalue);
        }
        result.Close();
        db.CloseSqlConnection();
    }
    //将当前状态的值显示在View中
    void showCurrentState()
    {
        if(currentStateViewArry.Count!=0)
        {
            foreach(CustomView.SCText it in currentStateViewArry)
            {
                Destroy(it._text);
            }
            currentStateViewArry.Clear();
        }
        float i = 0.98f;
        foreach (KeyValuePair<string, string> kvp in currentState.stateMap)
        {
            string text = kvp.Key +":" +kvp.Value;
            CustomView.SCText temp = new CustomView.SCText(5,"text",showTextParent.transform,text,textFont,FontStyle.Normal,new Color(0,0,0,1),TextAnchor.MiddleCenter,new Vector2(0.05f,i-0.08f),new Vector2(0.98f,i),new Vector2(0,0),new Vector2(0,0));
            currentStateViewArry.Add(temp);
            i = i - 0.08f;
        }
    }
    //创建组操作按钮
    void createBtn(Vector2 minpos,Vector2 maxpos,string name,Operation.SCGroup group)
    {
        CustomView.SCButton btn = new CustomView.SCButton(5, name, parentGamobject.transform, delegate ()
        {
            string gstr = group.g_name;
            //显示对话框
            produceDialog.GetComponent<Devdog.InventorySystem.UIWindow>().Show();
            gameObject.GetComponent<AudioSource>().Play();
            foreach (GameObject it in currentDialogBtnArry)
                Destroy(it);
            //清空对话框中现存的按钮
            currentDialogBtnArry.Clear();
            currentDrugArry.Clear();
            if(group.g_name == "注射"||group.g_name == "静点")
            {
                getDrug(group.g_ID,group.g_name);    
            }
            else
            {
                getOperation(group.g_ID);
            }
           
        }, btnImage,minpos,maxpos, new Vector2(0, 0), new Vector2(0, 0)// new Vector2(0, 0.92f), new Vector2(0.166f, 1.0f)
       );
        CustomView.SCText text
            = new CustomView.SCText(5, "text", btn._btn.transform, group.g_name, textFont, FontStyle.Normal, new Color(0, 0, 0, 1), TextAnchor.MiddleCenter, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 0));
    }
    //点击组按钮时读取数据库得到相对应组的操作
    void getOperation(string groupId)
    {
        DbAccess db = new DbAccess("data source = " + Application.dataPath + "/ModelData/NDtreatmentnew.db");
        string sqlstr = "select * from NT_operation where NC_groupid = '" + groupId + "'";
        SqliteDataReader result = db.ExecuteQuery(sqlstr);
        float i = 0.03f, j = 0.93f; int k = 0;
        while (result.Read())
        {
            string tempname = result.GetString(result.GetOrdinal("NC_name"));
            string tempid = result.GetString(result.GetOrdinal("NC_id"));
            string tempgid = result.GetString(result.GetOrdinal("NC_groupid"));
            string tempgname = result.GetString(result.GetOrdinal("NC_groupname"));
            string tempdescribe = result.GetString(result.GetOrdinal("NC_oprationdescribe"));
            string temporgan = result.GetString(result.GetOrdinal("NC_organ"));
            string tempdrug = result.GetString(6);
            string tempother = result.GetString(result.GetOrdinal("NC_other"));
            Operation.SChandle temp = new Operation.SChandle(tempid,tempname,tempgid,tempgname,tempdescribe,temporgan,tempdrug,tempother);
            if(k<3)
            {
                createNoGroupBtn(new Vector2(i,j-0.05f),new Vector2(i+0.27f,j),"btnDialog",temp);
                i = i + 0.3f;
                k++;
            }
            else
            {
                k = 0;
                i = 0.03f;
                j = j - 0.05f;
                createNoGroupBtn(new Vector2(i, j - 0.05f), new Vector2(i + 0.27f, j), "btnDialog", temp);
                i = i + 0.3f;
                k++;
            }
           
        }
        result.Close();
        db.CloseSqlConnection();
    }
    //获取药物并将药物填写在对话框中
    void getDrug(string groupId,string groupName)
    {
        DbAccess db = new DbAccess("data source = " + Application.dataPath + "/ModelData/NDtreatmentnew.db");
        string sqlstr = "select * from NT_drug";
        SqliteDataReader result = db.ExecuteQuery(sqlstr);
        float i = 0.03f, j = 0.93f; int k = 0;
        while (result.Read())
        {
            string tempid = result.GetString(result.GetOrdinal("NC_id"));
            string tempname = result.GetString(result.GetOrdinal("NC_name"));
            string tempclass = result.GetString(result.GetOrdinal("NC_class"));
            string tempconcentration = result.GetString(result.GetOrdinal("NC_concentration"));
            string tempdose = result.GetString(result.GetOrdinal("NC_dose"));
           
            Operation.Drugs temp = new Operation.Drugs(tempid, tempname, tempclass, tempconcentration, tempdose);
            if (k < 3)
            {
                createDrugBtn(new Vector2(i, j - 0.05f), new Vector2(i + 0.27f, j), "btnDialog", temp);
                i = i + 0.3f;
                k++;
            }
            else
            {
                k = 0;
                i = 0.03f;
                j = j - 0.05f;
                createDrugBtn(new Vector2(i, j - 0.05f), new Vector2(i + 0.27f, j), "btnDialog", temp);
                i = i + 0.3f;
                k++;
            }

        }
        result.Close();
        db.CloseSqlConnection();
        CustomView.SCButton sumitbtn = new CustomView.SCButton(5, "submit", parentDialogBtn.transform.parent, delegate ()
        {
            onSubmitBtn(groupId,groupName);
        }, btnImage, new Vector2(0.0f,0.9f), new Vector2(0.93f,1.0f), new Vector2(0, 0), new Vector2(0, 0)// new Vector2(0, 0.92f), new Vector2(0.166f, 1.0f)
    );
        CustomView.SCText text
            = new CustomView.SCText(5, "text", sumitbtn._btn.transform, "提交", textFont, FontStyle.Normal, new Color(0, 0, 0, 1), TextAnchor.MiddleCenter, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 0));
        currentDialogBtnArry.Add(sumitbtn._btn);
    }

    //创建一般操作按钮
    void createNoGroupBtn(Vector2 minpos, Vector2 maxpos, string name, Operation.SChandle handle)
    {
        CustomView.SCButton btn = new CustomView.SCButton(5, name, parentDialogBtn.transform, delegate ()
        {
            gameObject.GetComponent<AudioSource>().Play();
            //教学模式中判断本操作是否是当前要求的操作并对要求操作做更新
            if(isTeacher)
                gameObject.GetComponent<SCTeacher>().isAccord(handle);
            //查询这个操作是否在次序表中有，如果有则去状态转换表查看是否需要状态转换
            int nextstate = handle.stateChange(currentState);
            //如果转换结果没有则说明本操作错误，将操作及其正确与否进行存储
            if (nextstate != 0)
            {
                stateGetAndShow(nextstate);
                //将此正确操作存储
                operationList.Add(handle.h_name+ ":√");
            }
            else
            {
                //将此错误操作存储
                operationList.Add(handle.h_name+ ":×");
            }
        }, btnImagetwo, minpos, maxpos, new Vector2(0, 0), new Vector2(0, 0)// new Vector2(0, 0.92f), new Vector2(0.166f, 1.0f)
      );
        CustomView.SCText text
            = new CustomView.SCText(5, "text", btn._btn.transform, handle.h_name, textFont, FontStyle.Normal, new Color(0, 0, 0, 1), TextAnchor.MiddleCenter, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 0));
        currentDialogBtnArry.Add(btn._btn);
    }

    //创建药物按钮
    void createDrugBtn(Vector2 minpos, Vector2 maxpos, string name, Operation.Drugs drug)
    {
        GameObject instance = Instantiate(Resources.Load<GameObject>("toggle"));
        instance.name = name;
        instance.transform.parent = parentDialogBtn.transform;
        instance.GetComponent<RectTransform>().anchorMin = minpos;
        instance.GetComponent<RectTransform>().anchorMax = maxpos;
        instance.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
        instance.GetComponent<RectTransform>().offsetMax = new Vector2(0.0f, 0.0f);
        instance.GetComponentInChildren<Text>().text = drug.d_concentration + drug.d_name + drug.d_dose;
        instance.GetComponent<Toggle>().onValueChanged.AddListener(
            delegate(bool ison)
            {
                if (ison)
                {
                    currentDrugArry.Add(drug);
                }
                else
                {
                    currentDrugArry.Remove(drug);
                }
            }
            );
        currentDialogBtnArry.Add(instance);

    }

    void onSubmitBtn(string groupId,string groupName)
    {
        gameObject.GetComponent<AudioSource>().Play();
        string drugstr = "";
        string drugnamestr = "";
        currentDrugArry.Sort();
        for (int ii = 0; ii < currentDrugArry.Count; ii++)
        {
            Operation.Drugs it = currentDrugArry[ii];
      
            if (ii != currentDrugArry.Count - 1)
            {
                drugstr = drugstr + it.d_id + "|";
                drugnamestr = drugnamestr + it.d_name+"、";
            }    
            else
            {
                drugstr = drugstr + it.d_id;
                drugnamestr = drugnamestr + it.d_name;
            }
               
        }

        DbAccess db2 = new DbAccess("data source = " + Application.dataPath + "/ModelData/NDtreatmentnew.db");
        string sqlstr2 = "select * from NT_operation where NC_drug like '" + drugstr +"'" + "AND NC_groupid = '" + groupId+"'";

        SqliteDataReader result2 = db2.ExecuteQuery(sqlstr2);
        if (!result2.HasRows)
        {
            result2.Close();
            db2.CloseSqlConnection();
            Debug.Log("没有此药品");
            operationList.Add(groupName + drugnamestr + ":×");
            return;
        }
           
        else
        {
            result2.Read();
            string tempname = result2.GetString(result2.GetOrdinal("NC_name"));
            string tempid = result2.GetString(result2.GetOrdinal("NC_id"));
            string tempgid = result2.GetString(result2.GetOrdinal("NC_groupid"));
            string tempgname = result2.GetString(result2.GetOrdinal("NC_groupname"));
            string tempdescribe = result2.GetString(result2.GetOrdinal("NC_oprationdescribe"));
            string temporgan = result2.GetString(result2.GetOrdinal("NC_organ"));
            string tempdrug = result2.GetString(6);
            string tempother = result2.GetString(result2.GetOrdinal("NC_other"));
            Operation.SChandle temp = new Operation.SChandle(tempid, tempname, tempgid, tempgname, tempdescribe, temporgan, tempdrug, tempother);
            //教学模式中判断本操作是否是当前要求的操作并对要求操作做更新
            if(isTeacher)
                gameObject.GetComponent<SCTeacher>().isAccord(temp);

            int nextstate = temp.stateChange(currentState);
            if (nextstate != 0)
            {
                operationList.Add(temp.h_name + drugnamestr+ ":√");
                stateGetAndShow(nextstate);     
            }
            else
            {
                operationList.Add(temp.h_name + drugnamestr+ ":×");
            }
            result2.Close();
            db2.CloseSqlConnection();
        }   
    }

    //获取当前状态并判断其是否是结束状态如果不是则显示当前状态
    void stateGetAndShow(int nextstate)
    {
        //变换状态
        getCurrentState(nextstate);
        if (currentState.stateMap.ContainsKey("end"))
        {
            GameObject.FindWithTag("EndDialog").GetComponent<Devdog.InventorySystem.UIWindow>().Show();
            //结束时去掉教学脚本
            GameObject.Destroy(gameObject.GetComponent<SCTeacher>());
            AssessmentList();
        }
        else
            showCurrentState();
    }

    void AssessmentList()
    {
        if (endListView.Count != 0)
        {
            foreach (CustomView.SCText it in endListView)
            {
                Destroy(it._text);
            }
            endListView.Clear();
        }
        float i = 0.98f;
        foreach (string it in operationList)
        { 
            string text = it;
            CustomView.SCText temp = new CustomView.SCText(6, "text", gameObjectEndTextParent.transform, text, textFont, FontStyle.Normal, new Color(0, 0, 0, 1), TextAnchor.MiddleCenter, new Vector2(0.05f, i - 0.08f), new Vector2(0.98f, i), new Vector2(0, 0), new Vector2(0, 0));
            endListView.Add(temp);
            i = i - 0.08f;
        }
    }

    // Update is called once per frame
    void Update () {
        lock (action)
        {
           if(action.Count!=0)
            {
                foreach (var it in action)
                    it();
            }
        }
    }

}
