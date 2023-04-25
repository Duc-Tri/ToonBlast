using System;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;
using UnityEngine.UIElements;
using UnityEditor;

public class GameGrid
{
    enum GridState
    {
        WAIT_INPUT = 0,
        CLEAR_BLOCKS = 1,
        NEW_BLOCKS = 2
    }
    private GridState gridState;

    private string levelName = "level01";

    private int[,] layout;

    const int NO_BLOCK = 0;

    private Block[,] gridBlocks;

    public int NUM_COLUMNS
    {
        get; private set;
    }

    public int NUM_LINES
    {
        get; private set;
    }

    public const int MAX_COLUMNS = 9;

    public static float X_OFFSET { get; private set; }
    public static float Y_OFFSET { get; private set; }

    //public static GameObject blockPrefab;

    // Start is called before the first frame update
    public GameGrid(GameObject prefab, float maxX, float maxY)
    {
        gridState = GridState.NEW_BLOCKS;

        //blockPrefab = prefab;
        string levelContent = Resources.Load<TextAsset>(levelName).text;

        string[] lines = levelContent.Split('\n');
        NUM_LINES = lines.Length;

        for (int y = 0; y < NUM_LINES; y++)
        {
            string[] cells = lines[y].Split(';');
            NUM_COLUMNS = cells.Length;

            if (layout == null)
                layout = new int[NUM_COLUMNS, NUM_LINES];

            for (int x = 0; x < NUM_COLUMNS; x++)
                layout[x, y] = int.Parse(cells[x]);
        }

        Debug.Log(levelContent);

        X_OFFSET = 0.5f - NUM_COLUMNS * 0.5f;
        Y_OFFSET = 0.5f - NUM_LINES * 0.5f;

        Debug.Log("X_OFFSET = " + X_OFFSET + " Y_OFFSET = " + Y_OFFSET);

        gridBlocks = new Block[NUM_COLUMNS, NUM_LINES];
    }

    public int LayoutCell(int x, int y)
    {
        if (x < NUM_COLUMNS && y < NUM_LINES)
            return layout[x, y];
        else
            return -1;
    }

    const int ACTION_CLEAR = 10;
    const int ACTION_CREATE = 20;
    internal void ProcessTouch(Vector2 position, int action = ACTION_CLEAR)
    {
        if (gridState != GridState.WAIT_INPUT)
            return;

        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(position);
        int gridX = (int)(worldPosition.x - GameGrid.X_OFFSET + 0.5f);
        int gridY = (int)(worldPosition.y - GameGrid.Y_OFFSET + 0.5f);

        EnterClearBlocksState(gridX, gridY);
    }

    private void EnterClearBlocksState(int gridX, int gridY)
    {
        //Debug.Log(" ===== touch=" + position + " ===== cell=" + cellPos);

        Block block = TryToGetFromGrid(gridX, gridY);
        if (!DeleteIsPossible(block, gridX, gridY))
            return;

        gridState = GridState.CLEAR_BLOCKS;
        ExecuteClearBlocks(block.colorBlock, gridX, gridY);
        //gridState = GridState.WAIT_INPUT;

    }

    private bool DeleteIsPossible(Block block, int x, int y)
    {
        return block != null && (SameBlock(block.colorBlock, x - 1, y) || SameBlock(block.colorBlock, x + 1, y) || SameBlock(block.colorBlock, x, y + 1) || SameBlock(block.colorBlock, x, y - 1));
    }

    private bool SameBlock(int color2match, int x, int y)
    {
        Block block = TryToGetFromGrid(x, y);
        return (block != null && block.colorBlock == color2match);
    }

    private Block TryToGetFromGrid(int x, int y)
    {
        if (x < 0 || y < 0 || x >= NUM_COLUMNS || y >= NUM_LINES)
            return null;

        return gridBlocks[x, y];
    }

    private void ExecuteClearBlocks(int color2match, int x, int y)
    {
        Block block = TryToGetFromGrid(x, y);

        // si c'est la même couleur
        if (block != null && block.colorBlock == color2match)
        {
            SetBlockInGrid(null, x, y);
            block.Disapear();

            ExecuteClearBlocks(color2match, x - 1, y);
            ExecuteClearBlocks(color2match, x + 1, y);
            ExecuteClearBlocks(color2match, x, y + 1);
            ExecuteClearBlocks(color2match, x, y - 1);
        }
    }

    internal void SetBlockInGrid(Block block, int x, int y)
    {
        gridBlocks[x, y] = block;
    }

    internal void FillGridRandomly()
    {
        gridState = GridState.NEW_BLOCKS;
        for (int x = 0; x < NUM_COLUMNS - 1; x++)
        {
            for (int y = 0; y < NUM_LINES - 1; y++)
            {
                if (LayoutCell(x, y) != NO_BLOCK)
                {
                    Block b = MainGame.blocksPooler.GetItem().GetComponent<Block>();
                    gridBlocks[x, y] = b;
                    //b.SetBlock((int)(UnityEngine.Random.value * 555) % 5);
                    b.PutIntoGrid(x, y);
                }
            }
        }

        gridState = GridState.WAIT_INPUT;
    }

    public void DrawBackgroundMask(SpriteRenderer background)
    {
        Texture2D texture = new Texture2D(1 + (int)(MainGame.MAX_X * 2f), 1 + (int)(MainGame.MAX_Y * 2f), TextureFormat.RGBA32, false);

        float pixelsPerUnit = background.sprite.pixelsPerUnit;
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, MainGame.MAX_X * 2, MainGame.MAX_Y * 2), new Vector2(0.5f, 0.5f), 1);

        //texture.SetPixel(0, 0, Color.white);
        //texture.SetPixel(NUM_COLUMNS - 1, NUM_LINES - 1, Color.white);

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
                if (LayoutCell(x, y) > 0)
                    texture.SetPixel(x, y, Color.white);
        }
        texture.Apply();

        SpriteMask mask = background.gameObject.AddComponent<SpriteMask>();
        mask.alphaCutoff = 0.906f;
        mask.sprite = sprite;
        background.transform.position = new Vector2((2f * MainGame.MAX_X - NUM_COLUMNS) / 2f, (2f * MainGame.MAX_Y - NUM_LINES) / 2f);
    }

    internal void BlockDisapperingCount(int unavailableBlocks)
    {
        if (unavailableBlocks > 0)
        {
            //gridState = GridState.DELETE_BLOCKS;
            //gridState = GridState.WAIT_INPUT; // create blocks
            gridState = GridState.CLEAR_BLOCKS;
        }
        else
        {
            //gridState = GridState.CREATE_BLOCKS;
            //gridState = GridState.WAIT_INPUT; // create blocks
            EnterNewBlocksState();
        }

        Debug.Log("BlockDisapering = " + unavailableBlocks);
    }

    // si il y a une case vide, on entre dans l'état NEW_BLOCKS
    private void EnterNewBlocksState()
    {
        int columnToFill = 0;

        for (int x = 0; x < NUM_COLUMNS; x++)
        {
            for (int y = 0; y < NUM_LINES; y++)
            {
                if (LayoutCell(x, y) > 0 && gridBlocks[x, y] == null)
                {
                    columnToFill++;
                    ProcessNewBlocks(x, y);

                    break; // next column !
                }
            }
        }

        //if (columnToFill == 0)
        EnterWaitInputState();
    }

    private void ProcessNewBlocks(int column, int firstEmptyY)
    {
        //gridState = GridState.NEW_BLOCKS;
        Block block;
        for (int emptyY = firstEmptyY; emptyY < NUM_LINES; emptyY++)
        {
            if (gridBlocks[column, emptyY] == null)
            {
                for (int y = emptyY + 1; y < NUM_LINES; y++)
                {
                    block = gridBlocks[column, y];
                    if (block != null)
                    {
                        gridBlocks[column, emptyY] = block;
                        gridBlocks[column, y] = null;
                        block.MoveTo(column, emptyY);

                        break; // prochaine case vide !
                    }
                }
            }
        }
    }

    internal void BlockMovingCount(int unavailableBlocks)
    {
        if (unavailableBlocks > 0)
        {
            gridState = GridState.NEW_BLOCKS;
        }
        else
        {
            //gridState = GridState.CREATE_BLOCKS;
            //gridState = GridState.WAIT_INPUT; // create blocks
            EnterWaitInputState();
        }

        Debug.Log("BlockMovingCount ============ " + unavailableBlocks);
    }

    private void EnterWaitInputState()
    {
        gridState = GridState.WAIT_INPUT;
    }
}
