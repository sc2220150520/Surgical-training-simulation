// Copyright (c) 2011 Sundog Software LLC. All rights reserved worldwide.

using System;

public class SilverLiningLocation
{
    public SilverLiningLocation ()
    {
    }

    public SilverLiningLocation (SilverLiningLocation l)
    {
        latitude = l.GetLatitude ();
        longitude = l.GetLongitude ();
        altitude = l.GetAltitude ();
    }

    public void SetLatitude (double l)
    {
        latitude = l;
    }
    public void SetLongitude (double l)
    {
        longitude = l;
    }
    public void SetAltitude (double a)
    {
        altitude = a;
    }

    public double GetLatitude ()
    {
        return latitude;
    }
    public double GetLongitude ()
    {
        return longitude;
    }
    public double GetAltitude ()
    {
        return altitude;
    }

    public override int GetHashCode ()
    {
        return (int)(latitude * 1000) ^ (int)(longitude * 1000) ^ (int)(altitude * 1000);
    }

    public override bool Equals (Object t)
    {
        return (this == t);
    }

    public static bool operator == (SilverLiningLocation l1, SilverLiningLocation l2)
    {
        return (l1.GetLatitude () == l2.GetLatitude () && l1.GetLongitude () == l2.GetLongitude () && l1.GetAltitude () == l2.GetAltitude ());
    }

    public static bool operator != (SilverLiningLocation l1, SilverLiningLocation l2)
    {
        return (l1.GetLatitude () != l2.GetLatitude () || l1.GetLongitude () != l2.GetLongitude () || l1.GetAltitude () != l2.GetAltitude ());
    }

    private double latitude, longitude, altitude;
}


