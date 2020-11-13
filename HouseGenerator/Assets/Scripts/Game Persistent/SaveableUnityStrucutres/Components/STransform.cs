using UnityEngine;

public class STransform : SaveableUnityComponent<Transform>
{

    public Serializable3DVector localPosition;
    public Serializable3DVector position;
    public Serializable3DVector rotation;
    public Serializable3DVector scale;

    protected override void saveComponent(Transform component, PersistentGameDataController.SaveType saveType)
    {
        localPosition = new Serializable3DVector(component.localPosition);
        position = new Serializable3DVector(component.position);
        rotation = new Serializable3DVector(component.eulerAngles);
        scale = new Serializable3DVector(component.localScale);
    }

    protected override void restoreComponent(Transform transform)
    {
        transform.position = position.v;
        transform.localScale = scale.v;
        transform.eulerAngles = rotation.v;
    }

}
