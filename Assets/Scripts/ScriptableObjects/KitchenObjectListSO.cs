using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu()]
public class KitchenObjectListSO : ScriptableObject
{
    [SerializeField] private List<KitchenObjectSO> kitchenObjectSOList;

    public KitchenObjectSO this[int index] => kitchenObjectSOList[index];

    public int IndexOf(KitchenObjectSO kitchenObjectSO)
    {
        return kitchenObjectSOList.IndexOf(kitchenObjectSO);
    }
}