using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseSettings
{
    public enum FilterType
    {
        Simple,
        Rigid
    };

    public FilterType filterType;
    public float strength = 1;
    public float roughness = 2;
    public Vector3 center;
    [Range(1,8)]
    public int numberLayers;

    public float persistance = .5f;
    public float baseRoughness = 1f;
    public float minValue;

    public float weightMultiplier = .8f;
}
