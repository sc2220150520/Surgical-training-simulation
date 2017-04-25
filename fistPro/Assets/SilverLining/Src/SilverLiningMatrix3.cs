// Copyright (c) 2011 Sundog Software LLC. All rights reserved worldwide.

using UnityEngine;
using System;

public class SilverLiningMatrix3
{
    public SilverLiningMatrix3 ()
    {
        elem = new double[3, 3];
    }

    public SilverLiningMatrix3 (double e11, double e12, double e13, double e21, double e22, double e23, double e31, double e32, double e33)
    {
        elem = new double[3, 3];
        elem[0, 0] = e11;
        elem[0, 1] = e12;
        elem[0, 2] = e13;
        elem[1, 0] = e21;
        elem[1, 1] = e22;
        elem[1, 2] = e23;
        elem[2, 0] = e31;
        elem[2, 1] = e32;
        elem[2, 2] = e33;
    }

    public void FromRx (double rad)
    {
        double sinr = Math.Sin (rad);
        double cosr = Math.Cos (rad);
        
        elem[0, 0] = 1.0;
        elem[0, 1] = 0;
        elem[0, 2] = 0;
        elem[1, 0] = 0;
        elem[1, 1] = cosr;
        elem[1, 2] = sinr;
        elem[2, 0] = 0;
        elem[2, 1] = -sinr;
        elem[2, 2] = cosr;
    }

    public void FromRy (double rad)
    {
        double sinr = Math.Sin (rad);
        double cosr = Math.Cos (rad);
        
        elem[0, 0] = cosr;
        elem[0, 1] = 0;
        elem[0, 2] = -sinr;
        elem[1, 0] = 0;
        elem[1, 1] = 1.0;
        elem[1, 2] = 0;
        elem[2, 0] = sinr;
        elem[2, 1] = 0;
        elem[2, 2] = cosr;
    }

    public void FromRz (double rad)
    {
        double sinr = Math.Sin (rad);
        double cosr = Math.Cos (rad);
        
        elem[0, 0] = cosr;
        elem[0, 1] = sinr;
        elem[0, 2] = 0;
        elem[1, 0] = -sinr;
        elem[1, 1] = cosr;
        elem[1, 2] = 0;
        elem[2, 0] = 0;
        elem[2, 1] = 0;
        elem[2, 2] = 1;
    }

    public void FromXYZ (double Rx, double Ry, double Rz)
    {
        SilverLiningMatrix3 rx, ry, rz;
        rx = new SilverLiningMatrix3 ();
        ry = new SilverLiningMatrix3 ();
        rz = new SilverLiningMatrix3 ();
        
        rx.FromRx (Rx);
        ry.FromRy (Ry);
        rz.FromRz (Rz);
        
        SilverLiningMatrix3 result = rx * (ry * rz);
        
        elem = result.elem;
    }

    public static Vector3 operator * (SilverLiningMatrix3 m, Vector3 rkPoint)
    {
        Vector3 kProd = new Vector3 ();
        kProd.x = (float)m.elem[0, 0] * rkPoint.x + (float)m.elem[0, 1] * rkPoint.y + (float)m.elem[0, 2] * rkPoint.z;
        
        kProd.y = (float)m.elem[1, 0] * rkPoint.x + (float)m.elem[1, 1] * rkPoint.y + (float)m.elem[1, 2] * rkPoint.z;
        
        kProd.z = (float)m.elem[2, 0] * rkPoint.x + (float)m.elem[2, 1] * rkPoint.y + (float)m.elem[2, 2] * rkPoint.z;
        
        return kProd;
    }

    public static SilverLiningMatrix3 operator * (SilverLiningMatrix3 m1, SilverLiningMatrix3 m2)
    {
        SilverLiningMatrix3 mout = new SilverLiningMatrix3 ();
        for (int row = 0; row < 3; row++) {
            for (int col = 0; col < 3; col++) {
                mout.elem[row, col] = m1.elem[row, 0] * m2.elem[0, col] + m1.elem[row, 1] * m2.elem[1, col] + m1.elem[row, 2] * m2.elem[2, col];
            }
        }
        
        return mout;
    }

    public SilverLiningMatrix3 Transpose ()
    {
        SilverLiningMatrix3 mout = new SilverLiningMatrix3 ();
        for (int row = 0; row < 3; row++) {
            for (int col = 0; col < 3; col++) {
                mout.elem[row, col] = elem[col, row];
            }
        }
        
        return mout;
    }

    public static Vector3 operator * (Vector3 rkPoint, SilverLiningMatrix3 rkMatrix)
    {
        Vector3 kProd = new Vector3 ();
        
        kProd.x = rkPoint.x * (float)rkMatrix.elem[0, 0] + rkPoint.y * (float)rkMatrix.elem[1, 0] + rkPoint.z * (float)rkMatrix.elem[2, 0];
        
        kProd.y = rkPoint.x * (float)rkMatrix.elem[0, 1] + rkPoint.y * (float)rkMatrix.elem[1, 1] + rkPoint.z * (float)rkMatrix.elem[2, 1];
        
        kProd.z = rkPoint.x * (float)rkMatrix.elem[0, 2] + rkPoint.y * (float)rkMatrix.elem[1, 2] + rkPoint.z * (float)rkMatrix.elem[2, 2];
        
        return kProd;
    }


    public double[,] elem;
}


