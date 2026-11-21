using UnityEngine;

public class ConsumableItemDefinition : MonoBehaviour
{
    [SerializeField] private int itemId;

    public int ItemId => itemId;

    public bool CanUse(Player player)
    {
        ConsumableEffect[] effects = GetComponents<ConsumableEffect>();

        if (effects.Length == 0)
            return false;

        foreach (ConsumableEffect effect in effects)
        {
            if (!effect.CanApply(player))
                return false;
        }

        return true;
    }

    public void Use(Player player)
    {
        ConsumableEffect[] effects = GetComponents<ConsumableEffect>();

        foreach (ConsumableEffect effect in effects)
        {
            effect.Apply(player);
        }
    }
}