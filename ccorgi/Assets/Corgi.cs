using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corgi : MonoBehaviour
{
    private static Corgi _instance;
    public static Corgi instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("IMAGE_CACHE");
                _instance = go.AddComponent<Corgi>();
            }
            return _instance;
        }
    }

    public class CorgiChunk
    {
        public string url;
        public int version;
        public int refCount;
        public Texture2D textureRef;
    }

    public delegate void ResolveAction(Texture2D texture);
    public delegate void RejectAction(string errorMessage);
    public delegate void CorgiAction(string url, int version, ResolveAction resolve);

    public int preload_capacity = 10;
    public int memory_capacity = 50;
    public int disk_capacity = 500;

    Dictionary<string, CorgiChunk> memoryCache;
    Dictionary<string, CorgiChunk> diskCache;

    public enum CorgiLayer
    {
        PRELOAD,
        MEMORY,
        DISK,
        WEB,
    }

    public static void Fetch(string url, int version, ResolveAction resolve, RejectAction reject)
    {
        instance._Fetch(url, version, resolve, reject);
    }

    public void _Fetch(string url, int version, ResolveAction resolve, RejectAction reject)
    {

    }

    public static void AddCacheLayer(int priority, CorgiAction action)
    {
        instance._AddCacheLayer(priority, action);
    }

    public void _AddCacheLayer(int priority, CorgiAction action)
    {

    }

    public static void Capacity(CorgiLayer layer, int size)
    {
        instance._Capacity(layer, size);
    }

    public void _Capacity(CorgiLayer layer, int size)
    {

    }

    public static void Memorize()
    {
        instance._Memorize();
    }

    public void _Memorize()
    {

    }
}
