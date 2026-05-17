using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "Inventory/Recipe")]
public class CraftRecipe : ScriptableObject
{
    public string recipeName;
    public Ingredient[] ingredients;
    public Item result;
    public int resultQuantity = 1;

    [System.Serializable]
    public class Ingredient
    {
        public Item item;
        public int amount;
    }
}