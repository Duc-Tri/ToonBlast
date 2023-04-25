using System.Collections;
using UnityEngine;
using static GameGrid;

public class MainGame : MonoBehaviour
{
    [SerializeField]
    private string LayoutName = "level00";

    [SerializeField]
    private GameObject blockPrefab;

    static public float MAX_X = 0; // à calculer
    static public float MAX_Y = 0; // à calculer

    [SerializeField]
    private SpriteRenderer background;

    public static GameGrid gameGrid;
    public static Pooler blocksPooler;

    [SerializeField]
    private GridState gridstate;  // pour debug

    private bool canProcessInput = false;

    // Start is called before the first frame update
    void Start()
    {
        blocksPooler = new Pooler(blockPrefab);

        // on adapte le viewport de la caméra à la taille de la grille
        // (peut être mieux si fait après le chargement du layout ...)
        AdaptViewportToGrid();

        // en 2 temps, pour que la variable static soit dispo pour les blocs
        gameGrid = new GameGrid();
        gameGrid.LoadLayoutAndFill(LayoutName);

        // dessine le masque du fond selon le layout
        if (background != null)
            gameGrid.DrawBackgroundMask(background);

        canProcessInput = true;
    }

    private void AdaptViewportToGrid()
    {
        float ratio = (float)Screen.height / Screen.width;
        MAX_X = (MAX_COLUMNS + 1) * 0.5f; // +1 pour avoir un peu de marge autour de la grille
        MAX_Y = MAX_X * ratio;

        Camera.main.orthographicSize = MAX_Y;

        Debug.Log("Screen w*h = " + Screen.width + "*" + Screen.height + " / CamSize = " + MAX_X + "*" + MAX_Y);
    }

    // Update is called once per frame
    void Update()
    {
        bool gridIsBusy = false;

#if PLATFORM_ANDROID
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began && canProcessInput)
                gridIsBusy = gameGrid.ProcessInput(t.position);
        }
#endif

#if UNITY_EDITOR_WIN
        if (Input.GetMouseButtonDown(0) && canProcessInput)
        {
            gridIsBusy = gameGrid.ProcessInput(Input.mousePosition);
        }
#endif

        if (gridIsBusy)
        {
            canProcessInput = false;
            StartCoroutine(WaitALittleBeforeNextInput());
        }
    }

    private WaitForSeconds wait = new WaitForSeconds(0.1f);
    // pour éviter que le joueur spamme les touchs ...
    private IEnumerator WaitALittleBeforeNextInput()
    {
        yield return wait;
        canProcessInput = true;
    }
}
