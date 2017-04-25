// Copyright (c) 2012 Sundog Software LLC. All rights reserved worldwide.

using UnityEngine;
using System;

public class SilverLiningStratusCloud
{
    public SilverLiningStratusCloud (Vector3 position, float size, float thickness)
    {
        cloudSize = size;
        cloudThickness = thickness;
        fogStateCaptured = false;
        insideCloud = false;
        scudThickness = 100.0f;

        GameObject stratusClouds = GameObject.Find ("StratusClouds");
        GameObject cloudPrefab = GameObject.Find ("StratusCloudPrefab");

        fogShader = Shader.Find("Custom/Stratus");
        noFogShader = Shader.Find("Custom/StratusNoFog");

        if (stratusClouds != null && cloudPrefab != null)
        {
            // Plane primitive is 10x10, keep this in mind when scaling to desired size.
            cloudTop = (GameObject)GameObject.Instantiate (cloudPrefab, position, Quaternion.identity);
            cloudTop.transform.localScale = new Vector3 (size * 0.1f, 1.0f, size * 0.1f);
#if UNITY_3_5
            cloudTop.active = true;
#else
			cloudTop.SetActive (true);
#endif
            topRenderer = cloudTop.GetComponent<MeshRenderer> ();
            topRenderer.enabled = true;
            
            Quaternion q = Quaternion.AngleAxis (180.0f, new Vector3 (1.0f, 0.0f, 0.0f));
            cloudBottom = (GameObject)GameObject.Instantiate (cloudPrefab, position, q);
            cloudBottom.transform.localScale = new Vector3 (size * 0.1f, 1.0f, size * 0.1f);
#if UNITY_3_5
            cloudBottom.active = true;
#else
			cloudBottom.SetActive (true);
#endif
            bottomRenderer = cloudBottom.GetComponent<MeshRenderer> ();
            bottomRenderer.enabled = true;
    
            cloudTop.transform.parent = stratusClouds.transform;
            cloudBottom.transform.parent = stratusClouds.transform;
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


    public void Update(SilverLiningSky sky, float pDensity, Vector3 center, bool doFog)
    {
        topRenderer.material.SetFloat("_Density", pDensity);
        bottomRenderer.material.SetFloat("_Density", pDensity);

        topRenderer.material.SetFloat("_CloudSize", cloudSize);
        bottomRenderer.material.SetFloat("_CloudSize", cloudSize);

        if (doFog) {
            if (fogShader) {
                topRenderer.material.shader = fogShader;
                bottomRenderer.material.shader = fogShader;
            }
        } else {
            if (noFogShader) {
                topRenderer.material.shader = noFogShader;
                bottomRenderer.material.shader = noFogShader;
            }
        }

        float segmentSize = cloudSize / 20.0f;
        Vector3 camPos = Camera.main.transform.position;
        Vector3 offset = new Vector3();
        offset.x = (float)(-(camPos.x % segmentSize) + (center.x % segmentSize));
        offset.y = center.y - camPos.y;
        offset.z = (float)(-(camPos.z % segmentSize) + (center.z % segmentSize));

        cloudTop.transform.position = camPos + offset + new Vector3(0, cloudThickness, 0);
        cloudBottom.transform.position = camPos + offset;

        ApplyFog(center);
    }

    public bool IsInsideCloud()
    {
        return insideCloud;
    }

    private void ApplyFog(Vector3 cloudPos)
    {
        Vector3 camPos = Camera.main.transform.position;
        if (camPos.y >= (cloudPos.y - scudThickness) &&
            camPos.y <= (cloudPos.y + cloudThickness + scudThickness))
        {
            insideCloud = true;
            if (!fogStateCaptured)
            {
                savedFog = RenderSettings.fog;
                savedFogColor = RenderSettings.fogColor;
                savedFogDensity = RenderSettings.fogDensity;
                savedFogMode = RenderSettings.fogMode;
                fogStateCaptured = true;
            }

            float blend = 1.0f;
            if (camPos.y < cloudPos.y) {
                blend = 1.0f - (cloudPos.y - camPos.y) / scudThickness;
            } else if (camPos.y > cloudPos.y + cloudThickness) {
                blend = 1.0f - (camPos.y - (cloudPos.y + cloudThickness)) / scudThickness;
            }
            blend = blend * blend * blend * blend;
            RenderSettings.fog = true;
            Color fogColor = new Color(0.5f, 0.5f, 0.5f);
            RenderSettings.fogColor = (fogColor * blend) + (savedFogColor * (1.0f - blend));
            float fogDensity = 0.05f;
            float srcDensity = savedFog ? savedFogDensity : 1E-20f;
            RenderSettings.fogDensity = (fogDensity * blend) + (srcDensity * (1.0f - blend));
            RenderSettings.fogMode = UnityEngine.FogMode.ExponentialSquared;

            if (blend > 0.5f) {
                cloudBottom.GetComponent<Renderer>().enabled = false;
                cloudTop.GetComponent<Renderer>().enabled = false;
            } else {
                cloudBottom.GetComponent<Renderer>().enabled = true;
                cloudTop.GetComponent<Renderer>().enabled = true;
            }
        }
        else
        {
            insideCloud = false;
            cloudBottom.GetComponent<Renderer>().enabled = true;
            cloudTop.GetComponent<Renderer>().enabled = true;
            if (fogStateCaptured)
            {
                RenderSettings.fog = savedFog;
                RenderSettings.fogColor = savedFogColor;
                RenderSettings.fogDensity = savedFogDensity;
                RenderSettings.fogMode = savedFogMode;
                fogStateCaptured = false;
            }
        }
    }

    private GameObject cloudTop, cloudBottom;
    private Shader fogShader, noFogShader;
    private Renderer topRenderer, bottomRenderer;
    private float cloudSize, cloudThickness, scudThickness;
    private bool savedFog;
    private Color savedFogColor;
    private float savedFogDensity;
    private UnityEngine.FogMode savedFogMode;
    private bool fogStateCaptured;
    private bool insideCloud;
}


