using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class speed_test2 : MonoBehaviour
{
    public float choose = 1.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    float last_choose = 1.0f;
    void Update()
    {

        if (last_choose != choose)
            Time.timeScale = choose;

        last_choose = choose;
    }
}
