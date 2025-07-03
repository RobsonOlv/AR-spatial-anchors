using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class CustomObject
{
    [SerializeField]
    private Anchor prefabAnchor;

    [SerializeField]
    private GameObject objectPreview;

    [SerializeField]
    private Transform objectPreviewTransform;

    public Anchor PrefabAnchor => prefabAnchor;
    public GameObject ObjectPreview => objectPreview;
    public Transform ObjectPreviewTransform => objectPreviewTransform;
}