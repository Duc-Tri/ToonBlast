using System;
using UnityEngine;
using UnityEngine.UIElements;


public class GameGrid
{
    private string levelName = "level01";

    private int[,] layout;

    private float mAX_X;
    private float mAX_Y;
    const int NO_BLOCK = 0;

    private Block[,] boardBlocks;

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
    public static object Instance { get; internal set; }
    public static GameObject blockPrefab;

    // Start is called before the first frame update
    public GameGrid(GameObject prefab, float maxX, float maxY)
    {
        blockPrefab = prefab;
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

        boardBlocks = new Block[NUM_COLUMNS, NUM_LINES];
    }

    public int Layout(int x, int y)
    {
        if (x < NUM_COLUMNS && y < NUM_LINES)
            return layout[x, y];
        else
            return -1;
    }


    public void DrawBackgroundMask(SpriteRenderer background)

    {
        Texture2D texture = new Texture2D(1 + (int)(MainGame.MAX_X * 2f), 1 + (int)(MainGame.MAX_Y * 2f), TextureFormat.RGBA32, false);

        float pixelsPerUnit = background.sprite.pixelsPerUnit;
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, MainGame.MAX_X * 2, MainGame.MAX_Y * 2), new Vector2(0.5f, 0.5f), 1);

        //GetComponent<SpriteRenderer>().sprite = sprite;

        //texture.SetPixel(0, 0, Color.white);
        //texture.SetPixel(NUM_COLUMNS - 1, NUM_LINES - 1, Color.white);

        for (int y = 0; y < texture.height; y++) // += (int)pixelsPerUnit) // texture.height
        {
            for (int x = 0; x < texture.width; x++) // += (int)pixelsPerUnit) //Goes through each pixel // texture.width
            {
                if (Layout(x, y) > 0)
                {
                    //if (UnityEngine.Random.Range(0, 2) == 1) //50/50 chance it will be black or white

                    texture.SetPixel(x, y, Color.white);
                }
            }
        }
        texture.Apply();


        SpriteMask mask = background.gameObject.AddComponent<SpriteMask>();
        mask.alphaCutoff = 0.906f;
        mask.sprite = sprite;
        background.transform.position = new Vector2((2f * MainGame.MAX_X - NUM_COLUMNS) / 2f, (2f * MainGame.MAX_Y - NUM_LINES) / 2f);
    }

    public Sprite DrawMask2(SpriteRenderer spriteMask)
    {
        Color[] colors = new Color[100 * 100];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = Color.clear;


        Color[] colors2 = new Color[100 * 100];
        for (int i = 0; i < colors.Length; i++)
            colors2[i] = Color.white;

        Texture2D t = new Texture2D(1, 1); //  spriteMask.texture;

        //Texture2D t2 = new Texture2D(500, 500);
        for (int i = 0; i < 15; i++)
        {
            Debug.Log(i * 100);
            t.SetPixels(i * 100, i * 100, 100, 100, colors);
            t.Apply();
        }
        t = new Texture2D(1000, 1000, TextureFormat.ARGB32, false);
        t.Apply();

        return null; ///////////// spriteMask;
    }

    internal void ProcessTouch(Vector2 position)
    {


        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(position);
        Vector2Int cellPos = new Vector2Int((int)(worldPosition.x - GameGrid.X_OFFSET + 0.5f), (int)(worldPosition.y - GameGrid.Y_OFFSET + 0.5f));

        Debug.Log(" ===== touch=" + position + " ===== cell=" + cellPos);


        if (cellPos.x < NUM_COLUMNS && cellPos.y < NUM_LINES)
        {
            Block b = boardBlocks[cellPos.x, cellPos.y];
            if (b != null)
                b.gameObject.SetActive(false);
        }

    }

    internal void SetBlock(Block block, int x, int y)
    {
        boardBlocks[x, y] = block;
    }

    internal void RandomBlocks()
    {
        for (int x = 0; x < NUM_COLUMNS; x++)
        {
            for (int y = 0; y < NUM_LINES; y++)
            {
                if (Layout(x, y) != 0)
                {
                    Block b = GameObject.Instantiate(blockPrefab).GetComponent<Block>();
                    //b.SetBlock((int)(UnityEngine.Random.value * 555) % 5);
                    b.SetGridXY(x, y);
                }
            }
        }
    }
}
