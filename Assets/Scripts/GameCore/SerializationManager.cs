using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Serialization_Manager
{
    public class SerializationManager : MonoBehaviour
    {
        public static bool SaveObject<T>(string key, T Data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            string path = Path.Combine(Application.persistentDataPath + $"/{key}.data");
            FileStream stream = new FileStream(path, FileMode.Create);

            T data = Data;

            formatter.Serialize(stream, data);
            stream.Close();

            return true;
        }

        public static T LoadObject<T>(string key)
        {
            string path = Path.Combine(Application.persistentDataPath + $"/{key}.data");

            if (File.Exists(path))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(path, FileMode.Open);

                T data = (T)formatter.Deserialize(stream);
                stream.Close();

                return data;
            }
            else
            {
                Debug.LogError("Save file not found in " + path);
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