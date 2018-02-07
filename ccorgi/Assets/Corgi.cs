using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public delegate void FallbackAction(string url, int version, ResolveAction resolve);
public delegate void ResolveAction(Texture2D tex);

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

    const string INIT_DATA_PATH = "corgi_init_data.json";

    CorgiDisk disk;
    CorgiMemory memory;
    CorgiWeb web;
    List<CorgiMemorize> memorize;
    FallbackAction fallback;

    void Awake()
    {
        if(_instance == null)
            _instance = this;

        DontDestroyOnLoad(this);

        disk = gameObject.AddComponent<CorgiDisk>();
        memory = gameObject.AddComponent<CorgiMemory>();
        web = gameObject.AddComponent<CorgiWeb>();

        var path = Path.Combine(Application.persistentDataPath, INIT_DATA_PATH);
        if (!File.Exists(path))
            return;

        var content = File.ReadAllText(path);
        Debug.Log("Awake Data" + content);

        if (string.IsNullOrEmpty(content))
            return;

        var initData = JsonConvert.DeserializeObject<CorgiDiskData>(content);
        if (initData == null)
            return;

        disk.Init(initData);
    }

    void OnDestroy()
    {
    }

    public static void Fetch(string url, int version, ResolveAction resolve)
    {
        instance._Fetch(url, version, resolve);
    }

    void _Fetch(string url, int version, ResolveAction resolve)
    {
        memory.Load(url, version, resolve, OnMemoryFaield);
    }

    void OnMemoryFaield(string url, int version, ResolveAction resolve)
    {
        disk.Load(url, version, resolve, OnDiskFailed);
    }

    void OnDiskFailed(string url, int version, ResolveAction resolve)
    {
        web.Load(url, version, resolve, fallback);
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
        _fallback += _fallback;
    }

    public static void Memorize(List<CorgiMemorize> memo)
    {
        instance._Memorize(memo);
    }

    void _Memorize(List<CorgiMemorize> memo)
    {
        foreach (var m in memo)
        {
            _Fetch(m.url, m.version, (tex) => { });
        }
    }

    public static void SaveData()
    {
        _instance._SaveData();
    }

    public void _SaveData()
    {
        CorgiDiskData saveData = disk.GetData();
        if (saveData == null)
            return;

        var path = Path.Combine(Application.persistentDataPath, INIT_DATA_PATH);
        string content = JsonConvert.SerializeObject(saveData);
        Debug.Log(content);
        var t = new System.Threading.Thread(() =>
        {
            File.WriteAllText(path, content);
        });
        t.Start();
    }



}