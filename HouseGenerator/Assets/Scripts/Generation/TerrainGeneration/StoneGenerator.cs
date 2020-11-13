using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneGenerator : ShapeGenerator
{

    public enum CubeSide {TopLeft, BottomLeft, BottomRight, TopRight }

    [Range(1,100)]
    public int detail;

    private const int DEFAULT_TERRAIN_WIDTH = 4;

    public override int TerrainWidth => DEFAULT_TERRAIN_WIDTH * detail;

    protected override int ZSize => base.ZSize + 2;

    [Save]
    public int width = 1;

    [Save]
    public int height = 1;

    [Save]
    public int length = 1;

    [Save]
    public int seed = 1;

    protected Tuple<float, CubeSide> IndexToCubeInfo(int xIndex)
    {
        CubeSide cubeSide = (CubeSide)(int)(xIndex / (float)detail);

        float progress = xIndex % detail;

        return Tuple.Create(progress, cubeSide);
    }


    protected override float GetCurrentY(int x, int z)
    {
        float result = base.GetCurrentY(x, z);

        if (z == 0)
        {
            result = GetCurrentY((x + 1) % 5, z + 1);
        }
        else if (z == ZSize)
        {
            result = GetCurrentY((x + 1) % 5, z - 1);
        }
        else
        {

            switch (x)
            {
                case 0:
                case 1:
                case 4:
                    {
                        result += height / 2;
                        break;
                    }
                case 2:
                case 3:
                    {
                        result -= height / 2;
                        break;
                    }
                default:
                    {
                        throw new System.ArgumentException("X can`t be higher than 4 yet it is: " + x);
                    }
            }
        }
        return result;
    }

    protected override float GetCurrentPlainZ(int x, int z)
    {
        if (z == ZSize)
        {
            return z - 2;
        }
        else if (z > 0)
        {
            return z - 1;
        }
        else
        {
            return base.GetCurrentPlainZ(x, z);
        }
    }

    protected override float GetCurrentPlainX(int x, int z)
    {

        float result = 0;

        //if (z == 0 || z == ZSize)
        //{
        //    x = (x + 1) % 5;
        //}


        switch (x)
        {
            case 0:
            case 3:
            case 4:
                {
                    result -= width / 2;
                    break;
                }
            case 1:
            case 2:
                result += width / 2;
                break;

            default:
                {
                    throw new System.ArgumentException("X must be within the range of 0 and 4 yet it is " + x);
                }

        }
        return result;

    }

}
