using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class ObjectChange : MonoBehaviour
{
    [SerializeField]
    private List<CustomObject> elements = new List<CustomObject>();
    public CustomObject selectedObject;
    public AnchorUIManager anchorUIManager;
    private int startIndex = 0;

    void Start()
    {
        if (selectedObject != null) return;
        if (elements.Count == 0)
        {
            Debug.LogError("Elements list is empty or not assigned.");
            return;
        }
        selectedObject = elements[startIndex];
        updateManagerData();
    }

    public void ChangeObject()
    {
        if (elements.Count == 0)
        {
            Debug.LogError("Elements list is empty or not assigned.");
            return;
        }
        var isCurrentlyActive = selectedObject.ObjectPreview.activeSelf;
        if (isCurrentlyActive)
        {
            selectedObject.ObjectPreview.SetActive(false);
        }

        startIndex = (startIndex + 1) % elements.Count;
        selectedObject = elements[startIndex];
        if (isCurrentlyActive)
        {
            selectedObject.ObjectPreview.SetActive(true);
        }
        updateManagerData();
    }

    void updateManagerData()
    {
        SetPrivateField(anchorUIManager, "_anchorPrefab", selectedObject.PrefabAnchor);
        SetPrivateField(anchorUIManager, "_placementPreview", selectedObject.ObjectPreview);
        SetPrivateField(anchorUIManager, "_anchorPlacementTransform", selectedObject.ObjectPreviewTransform);
    }

    void SetPrivateField(object target, string fieldName, object value)
    {
        var type = target.GetType();
        var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(target, value);
        }
        else
        {
            Debug.LogWarning($"Campo '{fieldName}' não encontrado em {type.Name}");
        }
    }
}
