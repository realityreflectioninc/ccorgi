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
    public Dictionary<string, CorgiDiskChunk> map = new Dictionary<string, CorgiDiskChunk>();
    public LinkedList<string> queue = new LinkedList<string>();
    public long capacity = 500 * 1048576;
    public long useSize = 0;
}

public class CorgiDisk : MonoBehaviour
{
    Dictionary<string, CorgiDiskChunk> map = new Dictionary<string, CorgiDiskChunk>();
    LinkedList<string> queue = new LinkedList<string>();
    long capacity = 500 * 1048576;
    long useSize = 0;

    public void Init(CorgiDiskData data)
    {
        if (data == null)
            return;

        if (data.map != null)
            map = data.map;

        if (data.queue != null)
            queue = data.queue;

        if (data.capacity > 0)
            capacity = data.capacity;

        if (data.useSize > 0)
            useSize = data.useSize;
    }

    public CorgiDiskData GetData()
    {
        return new CorgiDiskData()
        {
            map = map,
            queue = queue,
            capacity = capacity,
            useSize = useSize,
        };
    }

    public void Save(byte[] bytes, string url, int version)
    {
        var path = Path.GetRandomFileName();
        var size = bytes.Length;
        map.Add(url, new CorgiDiskChunk() { url = url, version = version, path = path, size = size });
        queue.AddFirst(url);
        useSize += size;
        SaveFile(path, bytes);
        while (capacity < useSize)
        {
            RemoveLastChunk();
        }
    }

    public void Load(string url, int version, Action<Texture2D> resolve, FallbackAction reject)
    {
        CorgiDiskChunk chunk = null;
        if(map.TryGetValue(url, out chunk))
        {
            if (chunk.version >= version)
            {
                queue.Remove(chunk.url);
                queue.AddFirst(chunk.url);
                StartCoroutine(DownloadLocal(chunk, resolve, reject));
            }
            else
            {
                queue.Remove(chunk.url);
                RemoveChunk(chunk);
                StartCoroutine(DownloadURL(url, version, (bytes, tex) =>
                {
                    Save(bytes, chunk.url, chunk.version);
                    resolve(tex);
                }, reject));
            }   
        }
        else
        {
            StartCoroutine(DownloadURL(url, version, (bytes, tex) =>
            {
                Save(bytes, url, version);
                resolve(tex);
            }, reject));
        }
    }

    public void SetCapacity(int _capacity)
    {
        capacity = _capacity;
        while (capacity < useSize)
        {
            RemoveLastChunk();
        }
    }

    void RemoveLastChunk()
    {
        var lastKey = queue.Last.Value;
        CorgiDiskChunk targetChunk;
        if(map.TryGetValue(lastKey, out targetChunk))
            RemoveChunk(targetChunk);
    }

    void RemoveChunk(CorgiDiskChunk chunk)
    {
        Debug.Log("Remove Chunk" + chunk.url);
        var realPath = Path.Combine(Application.temporaryCachePath, chunk.path);
        useSize -= chunk.size;
        map.Remove(chunk.url);
        var t = new System.Threading.Thread(() =>
        {
            if (File.Exists(realPath))
                File.Delete(realPath);
        });

        t.Start();
    }

    void SaveFile(string path, byte[] data)
    {
        var realPath = Path.Combine(Application.temporaryCachePath, path);
        Debug.Log("Try SaveFile " + realPath + " " + data.Length);
        var t = new System.Threading.Thread(() =>
        {
            File.WriteAllBytes(realPath, data);
        });
        t.Start();
    }

    IEnumerator DownloadLocal(CorgiDiskChunk chunk, Action<Texture2D> resolve, FallbackAction reject)
    {
        var realPath = Path.Combine(Application.temporaryCachePath, chunk.path);
        var www = new WWW("file:///" + realPath);
        Debug.Log("path=" + "file:///" + realPath);
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.Log("[ERR]" + www.error + "\n" + chunk.url + " " + chunk.path);
            RemoveChunk(chunk);
            yield return DownloadURL(chunk.url, chunk.version, (bytes, tex) =>
            {
                Save(bytes, chunk.url, chunk.version);
                resolve(tex);
            }, reject);
        }
        else
        {
            Debug.Log("Disk hit");
            Texture2D tex = new Texture2D(0, 0);
            www.LoadImageIntoTexture(tex);
            resolve(tex);
        }
    }


    IEnumerator DownloadURL(string url, int version, Action<byte[], Texture2D> resolve, FallbackAction reject)
    {
        var www = new WWW(url);
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            reject(url, version);
        }
        else
        {
            Debug.Log("Web hit");
            Texture2D tex = new Texture2D(0, 0);
            www.LoadImageIntoTexture(tex);
            resolve(www.bytes, tex);
        }
    }
}
