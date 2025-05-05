using UnityEngine;
using Vuforia;
public class CarSkinApplier : MonoBehaviour
{
    [Header("Skin Settings")]
    public Material[] availableSkins;
    public string bodyMaterialName = "CarBody";

    private Renderer _carRenderer;
    private bool _appliedSkin;
    private int _pendingSkinIndex = -1;

    void Start()
    {
        // Get renderer even if inactive
        _carRenderer = GetComponentInChildren<Renderer>(true);

        if (_carRenderer == null)
        {
            Debug.LogError("No Renderer found in children!", this);
            enabled = false;
            return;
        }

        var eventHandler = GetComponentInParent<DefaultObserverEventHandler>();
        if (eventHandler != null)
        {
            eventHandler.OnTargetFound.AddListener(OnTrackingFound);
            eventHandler.OnTargetLost.AddListener(OnTrackingLost);
        }
        else
        {
            Debug.LogError("No DefaultObserverEventHandler found in parent objects!");
        }

        // Try immediate apply if already active
        if (_carRenderer.enabled)
        {
            ApplySkin(PlayerPrefs.GetInt("SelectedCarSkin", 0));
        }
    }

    void OnTrackingFound()
    {
        Debug.Log("Target found - applying skin");
        if (_pendingSkinIndex >= 0)
        {
            ApplySkin(_pendingSkinIndex);
            _pendingSkinIndex = -1;
        }
        else
        {
            ApplySkin(PlayerPrefs.GetInt("SelectedCarSkin", 0));
        }
    }

    void OnTrackingLost()
    {
        _appliedSkin = false;
    }

    public void ApplySkin(int skinIndex)
    {
        skinIndex = Mathf.Clamp(skinIndex, 0, availableSkins.Length - 1);

        if (!_carRenderer.enabled)
        {
            Debug.Log("Renderer inactive - queuing skin change");
            _pendingSkinIndex = skinIndex;
            return;
        }

        Debug.Log($"Applying skin index {skinIndex}: {availableSkins[skinIndex].name}");

        Material[] mats = _carRenderer.sharedMaterials;
        bool materialReplaced = false;

        for (int i = 0; i < mats.Length; i++)
        {
            if (mats[i] != null && mats[i].name.Contains(bodyMaterialName))
            {
                Debug.Log($"Replacing material at index {i} ({mats[i].name})");
                mats[i] = availableSkins[skinIndex];
                materialReplaced = true;
                break;
            }
        }

        if (!materialReplaced && mats.Length > 0)
        {
            Debug.LogWarning("No matching material found - using first slot");
            mats[0] = availableSkins[skinIndex];
        }

        _carRenderer.sharedMaterials = mats;
        _appliedSkin = true;
    }

    void OnDestroy()
    {
        var eventHandler = GetComponentInParent<DefaultObserverEventHandler>();
        if (eventHandler != null)
        {
            eventHandler.OnTargetFound.RemoveListener(OnTrackingFound);
            eventHandler.OnTargetLost.RemoveListener(OnTrackingLost);
        }
    }
}