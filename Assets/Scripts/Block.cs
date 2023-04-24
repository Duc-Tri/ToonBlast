using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Block : MonoBehaviour
{
    // data =====================================
    private const int MAX_BLOCKS = 5;

    public int colorBlock;

    // graphics =================================
    [SerializeField]
    private Sprite[] blockSprites;
    private SpriteRenderer spriteRenderer;
    public bool disappearingAnimation;
    public static int numBlocksDisapearing = 0;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetBlock(UnityEngine.Random.Range(0, MAX_BLOCKS));
    }

    internal void SetBlock(int color)
    {
        colorBlock = color;
        spriteRenderer.sprite = blockSprites[colorBlock];
        spriteRenderer.color = Color.white;
        spriteRenderer.transform.localScale = Vector3.one;

    }

    internal void SetGridXY(int x, int y)
    {
        transform.position = new Vector2(x + GameGrid.X_OFFSET, y + GameGrid.Y_OFFSET);
        MainGame.gameGrid.SetBlockInGrid(this, x, y);
    }

    public void SetRandomColor()
    {
        SetBlock(UnityEngine.Random.Range(0, MAX_BLOCKS));
    }

    public void Disapear()
    {
        numBlocksDisapearing++;
        StartCoroutine(FadeToCLear());
    }

    static WaitForSeconds wait = new WaitForSeconds(0.015f);
    private float SPEED_ANIM = 1f;
    const float ANIM_STEPS = 100f;
    private IEnumerator FadeToCLear()
    {
        spriteRenderer.transform.localScale = Vector3.one * 1.2f;
        for (int step = 0; step < ANIM_STEPS; step++)
        {
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, Color.clear, step / ANIM_STEPS);
            spriteRenderer.transform.localScale = Vector3.Lerp(spriteRenderer.transform.localScale, Vector3.zero, step / ANIM_STEPS);
            yield return wait;
        }
        //while (spriteRenderer.color.a > 0.01f);

        Debug.Log("FadeToCLear ................. " + this.name);
        numBlocksDisapearing--;
        MainGame.blocksPooler.SaveItem(this.gameObject);
    }

}