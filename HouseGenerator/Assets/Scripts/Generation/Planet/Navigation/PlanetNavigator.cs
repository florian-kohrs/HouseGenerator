using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlanetNavigator : SaveableMonoBehaviour, IPathfinder<TriangleInfo>
{

    public PlanetEntity entity;

    public PathAccuracy accuracy;

    public float speed;

    private IList<Vector3> wayPoints;

    public bool HasPath => wayPoints != null || missingSqrDistance > 0;

    public bool IsRunning => enabled && HasPath;

    protected float distance;

    protected float missingSqrDistance;

    private Vector3 currentTarget;

    private Vector3 CurrentTarget => (Quaternion.Euler(Planet.transform.eulerAngles) * currentTarget) + Planet.transform.position;

    public void SetPaused(bool paused)
    {
        enabled = !paused;
    }

    public void AbortPath()
    {
        ReachedDestination();
    }

    protected PlanetTriangle CurrentTriangle
    {
        get
        {
            return entity.CurrentTriangle;
        }
        set
        {
            entity.CurrentTriangle = value;
        }
    }

    protected PlanetBiomGenerator Planet => entity.Planet;

    //private void Start()
    //{
    //   var t =  CurrentTriangle;
    //}

    public void SetNextPathPart()
    {
        if (wayPoints.Count == 0)
        {
            ReachedDestination();
        }
        else
        {
            currentTarget = wayPoints[0];

            wayPoints.RemoveAt(0);

            missingSqrDistance = (transform.position - CurrentTarget).sqrMagnitude;

            entity.AlignToTriangle(CurrentTarget - entity.transform.position);
        }
    }


    public void ReachedDestination()
    {
        wayPoints = null;
        missingSqrDistance = 0;
    }

    public void StartPath(TriangleInfo to)
    {
        IList<PlanetTriangle> ts = Pathfinder<PlanetTriangle, TriangleInfo>
            .FindPath(Planet, CurrentTriangle, to, accuracy);

        wayPoints =
           ts.Select(tri => tri.UnrotatedMiddlePointOfTriangle).ToList();

        ///Replace end triangle with actual end position
        wayPoints.RemoveAt(0);

        if (wayPoints.Count > 0)
        {
            wayPoints.RemoveAt(wayPoints.Count - 1);
        }

        Vector3 lastWayPoint = Quaternion.Inverse(Quaternion.Euler(Planet.transform.eulerAngles)) * (to.position - Planet.transform.position);
     
        wayPoints.Add(lastWayPoint);

        //foreach (Vector3 v in wayPoints)
        //{
        //    //Debug.Log("Path Triangle has Position: " + v);
        //    GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        //    g.transform.position = v;
        //    g.transform.localScale = Vector3.one * 5;
        //    g.GetComponent<MeshRenderer>().material.color = Color.red;
        //}

        SetNextPathPart();

    }

    private void OnDrawGizmos()
    {
        if (currentTarget != default)
        {
            Gizmos.DrawSphere(CurrentTarget, 5);
        }
    }

    private void Update()
    {
        if (HasPath)
        {
            float distance = speed * Time.deltaTime;
            do
            {
                bool isCurrentNull = !entity.HasTriangleAssigned;

                PlanetTriangle newTri = CurrentTriangle.ClosestFromNeighboursAt(transform.position - Planet.transform.position);

                if (isCurrentNull || newTri != CurrentTriangle)
                {
                    CurrentTriangle = newTri;
                    entity.UpdateToTriangle(CurrentTarget - transform.position);
                }

                if (distance * distance >= missingSqrDistance)
                {
                    distance -= Mathf.Sqrt(missingSqrDistance);
                    missingSqrDistance = 0;
                    transform.position = CurrentTarget;
                    if (HasPath)
                    {
                        SetNextPathPart();
                    }
                    else
                    {
                        ReachedDestination();
                    }
                }
                else
                {
                    Vector3 nextPosition = transform.position + transform.forward * distance;

                    transform.position = nextPosition;
                    missingSqrDistance = (CurrentTarget - transform.position).sqrMagnitude;
                    distance = 0;
                }

            } while (distance > 0 && HasPath);
        }
    }

}
