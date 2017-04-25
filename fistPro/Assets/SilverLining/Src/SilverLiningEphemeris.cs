// Copyright (c) 2011 Sundog Software LLC. All rights reserved worldwide.

using UnityEngine;
using System;

public class SilverLiningEphemeris
{
    private double PI = 3.14159265;
    private double PLANET_DELTA_DAYS = (1.0 / 24.0);
    protected double DEGREES (double x)
    {
        return x * (180.0 / PI);
    }
    protected double RADIANS (double x)
    {
        return x * (PI / 180.0);
    }

    enum Planets
    {
        MERCURY = 0,
        VENUS,
        EARTH,
        MARS,
        JUPITER,
        SATURN,
        NUM_PLANETS
    }

    protected struct Planet
    {
        public double lastEpochDaysCalculated;
        public double rightAscension;
        public double declination;
        public double visualMagnitude;
    }

    protected struct OrbitalElements
    {
        public OrbitalElements (double pPeriod, double pEpochLongitude, double pPerihelionLongitude, double pEccentricity, double pSemiMajorAxis, double pInclination, double pLongitudeAscendingNode, double pAngularDiameter, double pVisualMagnitude)
        {
            period = pPeriod;
            epochLongitude = pEpochLongitude;
            perihelionLongitude = pPerihelionLongitude;
            eccentricity = pEccentricity;
            semiMajorAxis = pSemiMajorAxis;
            inclination = pInclination;
            longitudeAscendingNode = pLongitudeAscendingNode;
            angularDiameter = pAngularDiameter;
            visualMagnitude = pVisualMagnitude;
        }

        public double period;
        public double epochLongitude;
        public double perihelionLongitude;
        public double eccentricity;
        public double semiMajorAxis;
        public double inclination;
        public double longitudeAscendingNode;
        public double angularDiameter;
        public double visualMagnitude;
    }

    public SilverLiningEphemeris ()
    {
        equatorialToHorizon = new SilverLiningMatrix3 ();
        eclipticToHorizon = new SilverLiningMatrix3 ();
        eclipticToEquatorial = new SilverLiningMatrix3 ();
        equatorialToGeographic = new SilverLiningMatrix3 ();
        horizonToEquatorial = new SilverLiningMatrix3 ();
        horizonToGeographic = new SilverLiningMatrix3 ();
        geographicToEquatorial = new SilverLiningMatrix3 ();
        geographicToHorizon = new SilverLiningMatrix3 ();
        precession = new SilverLiningMatrix3 ();
        moonEq = new Vector3 ();
        sunEq = new Vector3 ();
        moonEcl = new Vector3 ();
        sunEcl = new Vector3 ();
        sunHoriz = new Vector3 ();
        moonHoriz = new Vector3 ();
        moonGeo = new Vector3 ();
        sunGeo = new Vector3 ();
        
        geoZUp = true;
        lastLocation = new SilverLiningLocation ();
        lastTime = new SilverLiningTime ();
        
        planets = new Planet[(int)Planets.NUM_PLANETS];
        planetElements = new OrbitalElements[(int)Planets.NUM_PLANETS];
        
        // Mercury
        planetElements[0] = new OrbitalElements (0.240852, RADIANS (60.750646), RADIANS (77.299833), 0.205633, 0.387099, RADIANS (7.004540), RADIANS (48.212740), 6.74, -0.42);
        
        // Venus
        planetElements[1] = new OrbitalElements (0.615211, RADIANS (88.455855), RADIANS (131.430236), 0.006778, 0.723332, RADIANS (3.394535), RADIANS (76.589820), 16.92, -4.40);
        
        // Earth
        planetElements[2] = new OrbitalElements (1.00004, RADIANS (99.403308), RADIANS (102.768413), 0.016713, 1.00000, 0, 0, 0, 0);
        
        // Mars
        planetElements[3] = new OrbitalElements (1.880932, RADIANS (240.739474), RADIANS (335.874939), 0.093396, 1.523688, RADIANS (1.849736), RADIANS (49.480308), 9.36, -1.52);
        
        // Jupiter
        planetElements[4] = new OrbitalElements (11.863075, RADIANS (90.638185), RADIANS (14.170747), 0.048482, 5.202561, RADIANS (1.303613), RADIANS (100.353142), 196.74, -9.40);
        
        // Saturn
        planetElements[5] = new OrbitalElements (29.471362, RADIANS (287.690033), RADIANS (92.861407), 0.055581, 9.554747, RADIANS (2.488980), RADIANS (113.576139), 165.60, -8.88);
        
    }

    public void Update (SilverLiningTime time, SilverLiningLocation location)
    {
        bool timeChanged = false;
        bool locationChanged = false;
        
        if (time != lastTime) {
            timeChanged = true;
            lastTime = new SilverLiningTime (time);
        }
        
        if (location != lastLocation) {
            locationChanged = true;
            lastLocation = new SilverLiningLocation (location);
        }
        
        if (timeChanged || locationChanged) {
            T = time.GetEpoch2000Centuries (true);
            Tuncorr = time.GetEpoch2000Centuries (false);
            epochDays = time.GetEpoch1990Days (false);
            
            SilverLiningMatrix3 Rx, Ry, Rz;
            Rx = new SilverLiningMatrix3 ();
            Ry = new SilverLiningMatrix3 ();
            Rz = new SilverLiningMatrix3 ();
            
            Rx.FromRx (-0.1118 * T);
            Ry.FromRy (0.00972 * T);
            Rz.FromRz (-0.01118 * T);
            precession = Rz * (Ry * Rx);
            
            GMST = 4.894961 + 230121.675315 * Tuncorr;
            // radians
            LMST = GMST + RADIANS (location.GetLongitude ());
            // radians
            double latitude = RADIANS (location.GetLatitude ());
            
            e = 0.409093 - 0.000227 * T;
            
            Ry.FromRy (-(latitude - PI / 2.0));
            Rz.FromRz (LMST);
            Rx.FromRx (-e);
            equatorialToHorizon = Ry * Rz * precession;
            eclipticToHorizon = Ry * Rz * Rx * precession;
            eclipticToEquatorial = Rx;
            
            equatorialToGeographic.FromRz (GMST);
            geographicToEquatorial = equatorialToGeographic.Transpose ();
            
            horizonToEquatorial = equatorialToHorizon.Transpose ();
            horizonToGeographic = equatorialToGeographic * horizonToEquatorial;
            geographicToHorizon = equatorialToHorizon * geographicToEquatorial;
            
            ComputeSunPosition ();
            ComputeMoonPosition ();
            ComputeEarthPosition ();
			ComputeMoonPositionAngle ();
            
            for (int i = 0; i < (int)Planets.NUM_PLANETS; i++) {
                if (i != (int)Planets.EARTH) {
                    ComputePlanetPosition (i);
                }
            }
        }
    }

    /** Returns the position of the sun in equatorial coordinates. Requires that
   Update() was previously called. */
    public Vector3 GetSunPositionEquatorial ()
    {
        return sunEq;
    }

    /** Returns the position of the moon in equatorial coordinates. Requires that
   Update() was previously called. */
    public Vector3 GetMoonPositionEquatorial ()
    {
        return moonEq;
    }

    /** Returns the position of the sun in geographic coordinates. Requires that
   Update() was previously called. */
    public Vector3 GetSunPositionGeographic ()
    {
        return sunGeo;
    }

    /** Returns the position of the moon in geographic coordinates. Requires that
   Update() was previously called. */
    public Vector3 GetMoonPositionGeographic ()
    {
        return moonGeo;
    }


    /** Returns the position of the sun in ecliptic coordinates. Requires that
   Update() was previously called. */
    public Vector3 GetSunPositionEcliptic ()
    {
        return sunEcl;
    }

    /** Returns the position of the moon in ecliptic coordinates. Requires that
   Update() was previously called. */
    public Vector3 GetMoonPositionEcliptic ()
    {
        return moonEcl;
    }

    /** Returns the position of the sun in horizon coordinates. Requires that
   Update() was previously called. */
    public Vector3 GetSunPositionHorizon ()
    {
        return sunHoriz;
    }

    /** Returns the position of the moon in horizon coordiantes. Requires that
   Update() was previously called. */
    public Vector3 GetMoonPositionHorizon ()
    {
        return moonHoriz;
    }

    /** Retrieves the "phase angle" of the moon. A full moon has a phase angle of 0
   degrees; a new moon has a phase angle of 180 degrees. Requires that Update()
   was previously called. */
    public double GetMoonPhaseAngle ()
    {
        return moonPhaseAngle;
    }

    /** Retrieves the phase of the moon in terms of percentage of the moon that is
   illuminated. Ranges from 0 (new moon) to 1.0 (full moon.) Requires that Update()
   was previously called. */
    public double GetMoonPhase ()
    {
        return moonPhase;
    }

    /** Retrieves the distance between the Earth and the Moon in kilometers. Useful
   for determining the brightness of the moon, in conjunction with the moon's phase.
   Requires that Update() was previously called. */
    public double GetMoonDistanceKM ()
    {
        return moonDistance;
    }

    /** Retrieves a 3x3 matrix that will transform ecliptic coordinates to horizon coordinates.
   Requires that Update() was previously called. */
    public SilverLiningMatrix3 GetEclipticToHorizonMatrix ()
    {
        return eclipticToHorizon;
    }

    /** Retrieves a 3x3 matrix that will transform equatorial coordinates (x through
   the vernal equinox) to geographic coordinates (x through the prime meridian.) */
    public SilverLiningMatrix3 GetEquatorialToGeographicMatrix ()
    {
        return equatorialToGeographic;
    }

    /** Retrieves a 3x3 matrix that will transform equatorial coordinates to horizon
   coordinates. Requires that Update() was previously called. */
    public SilverLiningMatrix3 GetEquatorialToHorizonMatrix ()
    {
        return equatorialToHorizon;
    }

    /** Retrieves a 3x3 matrix to transform horizon coordinates to equatorial
   coordinates. Requires that Update() was previously called. */
    public SilverLiningMatrix3 GetHorizonToEquatorialMatrix ()
    {
        return horizonToEquatorial;
    }

    /** Retrieves a 3x3 matrix to transform horizon coordinates to geocentric
   coordinates. Requires that Update() was previously called. */
    public SilverLiningMatrix3 GetHorizonToGeographicMatrix ()
    {
        return horizonToGeographic;
    }

    /** Retrieves a 3x3 matrix to transform geographic coordinates to horizon
   coordinates. Requires that Update() was previously called. */
    public SilverLiningMatrix3 GetGeographicToHorizonMatrix ()
    {
        return geographicToHorizon;
    }

    /** Returns the fractional number of centuries elapsed since January 1, 2000 GMT,
   terrestrial time (this is "atomic clock time," which does not account for leap seconds
   to correct for slowing of the Earth's rotation). */
    public double GetEpochCenturies ()
    {
        return T;
    }
	
	public double GetMoonRotation()
	{
		return moonRotationRadians;
	}
	
	private void ComputeMoonPositionAngle()
	{
		Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);
		
		Vector3 sunHorizNorm = sunHoriz;
		Vector3 moonHorizNorm = moonHoriz;
		sunHorizNorm.Normalize ();
		moonHorizNorm.Normalize();
		Vector3 toSun = sunHorizNorm - moonHorizNorm;
		toSun.Normalize();
		
		// Is the moon "left" or "right" of the sun?
		Vector3 cross = Vector3.Cross (sunHorizNorm, moonHorizNorm);
		bool left = cross.y < 0.0f;
		
		bool waxing = moonPhaseAngle < Math.PI;
		
		double ang = Math.Asin (Vector3.Dot (up, toSun));
		
		if (left) 
		{
			if (waxing) {
				// left waxing
				// Do nothing
			} else {
				// left waning
				ang = -(Math.PI - ang);
			}
		}
		else
		{
			if (waxing) {
				// right waxing
				ang = Math.PI - ang;
			} else {
				// right waning
				ang = -ang;
			}
		}
		
		moonRotationRadians = -ang;
		
	}

    private Vector3 ToCartesian (double r, double latitude, double longitude)
    {
        Vector3 v;
        v.x = (float)(r * Math.Cos (longitude) * Math.Cos (latitude));
        // n
        v.y = (float)(r * Math.Sin (longitude) * Math.Cos (latitude));
        // e
        v.z = (float)(r * Math.Sin (latitude));
        // up
        return v;
    }

    private double Refract (double elevation)
    {
        /*
         *    Refraction correction, degrees
         *        Zimmerman, John C.  1981.  Sun-pointing programs and their
         *            accuracy.
         *            SAND81-0761, Experimental Systems Operation Division 4721,
         *            Sandia National Laboratories, Albuquerque, NM.
         */        
        
        // temporary refraction correction
        double refcor;

        // tangent of the solar elevation angle
        double tanelev;

        /* If the sun is near zenith, the algorithm bombs; refraction near 0 */

        if (elevation > RADIANS (85.0)) {
            refcor = 0.0;
        } else {
            tanelev = Math.Tan (elevation);
            if (elevation >= RADIANS (5.0)) {
                refcor = 58.1 / tanelev - 0.07 / (Math.Pow (tanelev, 3)) + 0.000086 / (Math.Pow (tanelev, 5));
            } else if (elevation >= RADIANS (-0.575)) {
                double degElev = DEGREES (elevation);
                refcor = 1735.0 + degElev * (-518.2 + degElev * (103.4 + degElev * (-12.79 + degElev * 0.711)));
            } else {
                refcor = -20.774 / tanelev;
            }
            //prestemp    =
            //    ( pdat->press * 283.0 ) / ( 1013.0 * ( 273.0 + pdat->temp ) );
            //refcor     *= prestemp / 3600.0;
            refcor /= 3600.0;
        }

        // Refracted solar elevation angle
        double elev = elevation + RADIANS (refcor);

        return elev;
    }

    private Vector3 ConvertAxes (Vector3 v)
    {
        // Oriented -x=n z=up y=east
        Vector3 tmp = new Vector3 ();
        tmp.x = v.y;
        // x is east
        tmp.y = v.z;
        // y is up
        tmp.z = -v.x;
        // -z is north
        return tmp;
    }

    private void ComputeSunPosition ()
    {
        double M = 6.24 + 628.302 * T;

        double longitude = 4.895048 + 628.331951 * T + (0.033417 - 0.000084 * T) * Math.Sin (M) + 0.000351 * Math.Sin (2.0 * M);
        double latitude = 0;
        double geocentricDistance = 1.000140 - (0.016708 - 0.000042 * T) * Math.Cos (M) - 0.000141 * Math.Cos (2.0 * M);
        // AU's
        sunEclipticLongitude = longitude;
        sunEcl = ToCartesian (geocentricDistance, latitude, longitude);

        sunEq = eclipticToEquatorial * sunEcl;

        if (geoZUp) {
            sunGeo = equatorialToGeographic * sunEq;
        } else {
            sunGeo = ConvertAxes (equatorialToGeographic * sunEq);
        }

        sunHoriz = ConvertAxes (eclipticToHorizon * sunEcl);

        // Account for atmospheric refraction.
        Vector3 tmp2 = sunHoriz;
        double R = tmp2.magnitude;
        tmp2.Normalize ();
        double elev = Math.Asin (tmp2.y);
        elev = Refract (elev);
        sunHoriz.y = (float)(R * Math.Sin (elev));

    }

    private void InRange (ref double d)
    {
        while (d > 2.0 * PI)
            d -= 2.0 * PI;
        while (d < 0)
            d += 2.0 * PI;
    }

    private void InRangef (ref float d)
    {
        while (d > 2.0f * (float)PI)
            d -= 2.0f * (float)PI;
        while (d < 0.0f)
            d += 2.0f * (float)PI;
    }

    private void ComputeMoonPosition ()
    {
        double lp = 3.8104 + 8399.7091 * T;
        double m = 6.2300 + 628.3019 * T;
        double f = 1.6280 + 8433.4663 * T;
        double mp = 2.3554 + 8328.6911 * T;
        double d = 5.1985 + 7771.3772 * T;

        double longitude = lp + 0.1098 * Math.Sin (mp) + 0.0222 * Math.Sin (2 * d - mp) + 0.0115 * Math.Sin (2 * d) + 0.0037 * Math.Sin (2 * mp) - 0.0032 * Math.Sin (m) - 0.0020 * Math.Sin (2 * f) + 0.0010 * Math.Sin (2 * d - 2 * mp) + 0.0010 * Math.Sin (2 * d - m - mp) + 0.0009 * Math.Sin (2 * d + mp) + 0.0008 * Math.Sin (2 * d - m) + 0.0007 * Math.Sin (mp - m) - 0.0006 * Math.Sin (d) - 0.0005 * Math.Sin (m + mp);

        double latitude = +0.0895 * Math.Sin (f) + 0.0049 * Math.Sin (mp + f) + 0.0048 * Math.Sin (mp - f) + 0.0030 * Math.Sin (2 * d - f) + 0.0010 * Math.Sin (2 * d + f - mp) + 0.0008 * Math.Sin (2 * d - f - mp) + 0.0006 * Math.Sin (2 * d + f);

        double pip = +0.016593 + 0.000904 * Math.Cos (mp) + 0.000166 * Math.Cos (2 * d - mp) + 0.000137 * Math.Cos (2 * d) + 0.000049 * Math.Cos (2 * mp) + 0.000015 * Math.Cos (2 * d + mp) + 0.000009 * Math.Cos (2 * d - m);

        double dMoon = 1.0 / pip;
        // earth radii
        moonEcl = ToCartesian (dMoon, latitude, longitude);

        moonEq = eclipticToEquatorial * moonEcl;

        if (geoZUp) {
            moonGeo = equatorialToGeographic * moonEq;
        } else {
            moonGeo = ConvertAxes (equatorialToGeographic * moonEq);
        }

        moonHoriz = ConvertAxes (eclipticToHorizon * moonEcl);

        InRange (ref longitude);
        InRange (ref sunEclipticLongitude);
        moonPhaseAngle = longitude - sunEclipticLongitude;
        InRange (ref moonPhaseAngle);
		
		//if (moonPhaseAngle > Math.PI) moonRotationRadians += Math.PI;

        moonPhase = 0.5 * (1.0 - Math.Cos (moonPhaseAngle));

        Vector3 up = new Vector3 (0, 0, 1);
        moonDistance = (moonHoriz - up).magnitude * 6378.137;
    }

    private void ComputeEarthPosition ()
    {
        double Np = ((2.0 * PI) / 365.242191) * (epochDays / planetElements[(int)Planets.EARTH].period);
        InRange (ref Np);

        double Mp = Np + planetElements[(int)Planets.EARTH].epochLongitude - planetElements[(int)Planets.EARTH].perihelionLongitude;

        L = Np + 2.0 * planetElements[(int)Planets.EARTH].eccentricity * Math.Sin (Mp) + planetElements[(int)Planets.EARTH].epochLongitude;

        InRange (ref L);

        double vp = L - planetElements[(int)Planets.EARTH].perihelionLongitude;

        R = (planetElements[(int)Planets.EARTH].semiMajorAxis * (1.0 - planetElements[(int)Planets.EARTH].eccentricity * planetElements[(int)Planets.EARTH].eccentricity)) / (1.0 + planetElements[(int)Planets.EARTH].eccentricity * Math.Cos (vp));
    }

    private void GetPlanetPosition (int planet, ref double ra, ref double dec, ref double visualMagnitude)
    {
        if (planet < (int)Planets.NUM_PLANETS) {
            ra = planets[planet].rightAscension;
            dec = planets[planet].declination;
            visualMagnitude = planets[planet].visualMagnitude;
        }
    }

    private void ComputePlanetPosition (int planet)
    {
        if ((epochDays - planets[planet].lastEpochDaysCalculated) < PLANET_DELTA_DAYS) {
            return;
        } else {
            planets[planet].lastEpochDaysCalculated = epochDays;
        }

        double Np = ((2.0 * PI) / 365.242191) * (epochDays / planetElements[planet].period);
        InRange (ref Np);

        double Mp = Np + planetElements[planet].epochLongitude - planetElements[planet].perihelionLongitude;

        double heliocentricLongitude = Np + 2.0 * planetElements[planet].eccentricity * Math.Sin (Mp) + planetElements[planet].epochLongitude;

        InRange (ref heliocentricLongitude);

        double vp = heliocentricLongitude - planetElements[planet].perihelionLongitude;

        double r = (planetElements[planet].semiMajorAxis * (1.0 - planetElements[planet].eccentricity * planetElements[planet].eccentricity)) / (1.0 + planetElements[planet].eccentricity * Math.Cos (vp));

        double heliocentricLatitude = Math.Asin (Math.Sin (heliocentricLongitude - planetElements[planet].longitudeAscendingNode) * Math.Sin (planetElements[planet].inclination));

        InRange (ref heliocentricLatitude);

        double y = Math.Sin (heliocentricLongitude - planetElements[planet].longitudeAscendingNode) * Math.Cos (planetElements[planet].inclination);

        double x = Math.Cos (heliocentricLongitude - planetElements[planet].longitudeAscendingNode);

        double projectedHeliocentricLongitude = Math.Atan2 (y, x) + planetElements[planet].longitudeAscendingNode;

        double projectedRadius = r * Math.Cos (heliocentricLatitude);

        double eclipticLongitude;

        if (planet > (int)Planets.EARTH) {
            eclipticLongitude = Math.Atan ((R * Math.Sin (projectedHeliocentricLongitude - L)) / (projectedRadius - R * Math.Cos (projectedHeliocentricLongitude - L))) + projectedHeliocentricLongitude;
        } else {
            eclipticLongitude = PI + L + Math.Atan ((projectedRadius * Math.Sin (L - projectedHeliocentricLongitude)) / (R - projectedRadius * Math.Cos (L - projectedHeliocentricLongitude)));
        }

        InRange (ref eclipticLongitude);

        double eclipticLatitude = Math.Atan ((projectedRadius * Math.Tan (heliocentricLatitude) * Math.Sin (eclipticLongitude - projectedHeliocentricLongitude)) / (R * Math.Sin (projectedHeliocentricLongitude - L)));

        double ra = Math.Atan2 ((Math.Sin (eclipticLongitude) * Math.Cos (e) - Math.Tan (eclipticLatitude) * Math.Sin (e)), Math.Cos (eclipticLongitude));

        double dec = Math.Asin (Math.Sin (eclipticLatitude) * Math.Cos (e) + Math.Cos (eclipticLatitude) * Math.Sin (e) * Math.Sin (eclipticLongitude));

        double dist2 = R * R + r * r - 2 * R * r * Math.Cos (heliocentricLongitude - L);
        double dist = Math.Sqrt (dist2);

        double d = eclipticLongitude - heliocentricLongitude;
        double phase = 0.5 * (1.0 + Math.Cos (d));

        double visualMagnitude;

        if (planet == (int)Planets.VENUS) {
            d = DEGREES (d);
            visualMagnitude = -4.34 + 5.0 * Math.Log10 (r * dist) + 0.013 * d + 4.2E-7 * d * d * d;
        } else {
            visualMagnitude = 5.0 * Math.Log10 ((r * dist) / Math.Sqrt (phase)) + planetElements[planet].visualMagnitude;
        }

        planets[planet].rightAscension = ra;
        planets[planet].declination = dec;
        planets[planet].visualMagnitude = visualMagnitude;
    }

    private SilverLiningMatrix3 equatorialToHorizon, eclipticToHorizon, eclipticToEquatorial, equatorialToGeographic, horizonToEquatorial, horizonToGeographic, geographicToEquatorial, geographicToHorizon, precession;

    private double sunEclipticLongitude, moonPhase, moonPhaseAngle, moonDistance;
    private double GMST, LMST;

    private double T, Tuncorr, epochDays;
    private Vector3 moonEq, sunEq, moonEcl, sunEcl, sunHoriz, moonHoriz, moonGeo, sunGeo;
	private double moonRotationRadians;

    private bool geoZUp;

    private double R, L, e;

    private SilverLiningTime lastTime;
    private SilverLiningLocation lastLocation;
    private Planet[] planets;
    private OrbitalElements[] planetElements;
}


