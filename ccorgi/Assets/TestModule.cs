using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TestModule : MonoBehaviour
{
    List<string> urls = new List<string>();
    public RawImage targetImg;
    private int index = -1;
    public Button button;
    public Text text;
    public Texture2D fallbackTex;
    private DateTime startTime;

    void Awake()
    {
        var rand = new System.Random();
        for (int i = 0; i < 10; ++i)
        {
            urls.Add("https://picsum.photos/200/300/?image=" + rand.Next() % 100);
        }
    }

	void Start ()
    {
        ChangeImageIndex(0);
        button.onClick.AddListener(NextUrl);
        Corgi.Memorize(urls.Select(x => new CorgiMemorize() { url = x, version = 0 }).ToList());
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
        Corgi.Fetch(urls[index], 0, OnFetchDone);
        Corgi.Fallback(OnFetchFailed);
    }


    void OnFetchDone(Texture2D tex)
    {
        button.enabled = true;
        Debug.Log("Timespan : " + (DateTime.Now - startTime).TotalMilliseconds + " ms");
        targetImg.texture = tex;
        text.text = "Timespan : " + (DateTime.Now - startTime).TotalMilliseconds + " ms";
    }

    void OnFetchFailed(string url, int version, ResolveAction resolve)
    {
        Debug.Log("Fetch Failed! " + url + " v." + version);
        resolve(fallbackTex);
    }
}
