using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestModule : MonoBehaviour
{
    public List<string> urls;
    public RawImage targetImg;
    private int index = -1;

    private DateTime startTime;

	void Start ()
    {
        ChangeImageIndex(0);
    }

    public void NextUrl()
    {
        ChangeImageIndex((index + 1) % urls.Count);
    }

    void ChangeImageIndex(int _index)
    {
        if (index == _index)
            return;

        index = _index;
        startTime = DateTime.Now;
        Corgi.Fetch(urls[index], 0, OnFetchDone, OnFetchFailed);
    }


    void OnFetchDone(Texture2D tex)
    {
        Debug.Log("Timespan : " + (DateTime.Now - startTime).TotalMilliseconds + " ms");
        targetImg.texture = tex;
    }

    void OnFetchFailed(string url, int version)
    {
        Debug.Log("Fetch Faield " + url + " v" + version);
    }
}
