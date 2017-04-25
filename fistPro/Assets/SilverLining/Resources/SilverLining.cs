// Copyright (c) 2011-2013 Sundog Software LLC. All rights reserved worldwide.

//#define HAS_GUI

using UnityEngine;
using System;
using System.Collections;

#pragma warning disable 0414

public class SilverLining : MonoBehaviour
{
	private float todSliderValue = 0.0f;
	
#if HAS_GUI
	void OnGUI () {
		// Make a background box
		GUI.Box(new Rect(10,10,150,170), "SilverLining");
		GUI.Label(new Rect(25, 35, 100, 20), "Time of Day");
		todSliderValue = GUI.HorizontalSlider (new Rect (25, 55, 120, 20), todSliderValue, 0.0f, 24.0f);
		
		hasCumulusClouds = GUI.Toggle(new Rect(25, 75, 120, 20), hasCumulusClouds, "Cumulus Clouds");
		cumulusCoverage = GUI.HorizontalSlider(new Rect(25, 95, 120, 20), cumulusCoverage, 0.0f, 1.0f);
		hasStratusClouds = GUI.Toggle(new Rect(25, 115, 120, 20), hasStratusClouds, "Stratus Clouds");
		stratusDensity = GUI.HorizontalSlider(new Rect(25, 135, 120, 20), stratusDensity, 0.0f, 1.0f);
		hasCirrusClouds = GUI.Toggle(new Rect(25, 155, 120, 20), hasCirrusClouds, "Cirrus Clouds");
		
		hour = (int)(Math.Floor(todSliderValue));
		minutes = (int)((todSliderValue % 1.0) * 60.0);
		time.SetTime(hour, minutes, seconds, timeZone, daylightSavingsTime);
	}
#endif
	
	// Convert RA and Dec (in degrees) to GUI screen coordinates (with Y flipped for GUI use.)
	public Vector3 EquatorialToGUICoordinates(double rightAscension, double declination)
	{
		Vector3 direction = sky.EquatorialToHorizon (rightAscension, declination);
		direction = new Vector3(direction.y, direction.z, -direction.x);
		Vector3 worldPos = Camera.main.transform.position + direction * 90000.0f;
		Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPos);
		screenPosition.y = Screen.height - screenPosition.y; 	
		
		return screenPosition;
	}
	
	public void GetCelestialPole(ref Vector3 pole, ref Vector3 vernalEquinox)
	{
		pole = sky.EquatorialToHorizon(0, 90.0);
		pole = new Vector3(pole.y, pole.z, -pole.x);
		vernalEquinox = sky.EquatorialToHorizon (0, 0);
		vernalEquinox = new Vector3(vernalEquinox.y, vernalEquinox.z, -vernalEquinox.x);
	}
	
    // Use this for initialization
    void Start ()
    {
        //QualitySettings.vSyncCount = 0;

        time = new SilverLiningTime ();
        sky = new SilverLiningSky ();
        location = new SilverLiningLocation ();
        lastFogColor = new Color (0, 0, 0, 0);
        lastFogDensity = -1;
		
		todSliderValue = (float)hour + (float)minutes / (60.0f);
		
		sky.moonWidthDegrees = moonInitialWidthDegrees;
		sky.sunWidthDegrees = sunInitialWidthDegrees;
		
        sky.Start ();

        CreateClouds ();

        //Camera.main.clearFlags = CameraClearFlags.Depth;
        float farClip = (float)Math.Max (100000.0 * unitScale, Camera.main.farClipPlane);
        Camera.main.farClipPlane = farClip;

        cumulusCloudTransform = GameObject.Find ("CumulusClouds");
        cirrusCloudTransform = GameObject.Find ("CirrusClouds");
        stratusCloudTransform = GameObject.Find ("StratusClouds");
    }

    // Update is called once per frame
    void Update ()
    {
        bool forceFog = false;
        if (stratusCloud != null) {
            forceFog = stratusCloud.IsInsideCloud();
        }

        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf != null) {
            mf.mesh.bounds = new Bounds(Camera.main.transform.position, Vector3.one * 100000.0f);
        }

        time.SetDate (year, month, day);
        time.SetTime (hour, minutes, seconds, timeZone, daylightSavingsTime);
		
		if (continuousTimeAdvance) {
			float elapsed = Time.deltaTime * continuousTimeAdvanceRate;
			time.AddSeconds (elapsed);
			year = time.GetYear ();
			month = time.GetMonth ();
			day = time.GetDay ();
			hour = time.GetHour ();
			minutes = time.GetMinute ();
			seconds = time.GetSeconds ();
		}
		
        location.SetLatitude (latitude);
        location.SetLongitude (longitude);
        location.SetAltitude (altitude);

        sky.ambientLightScale = ambientLightScale;
        sky.sunLightScale = sunLightScale;
        sky.moonScale = moonLightScale;

        sky.Update (time, location, GetComponent<Renderer>(), IsOvercast(), applyFogToSkyDome || forceFog);

        if (hasCumulusClouds && cumulusClouds != null) {
             cumulusClouds.WrapAndUpdateClouds(wrapCumulusClouds, cumulusEllipseBounds,
                sky.GetSunOrMoonColor(), sky.GetSunOrMoonPosition());
			
			if (continuousTimeAdvance && cloudUpdatesPerFrame > 0) {
				cumulusClouds.UpdateLighting (sky.GetSunOrMoonColor (), sky.GetSunOrMoonPosition ());
				for (int i = 0; i < cloudUpdatesPerFrame; i++) {	
					cumulusClouds.UpdateOneCloud(sky.GetSunOrMoonColor(), sky.GetSunOrMoonPosition());
				}
			} else {
	             if (NeedsLightingUpdate()) {
	                cumulusClouds.UpdateLighting (sky.GetSunOrMoonColor (), sky.GetSunOrMoonPosition ());
					cumulusClouds.UpdateAllClouds(sky.GetSunOrMoonColor (), sky.GetSunOrMoonPosition());
	                cumulusClouds.Update ();
	            }
			}

            if (NeedsFogUpdate()) {
                cumulusClouds.UpdateFog(applyFogToClouds || forceFog);
            }
        }

        if (hasStratusClouds && stratusCloud != null) {
            stratusCloud.Update(sky, stratusDensity, stratusWindPosition, applyFogToClouds || forceFog);
        }

        Vector3 delta = Time.deltaTime * windVelocity;
        cumulusCloudTransform.transform.position += delta;
        cirrusCloudTransform.transform.position += delta;
        stratusWindPosition += delta;

        CheckRecreateClouds();
    }

    private void CheckRecreateClouds()
    {
        if (hasCumulusClouds != lastHasCumulusClouds || cumulusDimensions != lastCumulusDimensions ||
            cumulusPosition != lastCumulusPosition || cumulusCoverage != lastCumulusCoverage ||
            applyFogToClouds != lastApplyFogToClouds)
        {
            if (cumulusClouds != null) {
                cumulusClouds.DestroyClouds();
            }

            cumulusClouds = null;

            if (hasCumulusClouds) {
                cumulusClouds = new SilverLiningCumulusCloudLayer (cumulusDimensions, cumulusPosition, cumulusCoverage);
                cumulusClouds.UpdateLighting (sky.GetSunOrMoonColor (), sky.GetSunOrMoonPosition ());
				cumulusClouds.UpdateAllClouds (sky.GetSunOrMoonColor (), sky.GetSunOrMoonPosition());
                cumulusClouds.UpdateFog(applyFogToClouds);
                cumulusClouds.Update();
            }

            lastHasCumulusClouds = hasCumulusClouds;
            lastCumulusDimensions = cumulusDimensions;
            lastCumulusPosition = cumulusPosition;
            lastCumulusCoverage = cumulusCoverage;
            lastApplyFogToClouds = applyFogToClouds;
        }

        if (hasStratusClouds != lastHasStratusClouds || stratusPosition != lastStratusPosition ||
            stratusSize != lastStratusSize || stratusThickness != lastStratusThickness)
        {
            if (stratusCloud != null) {
                stratusCloud.Destroy();
            }

            stratusCloud = null;

            if (hasStratusClouds) {
                stratusCloud = new SilverLiningStratusCloud(stratusPosition, stratusSize, stratusThickness);
                stratusWindPosition = stratusPosition;
            }

            lastHasStratusClouds = hasStratusClouds;
            lastStratusPosition = stratusPosition;
            lastStratusSize = stratusSize;
            lastStratusThickness = stratusThickness;
        }

        if (hasCirrusClouds != lastHasCirrusClouds || cirrusPosition != lastCirrusPosition ||
            cirrusSize != lastCirrusSize)
        {
            if (cirrusCloud != null) {
                cirrusCloud.Destroy();
            }

            cirrusCloud = null;

            if (hasCirrusClouds) {
                cirrusCloud = new SilverLiningCirrusCloud(cirrusPosition, cirrusSize);
            }

            lastHasCirrusClouds = hasCirrusClouds;
            lastCirrusPosition = cirrusPosition;
            lastCirrusSize = cirrusSize;
        }

    }

    private void CreateClouds ()
    {
        if (hasCumulusClouds) {
            cumulusClouds = new SilverLiningCumulusCloudLayer (cumulusDimensions, cumulusPosition, cumulusCoverage);
        }

        if (hasCirrusClouds) {
            cirrusCloud = new SilverLiningCirrusCloud (cirrusPosition, cirrusSize);
        }

        if (hasStratusClouds) {
            stratusCloud = new SilverLiningStratusCloud (stratusPosition, stratusSize, stratusThickness);
            stratusWindPosition = stratusPosition;
        }

        lastHasCumulusClouds = hasCumulusClouds;
        lastCumulusDimensions = cumulusDimensions;
        lastCumulusPosition = cumulusPosition;
        lastCumulusCoverage = cumulusCoverage;
        lastHasCirrusClouds = hasCirrusClouds;
        lastCirrusPosition = cirrusPosition;
        lastCirrusSize = cirrusSize;
        lastHasStratusClouds = hasStratusClouds;
        lastStratusPosition = stratusPosition;
        lastStratusSize = stratusSize;
        lastStratusThickness = stratusThickness;
        lastApplyFogToClouds = applyFogToClouds;
    }

    private bool NeedsFogUpdate()
    {
        bool update = false;

        if (RenderSettings.fog != lastFog) {
            lastFog = RenderSettings.fog;
            update = true;
        }

        if (RenderSettings.fogColor != lastFogColor) {
            lastFogColor = RenderSettings.fogColor;
            update = true;
        }

        if (RenderSettings.fogDensity != lastFogDensity) {
            lastFogDensity = RenderSettings.fogDensity;
            update = true;
        }

        return update;
    }

    private bool NeedsLightingUpdate ()
    {
        bool update = false;

        if (sky.GetLightingChanged ())
            update = true;

        if (stratusDensity != lastStratusDensity) {
            lastStratusDensity = stratusDensity;
            update = true;
        }
		
		if (RenderSettings.ambientLight != lastAmbientColor) {
			lastAmbientColor = RenderSettings.ambientLight;
			update = true;
		}

        return update;
    }

    public bool IsOvercast() {
        bool overcast = (hasStratusClouds && stratusDensity >= 1.0);
        if (overcast && Camera.main.transform.position.y < stratusPosition.y) {
            return true;
        } else {
            return false;
        }
    }

    public int year = 2010, month = 7, day = 1, hour = 12, minutes = 0;
    public double seconds = 0;
    public bool daylightSavingsTime = true;
    public double timeZone = -8.0;
    public double latitude = 42.0, longitude = -122.0, altitude = 0;
    public bool applyFogToSkyDome = false;
    public bool applyFogToClouds = false;
    public bool hasCumulusClouds = true;
    public float cumulusCoverage = 0.5f;
    public Vector3 cumulusPosition = new Vector3(0.0f, 3000.0f, 0.0f);
    public Vector3 cumulusDimensions = new Vector3(80000.0f, 200.0f, 80000.0f);
    public bool wrapCumulusClouds = true;
    public bool cumulusEllipseBounds = false;
    public bool hasCirrusClouds = true;
    public Vector3 cirrusPosition = new Vector3(0.0f, 5000.0f, 0.0f);
    public float cirrusSize = 80000.0f;
    public bool hasStratusClouds = false;
    public Vector3 stratusPosition = new Vector3(0.0f, 2000.0f, 0.0f);
    public float stratusSize = 80000.0f;
    public float stratusDensity = 0.8f;
    public float stratusThickness = 1000.0f;
    public Vector3 windVelocity = new Vector3(10.0f, 0.0f, 10.0f);
    public float ambientLightScale = 1.0f, sunLightScale = 1.0f, moonLightScale = 1.0f;
	public bool continuousTimeAdvance = false;
	public float continuousTimeAdvanceRate = 1.0f;
	public int cloudUpdatesPerFrame = 3;
	public float moonInitialWidthDegrees = 1.0f;
	public float sunInitialWidthDegrees = 1.0f;

    private bool lastHasCumulusClouds, lastHasCirrusClouds, lastHasStratusClouds;
    private float lastCumulusCoverage, lastCirrusSize, lastStratusSize, lastStratusDensity, lastStratusThickness;
    private Vector3 lastCumulusPosition, lastCumulusDimensions, lastCirrusPosition, lastStratusPosition;
    private bool lastApplyFogToClouds;

    public static double unitScale = 1.0;

    private SilverLiningTime time;
    private SilverLiningLocation location;
    private SilverLiningSky sky;
    private SilverLiningCumulusCloudLayer cumulusClouds;
    private SilverLiningCirrusCloud cirrusCloud;
    private SilverLiningStratusCloud stratusCloud;
    private bool lastFog;
    private Color lastFogColor;
    private float lastFogDensity;
	private Color lastAmbientColor;
    private GameObject cumulusCloudTransform, cirrusCloudTransform, stratusCloudTransform;
    private Vector3 stratusWindPosition;
    
}
