using System.Collections.Generic;
using UnityEngine;

public class DumbbellSwapController : MonoBehaviour
{
    [Header("Zone & Sockets")]
    public InteractableTrigger benchZone;   
    public Transform leftHandSocket;        
    public Transform rightHandSocket;       

    [Header("Defaults (fallback)")]
    public GameObject defaultLeftProp;      
    public GameObject defaultRightProp;     

    private readonly List<(Transform tr, Transform prevParent)> _attached = new();

    /*private struct Saved//origional state of a picked object
    {
        public Transform tr, prevParent;
        public Rigidbody rb;
        public bool prevKinematic, prevUseGravity;
        public Vector3 prevLinVel, prevAngVel;
        public Collider[] cols;
        public bool[] prevIsTrigger, prevEnabled;
    }
    private readonly List<Saved> _attached = new();*/

    public void PrepareProps()
    {
        ResetProps(); 

        var pair = PickTwoDroppedFromZone();
        if (!pair.found)
        {
            
            SetDefaultsActive(true);
            return;
        }

        
        SetDefaultsActive(false);
        AttachOne(pair.a, leftHandSocket);
        AttachOne(pair.b, rightHandSocket);
    }

    public void ResetProps()
    {
        for (int i = _attached.Count - 1; i >= 0; --i)
        {
            var (tr, prev) = _attached[i];
            if (tr) tr.SetParent(prev, true);
        }
        _attached.Clear();

    }

    private (bool found, Transform a, Transform b) PickTwoDroppedFromZone()
    {
        var list = ReadZoneObjects();
        var ready = new List<Transform>(2);

        foreach (var go in list)
        {
            if (!go) continue;

            
            var mb = go.GetComponent<MonoBehaviour>();
            bool ok = false;
            if (mb != null)
            {
                var t = mb.GetType();
                var f = t.GetField("moving");
                if (f != null) ok = !(bool)f.GetValue(mb);
                else
                {
                    var p = t.GetProperty("moving");
                    if (p != null) ok = !(bool)p.GetValue(mb, null);
                }
            }
            if (!ok) continue;

            ready.Add(go.transform);
            if (ready.Count == 2) break;
        }

        if (ready.Count < 2) return (false, null, null);
        return (true, ready[0], ready[1]);
    }

    private List<GameObject> ReadZoneObjects()
    {
        var result = new List<GameObject>();
        if (!benchZone) return result;

        
        var fi = benchZone.GetType().GetField("contactList");
        if (fi != null)
        {
            var enumerable = fi.GetValue(benchZone) as System.Collections.IEnumerable;
            if (enumerable != null)
            {
                foreach (var o in enumerable)
                {
                    var go = o as GameObject;
                    if (go) result.Add(go);
                }
            }
        }
        return result;
    }

    private void AttachOne(Transform item, Transform socket)
    {
        if (!item || !socket) return;

        // remember original parent for later reset
        _attached.Add((item, item.parent));

        
        Transform grip = item.Find("LHand");
        if (!grip) grip = item.Find("RHand");
        if (!grip) grip = item.Find("Grip");

        item.SetParent(socket, false);
        item.localPosition = Vector3.zero;
        item.localRotation = Quaternion.identity;

        /*if (grip)
        {
            // match grip point to socket point
            Quaternion rotDelta = socket.rotation * Quaternion.Inverse(grip.rotation);
            item.rotation = rotDelta * item.rotation;

            Vector3 posDelta = socket.position - grip.position;
            item.position += posDelta;

            // parent while maintaining world position
            item.SetParent(socket, true);
        }
        else
        {
            item.SetParent(socket, false);
            item.localPosition = Vector3.zero;
            item.localRotation = Quaternion.identity;
        }*/
    }

    private void SetDefaultsActive(bool active)
    {
        if (defaultLeftProp)  defaultLeftProp.SetActive(active);
        if (defaultRightProp) defaultRightProp.SetActive(active);
    }
}
