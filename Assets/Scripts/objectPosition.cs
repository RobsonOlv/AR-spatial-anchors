using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPosition : MonoBehaviour
{
    public Transform parentToFollow;

    void Update()
    {
        transform.position = parentToFollow.position;
        float parentY = parentToFollow.rotation.eulerAngles.y;
        transform.localRotation = Quaternion.Euler(0f, parentY, 0f);
    }
}
