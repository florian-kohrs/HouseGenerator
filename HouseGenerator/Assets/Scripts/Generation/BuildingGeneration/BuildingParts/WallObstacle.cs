using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WallObstacle 
{

    public WallObstacle() { }

    public WallObstacle(WallObstacleScriptableObject source)
    {
        bottomLeftAnchorPosition =
            source.obstacle.bottomLeftAnchorPosition + new Vector2(source.extraXOffset,0);

        obstacleSize = source.obstacle.obstacleSize;
    }

    public Vector2 bottomLeftAnchorPosition;

    public Vector2 obstacleSize;

}
