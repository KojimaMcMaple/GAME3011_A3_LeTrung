using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid
{
    private int width_;
    private int height_;
    private float cell_size_;
    private Vector3 origin_;
    private int[,] grid_arr_;
    private TextMesh[,] debug_text_arr_;

    public Grid(int width, int height, float cell_size, Vector3 origin)
    {
        this.width_ = width;
        this.height_ = height;
        this.cell_size_ = cell_size;
        this.origin_ = origin;

        grid_arr_ = new int[width, height];
        debug_text_arr_ = new TextMesh[width, height];

        for (int x = 0; x < grid_arr_.GetLength(0); x++)
        {
            for (int y = 0; y < grid_arr_.GetLength(1); y++)
            {
                Vector3 offset = new Vector3(cell_size_, cell_size_) * 0.5f; //offset so texts don't stay at bottom left
                debug_text_arr_[x,y] = CreateWorldText(grid_arr_[x, y].ToString(), null, GetWorldPos(x, y) + offset, 20, Color.white, TextAnchor.MiddleCenter);
                Debug.DrawLine(GetWorldPos(x, y), GetWorldPos(x, y + 1), Color.green, 100f);
                Debug.DrawLine(GetWorldPos(x, y), GetWorldPos(x + 1, y), Color.magenta, 100f);
            }
        }
        Debug.DrawLine(GetWorldPos(0, height_), GetWorldPos(width_, height_), Color.red, 100f);
        Debug.DrawLine(GetWorldPos(width_, 0), GetWorldPos(width_, height_), Color.blue, 100f);
    }

    private Vector3 GetWorldPos(int x, int y) //GetCellWorldPos
    {
        return new Vector3(x, y) * cell_size_ + origin_;
    }

    private Vector2Int GetGridCoords(Vector3 world_pos)
    {
        int x = Mathf.FloorToInt((world_pos.x - origin_.x) / cell_size_);
        int y = Mathf.FloorToInt((world_pos.y - origin_.y) / cell_size_);
        return new Vector2Int(x, y);
    }

    public void SetValue(int x, int y, int value) //SetCellValue
    {
        if (x<0 || y<0 || x>=width_ && y>=height_)
        {
            return;
        }
        grid_arr_[x, y] = value;
        debug_text_arr_[x, y].text = grid_arr_[x, y].ToString();
    }

    public void SetValue(Vector3 world_pos, int value) //SetCellValue
    {
        Vector2Int coords = GetGridCoords(world_pos);
        SetValue(coords.x, coords.y, value);
    }

    public int GetValue(int x, int y)
    {
        if (x < 0 || y < 0 || x >= width_ && y >= height_)
        {
            return 0;
        }
        else
        {
            return grid_arr_[x, y];
        }
    }

    public int GetValue(Vector3 world_pos)
    {
        Vector2Int coords = GetGridCoords(world_pos);
        return GetValue(coords.x, coords.y);
    }

    public static TextMesh CreateWorldText(string text, Transform parent = null, Vector3 local_pos = default(Vector3), int font_size = 40, Color? color = null, TextAnchor text_anchor = TextAnchor.UpperLeft, TextAlignment text_align = TextAlignment.Left, int sort_order = 5000)
    {
        if (color == null)
        {
            color = Color.white;
        }
            
        return CreateWorldText(parent, text, local_pos, font_size, (Color)color, text_anchor, text_align, sort_order);
    }

    public static TextMesh CreateWorldText(Transform parent, string text, Vector3 local_pos, int font_size, Color color, TextAnchor text_anchor, TextAlignment text_align, int sort_order)
    {
        GameObject go = new GameObject("WorldText", typeof(TextMesh));
        Transform gt = go.transform;
        gt.SetParent(parent, false);
        gt.localPosition = local_pos;
        TextMesh tm = go.GetComponent<TextMesh>();
        tm.anchor = text_anchor;
        tm.alignment = text_align;
        tm.text = text;
        tm.fontSize = font_size;
        tm.color = color;
        tm.GetComponent<MeshRenderer>().sortingOrder = sort_order;
        return tm;
    }
}