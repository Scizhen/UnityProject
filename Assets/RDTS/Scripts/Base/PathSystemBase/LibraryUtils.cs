using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LibraryUtils
{
    //弯曲运输机中关于计算的方法类
    private static Quaternion rotation;
    private static Vector3 vec;

    public static Vector3 CalculateOneArcPos(Vector3 center, float radius, float angle, float startAngle, float endAngle, float deltaheight, bool flip, bool clockwise = false)
    {
        Vector3 point = Vector3.zero;
        Vector3 unit = (center + radius * Vector3.right);
        var perc = angle / (endAngle - startAngle) * deltaheight;
        if (flip)
        {
            if (!clockwise)
            {
                vec = RotateY(unit, 90 - endAngle + angle);
                vec = vec + Vector3.up * (deltaheight * perc);
            }
            else
            {
                vec = RotateY(unit, 270 + endAngle - angle);
                vec = vec + Vector3.up * (deltaheight * perc);           // vec = RotateY(unit, 270 + endAngle - i * deltaAngle);
            }
        }
        else
        {
            if (!clockwise)
            {
                vec = RotateY(unit, 90 - startAngle - angle);
                vec = vec + Vector3.up * (deltaheight * perc);
            }
            else
            {
                vec = RotateY(unit, 270 + startAngle + angle);
                vec = vec + Vector3.up * (deltaheight * perc);  //    vec = RotateY(unit, 270 + startAngle + i * deltaAngle);
            }
        }
        return vec;
    }

    public static Vector3 CalculateOneArcRot(float radius, float angle, float startAngle, float endAngle, float deltaheight, Vector3 rotaton, bool flip, bool clockwise = false)
    {
        Vector3 point = Vector3.zero;
        float deltatotal = endAngle - startAngle;
        float p = deltaheight / (deltatotal / 360);
        float tanbeta = p / (Mathf.PI * 2 * radius);
        float beta = Mathf.Rad2Deg * Mathf.Atan(tanbeta);
        if (flip)
        {
            if (!clockwise)
                vec = new Vector3(0 + rotation.x - beta, -endAngle + 270 + angle + rotaton.y, rotaton.z);
            else
                vec = new Vector3(0 + rotation.x - beta, endAngle + 270 - angle + rotaton.y, rotaton.z);

        }
        else
        {
            if (!clockwise)
                vec = new Vector3(0 + rotaton.x - beta, 90 - startAngle - angle + rotaton.y, rotaton.z);
            else
                vec = new Vector3(0 + rotaton.x - beta, 90 + startAngle + angle + rotaton.y, rotaton.z);
        }
        return vec;
    }

    public static List<Vector3> CalculateArcPos(Vector3 center, float radius, float startAngle, float endAngle, float deltaheight, int numberpoints, bool flip, bool clockwise = false)
    {
        List<Vector3> points = new List<Vector3>();
        Vector3 unit = (center + radius * Vector3.right);
        int count = numberpoints;
        float deltaAngle = Mathf.Abs((endAngle - startAngle) / (count - 1));
        if (flip)
        {
            if (!clockwise)
            {
                for (int i = 0; i < count; i++)
                {
                    vec = RotateY(unit, 90 - endAngle + i * deltaAngle);
                    vec = vec + Vector3.up * (deltaheight / (count - 1)) * i;
                    points.Add(vec);
                }
            }
            else
            {
                for (int i = 0; i < count; i++) //
                {
                    vec = RotateY(unit, 270 + endAngle - i * deltaAngle);
                    vec = vec + Vector3.up * (deltaheight / (count - 1)) * i;
                    points.Add(vec);
                }
            }

        }
        else
        {
            if (!clockwise)
            {
                for (int i = 0; i < count; i++)
                {
                    vec = RotateY(unit, 90 - startAngle - i * deltaAngle);
                    vec = vec + Vector3.up * (deltaheight / (count - 1)) * i;
                    points.Add(vec);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    vec = RotateY(unit, 270 + startAngle + i * deltaAngle);
                    vec = vec + Vector3.up * (deltaheight / (count - 1)) * i;
                    points.Add(vec);
                }
            }
        }
        return points;
    }


    public static List<Vector3> CalculateArcRot(float startAngle, float endAngle, float radius, float deltaheight, int numberpoints, Vector3 rotaton, bool flip, bool clockwise = false)
    {
        List<Vector3> rots = new List<Vector3>();
        int count = numberpoints;
        float deltaAngle = Mathf.Abs((endAngle - startAngle) / (count - 1));
        float deltatotal = endAngle - startAngle;
        float p = deltaheight / (deltatotal / 360);
        float tanbeta = p / (Mathf.PI * 2 * radius);
        float beta = Mathf.Rad2Deg * Mathf.Atan(tanbeta);
        if (flip)
        {
            if (!clockwise)
            {
                for (int i = 0; i < count; i++)
                {
                    vec = new Vector3(0 + rotation.x - beta, -endAngle + 270 + i * deltaAngle + rotaton.y, rotaton.z);
                    rots.Add(vec);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    vec = new Vector3(0 + rotation.x - beta, endAngle + 270 - i * deltaAngle + rotaton.y, rotaton.z);
                    rots.Add(vec);
                }
            }
        }
        else
        {
            if (!clockwise)
            {
                for (int i = 0; i < count; i++)
                {
                    vec = new Vector3(0 + rotaton.x - beta, 90 - startAngle - i * deltaAngle + rotaton.y, rotaton.z);
                    rots.Add(vec);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    vec = new Vector3(0 + rotaton.x - beta, 90 + startAngle + i * deltaAngle + rotaton.y, rotaton.z); //OK
                    rots.Add(vec);
                }
            }

        }
        return rots;
    }

    public static void SetArc(GameObject obj, List<Vector3> positions, List<Vector3> rotations, float radius, float startAngle, float endAngle)
    {
        //Vector3 value;

        SuperSpline spline = obj.GetComponent<SuperSpline>();
        if (spline != null)
        {
            for (int i = 0; i < spline.SplineNodes.Length; i++)
            {
                spline.SplineNodes[i].transform.localPosition = positions[i];
                spline.SplineNodes[i].transform.localRotation = Quaternion.Euler(rotations[i]);
            }
            spline.tension = 1.6f * radius * (endAngle - startAngle) / 180;
            spline.UpdateSpline();
            SplineMesh splineMesh = obj.GetComponent<SplineMesh>();
            if (splineMesh != null)
            {
                //splineMesh.segmentCount = Mathf.CeilToInt(radius * 100);
                splineMesh.segmentCount = 15;
                splineMesh.segmentStart = 0;
                splineMesh.segmentEnd = 1;
                splineMesh.UpdateMesh();
            }
        }
    }

    public static Vector3 RotateY(Vector3 current, float angle)
    {
        rotation = Quaternion.AngleAxis(angle, Vector3.up);
        return rotation * current;
    }

    public static void SetLocalPositionY(this GameObject obj, float y)
    {
        vec = obj.transform.localPosition;
        vec.y = y;
        obj.transform.localPosition = vec;
    }


}
