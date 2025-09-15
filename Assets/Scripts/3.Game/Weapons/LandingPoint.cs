using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LandingPoint : LandingPointManager
{
    [Header("Local Setting")]
    [SerializeField] RectTransform canvasRect;
    [SerializeField] CanvasScaler canvasScaler;
    [SerializeField] RectTransform rectTransform;

    float screenWidthbyWorldUnits;
    float screenHeightbyWorldUnits;
    float offsetX;
    float offsetY;

    float convertUI = 10f;
    float initSize = 100f;

    LandingPointManager landingPointManager;

    private void OnEnable()
    {
        rectTransform.sizeDelta = new Vector2(initSize, initSize);
    }

    protected override void Awake()
    {
        base.Awake();

        landingPointManager = LandingPointManager.Instance;

        screenWidthbyWorldUnits = Camera.main.orthographicSize * 2;
        screenHeightbyWorldUnits = screenWidthbyWorldUnits * Camera.main.aspect;
        offsetX = Camera.main.pixelHeight / screenWidthbyWorldUnits;
        offsetY = Camera.main.pixelWidth / screenHeightbyWorldUnits;
        convertUI = canvasScaler.referencePixelsPerUnit;

        Debug.Log("screenWidthbyWorldUnits: "+ screenWidthbyWorldUnits);
        Debug.Log("screenHeightbyWorldUnits: " + screenHeightbyWorldUnits);
        Debug.Log("Camera.main.pixelWidth: " + Camera.main.pixelWidth);
        Debug.Log("Camera.main.pixelHeight: " + Camera.main.pixelHeight);
        Debug.Log("offsetX: " + offsetX + " offsetY: " + offsetY);
        Debug.Log("convertUI: " + convertUI);
    }

    private void Update()
    {
        if (weaponManager.chargeTime > 0f)
            UpdateImageScale(weaponManager.chargeTime);

        else
            UpdateImageScale(0f);

    }

    public void UpdateLandingPoint(Vector3 targetPos)
    {
        // Debug.Log("targetPos.x : " + targetPos.x + " targetPos.z : " + targetPos.z);
        rectTransform.anchoredPosition = new Vector2(targetPos.x * offsetX, targetPos.z * offsetY);
    }

    void UpdateImageScale(float chargeTime)
    {
        float currentSize = initSize + (100 * (chargeTime / 2f));
        rectTransform.localScale = new Vector2(currentSize/100f, currentSize/100f);
    }
}
