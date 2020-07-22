using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

namespace Serialization_Manager
{
    public class SerializationManager : MonoBehaviour
    {
        public static string SaveObject<T>(T Data)
        {
            BinaryFormatter formatter = new BinaryFormatter();

            using (MemoryStream ms = new MemoryStream())
            {
                T data = Data;
                formatter.Serialize(ms, data);
                print(ms.ToArray().Length);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public static T LoadObject<T>(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                byte[] bytes = Convert.FromBase64String(key);
                BinaryFormatter formatter = new BinaryFormatter();

                using (var ms = new MemoryStream(bytes, 0, bytes.Length))
                {
                    T data = (T)formatter.Deserialize(ms);

                    return data;
                }
            }
            else
            {
                return default(T);
            }
        }
    }
}

namespace Serialization_Manager.Structs
{
    [Serializable]
    public struct TransformS
    {
        public Vector3S position;
        public QuaternionS rotation;

        public TransformS(Transform t)
        {
            position = new Vector3S(t.position);
            rotation = new QuaternionS(t.rotation);
        }

        public Transform Get(Transform t)
        {
            t.position = position.Get();
            t.rotation = rotation.Get();
            return t;
        }
    }

    [Serializable]
    public struct QuaternionS
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public QuaternionS(Quaternion q)
        {
            x = q.x;
            y = q.y;
            z = q.z;
            w = q.w;
        }

        public QuaternionS(object obj)
        {
            QuaternionS? v = obj as QuaternionS?;

            x = v.Value.x;
            y = v.Value.y;
            z = v.Value.z;
            w = v.Value.w;
        }

        public Quaternion Get()
        {
            return new Quaternion(x, y, z, w);
        }
    }

    [Serializable]
    public struct Vector3S
    {
        public float x;
        public float y;
        public float z;

        public Vector3S(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public Vector3S(object obj)
        {
            Vector3S? v = obj as Vector3S?;

            x = v.Value.x;
            y = v.Value.y;
            z = v.Value.z;
        }

        public Vector3 Get()
        {
            return new Vector3(x, y, z);
        }
    }

    [Serializable]
    public struct SavedString
	{
        public string Key;
        public string Value;

        public SavedString(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public SavedString(object obj)
        {
            SavedString? v = obj as SavedString?;

            Key = v.Value.Key;
            Value = v.Value.Value;
        }

        public SavedString Get()
        {
            return new SavedString(Key, Value);
        }
    }

    [Serializable]
    public struct SavedStrings
	{
        public List<SavedString> allStrings;

        public SavedStrings(List<SavedString> savedStrings)
        {
            allStrings = savedStrings;
        }

        public SavedStrings(object obj)
        {
            List<SavedString> v = obj as List<SavedString>;

            allStrings = v;
        }

        public SavedStrings Get()
        {
            return new SavedStrings(allStrings);
        }
    }
}