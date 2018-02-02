using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestModule : MonoBehaviour
{
    List<string> urls = new List<string>();
    public RawImage targetImg;
    private int index = -1;
    public Button button;
    private DateTime startTime;

    void Awake()
    {
        var rand = new System.Random();
        for (int i = 0; i < 10; ++i)
        {
            urls.Add("https://picsum.photos/200/300/?image=" + rand.Next() % 10);
        }
    }

	void Start ()
    {
        ChangeImageIndex(0);
        
        button.onClick.AddListener(NextUrl);
    }

    public void NextUrl()
    {
        ChangeImageIndex((index + 1) % urls.Count);
    }

    void ChangeImageIndex(int _index)
    {
        if (index == _index)
            return;

        button.enabled = false;
        index = _index;
        startTime = DateTime.Now;
        Corgi.Fetch(urls[index], 0, OnFetchDone, OnFetchFailed);
    }


    void OnFetchDone(Texture2D tex)
    {
        button.enabled = true;
        Debug.Log("Timespan : " + (DateTime.Now - startTime).TotalMilliseconds + " ms");
        targetImg.texture = tex;
    }

    void OnFetchFailed(string url, int version)
    {
        button.enabled = true;
        Debug.Log("Fetch Faield " + url + " v" + version);
    }
}
