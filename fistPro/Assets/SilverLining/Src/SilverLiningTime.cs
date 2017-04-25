// Copyright (c) 2011 Sundog Software LLC. All rights reserved worldwide.

using System;

public class SilverLiningTime
{
    public SilverLiningTime ()
    {
        localYear = 2011;
        localMonth = 9;
        localDay = 17;
        observingDST = true;
        localHours = 15;
        localMinutes = 50;
        localSeconds = 0;
        zoneCorrection = -8.0;
    }

    public SilverLiningTime (SilverLiningTime t)
    {
        localYear = t.GetYear ();
        localMonth = t.GetMonth ();
        localDay = t.GetDay ();
        observingDST = t.GetDST ();
        localHours = t.GetHour ();
        localMinutes = t.GetMinute ();
        localSeconds = t.GetSeconds ();
        zoneCorrection = t.GetTimeZone ();
    }
	
	public void AddSeconds(float numSeconds)
	{
		int intSeconds = (int)localSeconds;
		int milliseconds = (int)((localSeconds - intSeconds) * 1000.0f);
		System.DateTime dt = new System.DateTime(localYear, localMonth, localDay, localHours, localMinutes, intSeconds, milliseconds);
		
		System.DateTime newTime = dt.AddSeconds (numSeconds);
	
		
		localYear = newTime.Year;
		localMonth = newTime.Month;
		localDay = newTime.Day;
		localHours = newTime.Hour;
		localMinutes = newTime.Minute;
		localSeconds = (double)newTime.Second + (newTime.Millisecond * 0.001);
	}
	
    public override int GetHashCode ()
    {
        return localYear ^ localMonth ^ localDay ^ (observingDST ? 1 : 0) ^ localHours ^ localMinutes ^ (int)localSeconds ^ (int)zoneCorrection;
    }

    public override bool Equals (Object t)
    {
        return (this == t);
    }

    public static bool operator == (SilverLiningTime t1, SilverLiningTime t2)
    {
        return (t1.GetYear () == t2.GetYear () && t1.GetMonth () == t2.GetMonth () && t1.GetDay () == t2.GetDay () && t1.GetDST () == t2.GetDST () && t1.GetHour () == t2.GetHour () && t1.GetMinute () == t2.GetMinute () && t1.GetSeconds () == t2.GetSeconds () && t1.GetTimeZone () == t2.GetTimeZone ());
    }

    public static bool operator != (SilverLiningTime t1, SilverLiningTime t2)
    {
        return (t1.GetYear () != t2.GetYear () || t1.GetMonth () != t2.GetMonth () || t1.GetDay () != t2.GetDay () || t1.GetDay () != t2.GetDay () || t1.GetHour () != t2.GetHour () || t1.GetMinute () != t2.GetMinute () || t1.GetSeconds () != t2.GetSeconds () || t1.GetTimeZone () != t2.GetTimeZone ());
    }

    public bool SetDate (int year, int month, int day)
    {
        if (month > 0 && month <= 12 && day > 0 && day <= 31) {
            localYear = year;
            localMonth = month;
            localDay = day;
            return true;
        }
        return false;
    }

    public bool SetTime (int hour, int minutes, double seconds, double timeZone, bool dst)
    {
        if (hour >= 0 && hour < 24 && minutes >= 0 && minutes < 60 && seconds >= 0.0 && seconds < 60.0) {
            localHours = hour;
            localMinutes = minutes;
            localSeconds = seconds;
            zoneCorrection = timeZone;
            observingDST = dst;
            return true;
        }
        
        return false;
    }

    public double GetJulianDate (bool terrestrialTime)
    {
        // Convert to GMT
        double hours = localHours + (localMinutes + localSeconds / 60.0) / 60.0;
        
        if (observingDST)
            hours -= 1;
        
        hours -= zoneCorrection;
        
        double y, m;
        double d;
        d = localDay + (hours / 24.0);
        
        if (localMonth < 3) {
            y = localYear - 1;
            m = localMonth + 12;
        } else {
            y = localYear;
            m = localMonth;
        }
        
        double JD = 1720996.5 - Math.Floor (y / 100) + Math.Floor (y / 400) + (365.0 * y) + Math.Floor (30.6001 * (m + 1)) + d;
        
        if (terrestrialTime) {
            JD += 65.0 / 60.0 / 60.0 / 24.0;
        }
        
        return (double)JD;
    }

    public double GetEpoch2000Centuries (bool terrestrialTime)
    {
        // Convert to GMT
        double hours = localHours + (localMinutes + localSeconds / 60.0) / 60.0;
        
        if (observingDST)
            hours -= 1;
        
        hours -= zoneCorrection;
        
        double y, m;
        double d, mantissa;
        d = localDay + (hours / 24.0);
        
        if (localMonth < 3) {
            y = localYear - 1;
            m = localMonth + 12;
        } else {
            y = localYear;
            m = localMonth;
        }
        
        mantissa = 1720996.5 - Math.Floor (y / 100.0) + Math.Floor (y / 400.0) + Math.Floor (365.25 * y) + Math.Floor (30.6001 * (m + 1));
        mantissa -= 2451545.0;
        
        double JD = mantissa + d;
        
        if (terrestrialTime) {
            JD += 65.0 / 60.0 / 60.0 / 24.0;
        }
        
        JD /= 36525.0;
        
        return JD;
    }

    public double GetEpoch1990Days (bool terrestrialTime)
    {
        // Convert to GMT
        double hours = localHours + (localMinutes + localSeconds / 60.0) / 60.0;
        
        if (observingDST)
            hours -= 1;
        
        hours -= zoneCorrection;
        
        double y, m;
        double mantissa;
        double d;
        d = localDay + (hours / 24.0);
        
        if (localMonth < 3) {
            y = localYear - 1;
            m = localMonth + 12;
        } else {
            y = localYear;
            m = localMonth;
        }
        
        mantissa = 1720996.5 - Math.Floor (y / 100.0) + Math.Floor (y / 400.0) + Math.Floor (365.25 * y) + Math.Floor (30.6001 * (m + 1));
        
        mantissa -= 2447891.5;
        
        double JD = mantissa + d;
        
        if (terrestrialTime) {
            JD += 65.0 / 60.0 / 60.0 / 24.0;
        }
        
        return JD;
    }

    public int GetYear ()
    {
        return localYear;
    }
    public int GetMonth ()
    {
        return localMonth;
    }
    public int GetDay ()
    {
        return localDay;
    }
    public int GetHour ()
    {
        return localHours;
    }
    public int GetMinute ()
    {
        return localMinutes;
    }
    public double GetTimeZone ()
    {
        return zoneCorrection;
    }
    public double GetSeconds ()
    {
        return localSeconds;
    }
    public bool GetDST ()
    {
        return observingDST;
    }

    private int localYear, localMonth, localDay, localHours, localMinutes;
    private double zoneCorrection, localSeconds;
    private bool observingDST;
}

