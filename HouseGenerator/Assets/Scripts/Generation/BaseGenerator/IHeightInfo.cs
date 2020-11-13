using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public interface IHeightInfo
{

    float HeightOnPosition(Vector2 position);

    float AbsoluteHeightOnNearestIndex(Vector2 position);

    IndexInfo GetNearestIndexInfo(Vector2 position);

    Vector2 GlobalPosToProgress(Vector2 pos);

}

