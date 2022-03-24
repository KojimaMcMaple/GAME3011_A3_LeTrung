using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    private Grid<GridCell> grid_;
    private int width_;
    private int height_;

    private void Awake()
    {
        width_ = 10;
        height_ = 10;
        grid_ = new Grid<GridCell>(width_, height_, 1f, Vector3.zero, 
            (Grid<GridCell> grid_, int x, int y) => new GridCell(grid_,x,y));
    }

    public Grid<GridCell> GetMainGrid()
    {
        return grid_;
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

        public GemSO GetGem()
        {
            return gem_;
        }

        public Vector3 GetWorldPos()
        {
            return new Vector3(x_, y_);
        }

        public void SetGemCoords(int x, int y)
        {
            this.x_ = x;
            this.y_ = y;
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
