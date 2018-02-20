using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class CorgiMemory : ICorgiLayer
{
    private long capacity = 50 * 1048576;
    private long useSize = 0;
    private Dictionary<string, CorgiMemoryChunk> chunkData = new Dictionary<string, CorgiMemoryChunk>();
    private LinkedList<string> chunkPriorityQueue = new LinkedList<string>();
    private Dictionary<string, ResolveAction> resolveQueues = new Dictionary<string, ResolveAction>();
    private Corgi corgi;

    public void Start(Corgi _corgi)
    {
        corgi = _corgi;
    }

    public void Save(Texture2D tex, string url, int version)
    {
        if (tex == null)
        {
            if (chunkData.ContainsKey(url))
                chunkData.Remove(url);

            if (chunkPriorityQueue.Any(x => x == url))
                chunkPriorityQueue.Remove(url);

            return;
        }


        var size = tex.GetRawTextureData().Length;
        chunkData[url] = new CorgiMemoryChunk() { url = url, version = version, tex = tex, size = size };
        chunkPriorityQueue.AddFirst(url);
        useSize += size;

        while (capacity < useSize)
        {
            RemoveLastChunk();
        }
    }

    public void Load(string url, int version, ResolveAction resolve, FallbackAction fallback)
    {
        CorgiMemoryChunk chunk = null;
        if (chunkData.TryGetValue(url, out chunk))
        {
            // 아래 레이어에서 로드중인데 계속 요청이 들어오면 해당 key resolveQueue에 저장했다가 완료시 한꺼번에 호출
            if (chunk == null)
            {
                Debug.Log("fetch pending.. " + url + " v." + version);
                ResolveAction delegates;
                if (resolveQueues.TryGetValue(url, out delegates))
                    delegates += resolve;
                else
                    resolveQueues.Add(url, resolve);

                return;
            }
            else if (version >= 0 && chunk.version >= version)
            {
                chunkPriorityQueue.Remove(chunk.url);
                chunkPriorityQueue.AddFirst(chunk.url);
                resolve(null, chunk.tex);
                return;
            }
            else
            {
                chunkPriorityQueue.Remove(chunk.url);
                RemoveChunk(chunk);
            }
        }

        //아래 레이어에서 로드중인경우 체크용으로 해당 key chunk에 null을 넣어둔다.
        chunkData.Add(url, null);
            
        fallback(url, version, (bytes, tex) =>
        {
            resolve(bytes, tex);
            OnResolve(url, version, tex);
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

    void RemoveLastChunk()
    {
        var lastKey = chunkPriorityQueue.Last.Value;
        chunkPriorityQueue.RemoveLast();
        CorgiChunk targetChunk = chunkData[lastKey];
        RemoveChunk(targetChunk);
    }

    void RemoveChunk(CorgiChunk chunk)
    {
        useSize -= chunk.size;
        chunkData.Remove(chunk.url);
    }

    void OnResolve(string url, int version, Texture2D tex)
    {
        Save(tex, url, version);
        ResolveAction resolves;
        if(resolveQueues.TryGetValue(url, out resolves))
        {
            resolves(null, tex);
            resolveQueues.Remove(url);
        }
    }
}

