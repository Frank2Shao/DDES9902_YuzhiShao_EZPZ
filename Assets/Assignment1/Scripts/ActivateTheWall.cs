using UnityEngine;
using System.Collections.Generic;

public class ActivateTheWall : MonoBehaviour
{
    [SerializeField] private string dumbbellFilter = "dumbbell";
    [SerializeField] private GameObject wall; 

    private readonly HashSet<GameObject> touching = new();

    void OnCollisionEnter(Collision c)
    {
        var go = GetOtherGO(c);
        if (IsDumbbell(go))
        {
            touching.Add(go);
            UpdateWall();
        }
    }

    void OnCollisionExit(Collision c)
    {
        var go = GetOtherGO(c);
        if (IsDumbbell(go))
        {
            touching.Remove(go);
            UpdateWall();
        }
    }

    GameObject GetOtherGO(Collision c)
    {
        return c.rigidbody ? c.rigidbody.gameObject
                           : (c.collider ? c.collider.gameObject : null);
    }

    bool IsDumbbell(GameObject go)
    {
        if (!go) return false;

        var tf = go.GetComponent<TriggerFilter>();
        return tf && tf.filterString == dumbbellFilter;
    }

    void UpdateWall()
    {
        if (!wall) return;
        bool onGround = touching.Count > 0;
        if (wall.activeSelf != onGround) wall.SetActive(onGround);
    }
}
