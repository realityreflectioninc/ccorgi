using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;


public class CorgiWeb : MonoBehaviour
{
    public void Load(string url, int version, ResolveAction resolve, FallbackAction fallback)
    {
        StartCoroutine(DownloadURL(url, version, resolve, fallback));
    }
    
    IEnumerator DownloadURL(string url, int version, ResolveAction resolve, FallbackAction fallback)
    {
        var www = new WWW(url);
        yield return www;
        if (string.IsNullOrEmpty(www.error))
        { 
            Debug.Log("Web hit");
            Texture2D tex = new Texture2D(0, 0);
            www.LoadImageIntoTexture(tex);
            resolve(tex);
        }
        else
        {
            Debug.Log("Web Failed!");
            fallback(url, version, resolve);
        }
    }
}
