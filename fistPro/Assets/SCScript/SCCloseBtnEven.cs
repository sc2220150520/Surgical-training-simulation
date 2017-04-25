
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCCloseBtnEven:MonoBehaviour {
    public GameObject btn;
    public GameObject parent;
    
    public void Start()
    {
        btn.GetComponent<Button>().onClick.AddListener(
            delegate()
            {
                parent.GetComponent<Devdog.InventorySystem.UIWindow>().Hide();
            }
            );
    }
	
}
