using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
public class Astar : MonoBehaviour
{
    public Vector2 strt;
    public Vector2 end;
    [SerializeField] private int GridH = 10;
    [SerializeField] private int GridW = 10;
    [SerializeField] private float CellH = 1f;
    [SerializeField] private float CellW = 1f;
    [SerializeField] private int walls;

    [SerializeField] private bool generatePath;

    [SerializeField] private bool pathgGenerated;

    private Dictionary<Vector2, Cell> cells;

    [SerializeField] private List<Vector2> CellsToSearch;
    [SerializeField] private List<Vector2> Checked;
    [SerializeField] public List<Vector2> Path;

    void Update()
    {
        if (generatePath && !pathgGenerated)
        {
            GenerateGrid();
            findPath();
            pathgGenerated = true;
        }
        else if (!generatePath)
        {
            pathgGenerated = false;
        }
    }

    private void GenerateGrid()
    {
        cells = new Dictionary<Vector2, Cell>();

        // go through all the positions on the grid 
        for (float x = 0; x < GridW; x += CellW)
        {
            for (float y = 0; y < GridH; y += CellH)
            {
                Vector2 pos = new Vector2(x, y);
                cells.Add(pos, new Cell(pos));//add positions to the cell dictionary 
            }
        }
        //set walls randomly
        for (int i = 0; i < walls; i++)
        {
            float wallX = Mathf.Floor(Random.Range(0f, GridW / CellW)) * CellW;
            float wallY = Mathf.Floor(Random.Range(0f, GridH / CellH)) * CellH;
            Vector2 pos = new Vector2(wallX, wallY);

            if (cells.TryGetValue(pos, out Cell cell))
            {
                cell.isWall = true;
            }
        }
    }

    private void findPath()
    {
        CellsToSearch = new List<Vector2>();
        Checked = new List<Vector2>();
        Path = new List<Vector2>();

        Cell firstcell = cells[strt];

        firstcell.gcost = 0;
        firstcell.hcost = Getdistance(strt, end);
        firstcell.fcost = firstcell.gcost + firstcell.hcost;

        CellsToSearch.Add(strt);//add to make sure loop functions loop wont function without this

        while (CellsToSearch.Count > 0)
        {
            Vector2 celltoSearch = CellsToSearch[0];

            foreach (Vector2 pos in CellsToSearch) //go thorugh eatch position in the cells to search list
            {
                Cell c = cells[pos];
                if (c.fcost < cells[celltoSearch].fcost || //chack for cells with the lower fcost or
                    c.fcost == cells[celltoSearch].fcost && c.hcost == cells[celltoSearch].hcost) // if there are more than 1 with the same then look for the lowest hcost
                {
                    celltoSearch = pos;
                }
            }

            CellsToSearch.Remove(celltoSearch);
            Checked.Add(celltoSearch); //add the best cell with the chance of being the best path to the cells searched list

            if (celltoSearch == end)
            {//work backwards to find the path
                Debug.Log("end");
                Cell pathcell = cells[end];

                while (pathcell.position != strt)
                {
                    Path.Insert(0, pathcell.position);
                    pathcell = cells[pathcell.connection];
                }

                Path.Insert(0, strt);

                return;
            }
            searchNbors(celltoSearch);
        }
        if (Path.Count < 1)
        {
            Debug.Log("no path to destination found");
        }
    }

    private void searchNbors(Vector2 cellpos)
    {// serahcing the 8 sarounding cells of curent cell
        for (float x = cellpos.x - CellW; x <= CellW + cellpos.x; x += CellW)
        {
            for (float y = cellpos.y - CellH; y <= CellH + cellpos.y; y += CellH)
            {
                Vector2 Nborspos = new Vector2(x, y);
                if (cells.TryGetValue(Nborspos, out Cell c) && !Checked.Contains(Nborspos) && !cells[Nborspos].isWall)
                {
                    int GcostToNbor = cells[cellpos].gcost + Getdistance(cellpos, Nborspos); //calculate the gcost from curent to nbeighbors position

                    if (GcostToNbor < cells[Nborspos].gcost)
                    {
                        Cell Nbornode = cells[Nborspos];
                        Nbornode.connection = cellpos;
                        Nbornode.gcost = GcostToNbor;
                        Nbornode.hcost = Getdistance(Nborspos, end);
                        Nbornode.fcost = Nbornode.gcost + Nbornode.hcost;

                        if (!CellsToSearch.Contains(Nborspos))
                        {
                            CellsToSearch.Add(Nborspos);
                        }
                    }
                }
            }
        }
    }
    private int Getdistance(Vector2 pos1, Vector2 pos2)
    {
        Vector2Int dist = new Vector2Int(Mathf.Abs((int)pos1.x - (int)pos2.x), Mathf.Abs((int)pos1.y - (int)pos2.y)); //calculating the distance between eatch point

        int Lowest = Mathf.Min(dist.x, dist.y); // calculate the diagnols
        int Highst = Mathf.Max(dist.x, dist.y);// calculate the horizontal and vertical moves

        int horizontalMovesRequired = Highst - Lowest; //calculate how many straight moves are left after doing diagonals

        return Lowest * 14 + horizontalMovesRequired * 10;//calculate horizontal and diagnaly movment
    }
    private void OnDrawGizmos()
    {
        if (cells == null)
        { return; }

        foreach (KeyValuePair<Vector2, Cell> kvp in cells)
        {
            if (kvp.Value.isWall)
            {
                Gizmos.color = Color.magenta;
            }
            else
            {
                Gizmos.color = Color.black;
            }

            if (Path.Contains(kvp.Key))
            {
                Gizmos.color = Color.red;
            }

            Gizmos.DrawCube(kvp.Key + (Vector2)transform.position, new Vector2(CellW, CellH));
        }
    }

    private class Cell
    {
        public int fcost = int.MaxValue;
        public int gcost = int.MaxValue;
        public int hcost = int.MaxValue;

        public Vector2 position;
        public Vector2 connection;

        public bool isWall;

        public Cell(Vector2 pos)
        {
            position = pos;
        }
    }
}
