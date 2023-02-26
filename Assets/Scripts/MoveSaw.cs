using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSaw : MonoBehaviour
{

    public GameObject location1;
    public GameObject location2;
    public GameObject SawModel;
    public float speed;
    public bool flip;

    // Update is called once per frame
    void Update()
    {
        if(flip)
        {
            float step = speed * Time.deltaTime;
            SawModel.transform.position = Vector3.MoveTowards(SawModel.transform.position, location1.transform.position, step);
        }
        else
        {
            float step = speed * Time.deltaTime;
            SawModel.transform.position = Vector3.MoveTowards(SawModel.transform.position, location2.transform.position, step);
        }
        if(SawModel.transform.position == location1.transform.position)
        {
            flip = !flip;
        }
        if (SawModel.transform.position == location2.transform.position)
        {
            flip = !flip;
        }

        SawModel.transform.Rotate(new Vector3(0f, 0f, 1f), 5f);
    }
}
