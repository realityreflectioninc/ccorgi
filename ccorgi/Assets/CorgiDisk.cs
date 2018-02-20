using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class CorgiDiskData
{
    public Dictionary<string, CorgiDiskChunk> chunkData = new Dictionary<string, CorgiDiskChunk>();
    public LinkedList<string> chunkPriorityQueue = new LinkedList<string>();
    public long capacity = 500 * 1048576;
    public long useSize = 0;
}

public class CorgiDisk : ICorgiLayer
{
    const string INIT_DATA_PATH = "corgi_init_data.json";

    private Dictionary<string, CorgiDiskChunk> chunkData = new Dictionary<string, CorgiDiskChunk>();
    private LinkedList<string> chunkPriorityQueue = new LinkedList<string>();
    private long capacity = 500 * 1048576;
    private long useSize = 0;
    private Corgi corgi;

    public void Start(Corgi _corgi)
    {
        corgi = _corgi;

        var path = Path.Combine(Application.persistentDataPath, INIT_DATA_PATH);
        if (!File.Exists(path))
            return;

        var content = File.ReadAllText(path);
        if (string.IsNullOrEmpty(content))
            return;

        var initData = JsonConvert.DeserializeObject<CorgiDiskData>(content);
        if (initData == null)
            return;

        chunkData = initData.chunkData;
        chunkPriorityQueue = initData.chunkPriorityQueue;
        capacity = initData.capacity;
        useSize = initData.useSize;
    }

    public void Save(byte[] bytes, string url, int version)
    {
        if (bytes == null || bytes.Length == 0)
            return;

        Debug.Log("Corgi Save " + url + " v." + version);
        //var bytes = tex.GetRawTextureData();
        var path = Path.GetRandomFileName();
        var size = bytes.Length;
        chunkData[url] = new CorgiDiskChunk() { url = url, version = version, path = path, size = size };
        chunkPriorityQueue.AddFirst(url);
        useSize += size;
        SaveFile(path, bytes);
        while (capacity < useSize)
        {
            RemoveLastChunk();
        }

        SaveCorgiDiskData();
    }

    public void Load(string url, int version, ResolveAction resolve, FallbackAction fallback)
    {
        CorgiDiskChunk chunk = null;
        if(chunkData.TryGetValue(url, out chunk))
        {
            if (version >= 0 && chunk.version >= version)
            {
                chunkPriorityQueue.Remove(chunk.url);
                chunkPriorityQueue.AddFirst(chunk.url);
                corgi.StartCoroutine(DownloadLocal(chunk, resolve, fallback));
                return;
            }
            else
            {
                chunkPriorityQueue.Remove(chunk.url);
                RemoveChunk(chunk);
            }   
        }

        fallback(url, version, (bytes, tex) =>
        {
            Save(bytes, url, version);
            resolve(bytes, tex);
        });
    }

    public void SetCapacity(int _capacity)
    {
        capacity = _capacity;
        while (capacity < useSize)
        {
            RemoveLastChunk();
        }
    }

    private void RemoveLastChunk()
    {
        var lastKey = chunkPriorityQueue.Last.Value;
        CorgiDiskChunk targetChunk;
        if(chunkData.TryGetValue(lastKey, out targetChunk))
            RemoveChunk(targetChunk);
    }

    private void RemoveChunk(CorgiDiskChunk chunk)
    {
        Debug.Log("Remove Chunk" + chunk.url);
        var realPath = Path.Combine(Application.temporaryCachePath, chunk.path);
        useSize -= chunk.size;
        chunkData.Remove(chunk.url);
        var t = new System.Threading.Thread(() =>
        {
            if (File.Exists(realPath))
                File.Delete(realPath);
        });

        t.Start();
    }

    private void SaveFile(string path, byte[] data)
    {
        var realPath = Path.Combine(Application.temporaryCachePath, path);
        Debug.Log(realPath);
        var t = new System.Threading.Thread(() =>
        {
            File.WriteAllBytes(realPath, data);
        });
        t.Start();
    }

    private void SaveCorgiDiskData()
    {
        var path = Path.Combine(Application.persistentDataPath, INIT_DATA_PATH);
        string content = JsonConvert.SerializeObject(new CorgiDiskData()
        {
            chunkData = chunkData,
            chunkPriorityQueue = chunkPriorityQueue,
            capacity = capacity,
            useSize = useSize,
        });

        Debug.Log(content);
        var t = new System.Threading.Thread(() =>
        {
            File.WriteAllText(path, content);
        });
        t.Start();
    }

    IEnumerator DownloadLocal(CorgiDiskChunk chunk, ResolveAction resolve, FallbackAction fallback)
    {
        var realPath = Path.Combine(Application.temporaryCachePath, chunk.path);
        var www = new WWW("file:///" + realPath);
        Debug.Log("path=" + "file:///" + realPath);
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.Log("[ERR]" + www.error + "\n" + chunk.url + " " + chunk.path);
            RemoveChunk(chunk);
            fallback(chunk.url, chunk.version, (bytes, tex) => {
                Save(bytes, chunk.url, chunk.version);
                resolve(bytes, tex);
            });
        }
        else
        {
            Debug.Log("Disk hit");
            Texture2D tex = new Texture2D(0, 0);
            www.LoadImageIntoTexture(tex);
            resolve(null, tex);
        }
    }
}
