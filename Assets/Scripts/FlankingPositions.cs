using System;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class FlankingPositions : MonoBehaviour
{
    public Transform side1;
    public Transform[] side0;
    
    public float delta = 20;
    public float start;

    private void Start()
    {
        start = Vector3.SignedAngle(side1.position, side0[0].position, Vector3.down) * Mathf.PI / 180;
        Set();
    }

    private void Update()
    {
        Set();
    }

    private void Set()
    {
        float elements = side0.Length;
        float targetAngle = start - elements * delta / 2 + delta / 2;
        foreach (var s in side0)
        {
            var (position, direction) = GetFlankingPosition(25f, side1.position, targetAngle);

            s.position = position;
            Quaternion side0Rotation = s.rotation;
            side0Rotation.eulerAngles = direction;
            s.rotation = side0Rotation;
            targetAngle += delta;
        }
    }


    private (Vector3 targetPosition, Vector3 endDirection) GetFlankingPosition(float radius, Vector3 center, float angle)
    {
        
        float x = Mathf.Cos(angle) * radius + center.x;
        float y = center.y;
        float z = Mathf.Sin(angle) * radius + center.z;

        Vector3 targetPosition = new Vector3(x, y, z);
        
        var lookPos = center - targetPosition;
        var endDirection = Quaternion.LookRotation(lookPos, Vector3.up).eulerAngles;

        return (targetPosition, endDirection);

    }
}
