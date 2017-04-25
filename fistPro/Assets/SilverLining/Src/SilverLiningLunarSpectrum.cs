// Copyright (c) 2011 Sundog Software LLC. All rights reserved worldwide.

using System;

public class SilverLiningLunarSpectrum : SilverLiningSpectrum
{
    public SilverLiningLunarSpectrum ()
    {
        int i;
        
        double minLuminance = 700;
        double maxLuminance = 1350;
        
        for (i = 0; i < NSAMPLES; i++) {
            double a = (double)i / (double)NSAMPLES;
            powers[i] = minLuminance * (1.0 - a) + maxLuminance * a;
        }
    }
}


