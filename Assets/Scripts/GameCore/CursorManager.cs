using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static Dictionary<string, bool> locks = new Dictionary<string, bool>();


    static void LockCursor(bool lockMode)
	{
        Cursor.lockState = lockMode ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockMode;
    }

    public static void RefreshLock(string name, bool lockMode)
	{
		if (locks.ContainsKey(name))
		{
			locks.Remove(name);
            locks.Add(name, lockMode);
		}
		else
		{
            locks.Add(name, lockMode);
        }

        bool locked = true;
        foreach(bool b in locks.Values)
		{
            if (!b)
            {
                locked = false;
                break;
            }
		}

        LockCursor(locked);
	}

    public static void RemoveAll()
	{
        foreach (string n in locks.Keys.ToArray())
        {
            locks.Remove(n);
        }
	}
}