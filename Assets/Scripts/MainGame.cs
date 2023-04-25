using UnityEngine;

public class MainGame : MonoBehaviour
{

    [SerializeField]
    private GameObject blockPrefab;

    static public float MAX_X = 0; // en positif
    static public float MAX_Y = 0; // à calculer

    [SerializeField]
    private SpriteRenderer background;

    public static GameGrid gameGrid;
    public static Pooler blocksPooler;

    // Start is called before the first frame update
    void Start()
    {
        blocksPooler = new Pooler(blockPrefab);

        // on adapte la taille de la caméra à la résolution
        AdaptCamera();

        // donne à la grille la taille du viewport, le prefab
        gameGrid = new GameGrid(blockPrefab, MAX_X, MAX_Y);
        gameGrid.FillGridRandomly();

        // utilise les offsets de la grille pour dessiner le fond
        if (background != null)
            gameGrid.DrawBackgroundMask(background);
    }

    private void AdaptCamera()
    {
        float ratio = (float)Screen.height / Screen.width;
        MAX_X = (GameGrid.MAX_COLUMNS + 1) * 0.5f; // +1 pour avoir un peu de marge autour de la grille
        MAX_Y = MAX_X * ratio;

        Camera.main.orthographicSize = MAX_Y;

        Debug.Log("Screen w*h : " + Screen.width + " / " + Screen.height + " UNITS =" + MAX_X + " / " + MAX_Y);
    }

    // Update is called once per frame
    void Update()
    {

#if PLATFORM_ANDROID
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
                gameGrid.ProcessTouch(t.position);
        }
#endif

#if UNITY_EDITOR_WIN
        if (Input.GetMouseButtonDown(0))
        {
            gameGrid.ProcessTouch(Input.mousePosition);
        }
#endif

    }
}
