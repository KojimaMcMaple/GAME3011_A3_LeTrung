using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class View : MonoBehaviour
{
    private Main model_;
    private Grid<Main.GridCell> grid_;

    private void Start() //wait for Model to set up first
    {
        Init(FindObjectOfType<Main>(), FindObjectOfType<Main>().GetMainGrid());
    }

    public void Init(Main model, Grid<Main.GridCell> grid)
    {
        model_ = model;
        grid_ = grid;

        for (int x = 0; x < grid_.GetWidth(); x++)
        {
            for (int y = 0; y < grid_.GetHeight(); y++)
            {
                Main.GridCell cell = grid_.GetValue(x, y);
                Main.Gem gem = cell.GetCellItem();

                Vector3 position = grid_.GetWorldPos(x, y);
                position = new Vector3(position.x, 12f); //move gem way up at the start
                

            }
        }
    }

    public class GemVisual
    {
        private Transform transform_;
        private Main.Gem gem_;

        public GemVisual(Transform t, Main.Gem gem)
        {
            transform_ = t;
            gem_ = gem;
        }

        public void DoUpdate()
        {
            Vector3 target = gem_.GetWorldPos();
            Vector3 dir = target - transform_.position;
            float speed = 4f;
            transform_.position += dir * speed * Time.deltaTime;
        }
    }
}
