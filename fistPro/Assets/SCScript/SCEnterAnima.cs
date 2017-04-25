using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpeechLib;

public class SCEnterAnima : MonoBehaviour {

    // Use this for initialization
    public GameObject UICanvas;
    //是否第一次进入房间
    private bool isFirstEnter = true;
    //第一次出来房间
    private bool isFirstExit = true;
    //界面控制器
    public SCViewControl viewControl;
    //教学
    public SCTeacher teacher;
	void Start () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "PlayerFir" && gameObject.name == "TriggerCube")
        {
            SpVoice voice = new SpVoice();
            voice.Voice = voice.GetVoices(string.Empty, string.Empty).Item(0);
            voice.Speak("欢迎来到海总院船,请寻找医务室");
            if (other.gameObject.GetComponent<UnityStandardAssets.Cameras.AutoCam>() != null)
                Destroy(other.gameObject.GetComponent<UnityStandardAssets.Cameras.AutoCam>());
            Destroy(gameObject.GetComponent<SCEnterAnima>());
                
        }
        if(other.tag == "PlayerFir" && gameObject.name == "door")
        {
            Animation temp = gameObject.GetComponent<Animation>();
            if (temp != null)
            {
                temp.Play();
            }
            else
                Debug.Log("获得动画组件为空");
            //第一次进入门后激活界面
            if(isFirstEnter)
            {
                isFirstEnter = false;
                UICanvas.active = true;

            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "PlayerFir" && gameObject.name == "door")
        {
            if (isFirstExit)
            {
                isFirstExit = false;
                //激活教学和界面控制器
                viewControl.enabled = true;
                if (PlayerPrefs.GetString("mode") == "教学")
                    teacher.enabled = true;
                else
                    teacher.enabled = false;

            }
        }
            
    }
}
