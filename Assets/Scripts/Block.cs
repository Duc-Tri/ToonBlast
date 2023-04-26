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
    private const float FALLING_STEP = 0.6f;
    private const float DISAPEARING_ANIM_STEPS = 70; // étapes d'animation pour la disparition
    const int OK_STEP = 10;// 1 + (int)(DISAPEARING_ANIM_STEPS * 0.2f); // ok pour rendre la main

#if UNITY_EDITOR_WIN
    private static WaitForSeconds interframe = new WaitForSeconds(1 / 60f);
#else
    private static WaitForEndOfFrame interframe = new WaitForEndOfFrame();
#endif

    private void Awake()
    {

        spriteRenderer = GetComponent<SpriteRenderer>();
        InitBlock(UnityEngine.Random.Range(0, MAX_BLOCKS));

        if (grid == null)
        {
            Debug.Log("DAVID_GOOD_ENOUGH_STEP0=" + OK_STEP);
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

    internal void PutDirectlyInto(int x, int y)
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
            if (iStep == OK_STEP)
                grid.BlockDisapperingCount(--numBlocksDisapearing); // à remplacer par un event/delegate ...

            float percent = iStep / DISAPEARING_ANIM_STEPS;
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, Color.clear, percent);
            spriteRenderer.transform.localScale = Vector3.Lerp(spriteRenderer.transform.localScale, Vector3.one * 2f, percent);

            yield return interframe;
        }

        //Debug.Log("FadeToClear ................. " + this.name);

        pooler.SaveItem(this.gameObject);
    }

    internal void NewBlockFromOffScreen(int x, int y)
    {
        // met le bloc plus haut, en dehors de l'écran
        transform.position = new Vector2(x + GameGrid.X_OFFSET, y + MainGame.MAX_Y * 0.5f /*+ GameGrid.Y_OFFSET*/);
        InitFallingDown(x, y);
    }

    float FALLING_ANIM_STEPS = 0;
    internal void InitFallingDown(int gridX, int gridY)
    {
        grid.BlockMovingCount(++numBlocksMoving, this); // à remplacer par un event/delegate ...
        targetPos = new Vector2(gridX + GameGrid.X_OFFSET, gridY + GameGrid.Y_OFFSET);
        FALLING_ANIM_STEPS = (transform.position.y - targetPos.y) / FALLING_STEP; // étapes d'animation pour la chute
        StartCoroutine(AnimateFalling());
    }

    private IEnumerator AnimateFalling()
    {
        int count = 0;
        Vector2 p = transform.position;
        yield return interframe;
        grid.BlockMovingCount(--numBlocksMoving, this); // à remplacer par un event/delegate ...

        //for (int iStep = 0; iStep < 100*FALLING_ANIM_STEPS; iStep++)
        do
        {
            //if (transform.position.y - targetPos.y < 0.1f) // good enough                break;
            //float percent = iStep / FALLING_ANIM_STEPS;
            //transform.position = Vector2.Lerp(transform.position, targetPos, percent);

            p.y -= FALLING_STEP;
            transform.position = p;

            yield return interframe;
        }
        while (transform.position.y - targetPos.y > FALLING_STEP);

        //Debug.Log("MoveToTarget ................. " + this.name + " >>> " + targetPos);

        transform.position = targetPos; // parfois le bloc ne termine pas sa course si STEPS est petit ...
    }
}