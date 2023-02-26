using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateToTarget : MonoBehaviour
{
    private GameObject Direction;
    public float speed;
    private bool Dashed = false;
    // Start is called before the first frame update
    void Start()
    {
        Direction = transform.parent.transform.Find("MoveDirection").gameObject;
        if (speed == 0) { speed = 5; }
    }

    // Update is called once per frame
    void Update()
    {
        if (Dashed) return;
        if (Mathf.Abs(Direction.transform.localPosition.x) + Mathf.Abs(Direction.transform.localPosition.z) > 0.05f)
        {
            
            Vector3 targetDirection = Direction.transform.position - transform.position;
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, speed * Time.deltaTime, 0f);
            transform.rotation = Quaternion.LookRotation(newDirection);

        }
    }

    public void DashEvent()
    {
        Dashed = true;
    }

    public void DashEnd()
    {
        Dashed = false;
    }
}
