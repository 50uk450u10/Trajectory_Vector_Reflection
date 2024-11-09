using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileTurret : MonoBehaviour
{
    [SerializeField] float projectileSpeed = 1;
    [SerializeField] Vector3 gravity = new Vector3(0, -9.8f, 0);
    [SerializeField] LayerMask targetLayer;
    [SerializeField] GameObject crosshair;
    [SerializeField] float baseTurnSpeed = 3;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] GameObject gun;
    [SerializeField] Transform turretBase;
    [SerializeField] Transform barrelEnd;
    [SerializeField] LineRenderer line;
    [SerializeField] bool useLowAngle;

    List<Vector3> points = new List<Vector3>();

    // Update is called once per frame
    void Update()
    {
        TrackMouse();
        TurnBase();
        RotateGun();
        DrawTrajectory();

        if (Input.GetButtonDown("Fire1"))
            Fire();
    }

    void Fire()
    {
        GameObject projectile = Instantiate(projectilePrefab, barrelEnd.position, gun.transform.rotation);
        projectile.GetComponent<Rigidbody>().velocity = projectileSpeed * barrelEnd.transform.forward;
    }

    void TrackMouse()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if(Physics.Raycast(cameraRay, out hit, 1000, targetLayer))
        {
            crosshair.transform.forward = hit.normal;
            crosshair.transform.position = hit.point + hit.normal * 0.1f;
            //Debug.Log("hit ground");
        }
    }

    void TurnBase()
    {
        Vector3 directionToTarget = (crosshair.transform.position - turretBase.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, 0, directionToTarget.z));
        turretBase.transform.rotation = Quaternion.Slerp(turretBase.transform.rotation, lookRotation, Time.deltaTime * baseTurnSpeed);
    }

    void RotateGun()
    {
        float? angle = CalculateTrajectory(crosshair.transform.position, useLowAngle);
        if (angle != null)
            gun.transform.localEulerAngles = new Vector3(360f - (float)angle, 0, 0);
    }

    float? CalculateTrajectory(Vector3 target, bool useLow)
    {
        Vector3 targetDir = target - barrelEnd.position;
        
        float y = targetDir.y;
        targetDir.y = 0;

        float x = targetDir.magnitude;

        float v = projectileSpeed;
        float v2 = Mathf.Pow(v, 2);
        float v4 = Mathf.Pow(v, 4);
        float g = gravity.y;
        float x2 = Mathf.Pow(x, 2);

        float underRoot = v4 - g * ((g * x2) + (2 * y * v2));

        if (underRoot >= 0)
        {
            float root = Mathf.Sqrt(underRoot);
            float highAngle = v2 + root;
            float lowAngle = v2 - root;

            if (useLow)
                return (Mathf.Atan2(lowAngle, g * x) * Mathf.Rad2Deg);
            else
                return (Mathf.Atan2(highAngle, g * x) * Mathf.Rad2Deg);
        }
        else
            return null;
    }

    void DrawTrajectory()
    {
        points.Clear();
        //points.Add(barrelEnd.position);

        // Calculate the trajectory based on the projectile speed and angle
        Vector3 initialVelocity = barrelEnd.forward * projectileSpeed;

        float timeStep = 0.1f;  // How much time to increment each step
        float maxTime = 5f;  // Max time to draw the trajectory
        Vector3 prevPoint = barrelEnd.position;

        for (float t = 0; t < maxTime; t += timeStep)
        {
            //Vector3 position = CalculatePositionAtTime(initialVelocity, t);
            float y = Displacement(initialVelocity.y, t, -gravity.y);
            float x = Displacement(initialVelocity.x, t, 0);
            float z = Displacement(initialVelocity.z, t, 0);

            Vector3 displacement = new Vector3(x, y, z);
            points.Add(barrelEnd.position + displacement);
            Vector3 currPoint = barrelEnd.position + displacement;
            Vector3 difference = currPoint - prevPoint;

            RaycastHit hit;
            if (Physics.Raycast(prevPoint, difference.normalized, out hit, 0.5f, targetLayer))
            {
                Debug.Log(hit.point);
                
                break;
            }

            prevPoint = currPoint;
        }

        // Update the LineRenderer to display the trajectory points
        line.positionCount = points.Count;
        for (int i = 0; i < points.Count; i++)
        {
            line.SetPosition(i, points[i]);
        }
    }

    //Vector3 CalculatePositionAtTime(Vector3 initialVelocity, float time)
    //{
    //    // Kinematic equations for projectile motion
    //    float x = initialVelocity.x * time;
    //    float y = initialVelocity.y * time + 0.5f * gravity.y * (time * time);
    //    float z = initialVelocity.z * time;
    //    return new Vector3(x, y, barrelEnd.position.z + z);
    //}

    float Displacement(float vI, float t, float a)
    {
        return vI * t + 0.5f * a * (t * t);
    }
}
