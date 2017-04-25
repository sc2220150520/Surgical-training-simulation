using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCStateAndOpeation
{
    //状态类
    public class SCState : System.Object
    {
        //键值存储状态的属性，value存储属性的值，比如 心率，90；
        public Dictionary<string, string> stateMap;
        public int stateid;
        public SCState(int id)
        {
            stateid = id;
            stateMap = new Dictionary<string, string>();
        }
    }


}

