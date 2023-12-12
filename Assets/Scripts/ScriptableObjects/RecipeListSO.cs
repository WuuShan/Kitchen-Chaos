using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu()]
public class RecipeListSO : ScriptableObject
{
    [SerializeField] private List<RecipeSO> recipeSOList;

    public RecipeSO this[int index] => recipeSOList[index];

    public int Count => recipeSOList.Count;
}