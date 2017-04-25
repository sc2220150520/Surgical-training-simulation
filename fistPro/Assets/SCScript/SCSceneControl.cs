using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCSceneControl : MonoBehaviour {

	// Use this for initialization
	void Start () {
       
    }
	
	//重新加载
   public void load()
    {
        Application.LoadLevel(2);
    }
   public void exit()
    {
        Application.Quit();
    }
}
