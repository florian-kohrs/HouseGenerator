using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionAnimationCurve
{

    /// <summary>
    /// Integrates the animation curve using the chained trapezoidal rule
    /// </summary>
    /// <param name="c"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="slices"></param>
    /// <returns></returns>
    public static float Integrate(this AnimationCurve c, float start, float end, int slices)
    {
        if (c == null)
        {
            return 0;
        }
        float approximateResult = 0;

        float intergrationWidth = end - start;
        float sliceWidth = intergrationWidth / slices;

        float currentStart = start;

        for (int i = 0; i < slices; i++)
        {
            float nextStart = currentStart + sliceWidth;
            
            ///appromixate the current integration by calculating the average height between the two points
            ///muliplied by the sliceWidth
            approximateResult += ((c.Evaluate(nextStart) + c.Evaluate(currentStart)) / 2) * sliceWidth;
            currentStart = nextStart;
        }

        return approximateResult;
    }

    public static float IntegrateUntil(this AnimationCurve c, float start, float approximateIntegration, int accuracy = 100)
    {
        float approximateResult = 0;

        float intergrationWidth = 1;
        float sliceWidth = intergrationWidth / accuracy;

        float currentStart = start;

        int i = 0;
        for (; i < accuracy && approximateResult <= approximateIntegration ; i++)
        {
            float nextStart = currentStart + sliceWidth;

            ///appromixate the current integration by calculating the average height between the two points
            ///muliplied by the sliceWidth
            approximateResult += ((c.Evaluate(nextStart) + c.Evaluate(currentStart)) / 2) * sliceWidth;
            currentStart = nextStart;
        }

        return i / (float)accuracy;
    }

    public static float CombinedIntegration(this AnimationCurve c1, AnimationCurve c2, float start, float end, int slices)
    {
        float approximateResult = 0;

        float intergrationWidth = end - start;
        float sliceWidth = intergrationWidth / slices;

        float currentStart = start;

        for (int i = 0; i < slices; i++)
        {
            float nextStart = currentStart + sliceWidth;

            ///appromixate the current integration by calculating the average height between the two points
            ///muliplied by the sliceWidth
            float currentResult = ((c1.Evaluate(nextStart) + c1.Evaluate(currentStart)) / 2) * sliceWidth;
            ///multiply by the second curve at the same area
            currentResult *= ((c2.Evaluate(nextStart) + c2.Evaluate(currentStart)) / 2) * sliceWidth;
            approximateResult += currentResult;
            currentStart = nextStart;
        }

        return approximateResult;
    }

}
