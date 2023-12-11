using System;
using System.Collections.Generic;
using UnityEngine;

public class PlatesCounterVisual : MonoBehaviour
{
    [SerializeField] private PlatesCounter platesCounter;
    [SerializeField] private Transform counterTopPoint;
    [SerializeField] private Transform plateVisualPrefab;

    private Stack<GameObject> plateVisualGameObjectStack;

    private void Awake()
    {
        plateVisualGameObjectStack = new();
    }

    private void Start()
    {
        platesCounter.OnPlateSpawned += PlatesCounter_OnPlateSpawned;
        platesCounter.OnPlateRemoved += PlatesCounter_OnPlateRemoved;
    }

    private void PlatesCounter_OnPlateRemoved(object sender, EventArgs e)
    {
        if (plateVisualGameObjectStack.TryPop(out GameObject plateGameObject))
        {
            Destroy(plateGameObject);
        }
        else
        {
            Debug.LogError("没有盘子游戏对象可销毁");
        }
    }

    private void PlatesCounter_OnPlateSpawned(object sender, EventArgs e)
    {
        Transform plateVisualTransform = Instantiate(plateVisualPrefab, counterTopPoint);

        float plateOffsetY = 0.1f;
        plateVisualTransform.localPosition = new(0, plateOffsetY * plateVisualGameObjectStack.Count, 0);

        plateVisualGameObjectStack.Push(plateVisualTransform.gameObject);
    }
}