using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class GemSO : ScriptableObject
{
    public string name_;
    public GameObject prefab_;
    public Sprite sprite_;
    public Material mat_;
    public Animator anim_;
}
