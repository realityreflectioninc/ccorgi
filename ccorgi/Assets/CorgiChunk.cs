using UnityEngine;

public class CorgiChunk
{
    public string url;
    public int version;
    public int size;
}

public class CorgiMemorize
{
    public string url;
    public int version;
}

public class CorgiDiskChunk : CorgiChunk
{
    public string path;
}

public class CorgiMemoryChunk : CorgiChunk
{
    public Texture2D tex;
}