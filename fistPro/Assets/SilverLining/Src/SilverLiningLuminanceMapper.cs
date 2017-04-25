// Copyright (c) 2011 Sundog Software LLC. All rights reserved worldwide.

using UnityEngine;
using System;

public class SilverLiningLuminanceMapper
{
	private static double brightness = 0.8;
	
	public SilverLiningLuminanceMapper ()
	{
	}
	/** Sets the modeled maximum luminance of the display, in "nits", or
    candela per square meter. */
    public static void SetMaxDisplayLuminance(double nits)
	{
	    if (nits > 0)
	    {
	        Ldmax = nits;
	    }
	}

    public static void EnableToneMapping(bool enabled) {
        disableToneMapping = !enabled;
    }

/** Sets the log-average of the scene's luminance as perceived by both
   the eye's rods and cones, in nits. */
    public static void SetSceneLogAvg(double rodNits, double coneNits)
	{	
		if (rodNits < 0) rodNits = 0;
		if (coneNits < 0) coneNits = 0;
	    LsavgR = rodNits / brightness;
	    LsavgC = coneNits / brightness;
	    ComputeScaleFactors();
	}

/** Performs tone-mapping on an xyY color, where x and y are
   chromaticity and Y is luminance. The values passed in are modified
   by this method. Assumes that SetMaxDisplayLuminance() and
   SetSceneLogAvg() were previously called. */
    public static void DurandMapper(ref double x, ref double y, ref double Y)
    {
	    if (!disableToneMapping && ((y) > 0))
	    {
	        // From Durand
	        //const double xBlue = 0.3;
	        //const double yBlue = 0.3;
	
	        double X = (x) * ((Y) / (y));
	        double Z = (1.0 - (x) - (y)) * ((Y) / (y));
	        double R = X * -.702 + (Y) * 1.039 + Z * 0.433;
	
	        if (R < 0) R = 0;
	
	        // Straight Ferwerda tone mapping - gets us to normalized luminance at estimated
	        // display adaptation luminance
	        double Ldp = (Y) * mC;
	        double Lds = (R) *mR;
	
	        Y = Ldp + k * Lds;
	
	        Y /= Ldmax;
	
	        // Durand's blue-shift for scotopic vision
	        //*x = (*x) * (1-k) + k * xBlue;
	        //*y = (*y) * (1-k) + k * yBlue;
	    }
	}

/** Performs tone-mapping on a XYZ color. The color passed in is
   modified by this method. Assumes that SetMaxDisplayLuminance() and
   SetSceneLogAvg() were previously called. */
    public static void DurandMapperXYZ(ref Vector3 XYZ)
	{
	    if (!disableToneMapping)
	    {
	        double R = (XYZ.x) * -.702 + (XYZ.y) * 1.039 + (XYZ.z) * 0.433;
	
	        if (R < 0) R = 0;
	
	        Vector3 scotopic = new Vector3((float)(R * 0.3), (float)(R * 0.3), (float)(R * 0.4));
	
	        XYZ *= (float)((1 - k) * mC);
			XYZ += scotopic * (float)(k * mR);
			XYZ *= (float)(1.0 / Ldmax);
	
	    }
	
	}


/** Returns the computed scale factors for mapping luminance for
   both the eye's rods and cones. Assumes SetMaxDisplayLuminance() and
   SetSceneLogAvg() were previously called. */
    public static void GetLuminanceScales(out double rodSF, out double coneSF)
    {
        rodSF = mR;
        coneSF = mC;
    }

/** Retrieves the maximum display luminance previously set by
   SetMaxDisplayLuminance(). */
    public static double GetMaxDisplayLuminance() {
        return Ldmax;
    }

/** Returns the luminance, in nits, that is mapped to the maximum
   luminance the display can represent. Luminances higher than this are displayed
   as white. Assumes SetMaxDisplayLuminance() and SetSceneLogAvg() were
   previously called. */
    public static double GetBurnoutLuminance()
	{
	    return Ldmax / (mC + k * mR);
	}

/** Retrieves the computed blend factor between rod and cone perception
   based on the current lighting conditions. Assumes SetSceneLogAvg() was
   previously called. */
    public static double GetRodConeBlend() {
        return k;
    }

/** Retrieves the log-average rod and cone luminances in nits, as
   previously set by SetSceneLogAvg(). */
    public static void GetSceneLogAvg(out double rodNits, out double coneNits)
    {
        rodNits = LsavgR;
        coneNits = LsavgC;
    }
	
	private static double RodThreshold(double LaR)
	{
	    double logLaR = Math.Log(LaR);
	
	    double logEpsR;
	
	    if (logLaR <= -3.94)
	    {
	        logEpsR = -2.86;
	    }
	    else if (logLaR >= -1.44)
	    {
	        logEpsR = logLaR - 0.395;
	    }
	    else
	    {
	        logEpsR = Math.Pow(0.405 * logLaR + 1.6, 2.18) - 2.86;
	    }
	
	    return Math.Exp(logEpsR);
	}
	
	private static double ConeThreshold(double LaC)
	{
	    double logLaC = Math.Log(LaC);
	    double logEpsC;
	
	    if (logLaC <= -2.6)
	    {
	        logEpsC = -0.72;
	    }
	    else if (logLaC >= 1.9)
	    {
	        logEpsC = logLaC - 1.255;
	    }
	    else
	    {
	        logEpsC = Math.Pow(0.249 * logLaC + 0.65, 2.7) - 0.72;
	    }
	
	    return Math.Exp(logEpsC);
	}
	
	private static void ComputeScaleFactors()
	{
	    /** Ferwerda operator **/
	    double LdaC, LwaR, LwaC;
	
	    Ldmax = 100.0;
	
	    LdaC = Ldmax;
	
	    LwaR = LsavgR;
	    LwaC = LsavgC;
	
	    double displayThreshold = ConeThreshold(LdaC);
	
	    mR = displayThreshold / RodThreshold(LwaR);
	    mC = displayThreshold / ConeThreshold(LwaC);
	
	    const double sigma = 100.0;
	    k = (sigma - 0.25 * LwaR) / (sigma + LwaR);
	    if (k < 0) k = 0;
	}
	
	public static double Ldmax = 100.0;
    public static double mR = 1.0, mC = 1.0, k = 1.0, LsavgR = 100.0, LsavgC = 100.0;
    public static bool disableToneMapping = false;
}


