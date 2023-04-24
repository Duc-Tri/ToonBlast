using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pooler
{
    /*
    private Pooler _instance;
    public Pooler Instance
    {
        get
        {
            if (_instance == null)
                _instance = new Pooler();
            return _instance;
        }
    }
    */

    private GameObject prefab;
    private List<GameObject> pool = new List<GameObject>();
    int num = 0;

    public Pooler(GameObject p, int preload = 0)
    {
        prefab = p;
        pool = new List<GameObject>(preload);
        for (int i = 0; i < preload; i++)
        {
            pool.Add(GetItem());
        }
    }

    public GameObject GetItem()
    {
        GameObject go;
        if (pool.Count == 0)
        {
            go = GameObject.Instantiate(prefab); //.GetComponent<Block>();
            go.name = "ITEM_NEW_" + (num++);
            return go;
        }

        go = pool[0];
        pool.RemoveAt(0);
        go.name = "ITEM_RESTORE_" + (num++);
        go.SetActive(true);
        return go;
    }

    public void SaveItem(GameObject go)
    {
        pool.Add(go);
        go.name = "ITEM_SAVE_" + (num++);
        go.SetActive(false);
    }

}

