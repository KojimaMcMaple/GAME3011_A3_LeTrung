using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Main : MonoBehaviour
{
    public event EventHandler OnGridCellDestroyed;

    private Grid<GridCell> grid_;
    private int width_;
    private int height_;
    [SerializeField] private List<GemSO> gem_so_list_;
    private List<GridCell> processing_list_;

    private int score_;

    private void Awake()
    {
        width_ = 10;
        height_ = 10;
        grid_ = new Grid<GridCell>(width_, height_, 1f, Vector3.zero, 
            (Grid<GridCell> grid_, int x, int y) => new GridCell(grid_,x,y));

        for (int x = 0; x < width_; x++)
        {
            for (int y = 0; y < height_; y++)
            {
                GemSO gem_so = gem_so_list_[UnityEngine.Random.Range(0, gem_so_list_.Count)];
                Gem gem = new Gem(gem_so, x, y);
                grid_.GetValue(x,y).SetCellItem(gem);
            }
        }

        score_ = 0;
    }

    public Grid<GridCell> GetMainGrid()
    {
        return grid_;
    }

    private bool IsValidCoords(int x, int y)
    {
        if (x < 0 || y < 0 || x >= width_ || y >= height_)
        {
            return false;
        }
        return true;
    }

    private GemSO GetGemSOAtCoords(int x, int y)
    {
        if (!IsValidCoords(x, y))
        {
            return null;
        }
        GridCell cell = grid_.GetValue(x, y);
        return cell.GetCellItem().GetGemSO();
    }

    public bool HasMatch(int x, int y)
    {
        List<GridCell> result = GetMatchesAtCoords(x, y);
        return result != null && result.Count > 2;
    }

    public void SwapGridCells(int start_x, int start_y, int dest_x, int dest_y)
    {
        if (!IsValidCoords(start_x, start_y) || !IsValidCoords(dest_x, dest_y)) 
        {
            return;
        }
        if (start_x == dest_x && start_y == dest_y)
        {
            return;
        }

        GridCell start_cell = grid_.GetValue(start_x, start_y);
        GridCell dest_cell = grid_.GetValue(dest_x, dest_y);
        Gem start_gem = start_cell.GetCellItem();
        Gem dest_gem = dest_cell.GetCellItem();

        start_gem.SetGemCoords(dest_x, dest_y);
        dest_gem.SetGemCoords(start_x, start_y);
        start_cell.SetCellItem(dest_gem);
        dest_cell.SetCellItem(start_gem);
    }

    public bool TrySwapGridCells(int start_x, int start_y, int dest_x, int dest_y)
    {
        if (!IsValidCoords(start_x, start_y) || !IsValidCoords(dest_x, dest_y))
        {
            return false;
        }
        if (start_x == dest_x && start_y == dest_y)
        {
            return false;
        }
        if (GetGemSOAtCoords(start_x, start_y) == GetGemSOAtCoords(dest_x, dest_y))
        {
            return false;
        }

        SwapGridCells(start_x, start_y, dest_x, dest_y);
        bool has_match = HasMatch(start_x, start_y) || HasMatch(dest_x, dest_y);
        if (!has_match)
        {
            SwapGridCells(start_x, start_y, dest_x, dest_y);
        }

        return has_match;
    }

    public bool TryProcessMatches(int start_x, int start_y, int dest_x, int dest_y)
    {
        processing_list_ = new List<GridCell>();
        List<GridCell> list1 = GetMatchesAtCoords(start_x, start_y);
        List<GridCell> list2 = GetMatchesAtCoords(dest_x, dest_y);
        processing_list_ = list1.Union(list2).ToList();

        foreach (GridCell cell in processing_list_)
        {
            TryDestroyGem(cell);
        }

        return true;
    }

    private void TryDestroyGem(GridCell cell)
    {
        if (cell.HasCellItem())
        {
            cell.DestroyCellItem();
            OnGridCellDestroyed?.Invoke(cell, EventArgs.Empty);
            cell.ResetCellItem();

            score_ += 100;
        }
    }

    public List<GridCell> GetMatchesAtCoords(int x, int y) //main check
    {
        GemSO gem_so = GetGemSOAtCoords(x, y);
        if (gem_so == null)
        {
            return null;
        }

        // RIGHT
        int matches_right = 0;
        for (int i = 1; i < width_; i++)
        {
            if (!IsValidCoords(x + i, y))
            {
                break;
            }
            GemSO next_gem_so = GetGemSOAtCoords(x + i, y);
            if (next_gem_so != gem_so)
            {
                break;
            }
            matches_right++;
        }


        // LEFT
        int matches_left = 0;
        for (int i = 1; i < width_; i++)
        {
            if (!IsValidCoords(x - i, y))
            {
                break;
            }
            GemSO next_gem_so = GetGemSOAtCoords(x - i, y);
            if (next_gem_so != gem_so)
            {
                break;
            }
            matches_left++;
        }

        // UP
        int matches_up = 0;
        for (int i = 1; i < height_; i++)
        {
            if (!IsValidCoords(x, y + i))
            {
                break;
            }
            GemSO next_gem_so = GetGemSOAtCoords(x, y + i);
            if (next_gem_so != gem_so)
            {
                break;
            }
            matches_up++;
        }

        // DOWN
        int matches_down = 0;
        for (int i = 1; i < height_; i++)
        {
            if (!IsValidCoords(x, y - i))
            {
                break;
            }
            GemSO next_gem_so = GetGemSOAtCoords(x, y - i);
            if (next_gem_so != gem_so)
            {
                break;
            }
            matches_down++;
        }

        int matches_horizontal = 1 + matches_right + matches_left;
        int matches_vertical = 1 + matches_up + matches_down;
        List < GridCell > result = new List < GridCell >(); 
        if (matches_horizontal > 2)
        {
            int bound_left = x - matches_left;
            for (int i = 0; i < matches_horizontal; i++)
            {
                if (bound_left + i != x)
                {
                    result.Add(grid_.GetValue(bound_left + i, y));
                }
            }
        }
        if (matches_vertical > 2)
        {
            int bound_down = y - matches_down;
            for (int i = 0; i < matches_vertical; i++)
            {
                if (bound_down + i != y)
                {
                    result.Add(grid_.GetValue(x, bound_down + i));
                }
            }
        }
        if (result.Count != 0)
        {
            result.Add(grid_.GetValue(x, y));
        }
        return result;
    }



    public class GridCell
    {
        private Gem cell_item_; //item on cell

        private Grid<GridCell> grid_;
        private int x_;
        private int y_;

        public GridCell(Grid<GridCell> grid, int x, int y)
        {
            grid_ = grid;
            x_ = x;
            y_ = y;
        }

        public Gem GetCellItem()
        {
            return cell_item_;
        }
        public void SetCellItem(Gem cell_item)
        {
            cell_item_ = cell_item;
            grid_.DoTriggerGridObjChanged(x_, y_);
        }
        public bool HasCellItem()
        {
            return cell_item_ != null;
        }
        public void ResetCellItem()
        {
            cell_item_ = null;
        }
        public void DestroyCellItem()
        {
            cell_item_?.Destroy();
            grid_.DoTriggerGridObjChanged(x_, y_);
        }

        public Grid<GridCell> GetGrid()
        {
            return grid_;
        }

        public int GetX()
        {
            return x_;
        }
        public int GetY()
        {
            return y_;
        }
        public Vector3 GetWorldPos()
        {
            return grid_.GetWorldPos(x_, y_);
        }
    }



    public class Gem
    {
        public event EventHandler OnDestroyed;

        private GemSO gem_;
        private int x_;
        private int y_;
        private bool is_dead_;

        public Gem(GemSO gem, int x, int y)
        {
            gem_ = gem;
            x_ = x;
            y_ = y;
            is_dead_ = false;
        }

        public GemSO GetGemSO()
        {
            return gem_;
        }

        public Vector3 GetWorldPos()
        {
            return new Vector3(x_, y_);
        }

        public void SetGemCoords(int x, int y)
        {
            x_ = x;
            y_ = y;
        }

        public void Destroy()
        {
            is_dead_ = true;
            OnDestroyed?.Invoke(this, EventArgs.Empty);
        }

        public override string ToString()
        {
            return is_dead_.ToString();
        }
    }
}
