using UnityEngine;

// grille du jeu
public class GameGrid
{
    public const int MAX_COLUMNS = 9;

    public enum GridState : int
    {
        EMPTY = 0,
        WAIT_INPUT = 1,
        CLEAR_BLOCKS = 2,
        NEW_BLOCKS = 3
    }
    public GridState gridState;

    private int[,] layout; // lu à partir du CSV
    private const int LAYOUT_BLOCK_FORBIDDEN = 0; // valeur dans le CSV
    private const int LAYOUT_BLOCK_ENABLE = 1; // valeur dans le CSV
    private const int OUTSIDE_LAYOUT = -1; // valeur dans le CSV

    private Block[,] gridBlocks;

    public int NUM_COLUMNS { get; private set; }

    public int NUM_LINES { get; private set; }

    public static float X_OFFSET { get; private set; }
    public static float Y_OFFSET { get; private set; }

    public GameGrid()
    {
    }

    public void LoadLayoutAndFill(string layoutFile)
    {
        gridState = GridState.EMPTY;

        //blockPrefab = prefab;
        string levelContent = Resources.Load<TextAsset>(layoutFile).text;

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

        FillGridRandomly();
    }

    private void FillGridRandomly()
    {
        gridState = GridState.NEW_BLOCKS;

        for (int x = 0; x < NUM_COLUMNS; x++)
        {
            for (int y = 0; y < NUM_LINES; y++)
            {
                if (IsBlockAuthorizedInLayout(x, y))
                {
                    Block b = MainGame.blocksPooler.GetItem().GetComponent<Block>();
                    gridBlocks[x, y] = b;
                    //b.SetBlock((int)(UnityEngine.Random.value * 555) % 5);
                    b.PutDirectlyIntoGrid(x, y);
                }
            }
        }

        gridState = GridState.WAIT_INPUT;
    }


    // renvoie true si des bloc peuvent être eliminés
    internal bool ProcessInput(Vector2 position)
    {
        if (gridState != GridState.WAIT_INPUT)
            return false;

        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(position);
        int gridX = (int)(worldPosition.x - GameGrid.X_OFFSET + 0.5f);
        int gridY = (int)(worldPosition.y - GameGrid.Y_OFFSET + 0.5f);

        if (!TryEnterClearBlocksState(gridX, gridY))
        {
            EnterWaitInputState();
            return false;
        }

        return true;
    }

    // renvoie true si des bloc peuvent être eliminés
    private bool TryEnterClearBlocksState(int gridX, int gridY)
    {
        //Debug.Log(" ===== touch=" + position + " ===== cell=" + cellPos);

        Block block = TryToGetFromGrid(gridX, gridY);
        if (!SameColorAdjacent(block, gridX, gridY))
            return false;

        gridState = GridState.CLEAR_BLOCKS;
        ExecuteClearBlocks(block.blockColor, gridX, gridY);

        return true;
    }

    private bool SameColorAdjacent(Block block, int x, int y)
    {
        return block != null && (SameColorAt(block.blockColor, x - 1, y) ||
            SameColorAt(block.blockColor, x + 1, y) ||
            SameColorAt(block.blockColor, x, y + 1) ||
            SameColorAt(block.blockColor, x, y - 1));
    }

    private bool SameColorAt(int color2match, int x, int y)
    {
        Block block = TryToGetFromGrid(x, y);
        return (block != null && block.blockColor == color2match);
    }

    private Block TryToGetFromGrid(int x, int y)
    {
        if (!IsBlockAuthorizedInLayout(x, y))
            return null;

        return gridBlocks[x, y];
    }

    private void ExecuteClearBlocks(int color2match, int x, int y)
    {
        Block block = TryToGetFromGrid(x, y);

        // si c'est la même couleur
        if (block != null && block.blockColor == color2match)
        {
            SetBlockInGrid(null, x, y);
            block.InitDisapearing();

            ExecuteClearBlocks(color2match, x - 1, y);
            ExecuteClearBlocks(color2match, x + 1, y);
            ExecuteClearBlocks(color2match, x, y + 1);
            ExecuteClearBlocks(color2match, x, y - 1);
        }
    }

    internal void SetBlockInGrid(Block block, int x, int y)
    {
        if (IsBlockAuthorizedInLayout(x, y))
            gridBlocks[x, y] = block;
    }

    public void DrawBackgroundMask(SpriteRenderer background)
    {
        Texture2D texture = new Texture2D(10 + (int)(MainGame.MAX_X * 2f), 10 + (int)(MainGame.MAX_Y * 2f), TextureFormat.RGBA32, false);

        float pixelsPerUnit = background.sprite.pixelsPerUnit;
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, MainGame.MAX_X * 2, MainGame.MAX_Y * 2), new Vector2(0.5f, 0.5f), 1);

        //texture.SetPixel(0, 0, Color.white);
        //texture.SetPixel(NUM_COLUMNS - 1, NUM_LINES - 1, Color.white);

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
                if (IsBlockAuthorizedInLayout(x, y))
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
        // on peut gagner du temps en enchainant la création de blocs directement
        //EnterNewBlocksState();

        if (unavailableBlocks > 0)
        {
            gridState = GridState.CLEAR_BLOCKS;
        }
        else
        {
            EnterNewBlocksState();
        }

        //Debug.Log("BlockDisapering = " + unavailableBlocks);
    }

    // si il y a une case vide, on entre dans l'état NEW_BLOCKS
    private void EnterNewBlocksState()
    {
        int columnToFill = 0;

        for (int x = 0; x < NUM_COLUMNS; x++)
        {
            for (int y = 0; y < NUM_LINES; y++)
            {
                if (IsBlockAuthorizedInLayout(x, y) && gridBlocks[x, y] == null)
                {
                    columnToFill++;
                    ProcessNewBlocks(x, y);

                    break; // next column !
                }
            }
        }

        if (columnToFill == 0)
        {
            EnterWaitInputState();
        }
    }

    private void ProcessNewBlocks(int column, int firstEmptyY)
    {
        gridState = GridState.NEW_BLOCKS;
        Block block;
        bool upperBlockFound = false;
        bool newBlocksToCreate = false;

        for (int emptyY = firstEmptyY; emptyY < NUM_LINES; emptyY++)
        {
            // case vide, faire descendre les blocs plus haut
            if (IsBlockAuthorizedInLayout(column, emptyY) && gridBlocks[column, emptyY] == null)
            {
                newBlocksToCreate = true;
                upperBlockFound = false;
                for (int y = emptyY + 1; y < NUM_LINES; y++)
                {
                    if (IsBlockAuthorizedInLayout(column, y))
                    {
                        block = gridBlocks[column, y];
                        if (block != null)
                        {
                            gridBlocks[column, emptyY] = block;
                            gridBlocks[column, y] = null;
                            block.InitFallingDown(column, emptyY);
                            upperBlockFound = true;
                            break; // prochaine case vide !
                        }
                    }
                }

                // si pas de bloc existant plus haut que la case vide, création !
                if (!upperBlockFound)
                {
                    gridBlocks[column, emptyY] = CreateNewMovingBlock(column, emptyY);
                }
            }
        }

    }

    private Block CreateNewMovingBlock(int column, int emptyY)
    {
        Block block = MainGame.blocksPooler.GetItem().GetComponent<Block>();
        block.InitBlockWithtRandomColor();
        block.FallFromOffScreen(column, emptyY);
        return block;
    }

    internal void BlockMovingCount(int unavailableBlocks, Block block = null)
    {
        //Debug.Log(((block == null) ? " *" : block.name) + " BlockMovingCount ============ " + unavailableBlocks);

        if (unavailableBlocks > 0)
        {
            gridState = GridState.NEW_BLOCKS;
        }
        else
        {
            EnterWaitInputState();
        }
    }

    private void EnterWaitInputState()
    {
        //Debug.Log("EnterWaitInputState ################### ");

        gridState = GridState.WAIT_INPUT;
    }

    private bool IsBlockAuthorizedInLayout(int x, int y)
    {
        return LayoutCell(x, y) != LAYOUT_BLOCK_FORBIDDEN && LayoutCell(x, y) != OUTSIDE_LAYOUT;
    }

    private int LayoutCell(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < NUM_COLUMNS && y < NUM_LINES)
            return layout[x, y];
        else
            return OUTSIDE_LAYOUT;
    }

}
