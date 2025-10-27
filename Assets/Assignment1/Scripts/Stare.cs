using UnityEngine;

public class Stare : MonoBehaviour
{
    public Transform target;  // The object to stare at
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(target);
    }
}
