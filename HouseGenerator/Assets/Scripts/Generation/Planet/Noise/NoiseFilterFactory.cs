using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseFilterFactory
{

    public static INoiseFilter CreateNoiseFilter(NoiseSettings settings)
    {
        switch (settings.filtertype)
        {
            case NoiseSettings.FilterType.Rigid:
                {
                    return new RigidNoiseFilter(settings);
                }
            case NoiseSettings.FilterType.Simple:
                {
                    return new SimpleNoiseFilter(settings);
                }
            default:
                {
                    throw new System.Exception("Unknown Noise Type: " + settings.filtertype);
                }
        }
    }

}
