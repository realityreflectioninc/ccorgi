using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class CorgiDisk : MonoBehaviour
{
    public int capacity;
    public int useSize;
    public Dictionary<string, CorgiChunk> data = new Dictionary<string, CorgiChunk>();
    public LinkedList<string> queue = new LinkedList<string>();

    public void SaveData(string url, int version, string path, int size)
    {
        data.Add(url, new CorgiChunk() { url = url, version = version, path = path, size = size });
        queue.AddFirst(url);
        useSize += size;
        //SaveFile(path, bytes);

        while (capacity < useSize)
        {
            RemoveLastChunk();
        }
    }

    public CorgiChunk GetData(string url, int version)
    {
        CorgiChunk chunk = null;
        if(data.TryGetValue(url, out chunk))
        {
            if (chunk.version >= version)
            {
                queue.Remove(chunk.url);
                queue.AddFirst(chunk.url);
                return chunk;
            }
            else
            {
                
            }
        }
        return null;
    }

    void RemoveLastChunk()
    {
        var lastKey = queue.Last.Value;
        CorgiChunk targetChunk = data[lastKey];
        useSize -= targetChunk.size;
        RemoveFile(targetChunk.path);
    }

    void RemoveFile(string path)
    {
        var realPath = Path.Combine(Application.temporaryCachePath , path);
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
        var t = new System.Threading.Thread(() =>
        {
            File.WriteAllBytes(path, data);
        });
        t.Start();
    }

    /*
    IEnumerator DownloadURL(string url, int version, Action<byte[], Texture2D> callback, FallbackAction reject)
    {
        var www = new WWW(url);
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
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
    */
}

