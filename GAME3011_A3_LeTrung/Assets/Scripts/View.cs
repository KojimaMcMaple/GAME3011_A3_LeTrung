using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class View : MonoBehaviour
{
    [SerializeField] private Transform gem_visual_template;
    [SerializeField] private Transform cell_visual_template;

    private Main model_;
    private Grid<Main.GridCell> grid_;
    private Dictionary<Main.Gem, GemVisual> gem_dict_; //linker between model and view

    private Transform cam_;

    private void Start()
    {
        cam_ = Camera.main.transform;

        Init(FindObjectOfType<Main>(), FindObjectOfType<Main>().GetMainGrid());
    }

    private void Update()
    {
        DoUpdateView();
    }

    public void Init(Main model, Grid<Main.GridCell> grid)
    {
        model_ = model;
        grid_ = grid;

        float cam_offset_y = 1f;
        cam_.position = new Vector3(grid_.GetWidth() *.5f, grid_.GetHeight() * .5f + cam_offset_y, cam_.position.z);

        gem_dict_ = new Dictionary<Main.Gem, GemVisual>();

        for (int x = 0; x < grid_.GetWidth(); x++)
        {
            for (int y = 0; y < grid_.GetHeight(); y++)
            {
                Main.GridCell cell = grid_.GetValue(x, y);
                Main.Gem gem = cell.GetCellItem();

                Vector3 position = grid_.GetWorldPos(x, y);
                position = new Vector3(position.x, 12f); //move gem way up at the start

                Transform scene_gem = Instantiate(gem_visual_template, position, Quaternion.identity);
                scene_gem.Find("Sprite").GetComponent<SpriteRenderer>().sprite = gem.GetGem().prefab_.GetComponent<SpriteRenderer>().sprite;
                scene_gem.Find("Sprite").GetComponent<SpriteRenderer>().material = gem.GetGem().prefab_.GetComponent<SpriteRenderer>().sharedMaterial;
                scene_gem.Find("Sprite").GetComponent<Animator>().runtimeAnimatorController = gem.GetGem().prefab_.GetComponent<Animator>().runtimeAnimatorController;

                GemVisual gem_visual = new GemVisual(scene_gem, gem);

                gem_dict_[gem] = gem_visual;

                Instantiate(cell_visual_template, grid_.GetWorldPos(x, y), Quaternion.identity);
            }
        }
    }

    private void DoUpdateView()
    {
        foreach (Main.Gem gem in gem_dict_.Keys)
        {
            gem_dict_[gem].DoUpdate();
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
