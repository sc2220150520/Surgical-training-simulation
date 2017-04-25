using System.Collections;
using UnityEngine;
using System;
using System.Data;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;

public class SCSceneCross : MonoBehaviour {
    public Slider proLoadingBar;
    public Text labProgress;
    // Use this for initialization
    void Start () {
        LoadGame();
	}
   
    // Update is called once per frame
    //void Update()
    //{
    //    if (Application.CanStreamedLevelBeLoaded(2))
    //    {
    //        Application.LoadLevel(2);

    //    }

    //}
    public void LoadGame()
    {
        StartCoroutine(StartLoading(2));
    }

    private IEnumerator StartLoading(int sceneName)
    {
        Debug.Log("shenchen");
        int displayProgress = 0;
        int toProgress = 0;
        AsyncOperation op = Application.LoadLevelAsync(2);   //异步对象  
        op.allowSceneActivation = false;
        while (op.progress < 0.9f)
        {
            toProgress = (int)op.progress * 100;
            while (displayProgress < toProgress)
            {
                ++displayProgress;
                SetLoadingPercentage(displayProgress);
                yield return new WaitForEndOfFrame();
            }
        }

        toProgress = 100;
        while (displayProgress < toProgress)
        {
            ++displayProgress;
            SetLoadingPercentage(displayProgress);
            yield return new WaitForEndOfFrame();
        }
        op.allowSceneActivation = true;
    }

    private void SetLoadingPercentage(int DisplayProgress)     //设置显示进度  
    {
        proLoadingBar.value = DisplayProgress * 0.01f;
        labProgress.text = DisplayProgress.ToString() + "%";
    }
    
}
