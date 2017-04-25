// Copyright (c) 2013 Sundog Software LLC. All rights reserved worldwide.

using UnityEngine;
using System.Collections;

public class SilverLiningSphereOrientor : MonoBehaviour {
	
	public bool flipVertical = false;
	public bool flipHorizontal = false;
	
	SilverLining silverLining = null;
	
	void Update () {
		
		if (silverLining == null) {
			silverLining = (SilverLining)(GameObject.FindObjectOfType(typeof(SilverLining)));
		}
		if (silverLining != null) {
			
			Vector3 celestialPole = new Vector3(), vernalEquinox = new Vector3();
			silverLining.GetCelestialPole(ref celestialPole, ref vernalEquinox);
			if (flipVertical) {
				celestialPole = celestialPole * -1.0f;
			}
			if (flipHorizontal) {
				vernalEquinox = vernalEquinox * -1.0f;
			}
			gameObject.transform.LookAt (vernalEquinox, celestialPole);
		}
	}
}