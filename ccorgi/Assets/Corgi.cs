using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public delegate void FallbackAction(string url, int version, ResolveAction resolve);
public delegate void ResolveAction(byte[] bytes, Texture2D tex);

public class Corgi : MonoBehaviour
{
    public static int DownloadAlways = -1;
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

    private List<ICorgiLayer> corgiLayers;
    private List<CorgiMemorize> memorizeList;
    private FallbackAction fallbackDelegate;

    void Awake()
    {
        if(_instance == null)
            _instance = this;

        DontDestroyOnLoad(this);

        corgiLayers = new List<ICorgiLayer>(){ new CorgiMemory(), new CorgiDisk(),  new CorgiWeb() };

        foreach (var corgiLayer in corgiLayers)
        {
            corgiLayer.Start(this);
        }
    }

    void OnDestroy()
    {
    }

    public static void Fetch(string url, int version, ResolveAction resolve)
    {
        instance._Fetch(url, version, resolve, 0);
    }

    void _Fetch(string url, int version, ResolveAction resolve, int layerIndex)
    {
        if (layerIndex >= corgiLayers.Count)
        {
            fallbackDelegate(url, version, resolve);
            return;
        }

        corgiLayers[layerIndex].Load(url, version, resolve,
            (_url, _version, _resolve) => {
                _Fetch(_url, _version, _resolve, layerIndex + 1);
            });
    }

    /*
    public static void AddCacheLayer(int priority, FallbackAction action)
    {
        instance._AddCacheLayer(priority, action);
    }

    void _AddCacheLayer(int priority, FallbackAction action)
    {
    }
    */

    public static void Fallback(FallbackAction fallback)
    {
        instance._Fallback(fallback);
    }

    void _Fallback(FallbackAction _fallback)
    {
        fallbackDelegate = _fallback;
    }

    public static void Memorize(CorgiMemorize[] memo)
    {
        foreach (var m in memo)
        {
            if (string.IsNullOrEmpty(m.url))
                continue;

            Fetch(m.url, m.version, (bytes, tex) => { });
        }
    }
}