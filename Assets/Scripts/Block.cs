using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Block : MonoBehaviour
{
    // data =====================================
    private const int MAX_BLOCKS = 5;
    [SerializeField]
    private int numBlock;

    // graphics =================================
    [SerializeField]
    private Sprite[] blockSprites;
    private SpriteRenderer spriteRenderer;


    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetBlock(UnityEngine.Random.Range(0, MAX_BLOCKS));
    }

    internal void SetBlock(int v)
    {
        numBlock = v;
        spriteRenderer.sprite = blockSprites[numBlock];
    }

    internal void SetGridXY(int x, int y)
    {
        transform.position = new Vector2(x + GameGrid.X_OFFSET, y + GameGrid.Y_OFFSET);
        MainGame.gameGrid.SetBlock(this, x, y);
    }
}