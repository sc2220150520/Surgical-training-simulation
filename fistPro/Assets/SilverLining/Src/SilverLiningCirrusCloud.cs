// Copyright (c) 2011 Sundog Software LLC. All rights reserved worldwide.

using UnityEngine;
using System;

public class SilverLiningCirrusCloud
{
    public SilverLiningCirrusCloud (Vector3 position, float size)
    {
        GameObject cirrusClouds = GameObject.Find ("CirrusClouds");
        GameObject cloudPrefab = GameObject.Find ("CirrusCloudPrefab");

        if (cirrusClouds != null && cloudPrefab != null)
        {
            cloudTop = (GameObject)GameObject.Instantiate (cloudPrefab, position, Quaternion.identity);
            cloudTop.transform.localScale = new Vector3 (size, 1.0f, size);
#if UNITY_3_5
            cloudTop.active = true;
#else
			cloudTop.SetActive (true);
#endif
            cloudTop.GetComponent<Renderer>().material.renderQueue = 2002;
            MeshRenderer ren = cloudTop.GetComponent<MeshRenderer> ();
            ren.enabled = true;
            
            Quaternion q = Quaternion.AngleAxis (180.0f, new Vector3 (1.0f, 0.0f, 0.0f));
            cloudBottom = (GameObject)GameObject.Instantiate (cloudPrefab, position, q);
            cloudBottom.transform.localScale = new Vector3 (size, 1.0f, size);
#if UNITY_3_5
            cloudBottom.active = true;
#else
			cloudBottom.SetActive (true);
#endif
            cloudBottom.GetComponent<Renderer>().material.renderQueue = 2002;
            ren = cloudBottom.GetComponent<MeshRenderer> ();
            ren.enabled = true;
    
            cloudTop.transform.parent = cirrusClouds.transform;
            cloudBottom.transform.parent = cirrusClouds.transform;
        }
    }

    public void Destroy()
    {
        if (cloudTop != null) {
            UnityEngine.Object.Destroy(cloudTop);
        }

        if (cloudBottom != null) {
            UnityEngine.Object.Destroy(cloudBottom);
        }
    }

    private GameObject cloudTop, cloudBottom;
}


