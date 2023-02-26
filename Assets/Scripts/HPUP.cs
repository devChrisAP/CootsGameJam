using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPUP : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerController>().Health == 3) return;
        other.gameObject.GetComponent<PlayerController>().Health += 1;
        other.gameObject.GetComponent<PlayerController>().CheckHealth();
        Destroy(gameObject);
    }
}
