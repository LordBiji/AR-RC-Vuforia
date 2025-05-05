using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class CarSkinSelector : MonoBehaviour
{
    [Header("Car Setup")]
    public GameObject carModel;
    public Material[] skinMaterials;
    public string bodyMaterialName = "CarBody"; // Name of your car's body material

    [Header("UI Elements")]
    public RectTransform skinPanel;
    public Image[] skinThumbnails;
    public Color selectedColor = Color.white;
    public Color unselectedColor = new Color(1, 1, 1, 0.5f);
    public Color[] thumbnailColors; // Assign your specific colors here
    public Button nextButton;
    public Button prevButton;

    [Header("Rotation")]
    public float rotationSpeed = 15f;
    public bool autoRotate = true;

    private Renderer carRenderer;
    private Material bodyMaterial;
    private int currentSkinIndex;
    private Vector2 touchStartPos;

    void Start()
    {
        // Get car body material
        carRenderer = carModel.GetComponentInChildren<Renderer>();
        bodyMaterial = FindBodyMaterial(carRenderer);

        // Initialize thumbnails
        UpdateThumbnailSelection();

        // Button listeners
        nextButton.onClick.AddListener(NextSkin);
        prevButton.onClick.AddListener(PreviousSkin);

        // Default rotation
        if (autoRotate) carModel.transform.DORotate(new Vector3(0, 360, 0), 10f, RotateMode.LocalAxisAdd)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear);
    }

    void Update()
    {
        // Touch/mouse swipe detection
        if (Input.GetMouseButtonDown(0))
        {
            touchStartPos = Input.mousePosition;
            if (autoRotate) DOTween.Kill(carModel.transform);
        }
        else if (Input.GetMouseButton(0))
        {
            Vector2 direction = (Vector2)Input.mousePosition - touchStartPos;
            if (direction.magnitude > 10f)
            {
                float rotateAmount = direction.x * 0.1f;
                carModel.transform.Rotate(0, -rotateAmount, 0);
                touchStartPos = Input.mousePosition;
            }
        }
        else if (Input.GetMouseButtonUp(0) && autoRotate)
        {
            ResetAutoRotation();
        }
    }

    Material FindBodyMaterial(Renderer renderer)
    {
        foreach (Material mat in renderer.sharedMaterials)
        {
            if (mat.name.Contains(bodyMaterialName))
                return mat;
        }
        return renderer.material;
    }

    public void NextSkin()
    {
        currentSkinIndex = (currentSkinIndex + 1) % skinMaterials.Length;
        ChangeSkinWithAnimation();
    }

    public void PreviousSkin()
    {
        currentSkinIndex = (currentSkinIndex - 1 + skinMaterials.Length) % skinMaterials.Length;
        ChangeSkinWithAnimation();
    }

    void ChangeSkinWithAnimation()
    {
        // Slide animation
        skinPanel.DOComplete();
        skinPanel.DOLocalMoveX(-currentSkinIndex * 200f, 0.3f).SetEase(Ease.OutQuad);

        // Material fade effect
        DOTween.Sequence()
            .Append(carRenderer.material.DOFade(0, 0.2f).SetEase(Ease.OutQuad))
            .AppendCallback(() => bodyMaterial.CopyPropertiesFromMaterial(skinMaterials[currentSkinIndex]))
            .Append(carRenderer.material.DOFade(1, 0.3f).SetEase(Ease.InQuad));

        UpdateThumbnailSelection();
    }

    void UpdateThumbnailSelection()
    {
        for (int i = 0; i < skinThumbnails.Length; i++)
        {
            // Preserve the original color while changing opacity
            Color targetColor = thumbnailColors[i];
            targetColor.a = (i == currentSkinIndex) ? selectedColor.a : unselectedColor.a;

            skinThumbnails[i].color = targetColor;

            // Optional: Add scale effect
            skinThumbnails[i].transform.localScale =
                (i == currentSkinIndex) ? Vector3.one * 1.1f : Vector3.one;
        }
    }

    void ResetAutoRotation()
    {
        carModel.transform.DORotate(new Vector3(0, 360, 0), 10f, RotateMode.LocalAxisAdd)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear);
    }

    public void OnPlayButtonClicked()
    {
        // Save selected skin index
        PlayerPrefs.SetInt("SelectedCarSkin", currentSkinIndex);
        PlayerPrefs.Save(); // Force immediate save

        // Load AR scene
        SceneManager.LoadScene("ARCar");
    }
}