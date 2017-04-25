using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCClickToPicUp : MonoBehaviour {
    public GameObject[] uiGameObjectList;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame8

	void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            int layer = LayerMask.NameToLayer("Patient");
            int uiLayer = LayerMask.NameToLayer("UI");
            RaycastHit hit;
            //检测是否点击病人层
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << layer))
            {
                //Debug.Log("hello world");
                GameObject hit_gameObject = hit.collider.gameObject;
                Transform child = hit_gameObject.transform.FindChild("Canvas");
                if (child != null)
                {
                    child.gameObject.active = true;
                }
            }
            //是否点击ui层
            else if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << uiLayer))
            {
                //
                //Debug.Log("Hello world");
            }
            //点击其他层
            else if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~(1 << layer | 1 << uiLayer)))
            {
                foreach (GameObject it in uiGameObjectList)
                {
                    it.gameObject.active = false;
                    
                }
            }
        }
	}
}
