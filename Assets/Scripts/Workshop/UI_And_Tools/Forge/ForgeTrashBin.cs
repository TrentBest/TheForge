using System;
using System.Collections.Generic;
using UnityEngine;

namespace Workshop.UI_And_Tools.Forge.Core
{
    public static class ForgeTrashBin
    {
        public static List<GameObject> TrashedObjects { get; private set; } = new List<GameObject>();
        public static event Action OnTrashUpdated;

        public static void Toss(GameObject obj)
        {
            if (obj == null || TrashedObjects.Contains(obj)) return;

            obj.SetActive(false); // Remove it from Hermit's senses
            TrashedObjects.Add(obj);
            OnTrashUpdated?.Invoke();

            Debug.Log($"[ForgeTrash] '{obj.name}' was tossed into the bin.");
        }

        public static void Restore(GameObject obj)
        {
            if (TrashedObjects.Remove(obj))
            {
                if (obj != null) obj.SetActive(true);
                OnTrashUpdated?.Invoke();
            }
        }

        public static void Empty()
        {
            int count = 0;
            foreach (var obj in TrashedObjects)
            {
                if (obj != null)
                {
                    GameObject.DestroyImmediate(obj);
                    count++;
                }
            }
            TrashedObjects.Clear();
            OnTrashUpdated?.Invoke();
            Debug.Log($"[ForgeTrash] Incinerator activated. {count} objects permanently destroyed.");
        }
    }
}