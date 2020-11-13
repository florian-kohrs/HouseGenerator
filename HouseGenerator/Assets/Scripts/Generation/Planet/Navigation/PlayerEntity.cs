using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEntity : PlanetEntity
{

    public Transform CameraRotator;


    public override void AlignToTriangle(Vector3 forward)
    {
        base.AlignToTriangle(forward);
    }
    
    public Sprite GetImage()
    {
        throw new System.NotImplementedException();
    }

    public void MouseExit()
    {
        //hoverUI.SetActive(false);
    }

    public void MouseEnter()
    {
        Debug.Log("Hovered on player");
        //hoverUI = MapHoverInterface.GetMapHoverInterfaceFor(transform);
        //hoverUI.SetActive(true);
        //hoverUI.SetTitle("Player");
    }

    protected override void OnTriangleSet(PlanetTriangle t)
    {
        //Planet.DiscoverTriangle(t);
    }

}
