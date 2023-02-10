using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sm : MonoBehaviour
{
    public void Death()
    {
        FindObjectOfType<Player>().SmCount();
        Destroy(gameObject);

    }
}
