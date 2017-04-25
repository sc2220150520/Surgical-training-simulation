using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SCChangeScene : MonoBehaviour {
    public Text btnText;
	// Use this for initialization
	void Start () {
		
	}

    // Update is called once per frame
    public void onClickd()
    {

        Application.LoadLevel(3);
        
    }
    public void onClickEnterScene()
    {
        PlayerPrefs.SetString("mode",btnText.text);
        Application.LoadLevel(1);
    }


}
