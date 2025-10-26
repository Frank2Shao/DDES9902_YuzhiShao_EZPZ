using System.Collections.Generic;
using UnityEngine;

public class DumbbellSwapController : MonoBehaviour
{
    [Header("Zone & Sockets")]
    public InteractableTrigger benchZone;   // 哑铃凳的触发区（必须挂有 InteractableTrigger）
    public Transform leftHandSocket;        // 人偶左手空节点
    public Transform rightHandSocket;       // 人偶右手空节点

    [Header("Defaults (fallback)")]
    public GameObject defaultLeftProp;      // 默认左哑铃（模型里的那只）
    public GameObject defaultRightProp;     // 默认右哑铃

    private struct Saved//origional state of a picked object
    {
        public Transform tr;
        public Transform prevParent;
        public Rigidbody rb;
        public bool prevKinematic;
        public bool prevUseGravity;
        public Vector3 prevLinVel;
        public Vector3 prevAngVel;
        public Collider[] cols;
        public bool[] prevIsTrigger;
    }
    private readonly List<Saved> _attached = new();

    public void PrepareProps()
    {
        // 清理上一次装在手上的玩家哑铃（若有）
        ResetProps();

        // find dropped dumbbells in zone
        var pair = PickTwoDroppedDumbbells();
        if (!pair.found)
        {
            // 没凑齐：默认道具保持激活
            SetDefaultsActive(true);
            return;
        }

        // 有两只：隐藏默认道具，装玩家的
        SetDefaultsActive(false);
        AttachToHand(pair.left, leftHandSocket);
        AttachToHand(pair.right, rightHandSocket);
    }

    public void ResetProps()
    {
        for (int i = _attached.Count - 1; i >= 0; --i)
        {
            var s = _attached[i];

            // 还原父子
            if (s.tr) s.tr.SetParent(s.prevParent, worldPositionStays: true);

            // 还原物理
            if (s.rb)
            {
                s.rb.isKinematic = s.prevKinematic;
                s.rb.useGravity  = s.prevUseGravity;
                s.rb.linearVelocity  = s.prevLinVel;
                s.rb.angularVelocity = s.prevAngVel;
            }

            // 还原碰撞
            if (s.cols != null && s.prevIsTrigger != null && s.cols.Length == s.prevIsTrigger.Length)
                for (int c = 0; c < s.cols.Length; c++)
                    if (s.cols[c]) s.cols[c].isTrigger = s.prevIsTrigger[c];
        }
        _attached.Clear();

        // 复位默认道具（看你需要；通常恢复可见）
        SetDefaultsActive(true);
    }

    private IEnumerable<GameObject> GetZoneObjects()
    {
        if (!benchZone) yield break;

        // 通过反射获取 contactList（为了兼容你现有的 InteractableTrigger）
        var fi = benchZone.GetType().GetField("contactList");
        if (fi != null)
        {
            var listObj = fi.GetValue(benchZone) as System.Collections.IEnumerable;
            if (listObj != null)
            {
                foreach (var o in listObj)
                {
                    var go = o as GameObject;
                    if (go) yield return go;
                }
            }
        }
    }

    private (bool found, Transform left, Transform right) PickTwoDroppedDumbbells()
    {
        var picked = new List<Transform>(2);

        foreach (var go in GetZoneObjects())
        {
            if (!go) continue;

            // 必须具有 Movable 且 moving==false（代表玩家已松手）
            var movable = go.GetComponent<MonoBehaviour>();
            bool isMovable = false, isHeld = false;
            Transform tr = go.transform;

            if (movable) // 尝试读 moving 字段/属性（通过反射，避开强耦合）
            {
                var t = movable.GetType();

                var movingField = t.GetField("moving");
                if (movingField != null)
                {
                    isMovable = true;
                    isHeld = (bool)movingField.GetValue(movable);
                }
                else
                {
                    var movingProp = t.GetProperty("moving");
                    if (movingProp != null)
                    {
                        isMovable = true;
                        isHeld = (bool)movingProp.GetValue(movable, null);
                    }
                }
            }

            if (!isMovable || isHeld) continue; // 没有 Movable 或仍在手里 -> 跳过

            picked.Add(tr);
            if (picked.Count == 2) break;
        }

        if (picked.Count < 2) return (false, null, null);
        return (true, picked[0], picked[1]);
    }

    private void AttachToHand(Transform item, Transform handSocket)
    {
        if (!item || !handSocket) return;

        // 保存状态
        var s = new Saved
        {
            tr = item,
            prevParent = item.parent,
            rb = item.GetComponent<Rigidbody>(),
            cols = item.GetComponentsInChildren<Collider>(includeInactive: true)
        };
        if (s.rb)
        {
            s.prevKinematic  = s.rb.isKinematic;
            s.prevUseGravity = s.rb.useGravity;
            s.prevLinVel     = s.rb.linearVelocity;
            s.prevAngVel     = s.rb.angularVelocity;
        }
        if (s.cols != null)
        {
            s.prevIsTrigger = new bool[s.cols.Length];
            for (int i = 0; i < s.cols.Length; i++)
                s.prevIsTrigger[i] = s.cols[i] ? s.cols[i].isTrigger : false;
        }

        // 临时关闭物理/碰撞干扰
        if (s.rb)
        {
            s.rb.isKinematic = true;
            s.rb.useGravity = false;
            s.rb.linearVelocity = Vector3.zero;
            s.rb.angularVelocity = Vector3.zero;
        }
        if (s.cols != null)
            foreach (var c in s.cols) if (c) c.isTrigger = true;

        // 亲缘关系：用“握把点”对齐到手（若找得到）
        var grip = FindGrip(item); // 先找子物体名 attachPoint / AttachPoint / Grip
        if (grip)
        {
            var gripPrevParent = grip.parent;
            grip.SetParent(handSocket, worldPositionStays: false);
            grip.localPosition = Vector3.zero;
            grip.localRotation = Quaternion.identity;

            item.SetParent(handSocket, worldPositionStays: true);
            grip.SetParent(gripPrevParent, worldPositionStays: true);
        }
        else
        {
            item.SetParent(handSocket, worldPositionStays: false);
            item.localPosition = Vector3.zero;
            item.localRotation = Quaternion.identity;
        }

        _attached.Add(s);
    }

    private Transform FindGrip(Transform root)
    {
        // 常见命名：attachPoint / AttachPoint / Attach Point / Grip
        var names = new[] { "attachPoint", "AttachPoint", "Attach Point", "Grip", "grip" };
        foreach (var n in names)
        {
            var t = root.Find(n);
            if (t) return t;
        }
        // 没有握把也没关系，直接对齐到 socket
        return null;
    }

    private void SetDefaultsActive(bool active)
    {
        if (defaultLeftProp)  defaultLeftProp.SetActive(active);
        if (defaultRightProp) defaultRightProp.SetActive(active);
    }
}
