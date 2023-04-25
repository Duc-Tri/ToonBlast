using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Block : MonoBehaviour
{
    // data =====================================
    private static GameGrid grid;
    private static Pooler pooler;

    private const int MAX_BLOCKS = 5;
    public int blockColor { get; private set; }

    public static int numBlocksDisapearing = 0;
    public static int numBlocksMoving = 0;

    // graphics =================================
    [SerializeField]
    private Sprite[] blockSprites;

    private SpriteRenderer spriteRenderer;

    // animation --------------------------------
    private Vector2 targetPos;
    private static WaitForSeconds interframe = new WaitForSeconds(0.02f);
    private const float DISAPEARING_ANIM_STEPS = 15; // étapes d'animation pour la disparition
    private const float FALLING_ANIM_STEPS = 20; // étapes d'animation pour le mouvement

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
        blockColor = color;
        spriteRenderer.sprite = blockSprites[blockColor];
        spriteRenderer.color = Color.white;
        spriteRenderer.transform.localScale = Vector3.one;
    }

    internal void PutDirectlyIntoGrid(int x, int y)
    {
        transform.position = new Vector2(x + GameGrid.X_OFFSET, y + GameGrid.Y_OFFSET);
        grid.SetBlockInGrid(this, x, y);
    }

    public void InitBlockWithtRandomColor()
    {
        InitBlock(UnityEngine.Random.Range(0, MAX_BLOCKS));
    }

    public void InitDisapearing()
    {
        grid.BlockDisapperingCount(++numBlocksDisapearing); // à remplacer par un event/delegate ...
        StartCoroutine(AnimateDisapearing());
    }

    private IEnumerator AnimateDisapearing()
    {
        spriteRenderer.transform.localScale = Vector3.one * 1.2f;
        for (int iStep = 0; iStep < DISAPEARING_ANIM_STEPS; iStep++)
        {
            float percent = iStep / DISAPEARING_ANIM_STEPS;
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, Color.clear, percent);
            spriteRenderer.transform.localScale = Vector3.Lerp(spriteRenderer.transform.localScale, Vector3.zero, percent);

            yield return interframe;
        }

        //Debug.Log("FadeToClear ................. " + this.name);
        grid.BlockDisapperingCount(--numBlocksDisapearing); // à remplacer par un event/delegate ...
        pooler.SaveItem(this.gameObject);
    }

    internal void FallFromOffScreen(int column, int emptyY)
    {
        transform.position = new Vector2(column + GameGrid.X_OFFSET, 2f * MainGame.MAX_Y + GameGrid.Y_OFFSET);
        InitFallingDown(column, emptyY);
    }


    internal void InitFallingDown(int gridX, int gridY)
    {
        targetPos = new Vector2(gridX + GameGrid.X_OFFSET, gridY + GameGrid.Y_OFFSET);

        grid.BlockMovingCount(++numBlocksMoving, this); // à remplacer par un event/delegate ...
        StartCoroutine(AnimateFalling());
    }

    private IEnumerator AnimateFalling()
    {
        for (int iStep = 0; iStep < FALLING_ANIM_STEPS; iStep++)
        {
            float percent = iStep / FALLING_ANIM_STEPS;
            transform.position = Vector2.Lerp(transform.position, targetPos, percent);

            yield return interframe;
        }

        //Debug.Log("MoveToTarget ................. " + this.name + " >>> " + targetPos);
        grid.BlockMovingCount(--numBlocksMoving, this); // à remplacer par un event/delegate ...
    }
}