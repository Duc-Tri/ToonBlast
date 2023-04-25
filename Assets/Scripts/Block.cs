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
    public static int numBlocksMoving = 0;

    private static GameGrid grid;
    private static Pooler pooler;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        InitBlock(UnityEngine.Random.Range(0, MAX_BLOCKS));

        if (grid == null)
            grid = MainGame.gameGrid;

        if (pooler == null)
            pooler = MainGame.blocksPooler;
    }

    internal void InitBlock(int color)
    {
        colorBlock = color;
        spriteRenderer.sprite = blockSprites[colorBlock];
        spriteRenderer.color = Color.white;
        spriteRenderer.transform.localScale = Vector3.one;
    }

    internal void PutIntoGrid(int x, int y)
    {
        transform.position = new Vector2(x + GameGrid.X_OFFSET, y + GameGrid.Y_OFFSET);
        grid.SetBlockInGrid(this, x, y);
    }

    public void SetRandomColor()
    {
        InitBlock(UnityEngine.Random.Range(0, MAX_BLOCKS));
    }

    public void Disapear()
    {
        grid.BlockDisapperingCount(++numBlocksDisapearing); // à remplacer par un event/delegate ...
        StartCoroutine(FadeToClear());
    }

    static WaitForSeconds interframe = new WaitForSeconds(0.015f);
    const float ANIM_STEPS = 20f;
    private IEnumerator FadeToClear()
    {
        spriteRenderer.transform.localScale = Vector3.one * 1.2f;
        for (int iStep = 0; iStep < ANIM_STEPS; iStep++)
        {
            float percent = iStep / ANIM_STEPS;
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, Color.clear, percent);
            spriteRenderer.transform.localScale = Vector3.Lerp(spriteRenderer.transform.localScale, Vector3.zero, percent);

            yield return interframe;
        }

        //Debug.Log("FadeToClear ................. " + this.name);
        grid.BlockDisapperingCount(--numBlocksDisapearing); // à remplacer par un event/delegate ...
        pooler.SaveItem(this.gameObject);
    }

    Vector2 targetPos;
    internal void MoveTo(int gridX, int gridY)
    {
        targetPos = new Vector2(gridX + GameGrid.X_OFFSET, gridY + GameGrid.Y_OFFSET);
        // new Vector2(gridX, gridY);

        grid.BlockMovingCount(++numBlocksMoving); // à remplacer par un event/delegate ...
        StartCoroutine(MoveToTarget());
    }

    private IEnumerator MoveToTarget()
    {
        for (int iStep = 0; iStep < ANIM_STEPS; iStep++)
        {
            float percent = iStep / ANIM_STEPS;
            transform.position = Vector2.Lerp(transform.position, targetPos, percent);

            yield return interframe;
        }

        yield return interframe;

        Debug.Log("MoveToTarget ................. " + this.name + " >>> " + targetPos);
        grid.BlockMovingCount(--numBlocksMoving); // à remplacer par un event/delegate ...
    }
}