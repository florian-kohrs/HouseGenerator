using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Biom
{

    public List<BiomSurface> surfaces;


    public float latitude;


    public void SortSurfaces()
    {
        //surfaces.Sort((s1, s2) => s1.latitude.CompareTo(s2.latitude));
    }

    public Surface GetSurfaceForProgress(float heightProgress)
    {
        Surface result = surfaces[0].surface;

        for (int i = surfaces.Count - 1; i >= 0; i--)
        {
            if (heightProgress > surfaces[i].appearAtHeightProgress)
            {
                result = surfaces[i].surface;
                break;
            }
        }

        return result;

    }

    public Surface GetSurfaceForSqrProgress(float sqrHeightProgress)
    {
        Surface result = surfaces[0].surface;

        for (int i = surfaces.Count - 1; i >= 0; i--)
        {
            if (sqrHeightProgress > surfaces[i].appearAtHeightProgress * surfaces[i].appearAtHeightProgress)
            {
                result = surfaces[i].surface;
                break;
            }
        }

        return result;

    }

}
