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
    private static WaitForSeconds interframe = new WaitForSeconds(1f / 80);
    private const float DISAPEARING_ANIM_STEPS = 25; // étapes d'animation pour la disparition
    const int DAVID_GOOD_ENOUGH_STEP0 = 1 + (int)(DISAPEARING_ANIM_STEPS * 0.2f); // ça va, c'est bon comme ça !

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        InitBlock(UnityEngine.Random.Range(0, MAX_BLOCKS));

        if (grid == null)
        {
            Debug.Log("DAVID_GOOD_ENOUGH_STEP0=" + DAVID_GOOD_ENOUGH_STEP0);
            grid = MainGame.gameGrid;
        }

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
            if (iStep == DAVID_GOOD_ENOUGH_STEP0)
                grid.BlockDisapperingCount(--numBlocksDisapearing); // à remplacer par un event/delegate ...

            float percent = iStep / DISAPEARING_ANIM_STEPS;
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, Color.clear, percent);
            spriteRenderer.transform.localScale = Vector3.Lerp(spriteRenderer.transform.localScale, Vector3.one * 2f, percent);

            yield return interframe;
        }

        //Debug.Log("FadeToClear ................. " + this.name);

        pooler.SaveItem(this.gameObject);
    }

    internal void NewBlockFromOffScreen(int column, int emptyY)
    {
        transform.position = new Vector2(column + GameGrid.X_OFFSET, emptyY + MainGame.MAX_Y * 0.5f /*+ GameGrid.Y_OFFSET*/);
        InitFallingDown(column, emptyY);
    }

    float FALLING_ANIM_STEPS = 0;
    internal void InitFallingDown(int gridX, int gridY)
    {
        targetPos = new Vector2(gridX + GameGrid.X_OFFSET, gridY + GameGrid.Y_OFFSET);
        FALLING_ANIM_STEPS = (transform.position.y - targetPos.y) / 0.4f; // étapes d'animation pour la chute

        grid.BlockMovingCount(++numBlocksMoving, this); // à remplacer par un event/delegate ...
        StartCoroutine(AnimateFalling());
    }

    private IEnumerator AnimateFalling()
    {
        for (int iStep = 0; iStep < FALLING_ANIM_STEPS; iStep++)
        {
            if (transform.position.y - targetPos.y < 0.04f) // good enough
                break;

            float percent = iStep / FALLING_ANIM_STEPS;
            transform.position = Vector2.Lerp(transform.position, targetPos, percent);

            yield return interframe;
        }

        //Debug.Log("MoveToTarget ................. " + this.name + " >>> " + targetPos);

        transform.position = targetPos; // parfois le bloc ne termine pas sa course si STEPS est petit ...
        grid.BlockMovingCount(--numBlocksMoving, this); // à remplacer par un event/delegate ...
    }
}