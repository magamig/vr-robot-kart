using UnityEngine;

namespace PVR
{
    namespace Unity
    {
        public class GetParent
        {
            public static GameObject Get(GameObject go)
            {
                if (null == go || null == go.transform || null == go.transform.parent)
                {
                    return null;
                }
                Transform parent = go.transform.parent;
                return parent.gameObject;
            }
        }
    }
}
