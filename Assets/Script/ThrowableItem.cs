using UnityEngine;

[CreateAssetMenu(fileName = "NewThrowable", menuName = "Inventory/ThrowableItem")]
public class ThrowableItem : Item
{
    [Header("Throw Settings")]
    public GameObject projectilePrefab;  // the physical object spawned when thrown
    public float throwForce = 12f;
    public float throwAngle = 35f;       // upward angle in degrees
}