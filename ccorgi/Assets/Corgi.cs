using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    public delegate void FallbackAction(string url, int version);
    public delegate void CorgiAction(string url, int version, Action<Texture2D> resolve);

    public int preload_capacity = 10;
    public int memory_capacity = 50;
    public int disk_capacity = 500;
    //public FallbackAction fallback_delegate;

    const string DISK_CACHE_PATH = "diskcache_test.json";

    Dictionary<string, Texture2D> memoryCache = new Dictionary<string, Texture2D>();
    Dictionary<string, string> diskCache = new Dictionary<string, string>();

    // TODO
    // CorgiDisk corgiDisk = new CorgiDisk();


    public enum CorgiLayer
    {
        PRELOAD,
        MEMORY,
        DISK,
        WEB,
    }

    void Awake()
    {
        if(_instance == null)
            _instance = this;

        DontDestroyOnLoad(this);
        var path = Path.Combine(Application.persistentDataPath, DISK_CACHE_PATH);
        var content = File.ReadAllText(path);
        Debug.Log("Awake Data" + content);

        if (!string.IsNullOrEmpty(content))
            diskCache = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);

        if (diskCache == null)
            diskCache = new Dictionary<string, string>();
    }
    
    void OnDestroy()
    {
        var path = Path.Combine(Application.persistentDataPath, DISK_CACHE_PATH);
        string content = JsonConvert.SerializeObject(diskCache);
        Debug.Log(content);
        File.WriteAllText(path, content);
    }

    public static void Fetch(string url, int version, Action<Texture2D> resolve, FallbackAction reject)
    {
        instance._Fetch(url, version, resolve, reject);
    }

    public void _Fetch(string url, int version, Action<Texture2D> resolve, FallbackAction reject)
    {
        Texture2D tex;
        string path;
        if (memoryCache.TryGetValue(url, out tex))
        {
            Debug.Log("Memory hit");
            resolve(tex);
        }
        else if (diskCache.TryGetValue(url, out path))
        {
            StartCoroutine(DownloadLocal(url, version, path, resolve, reject));
        }
        else
        {
            StartCoroutine(DownloadURL(url, version, (bytes, texture) =>
            {
                resolve(texture);
                SaveFile(bytes, url);
            }, reject));
        }
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

    void SaveFile(byte[] bytes, string url)
    {
        var path = Path.GetRandomFileName();
        diskCache[url] = path;
        var realPath = Path.Combine(Application.temporaryCachePath, path);
        var t = new System.Threading.Thread(() =>
        {
            File.WriteAllBytes(realPath, bytes);
        });
        t.Start();
    }

    IEnumerator DownloadURL(string url, int version, Action<byte[], Texture2D> callback, FallbackAction reject)
    {
        var www = new WWW(url);
        yield return www;
        if(!string.IsNullOrEmpty(www.error))
        {
            reject(url, version);
            //fallback_delegate(url, version);
        }
        else
        {
            Debug.Log("Web hit");
            Texture2D tex;
            if (!memoryCache.TryGetValue(url, out tex))
            {
                tex = new Texture2D(0, 0);
                memoryCache.Add(url, tex);
            }
            www.LoadImageIntoTexture(tex);
            callback(www.bytes, tex);
        }
    }

    IEnumerator DownloadLocal(string url, int version, string path, Action<Texture2D> resolve, FallbackAction reject)
    {
        var realPath = Path.Combine(Application.temporaryCachePath, path);
        var www = new WWW("file:///" + realPath);
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.Log("Download Local Failed!");
            yield return DownloadURL(url, version, (bytes, texture) =>
            {
                resolve(texture);
                SaveFile(bytes, url);
            }, reject);
        }
        else
        {
            Debug.Log("Disk hit");
            Texture2D tex;
            if (!memoryCache.TryGetValue(url, out tex))
            {
                tex = new Texture2D(0, 0);
                memoryCache.Add(url, tex);
            }
            www.LoadImageIntoTexture(tex);
            resolve(tex);
        }
    }
}