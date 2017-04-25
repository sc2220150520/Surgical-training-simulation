using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mono.Data.Sqlite;

namespace Operation
{
    public class SCPersonInfo : System.Object
    {
        public string p_name;//姓名
        public string p_sex;//性别
        public float p_height;//身高
        public float p_weight;//体重
        public int p_age;//年龄
        public string p_other;//其他
        public string p_allergicHistory;//过敏史
        public SCPersonInfo(string name, string sex, float h, float w, int age,string allhis, string other)
        {
            p_name = name;
            p_sex = sex;
            p_height = h;
            p_weight = w;
            p_age = age;
            p_allergicHistory = allhis;
            p_other = other;
        }

    }
    public class SCState : System.Object
    {
        public string s_id;
        public string s_consciousness;//意识
        public string s_pulse;//脉搏
        public string s_bloodPressure;//血压
        public string s_heartRate;//心率
        public string s_oxygenSaturation;//氧饱和度
        public string s_respiratoryFre;//呼吸频率
        public string s_other;//其他

        public SCState(string id,string con,string pul,string blo,string hea,string oxy,string res,string oth)
        {
            s_id = id;
            s_consciousness = con;
            s_pulse = pul;
            s_bloodPressure = blo;
            s_heartRate = hea;
            s_oxygenSaturation = oxy;
            s_respiratoryFre = res;
            s_other = oth;
        }
    }
    //处置操作
    public class SChandle : System.Object
    {
        public string h_id;//操作id
        public string h_name;//操作名称
        public string h_groupId;//操作所属的组id
        public string h_groupName;//操作所属的组名称
        public string h_describe;//操作描述
        public string h_organ;//作用器官
        public string h_drugs;//操作所需药品
        public string h_other;//其他信息
    
        public SChandle(string id,string name,string groupId,string groupName,string describe,string organ,string drugs,string other)
        {
            h_id = id;
            h_name = name;
            h_groupId = groupId;
            h_groupName = groupName;
            h_describe = describe;
            h_organ = organ;
            h_drugs = drugs;
            h_other = other;
        }

        public int stateChange(SCStateAndOpeation.SCState state)
        {
            //连接数据库
            DbAccess db = new DbAccess("data source = " + Application.dataPath + "/ModelData/NDtreatmentnew.db");
            //根据当前状态和操作寻找下一个状态
            string sqlstr = "select NC_nextstate from NT_stateChange where NC_prestate = " + state.stateid + " AND NC_operation = '"+h_id +"'";
            SqliteDataReader resultchange = db.ExecuteQuery(sqlstr);
            //如果标准流程状态关系转换表中没有查到则没有则默认此操作为错误操作（由于在有限机状态训练中状态有限，所以一切错误操作可能会导致很多种状态，而这些状态最终结果肯定是死亡或则不好的，所以在系统中直接不存库）
            //标准库中只存正确的一些救治方式
            if (!resultchange.HasRows)
            {
                //关闭数据库
                resultchange.Close();
                db.CloseSqlConnection();
                return 0;
            }
                
            else
            {
                resultchange.Read();
                string nextstate = resultchange.GetString(resultchange.GetOrdinal("NC_nextstate"));
                resultchange.Close();
                db.CloseSqlConnection();
                return int.Parse(nextstate);
            }
           // return 0;
        }
    }

    public class SCGroup:System.Object
    {
        public string g_ID;
        public string g_name;
       // public List<SChandle> g_handleCollection;//操作集合
       // public string g_type;//这个组属于的类型，属于什么大类

        public SCGroup(string id,string name)//,string type
        {
            g_ID = id;
            g_name = name;
           // g_handleCollection = new List<SChandle>();
            //g_type = type;
        }
    }


    public class Drugs: IComparable<Drugs>
    {
        public string d_id;
        public string d_name;
        public string d_class;
        public string d_concentration;
        public string d_dose;
        public Drugs(string id,string name,string classd, string con,string dose)
        {
            d_id = id;
            d_name = name;
            d_concentration = con;
            d_class = classd;
            d_dose = dose;
        }
        public int CompareTo(Drugs b)
        {
            return this.d_id.CompareTo(b.d_id);
        }
        //public int Comparer(Drugs a,Drugs b)
        // {
        //     return a.d_id.CompareTo(b.d_id);
        // }
    }

    public class Order:System.Object
    {
        public int o_order;
        public string o_handleId;
        public string o_handleName;

        public Order(int order ,string handle,string name)
        {
            o_order = order;
            o_handleId = handle;
            o_handleName = name;
        }
    }


}

