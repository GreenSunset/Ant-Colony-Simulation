using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Flock/Behaviour/Avoid Centre")]
public class AvoidCenter : FlockBehaviour
{
    public Vector2 centre;
    public float radius = 15f;

    public override Vector2 CalculateMove(FlockAgent agent, List<Transform> context, Flock flock)
    {
        Vector2 centerOffset = centre - (Vector2)agent.transform.position;
        float t = centerOffset.magnitude;
        if (t > radius)
        {
            return Vector2.zero;
        }
        if (t == 0) t = 1;
        return -centerOffset / (t*t);
    }
}
