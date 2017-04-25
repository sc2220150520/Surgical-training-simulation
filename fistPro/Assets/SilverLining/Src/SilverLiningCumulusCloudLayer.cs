// Copyright (c) 2011 Sundog Software LLC. All rights reserved worldwide.

using UnityEngine;
using System;
using System.Collections;

public class SilverLiningCumulusCloudLayer
{
    public double chi = 0.984;
    public double alpha = 0.001;
    public double nu = 0.5;
    public double beta = -0.10;
    public double minSize = 500;
    public double maxSize = 5000;
    public double epsilon = 100;
    public float ambientBoost = 0.7f;
    public static float cloudAlpha = 0.5f;

    public SilverLiningCumulusCloudLayer (Vector3 pDimensions, Vector3 pCenter, double pCoverage)
    {
        alpha /= SilverLining.unitScale;
        minSize *= SilverLining.unitScale;
        maxSize *= SilverLining.unitScale;
        epsilon *= SilverLining.unitScale;
        
        dimensions = pDimensions;
        a = dimensions.x * 0.5f;
        b = dimensions.z * 0.5f;
        center = pCenter;
        coverage = pCoverage;
        
        clouds = new ArrayList ();
        
        CreateClouds ();
    }

    private bool TestInEllipse(Vector3 cloudPos)
    {
        // Make position relative to cloud layer center
        Vector3 P = cloudPos - center;

        return ( (P.x * P.x) / (a * a) + (P.z * P.z) / (b * b) < 1.0f );
    }

    public void Update ()
    {
        for (int i = 0; i < clouds.Count; i++) {
            SilverLiningCumulusCloud cloud = (SilverLiningCumulusCloud)clouds[i];
            cloud.Update ();
        }
    }

    public void UpdateFog(bool doFog)
    {
        Color fogColor = RenderSettings.fogColor;
        Vector4 fog = new Vector4 (fogColor.r, fogColor.g, fogColor.b, RenderSettings.fogDensity);
        if (!RenderSettings.fog || !doFog)
            fog.w = 0;

        Shader.SetGlobalVector("fog", fog);
    }
	
	public void UpdateOneCloud(Color lightColor, Vector3 lightDirection)
	{
        SilverLiningCumulusCloud cloud = (SilverLiningCumulusCloud)clouds[lastCloudUpdated];
        cloud.UpdateLighting (lightColor, lightDirection);
        cloud.GetRenderer().material.SetFloat("fade", cloudAlpha * cloud.GetAlpha());	
		cloud.Update ();
		
		lastCloudUpdated++;
		if (lastCloudUpdated == clouds.Count) {
			lastCloudUpdated = 0;
		}
	}
	
	public void UpdateAllClouds(Color lightColor, Vector3 lightDirection)
	{
        for (int i = 0; i < clouds.Count; i++) {
            SilverLiningCumulusCloud cloud = (SilverLiningCumulusCloud)clouds[i];
            cloud.UpdateLighting (lightColor, lightDirection);
            cloud.GetRenderer().material.SetFloat("fade", cloudAlpha * cloud.GetAlpha());
        }		
	}
	
    public void UpdateLighting (Color lightColor, Vector3 lightDirection)
    {
        Color ambientColor = RenderSettings.ambientLight;
        Vector4 ambientVector = new Vector4 (ambientColor.r, ambientColor.g, ambientColor.b, 1.0f);
        Vector4 diffuseVector = new Vector4 (lightColor.r, lightColor.g, lightColor.b, 1.0f);
        Vector4 lightDirVector = new Vector4 (lightDirection.x, lightDirection.y, lightDirection.z);

        Shader.SetGlobalFloat("minPhase", (float)SilverLiningCumulusCloud.minPhase);
        Shader.SetGlobalFloat("maxPhase", (float)SilverLiningCumulusCloud.maxPhase);
        Shader.SetGlobalFloat("ambientBoost", ambientBoost);
        Shader.SetGlobalVector("ambient", ambientVector);
        Shader.SetGlobalVector("lightColor", diffuseVector);
        Shader.SetGlobalVector("lightDir", lightDirVector);
    }

    public void WrapAndUpdateClouds(bool cloudWrap, bool cull, Color lightColor, Vector3 lightDir)
    {
        Vector3 anchor = center;

        float halfWidth = dimensions.x * 0.5f;
        float halfLength = dimensions.z * 0.5f;

        for (int i = 0; i < clouds.Count; i++)
        {
            SilverLiningCumulusCloud c = (SilverLiningCumulusCloud)clouds[i];

            Vector3 newCloudPos = c.GetPosition();

            Vector3 delta = newCloudPos - c.lightingWorldPos;
            if (delta.magnitude > (100.0 * SilverLining.unitScale)) {
                c.lightingWorldPos = newCloudPos;
                Material m = c.GetRenderer().material;
                m.SetVector("cloudPos", newCloudPos);
            }
			
#if UNITY_3_5
            if (cull) {
                if (c.shuriken) {
                    bool wasActive = c.cloudObject.active;
                    c.cloudObject.active = TestInEllipse(newCloudPos);
                    if (c.cloudObject.active == true && !wasActive) {
                        c.particleSystem.Simulate(0.1f);
                    }
                } else {
                    c.cloudObject.GetComponent<ParticleRenderer> ().enabled = TestInEllipse(newCloudPos);
                }
            }
			if (!cloudWrap || !c.cloudObject.active) continue;
#else
			if (cull) {
                if (c.shuriken) {
                    bool wasActive = c.cloudObject.activeSelf;
                    c.cloudObject.SetActive(TestInEllipse(newCloudPos));
                    if (c.cloudObject.activeSelf == true && !wasActive) {
                        c.particleSystem.Simulate(0.1f);
                    }
                } else {
                    c.cloudObject.GetComponent<ParticleRenderer> ().enabled = TestInEllipse(newCloudPos);
                }
            }

            if (!cloudWrap || !c.cloudObject.activeSelf) continue;
#endif
            // Iterate until it's in bounds.
            bool adj;
            bool changed = false;

            do
            {
                adj = false;

                if (newCloudPos.x > anchor.x + halfWidth)
                {
                    newCloudPos.x = (anchor.x - halfWidth) + (newCloudPos.x - (anchor.x + halfWidth));
                    adj = changed = true;
                }
                if (newCloudPos.x < anchor.x - halfWidth)
                {
                    newCloudPos.x = (anchor.x + halfWidth) - ((anchor.x - halfWidth) - newCloudPos.x);
                    adj = changed = true;
                }
                if (newCloudPos.z > anchor.z + halfLength)
                {
                    newCloudPos.z = (anchor.z - halfLength) + (newCloudPos.z - (anchor.z + halfLength));
                    adj = changed = true;
                }
                if (newCloudPos.z < anchor.z - halfLength)
                {
                    newCloudPos.z = (anchor.z + halfLength) - ((anchor.z - halfLength) - newCloudPos.z);
                    adj = changed = true;
                }
            } while (adj);

            if (changed) {
                c.SetPosition(newCloudPos);
            }

            float minDist = halfWidth * 2.0f + halfLength * 2.0f;
            float dist = Math.Abs(newCloudPos.x - (anchor.x + halfWidth)) / halfWidth;
            if (dist < minDist) minDist = dist;
            dist = Math.Abs(newCloudPos.x - (anchor.x - halfWidth)) / halfWidth;
            if (dist < minDist) minDist = dist;
            dist = Math.Abs(newCloudPos.z - (anchor.z + halfLength)) / halfLength;
            if (dist < minDist) minDist = dist;
            dist = Math.Abs(newCloudPos.z - (anchor.z - halfLength)) / halfLength;
            if (dist < minDist) minDist = dist;

            float t = 1.0f - minDist;
            if (t < 0) t = 0;
            if (t > 1.0f) t = 1.0f;

            float fade = 1.0f - (t * t * t * t * t);

            float epsilon = 0.01f;
            if (Math.Abs(c.GetAlpha() - fade) > epsilon) {
                c.SetAlpha(fade);
            }
        }
    }

    protected void CreateClouds ()
    {
        currentN = 0;
        targetN = 0;
        currentD = maxSize * epsilon * 0.5;
        
        double width, depth, height;
        while (GetNextCloud (out width, out depth, out height)) {
            Vector3 position = new Vector3 (UnityEngine.Random.value * dimensions.x, UnityEngine.Random.value * dimensions.y, UnityEngine.Random.value * dimensions.z);
            position += center;
            position -= new Vector3 (dimensions.x * 0.5f, 0, dimensions.z * 0.5f);
            
            SilverLiningCumulusCloud cloud = new SilverLiningCumulusCloud ((float)width, (float)height, (float)depth, position);
            clouds.Add (cloud);
        }
    }

    public void DestroyClouds()
    {
        for (int i = 0; i < clouds.Count; i++)
        {
            SilverLiningCumulusCloud c = (SilverLiningCumulusCloud)clouds[i];
            c.Destroy();
        }
        clouds.Clear();
    }

    protected bool GetNextCloud (out double width, out double depth, out double height)
    {
        width = depth = height = 0;
        
        while (currentN >= targetN) {
            currentD -= epsilon;
            
            if (currentD <= minSize) {
                return false;
            }
            
            currentN = 0;
            
            targetN = (int)(((2.0 * dimensions.x * dimensions.z * epsilon * alpha * alpha * alpha * coverage) / (Math.PI * chi)) * Math.Exp (-alpha * (currentD)));
            
            //printf("Creating %d clouds from %5.2f to %5.2f m diameter.\n", targetN,
            //     currentD - epsilon * 0.5, currentD + epsilon * 0.5);
        }
        
        if (currentD <= minSize) {
            return false;
        }
        
        // select random diameter within currentD += bandwidth
        double variationW = UnityEngine.Random.value * epsilon;
        double variationH = UnityEngine.Random.value * epsilon;
        
        width = currentD - epsilon * 0.5 + variationW;
        depth = currentD - epsilon * 0.5 + variationH;
        
        double D = (width + depth) * 0.5;
        
        double hOverD = nu * Math.Pow (D / maxSize, beta);
        height = D * hOverD;
        
        currentN++;
        
        return true;
    }

    ArrayList clouds;
    Vector3 dimensions, center;
    int currentN, targetN;
    double currentD, coverage;
    float a, b;
	int lastCloudUpdated = 0;
}


