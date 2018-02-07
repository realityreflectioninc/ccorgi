using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public delegate void FallbackAction(string url, int version);
public delegate void CorgiAction(string url, int version, Action<Texture2D> resolve);

public class CorgiInitData
{
    public CorgiDiskData diskData;
    public List<CorgiMemorize> memorize; 
}

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

    //public FallbackAction fallback_delegate;
    const string INIT_DATA_PATH = "corgi_init_data.json";
    CorgiDisk disk;
    CorgiMemory memory;
    List<CorgiMemorize> memorize;
    FallbackAction fallbacks;

    void Awake()
    {
        if(_instance == null)
            _instance = this;

        DontDestroyOnLoad(this);

        disk = gameObject.AddComponent<CorgiDisk>();
        memory = gameObject.AddComponent<CorgiMemory>();

        var path = Path.Combine(Application.persistentDataPath, INIT_DATA_PATH);
        if (!File.Exists(path))
            return;

        var loadData = File.ReadAllText(path);
        Debug.Log("Awake Data" + loadData);

        if (string.IsNullOrEmpty(loadData))
            return;

        var initData = JsonConvert.DeserializeObject<CorgiInitData>(loadData);
        if (initData == null)
            return;

        disk.Init(initData.diskData);

        if (initData.memorize == null)
            return;

        foreach (var memo in initData.memorize)
        {
            _Fetch(memo.url, memo.version, (tex) => { }, (url, version) => { });
        }
    }

    void OnDestroy()
    {
    }

    public static void Fetch(string url, int version, Action<Texture2D> resolve, FallbackAction reject)
    {
        instance._Fetch(url, version, resolve, reject);
    }

    void _Fetch(string url, int version, Action<Texture2D> resolve, FallbackAction reject)
    {
        memory.Load(url, version, resolve, (u, v) =>
        {
            disk.Load(url, version, (tex) =>
            {
                resolve(tex);
                memory.Save(tex, url, version);
                _SaveData();
            }, 
            fallbacks);
        });
    }

    public static void AddCacheLayer(int priority, CorgiAction action)
    {
        instance._AddCacheLayer(priority, action);
    }

    void _AddCacheLayer(int priority, CorgiAction action)
    {
    }

    public static void Fallback(FallbackAction fallback)
    {
        instance._Fallback(fallback);
    }

    void _Fallback(FallbackAction fallback)
    {
        fallbacks += fallback;
    }

    public static void Memorize(List<CorgiMemorize> memo)
    {
        instance._Memorize(memo);
    }

    void _Memorize(List<CorgiMemorize> memo)
    {
        memorize = memo;
    }

    public static void SaveData()
    {
        _instance._SaveData();
    }

    public void _SaveData()
    {
        CorgiInitData saveData = new CorgiInitData();
        saveData.diskData = disk.GetData();
        saveData.memorize = memorize;
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