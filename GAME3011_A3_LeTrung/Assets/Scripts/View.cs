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
    private State state_;
    private float busy_timer_;

    private Transform cam_;
    private int start_drag_x_;
    private int start_drag_y_;
    private int dest_drag_x_;
    private int dest_drag_y_;

    private Action OnEndBusyAction;

    private VfxManager vfx_manager_;

    public enum State
    {
        kBusy,
        kAvailable,
        kProcessing,
        kGameOver
    }

    private void Awake()
    {
        state_ = State.kBusy;
        cam_ = Camera.main.transform;
        vfx_manager_ = FindObjectOfType<VfxManager>();
    }

    private void Start()
    {
        Init(FindObjectOfType<Main>(), FindObjectOfType<Main>().GetMainGrid());
    }

    private void Update()
    {
        DoUpdateView();

        switch (state_)
        {
            case State.kBusy:
                if (busy_timer_ > 0)
                {
                    busy_timer_ -= Time.deltaTime;
                }
                else
                {
                    OnEndBusyAction();
                }
                break;
            case State.kAvailable:
                if (Input.GetMouseButtonDown(0))
                {
                    Vector3 world_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector2Int coords = grid_.GetGridCoords(world_pos);
                    start_drag_x_ = coords.x;
                    start_drag_y_ = coords.y;

                    vfx_manager_.GetVfx(new Vector3(start_drag_x_, start_drag_y_), GlobalEnums.VfxType.HIT);
                }

                if (Input.GetMouseButtonUp(0))
                {
                    Vector3 world_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector2Int coords = grid_.GetGridCoords(world_pos);
                    dest_drag_x_ = coords.x;
                    dest_drag_y_ = coords.y;

                    if (dest_drag_x_ != start_drag_x_)
                    {
                        dest_drag_y_ = start_drag_y_;
                        if (dest_drag_x_ < start_drag_x_)
                        {
                            dest_drag_x_ = start_drag_x_ - 1;
                        }
                        else
                        {
                            dest_drag_x_ = start_drag_x_ + 1;
                        }
                    }
                    else
                    {
                        dest_drag_x_ = start_drag_x_;
                        if (dest_drag_y_ < start_drag_y_)
                        {
                            dest_drag_y_ = start_drag_y_ - 1;
                        }
                        else
                        {
                            dest_drag_y_ = start_drag_y_ + 1;
                        }
                    }

                    if (model_.TrySwapGridCells(start_drag_x_, start_drag_y_, dest_drag_x_, dest_drag_y_))
                    {
                        SetBusyState(.5f, () => state_ = State.kProcessing);
                    }
                }
                break;
            case State.kProcessing:
                if (model_.TryProcessMatches(start_drag_x_, start_drag_y_, dest_drag_x_, dest_drag_y_))
                {
                    SetBusyState(.5f, () => state_ = State.kAvailable);
                }
                break;
        }
    }

    public void Init(Main model, Grid<Main.GridCell> grid)
    {
        model_ = model;
        grid_ = grid;

        float cam_offset_y = 1f;
        cam_.position = new Vector3(grid_.GetWidth() *.5f, grid_.GetHeight() * .5f + cam_offset_y, cam_.position.z);

        model_.OnGridCellDestroyed += HandleGridCellDestroyedEvent;

        gem_dict_ = new Dictionary<Main.Gem, GemVisual>();
        for (int x = 0; x < grid_.GetWidth(); x++)
        {
            for (int y = 0; y < grid_.GetHeight(); y++)
            {
                Main.GridCell cell = grid_.GetValue(x, y);
                Main.Gem gem = cell.GetCellItem();

                Vector3 position = grid_.GetWorldPos(x, y);
                position = new Vector3(position.x, 13f); //move gem way up at the start

                Transform scene_gem = Instantiate(gem_visual_template, position, Quaternion.identity);
                scene_gem.Find("Sprite").GetComponent<SpriteRenderer>().sprite = gem.GetGemSO().prefab_.GetComponent<SpriteRenderer>().sprite;
                scene_gem.Find("Sprite").GetComponent<SpriteRenderer>().material = gem.GetGemSO().prefab_.GetComponent<SpriteRenderer>().sharedMaterial;
                scene_gem.Find("Sprite").GetComponent<Animator>().runtimeAnimatorController = gem.GetGemSO().prefab_.GetComponent<Animator>().runtimeAnimatorController;

                GemVisual gem_visual = new GemVisual(scene_gem, gem);

                gem_dict_[gem] = gem_visual;

                Instantiate(cell_visual_template, grid_.GetWorldPos(x, y), Quaternion.identity);
            }
        }

        SetBusyState(.5f, () => state_ = State.kAvailable);
    }

    private void DoUpdateView()
    {
        foreach (Main.Gem gem in gem_dict_.Keys)
        {
            gem_dict_[gem].DoUpdate();
        }
    }

    private void SetBusyState(float wait_time, Action NewAction)
    {
        state_ = State.kBusy;
        busy_timer_ = wait_time;
        OnEndBusyAction = NewAction;
    }

    public void SwapGridCells(int start_x, int start_y, int dest_x, int dest_y)
    {
        model_.SwapGridCells(start_x, start_y, dest_x, dest_y);

        SetBusyState(0.5f, () => state_ = State.kProcessing);
    }


    private void HandleGridCellDestroyedEvent(object sender, System.EventArgs e)
    {
        Main.GridCell cell = sender as Main.GridCell;
        if (cell != null && cell.GetCellItem() != null)
        {
            gem_dict_.Remove(cell.GetCellItem());
        }
    }



    public class GemVisual
    {
        private Transform transform_;
        private Main.Gem gem_;
        private VfxManager vfx_manager_; //[TODO] can be made into event

        public GemVisual(Transform t, Main.Gem gem)
        {
            transform_ = t;
            gem_ = gem;

            gem_.OnDestroyed += HandleGemDestroyedEvent;

            vfx_manager_ = FindObjectOfType<VfxManager>();
        }

        public void DoUpdate()
        {
            Vector3 target = gem_.GetWorldPos();
            Vector3 dir = target - transform_.position;
            float speed = 3.5f;
            transform_.position += dir * speed * Time.deltaTime;
        }

        private void HandleGemDestroyedEvent(object sender, System.EventArgs e)
        {
            vfx_manager_.GetVfx(transform_.position, GlobalEnums.VfxType.GEM_CLEAR);
            Destroy(transform_.gameObject, 1.0f);
        }
    }
}
