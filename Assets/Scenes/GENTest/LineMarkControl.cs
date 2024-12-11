using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineMarkControl : MonoBehaviour
{
    public float random;
    private Transform parentAGV;
    private LineRenderer line;
    private int lineCount;

    // Start is called before the first frame update
    void Start()
    {
        parentAGV = this.transform.parent;
        line = this.GetComponent<LineRenderer>();
        lineCount = line.positionCount;
    }

    Vector3 currentPosition;
    Vector3 lastPosition;
    // Update is called once per frame
    void FixedUpdate()
    {
        currentPosition = parentAGV.position;
        if (currentPosition != lastPosition)
        {
            lineCount++;
            line.positionCount = lineCount;
            float xRand = Random.Range(currentPosition.x * -random, currentPosition.x * random);
            float yRand = Random.Range(currentPosition.y * -random, currentPosition.y * random);
            float zRand = Random.Range(currentPosition.z * -random, currentPosition.z * random);
            line.SetPosition(lineCount - 1,currentPosition + new Vector3(xRand,yRand,zRand));
        }

        lastPosition = currentPosition;
    }
}
