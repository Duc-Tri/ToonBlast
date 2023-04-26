using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// layout du jeu (disposition des obstacles et forme de la grille)
public class Layout
{
    private string layoutFile;
    private char[,] arrLayout; // lu à partir du CSV

    public int NUM_LINES { get; private set; }
    public int NUM_COLUMNS { get; private set; }

    private const char BLOCK_FORBIDDEN = 'X'; // 
    private const char BLOCK_RANDOM = '*'; // couleur à tirer aléatoirement
    private const char OUTSIDE_LAYOUT = '$'; // valeur PAS dans le CSV

    public bool IsBlockAuthorized(int x, int y) => Cell(x, y) != OUTSIDE_LAYOUT && Cell(x, y) != BLOCK_FORBIDDEN; // to optimize
    public bool IsBlockRAndom(int x, int y) => Cell(x, y) == BLOCK_RANDOM;

    public Layout(string layoutName)
    {
        layoutFile = layoutName;
        string levelContent = Resources.Load<TextAsset>(layoutName).text;

        string[] lines = levelContent.Split('\n');
        NUM_LINES = lines.Length;
        arrLayout = null;

        for (int y = 0; y < NUM_LINES; y++)
        {
            string[] cells = lines[y].Split(';');
            NUM_COLUMNS = cells.Length;

            if (arrLayout == null)
                arrLayout = new char[NUM_COLUMNS, NUM_LINES];

            for (int x = 0; x < NUM_COLUMNS; x++)
            {
                arrLayout[x, y] = char.Parse(cells[x].Trim());

                Debug.Log("arrLayout --- " + arrLayout[x, y]);
            }
        }
    }

    public char Cell(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < NUM_COLUMNS && y < NUM_LINES)
            return arrLayout[x, y];
        else
            return OUTSIDE_LAYOUT;
    }

}
