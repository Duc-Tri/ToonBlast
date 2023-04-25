using System.Collections.Generic;
using UnityEngine;

// un pooler basique
public class Pooler
{
    private GameObject prefab;
    private List<GameObject> pool = new List<GameObject>();
    private int numOp = 0;

    public Pooler(GameObject p, int preload = 0)
    {
        numOp = 0;
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
            go.name = "ITEM_NEW_" + (numOp++);
            return go;
        }

        go = pool[0];
        pool.RemoveAt(0);
        go.name = "ITEM_RESTORED_" + (numOp++);
        go.SetActive(true);
        return go;
    }

    public void SaveItem(GameObject go)
    {
        pool.Add(go);
        go.name = "ITEM_SAVED_" + (numOp++);
        go.SetActive(false);
    }

}

