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
