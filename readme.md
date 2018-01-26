ccorgi
====

```csharp
Corgi.Fetch((texture) => {
  // Here's your image.
});
```

```csharp
Corgi.AddCacheLayer(100, (url, version, resolver) => {
  resolver(/* ... */);
});

Corgi.Fallback((url, version, resolver) => {
  resolver(DownloadTextureFrom(url));
});
```

capacity
----
```csharp
Corgi.Capacity("memory", count:50, size: 50, algo: "lru");
Corgi.Capacity("disk", size: 500, algo: "lru");
```

preload
----
```chsarp
Corgi.Memorize();
```

