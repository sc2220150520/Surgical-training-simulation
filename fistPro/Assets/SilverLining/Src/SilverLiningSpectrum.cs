// Copyright (c) 2011 Sundog Software LLC. All rights reserved worldwide.

using UnityEngine;
using System;

public class SilverLiningSpectrum
{
	protected const int NSAMPLES = 81;
	protected const int SAMPLEWIDTH = 5;
	
	public int redshift = 0;
	public double airMassMultiplier = 1.0;
	public double H = 8435.0; // pressure scale height
	public double O3 = 0.35; // ozone mass
	public double W = 2.5; // water vapor mass
	public double rho = 0.0; // ground albedo
	
	protected double[] powers = new double[NSAMPLES];
		
	protected double PI = 3.14159265;
	protected double DEGREES(double x) {return x * (180.0 / PI);}
	protected double RADIANS(double x) {return x * (PI / 180.0);}

	public SilverLiningSpectrum ()
	{
        H *= SilverLining.unitScale;
	}
	
    protected double[,] cie_colour_match = new double[NSAMPLES,3] {
        {0.0014,0.0000,0.0065}, {0.0022,0.0001,0.0105}, {0.0042,0.0001,0.0201},
        {0.0076,0.0002,0.0362}, {0.0143,0.0004,0.0679}, {0.0232,0.0006,0.1102},
        {0.0435,0.0012,0.2074}, {0.0776,0.0022,0.3713}, {0.1344,0.0040,0.6456},
        {0.2148,0.0073,1.0391}, {0.2839,0.0116,1.3856}, {0.3285,0.0168,1.6230},
        {0.3483,0.0230,1.7471}, {0.3481,0.0298,1.7826}, {0.3362,0.0380,1.7721},
        {0.3187,0.0480,1.7441}, {0.2908,0.0600,1.6692}, {0.2511,0.0739,1.5281},
        {0.1954,0.0910,1.2876}, {0.1421,0.1126,1.0419}, {0.0956,0.1390,0.8130},
        {0.0580,0.1693,0.6162}, {0.0320,0.2080,0.4652}, {0.0147,0.2586,0.3533},
        {0.0049,0.3230,0.2720}, {0.0024,0.4073,0.2123}, {0.0093,0.5030,0.1582},
        {0.0291,0.6082,0.1117}, {0.0633,0.7100,0.0782}, {0.1096,0.7932,0.0573},
        {0.1655,0.8620,0.0422}, {0.2257,0.9149,0.0298}, {0.2904,0.9540,0.0203},
        {0.3597,0.9803,0.0134}, {0.4334,0.9950,0.0087}, {0.5121,1.0000,0.0057},
        {0.5945,0.9950,0.0039}, {0.6784,0.9786,0.0027}, {0.7621,0.9520,0.0021},
        {0.8425,0.9154,0.0018}, {0.9163,0.8700,0.0017}, {0.9786,0.8163,0.0014},
        {1.0263,0.7570,0.0011}, {1.0567,0.6949,0.0010}, {1.0622,0.6310,0.0008},
        {1.0456,0.5668,0.0006}, {1.0026,0.5030,0.0003}, {0.9384,0.4412,0.0002},
        {0.8544,0.3810,0.0002}, {0.7514,0.3210,0.0001}, {0.6424,0.2650,0.0000},
        {0.5419,0.2170,0.0000}, {0.4479,0.1750,0.0000}, {0.3608,0.1382,0.0000},
        {0.2835,0.1070,0.0000}, {0.2187,0.0816,0.0000}, {0.1649,0.0610,0.0000},
        {0.1212,0.0446,0.0000}, {0.0874,0.0320,0.0000}, {0.0636,0.0232,0.0000},
        {0.0468,0.0170,0.0000}, {0.0329,0.0119,0.0000}, {0.0227,0.0082,0.0000},
        {0.0158,0.0057,0.0000}, {0.0114,0.0041,0.0000}, {0.0081,0.0029,0.0000},
        {0.0058,0.0021,0.0000}, {0.0041,0.0015,0.0000}, {0.0029,0.0010,0.0000},
        {0.0020,0.0007,0.0000}, {0.0014,0.0005,0.0000}, {0.0010,0.0004,0.0000},
        {0.0007,0.0002,0.0000}, {0.0005,0.0002,0.0000}, {0.0003,0.0001,0.0000},
        {0.0002,0.0001,0.0000}, {0.0002,0.0001,0.0000}, {0.0001,0.0000,0.0000},
        {0.0001,0.0000,0.0000}, {0.0001,0.0000,0.0000}, {0.0000,0.0000,0.0000}
    };
	
	protected double[] Ao = new double[NSAMPLES] {
        0, 0, 0, 0, // 380-395
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.003, // 400-450
        0.004, 0.006, 0.007, 0.009, 0.011, 0.014, 0.017, 0.021, 0.025, 0.03, //455 - 500
        0.035, 0.04, 0.044, 0.048, 0.055, 0.063, 0.071, 0.075, 0.08, 0.085, // 505 - 550
        0.091, 0.12, 0.12, 0.12, 0.12, 0.12, 0.12, 0.12, 0.119, 0.12, //555-600
        0.12, 0.12, 0.10, 0.09, 0.09, 0.085, 0.08, 0.075, 0.07, 0.07, //605-650
        0.065, 0.06, 0.055, 0.05, 0.045, 0.04, 0.035, 0.028, 0.25, 0.023, // 655-700
        0.02, 0.018, 0.016, 0.012, 0.012, 0.012, 0.012, 0.01, 0.01, 0.01, // 705 - 750
        0.008, 0.007, 0.006, 0.005, 0.003, 0
    };

    protected double[] Au = new double[NSAMPLES] {
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //380-500
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 505-550
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 550-600
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 605-650
        0, 0, 0, 0, 0, 0, 0, 0.15, 0, 0, // 655-700
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //705-750
        0, 4.0, 0, 0, 0, 0, //755-780
    };

    protected double[] Aw = new double[NSAMPLES] {
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //380-500
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 505-550
        0, 0, 0, 0, 0, 0, 0, 0.075, 0, 0, // 550-600
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 605-650
        0, 0, 0, 0, 0, 0, 0, 0.016, 0.015, 0.014, // 655-700
        0.013, 0.0125, 1.8, 2.3, 2.5, 2.3, 1.8, 0.061, 0.003, 0.0008, //705-750
        0.0001, 0.00001, 0.00001, 0.0001, 0.0003, 0.0006, //755-780
    };
	
	public Vector3 ToXYZ()
	{
	    Vector3 xyz = new Vector3(0,0,0);
		
	    for (int i = 1; i < NSAMPLES; i++)
	    {
	        int iShift = i + redshift;
	        if (iShift > NSAMPLES - 1) iShift = NSAMPLES - 1;
	        if (iShift < 1) iShift = 1;
	
	        xyz.x += (float)(powers[iShift] * cie_colour_match[i,0]);
	        xyz.y += (float)(powers[iShift] * cie_colour_match[i,1]);
	        xyz.z += (float)(powers[iShift] * cie_colour_match[i,2]);
	    }
	
	    return xyz;
	}
	
	public static SilverLiningSpectrum operator * (SilverLiningSpectrum s1, SilverLiningSpectrum s2)
	{
	    SilverLiningSpectrum sOut = new SilverLiningSpectrum();
	
	    for (int i = 0; i < NSAMPLES; i++)
	    {
	        sOut.powers[i] = s1.powers[i] * s2.powers[i];
	    }
	
	    return sOut;
	}
	
	public void ApplyAtmosphericTransmittance(double zenithAngle, double cosZenith, double T, double alt,
	                                          ref SilverLiningSpectrum directIrradiance, 
	                                          ref SilverLiningSpectrum scatteredIrradiance)
	{
	
	    double beta = 0.04608 * T - 0.04586;
	
	    double zenithDeg = DEGREES(zenithAngle);
	
	    const double modelLimit = 90.0; //93.885
	
	    if (zenithDeg < modelLimit)
	    {
	    	// SPECTRL2 air mass model
	        double m = 1.0 / (cosZenith + 0.50572 * Math.Pow(96.07995 - zenithDeg, -1.6364));	
	        m *= airMassMultiplier;
	
	        // Account for high altitude. As you lose atmosphere, less scattering occurs.
	        H *= SilverLining.unitScale;
	        double isothermalEffect = Math.Exp(-(alt / H));
	        m *= isothermalEffect;
	
	        // ozone mass
	        double Mo = 1.003454 / Math.Sqrt ( cosZenith * cosZenith + 0.006908 );
	
	        const double omega = 0.945; // single scaterring albedo, 0.4 microns
	        const double omegap = 0.095; // Wavelength variation factor
	        const double assym = 0.65; // aerosol assymetry factor
	
	        double alg = Math.Log(1.0 - assym);
	        double afs = alg * (1.459 + alg * (0.1595 + alg * 0.4129));
	        double bfs = alg * (0.0783 + alg * (-0.3824 - alg * 0.5874));
	        double fsp = 1.0 - 0.5 * Math.Exp((afs + bfs / 1.8) / 1.8);
	        double fs = 1.0 - 0.5 * Math.Exp((afs + bfs * cosZenith) * cosZenith);
	
	        for (int i = 0; i < NSAMPLES; i++)
	        {
	            double um = 0.380 + (i * 0.005);
	
	            // Rayleigh scattering
	            double Tr = Math.Exp(-m / (Math.Pow(um, 4.0) * (115.6406 - (1.3366 / (um * um)))));
	
	            // Aerosols
	            double a;
				if (um < 0.5) {
					a = 1.0274;
				} else {
					a = 1.2060;
				}
				
	            double c1 = beta * Math.Pow(2.0 * um, -a);
	            double Ta = Math.Exp(-c1 * m);
	
	            // Water vapor
	            double aWM = Aw[i] * W * m;
	            double Tw = Math.Exp(-0.2385 * aWM / Math.Pow(1.0 + 20.07 * aWM, 0.45));
	
	            // Ozone
	            double To = Math.Exp(-Ao[i] * O3 * Mo);
	
	            // Mixed gas is only important in infrared
	            double Tm = Math.Exp((-1.41 * Au[i] * m) / Math.Pow(1.0 + 118.3 * Au[i] * m, 0.45));
	
	            // Aerosol scattering
	            double logUmOver4 = Math.Log(um / 0.4);
	            double omegl = omega * Math.Exp (-omegap * logUmOver4 * logUmOver4);
	            double Tas = Math.Exp(-omegl * c1 * m);
	
	            // Aerosol absorptance
	            double Taa = Math.Exp((omegl - 1.0) * c1 * m);
	
	            // Primed Rayleigh scattering (m = 1.8)
	            double Trp = Math.Exp(-1.8 / (um * um * um * um) * (115.6406 - 1.3366 / (um * um)));
	
	            // Primed water vapor scattering
	            double Twp = Math.Exp(-0.4293 * Aw[i] * W / Math.Pow((1.0 + 36.126 * Aw[i] * W), 0.45));
	
	            // Mixed gas
	            double Tup = Math.Exp(-2.538 * Au[i] / Math.Pow((1.0 + 212.94 * Au[i]), 0.45));
	
	            // Primed aerosol scattering
	            double Tasp = Math.Exp(-omegl * c1 * 1.8);
	
	            // Primed aerosol absorptance
	            double Taap = Math.Exp((omegl - 1.0) * c1 * 1.8);
	
	            // Direct energy
	            double xmit = Tr * Ta * Tw * To * Tm;
	
	            directIrradiance.powers[i] = powers[i] * xmit;
	
	            // diffuse energy
	            double c2 = powers[i] * To * Tw * Tm * cosZenith * Taa;
	            double c4 = 1.0;
	            if (um <= 0.45) {
	                c4 = Math.Pow( (um + 0.55), 1.8);
	            }
	
	            double rhoa = Twp * Tup * Taap * (0.5 * (1.0 - Trp) + (1.0 - fsp) * Trp * (1.0 - Tasp) );
	            double dray = c2 * (1.0 - Math.Pow(Tr, 0.95)) / 2.0;
	            double daer = c2 * Math.Pow(Tr, 1.5) * (1.0 - Tas) * fs;
	            double drgd = ( directIrradiance.powers[i] * cosZenith + dray + daer) * rho * rhoa / (1.0 - rho * rhoa);
	
	            scatteredIrradiance.powers[i] = (dray + daer + drgd) * c4;
	
	            if (scatteredIrradiance.powers[i] < 0)
	                scatteredIrradiance.powers[i] = 0;
	        }
	    }
	    else
	    {
	        for (int i = 0; i < NSAMPLES; i++)
	        {
	            directIrradiance.powers[i] = 0;
	            scatteredIrradiance.powers[i] = 0;
	        }
	    }
	}

}


