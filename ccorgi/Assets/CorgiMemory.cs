using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class CorgiMemory : MonoBehaviour
{
    long capacity = 50 * 1048576;
    long useSize = 0;
    Dictionary<string, CorgiMemoryChunk> data = new Dictionary<string, CorgiMemoryChunk>();
    LinkedList<string> queue = new LinkedList<string>();

    public void Save(Texture2D tex, string url, int version)
    {
        var size = tex.GetRawTextureData().Length;
        data[url] = new CorgiMemoryChunk() { url = url, version = version, tex = tex, size = size };
        queue.AddFirst(url);
        useSize += size;

        while (capacity < useSize)
        {
            RemoveLastChunk();
        }
    }

    public void Load(string url, int version, ResolveAction resolve, FallbackAction fallback)
    {
        CorgiMemoryChunk chunk = null;
        if (data.TryGetValue(url, out chunk))
        {
            if (chunk.version >= version)
            {
                queue.Remove(chunk.url);
                queue.AddFirst(chunk.url);
                resolve(chunk.tex);
                return;
            }
            else
            {
                queue.Remove(chunk.url);
                RemoveChunk(chunk);
            }
        }

        ResolveAction newResolve = (tex) =>
        {
            Debug.Log("memory new resolved!");
            resolve(tex);
            Save(tex, url, version);
        };
        fallback(url, version, newResolve);
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
        CorgiChunk targetChunk = data[lastKey];
        RemoveChunk(targetChunk);
    }

    void RemoveChunk(CorgiChunk chunk)
    {
        useSize -= chunk.size;
        data.Remove(chunk.url);
    }
}

