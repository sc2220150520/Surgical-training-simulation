// Copyright (c) 2012 Sundog Software LLC. All rights reserved worldwide.

using UnityEngine;
using System;

public class SilverLiningCumulusCloud
{
    public struct Voxel
    {
        public bool phaseTransition, vapor, hasCloud;
        public Color color;
    }

    public float voxelSize = 350.0f;
    public float extinctionProbability = 0.05f;
    public float transitionProbability = 0.001f;
    public float vaporProbability = 0.06f;
    public float initialVaporProbability = 0.3f;
    public int initialEvolve = 30;
    public double albedo = 0.9;
    public double quickLightingAttenuation = 6000.0;
    public double maxT = 1.0;
    public double dropletSize = 4.8E-6;
    public double dropletsPerCubicCm = 460.0;
    public double colorRandomness = 0.15;
    public static double minPhase = 0.75;
    public static double maxPhase = 1.0;
    public bool shuriken = false;

    public SilverLiningCumulusCloud (float width, float height, float depth, Vector3 position)
    {
        voxelSize *= (float)SilverLining.unitScale;
        quickLightingAttenuation *= SilverLining.unitScale;

        sizeX = (int)(width / voxelSize);
        sizeY = (int)(height / voxelSize);
        sizeZ = (int)(depth / voxelSize);

        if (shuriken) {
            particles = new ParticleSystem.Particle[sizeX * sizeY * sizeZ];
        } else {
            legacyParticles = new Particle[sizeX * sizeY * sizeZ];
        }

        voxels = new Voxel[sizeX * sizeY * sizeZ];

        alpha = 1.0f;

        double dropletsPerCubicM = (dropletsPerCubicCm / Math.Pow (SilverLining.unitScale, 3.0)) * 100 * 100 * 100;
        dropletSize *= SilverLining.unitScale;
        opticalDepth = (Math.PI * (dropletSize * dropletSize) * voxelSize * dropletsPerCubicM);
        
        cloudPrefab = GameObject.Find (shuriken ? "CumulusCloudPrefab" : "CumulusCloudPrefabLegacy");
        if (cloudPrefab != null) {
            cloudObject = (GameObject)GameObject.Instantiate (cloudPrefab, position, Quaternion.identity);
            GameObject cloudLayer = GameObject.Find ("CumulusClouds");
            if (cloudLayer) {
                cloudObject.transform.parent = cloudLayer.transform;
            }

            if (shuriken) {
                particleSystem = cloudObject.GetComponent<ParticleSystem> ();
                particleSystem.GetComponent<Renderer>().enabled = true;
            } else {
                emitter = cloudObject.GetComponent<ParticleEmitter> ();
                cloudObject.GetComponent<ParticleRenderer> ().enabled = true;
            }
            
            InitializeVoxels ();
        }
    }

    public void Destroy()
    {
        if (cloudObject != null)
        {
            UnityEngine.Object.Destroy(cloudObject);
        }
    }

    public void Update ()
    {
        if (shuriken) {
            if (particleSystem != null) {
#if UNITY_3_5
                if (cloudObject.active) {
#else	
				if (cloudObject.activeSelf) {
#endif
                    particleSystem.SetParticles(particles, sizeX * sizeY * sizeZ);
                    particleSystem.Simulate(0.1f);
                }
            }
        } else {
            if (emitter != null) {
                emitter.particles = legacyParticles;
            }
        }
    }

    protected void InitializeVoxels ()
    {
        Vector3 mid = new Vector3 (voxelSize * (sizeX / 2), voxelSize * (sizeY / 2), voxelSize * (sizeZ / 2));
        
        for (int i = 0; i < sizeX; i++) {
            for (int j = 0; j < sizeY; j++) {
                for (int k = 0; k < sizeZ; k++) {
                    int idx = i * (sizeY * sizeZ) + j * sizeZ + k;
                    Vector3 position = new Vector3 (i * voxelSize, j * voxelSize, k * voxelSize) - mid;

                    if (shuriken) {
                        particles[idx].color = new Color (1.0f, 1.0f, 0.0f, 1.0f);
                        float sizeVariation = (UnityEngine.Random.value - 0.5f) * (voxelSize * 0.2f);
                        particles[idx].size = (voxelSize + sizeVariation) * 1.414f * 2.0f;
                        particles[idx].position = position;
                        particles[idx].rotation = UnityEngine.Random.value * 360.0f;
                        particles[idx].velocity = new Vector3(0, 0, 0);
                        Vector3 posVariance = new Vector3 (UnityEngine.Random.value * voxelSize * 0.5f - voxelSize * 0.25f, UnityEngine.Random.value * voxelSize * 0.5f - voxelSize * 0.25f, UnityEngine.Random.value * voxelSize * 0.5f - voxelSize * 0.25f);
                        particles[idx].position += posVariance;
                        particles[idx].angularVelocity = (UnityEngine.Random.value - 0.5f) * 180.0f;
                    } else {
                        legacyParticles[idx].color = new Color (1.0f, 1.0f, 0.0f, 1.0f);
                        float sizeVariation = (UnityEngine.Random.value - 0.5f) * (voxelSize * 0.2f);
                        legacyParticles[idx].size = (voxelSize + sizeVariation) * 1.414f * 2.0f;
                        legacyParticles[idx].position = position;
                        legacyParticles[idx].rotation = UnityEngine.Random.value * 360.0f;
                        legacyParticles[idx].energy = 1.0f;
                        Vector3 posVariance = new Vector3 (UnityEngine.Random.value * voxelSize * 0.5f - voxelSize * 0.25f, UnityEngine.Random.value * voxelSize * 0.5f - voxelSize * 0.25f, UnityEngine.Random.value * voxelSize * 0.5f - voxelSize * 0.25f);
                        legacyParticles[idx].position += posVariance;
                        legacyParticles[idx].angularVelocity = (UnityEngine.Random.value - 0.5f) * 180.0f;
                    }
                    
                    voxels[idx].vapor = UnityEngine.Random.value < initialVaporProbability;
                    voxels[idx].hasCloud = false;
                    voxels[idx].phaseTransition = false;
                    voxels[idx].color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                }
            }
        }
        
        if (sizeX >= 4 && sizeZ >= 4) {
            int idx = (sizeX >> 2) * sizeY * sizeZ + (sizeZ >> 2);
            voxels[idx].phaseTransition = true;
            idx = (sizeX >> 2) * sizeY * sizeZ + (sizeZ - (sizeZ >> 2));
            voxels[idx].phaseTransition = true;
            idx = (sizeX - (sizeX >> 2)) * sizeY * sizeZ + (sizeZ - (sizeZ >> 2));
            voxels[idx].phaseTransition = true;
            idx = (sizeX - (sizeX >> 2)) * sizeY * sizeZ + (sizeZ >> 2);
            voxels[idx].phaseTransition = true;
        }
        
        for (int i = 0; i < initialEvolve; i++) {
            IterateCellularAutomata ();
        }
    }

    protected int GetIdx (int x, int y, int z)
    {
        return x * sizeY * sizeZ + y * sizeZ + z;
    }

    protected void IterateCellularAutomata ()
    {
        double midX = (double)sizeX * 0.5;
        double midY = (double)sizeY;
        // Use full height to simulate hemi-ellipsoid
        double midZ = (double)sizeZ * 0.5;
        
        double a2 = midX * midX;
        double b2 = midY * midY;
        double c2 = midZ * midZ;
        
        int i, j, k;
        
        for (i = 0; i < sizeX; i++) {
            for (j = 0; j < sizeY; j++) {
                for (k = 0; k < sizeZ; k++) {
                    double x = (double)i - midX;
                    double y = (double)j;
                    double z = (double)k - midZ;
                    double dist = (x * x) / a2 + (y * y) / b2 + (z * z) / c2;
                    double scale = 1.0 - dist;
                    if (scale < 0)
                        scale = 0;
                    
                    bool fAct = ((i + 1 < sizeX ? voxels[GetIdx (i + 1, j, k)].phaseTransition : false) || (k + 1 < sizeZ ? voxels[GetIdx (i, j, k + 1)].phaseTransition : false) || (j + 1 < sizeY ? voxels[GetIdx (i, j + 1, k)].phaseTransition : false) || (i - 1 >= 0 ? voxels[GetIdx (i - 1, j, k)].phaseTransition : false) || (k - 1 >= 0 ? voxels[GetIdx (i, j, k - 1)].phaseTransition : false) || (j - 1 >= 0 ? voxels[GetIdx (i, j - 1, k)].phaseTransition : false) || (i - 2 >= 0 ? voxels[GetIdx (i - 2, j, k)].phaseTransition : false) || (i + 2 < sizeX ? voxels[GetIdx (i + 2, j, k)].phaseTransition : false) || (k - 2 >= 0 ? voxels[GetIdx (i, j, k - 2)].phaseTransition : false) || (k + 2 < sizeZ ? voxels[GetIdx (i, j, k + 2)].phaseTransition : false) || (j - 2 >= 0 ? voxels[GetIdx (i, j - 2, k)].phaseTransition : false));
                    
                    int idx = GetIdx (i, j, k);
                    bool thisAct = voxels[idx].phaseTransition;
                    
                    voxels[idx].phaseTransition = ((!thisAct) && voxels[idx].vapor && fAct) || (UnityEngine.Random.value < transitionProbability * scale);
                    
                    voxels[idx].vapor = (voxels[idx].vapor && !thisAct) || (UnityEngine.Random.value < vaporProbability * scale);
                    
                    voxels[idx].hasCloud = (voxels[idx].hasCloud || thisAct) && (UnityEngine.Random.value > extinctionProbability * (1.0 - scale));

                    if (shuriken) {
                        particles[idx].remainingLifetime = voxels[idx].hasCloud ? Single.MaxValue : 0.0f;
                    } else {
                        legacyParticles[idx].energy = voxels[idx].hasCloud ? 1.0f : 0.0f;
                    }
                }
            }
        }
    }

    public void UpdateLighting (Color lightColor, Vector3 lightDir)
    {
        lightDir.Normalize();
        Vector3 V = lightDir * -1;

        double w, h, d;
        w = sizeX * voxelSize + voxelSize * 2.0;
        h = sizeY * voxelSize + voxelSize * 2.0;
        d = sizeZ * voxelSize + voxelSize * 2.0;
        
        // ray / ellipsoid intersection algorithm from Lengyel
        double m = w /(h * 2.0);
        // height*2 since the volume is a
        // hemi-ellipsoid, we want the full ellipsoid.
        double n = w / d;
        
        double a = (V.x * V.x) + (m * m * V.y * V.y) + (n * n * V.z * V.z);
        double twoa = 2.0 * a;
        double invTwoA = 1.0 / twoa;
        double foura = 4.0 * a;
        double r = w * 0.5;
        double r2 = r * r;
        double m2 = m * m;
        double n2 = n * n;
        
        double constTerm = albedo / (4.0 * Math.PI);
        double invAttenuation = 1.0 / quickLightingAttenuation;
        
        double extinction = 1.0 - Math.Exp (-opticalDepth);
        GetRenderer().material.SetFloat("extinction", (float)extinction );

        //double phase = ComputePhaseFunction(cloudObject.transform.position, lightDir);
        
        for (int i = 0; i < sizeX * sizeY * sizeZ; i++) {
            if (!voxels[i].hasCloud)
                continue;
            
            Vector3 S = shuriken ? particles[i].position : legacyParticles[i].position;

            double b = 2.0 * (S.x * V.x + m2 * S.y * V.y + n2 * S.z * V.z);
            double c = S.x * S.x + m2 * S.y * S.y + n2 * S.z * S.z - r2;
            double D = b * b - foura * c;
            
            double t;
            
            double rnd = Math.Abs (((S.x + S.y + S.z) % 100.0)) * 0.01;
            rnd = (1.0 - colorRandomness) + (colorRandomness * rnd);
            
            if (D >= 0.0) {
                t = (-b - Math.Sqrt (D)) * invTwoA;
                // argh, this sqrt seems unavoidable.
                t *= -invAttenuation;
                
                if (t > maxT)
                    t = maxT;
                if (t < 0.0)
                    t = 0.0;
                
                // t is now a parameter along the ray from the voxel to the sun.
                // Flip to represent the distance into the cloud volume.
                t = 1.0 - t;
                
                t *= constTerm * opticalDepth;

                voxels[i].color = lightColor * (float)t;
                voxels[i].color.a = (float)rnd;
            } else {
                voxels[i].color = (lightColor * (float)constTerm * (float)opticalDepth);
                voxels[i].color.a = (float)rnd;
            }

            if (shuriken) {
                particles[i].color = voxels[i].color;
            } else {
                legacyParticles[i].color = voxels[i].color;
            }
        }
        
    }

    public void SetAlpha(float pAlpha)
    {
        if (pAlpha > 1.0f) pAlpha = 1.0f;
        if (pAlpha < 0.0f) pAlpha = 0.0f;

        alpha = pAlpha;
        cloudObject.GetComponent<Renderer>().material.SetFloat ("fade", SilverLiningCumulusCloudLayer.cloudAlpha * alpha);
    }

    public float GetAlpha()
    {
        return alpha;
    }

    public Renderer GetRenderer ()
    {
        return cloudObject.GetComponent<Renderer>();
    }

    public Vector3 GetPosition()
    {
        return cloudObject.transform.position;
    }

    public void SetPosition(Vector3 position)
    {
        cloudObject.transform.position = position;
    }

    public Vector3 lightingWorldPos;
    private Voxel[] voxels;
    private ParticleSystem.Particle[] particles;
    private Particle[] legacyParticles;
    private ParticleEmitter emitter;
    public GameObject cloudPrefab;
    public GameObject cloudObject;
    private int sizeX, sizeY, sizeZ;
    public ParticleSystem particleSystem;
    private double opticalDepth;
    private float alpha;
}


