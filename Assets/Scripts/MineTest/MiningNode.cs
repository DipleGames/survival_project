using System.Collections.Generic;
using UnityEngine;

namespace MineTest
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider), typeof(SpriteRenderer))]
    public sealed class MiningNode : MonoBehaviour
    {
        [SerializeField, Min(1)] private int maxHealth = 25;
        [SerializeField] private bool randomJewelColor;
        [SerializeField, Min(0.001f)] private float outlineOffset = 0.07f;
        [SerializeField, Range(0.05f, 1f)] private float occludedAlpha = 0.5f;
        [SerializeField, Range(0f, 1f)] private float shadowAlpha = 0.25f;
        [SerializeField] private Vector2 shadowScale = new Vector2(0.72f, 0.24f);
        [SerializeField] private Vector2 shadowOffset = new Vector2(0f, -0.38f);

        private static readonly Color[] JewelColors =
        {
            Color.red, Color.blue, Color.green, Color.yellow,
            new Color(0.6f, 0.2f, 0.8f, 1f)
        };

        private readonly List<SpriteRenderer> outlines = new List<SpriteRenderer>();
        private SpriteRenderer mainRenderer;
        private SpriteRenderer shadowRenderer;
        private MiningManager owner;
        private int currentHealth;
        private bool destroyed;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public SpriteRenderer MainRenderer => mainRenderer;

        private void Awake()
        {
            mainRenderer = GetComponent<SpriteRenderer>();
            currentHealth = maxHealth;
            BuildShadow();
            BuildOutline();
            SetHighlighted(false);
        }

        public void Initialize(MiningManager manager)
        {
            owner = manager;
            currentHealth = maxHealth;
            destroyed = false;
            if (randomJewelColor && mainRenderer != null)
                mainRenderer.color = JewelColors[Random.Range(0, JewelColors.Length)];
        }

        public void TakeDamage(int amount)
        {
            if (destroyed || amount <= 0) return;
            currentHealth -= amount;
            Debug.Log($"[MineTest] {name} 채광 피해 {amount}, 남은 체력 {Mathf.Max(0, currentHealth)}/{maxHealth}", this);
            if (currentHealth > 0) return;

            destroyed = true;
            SetHighlighted(false);
            Debug.Log(CompareTag("jewel")
                ? "[MineTest] 광석을 획득했습니다."
                : "[MineTest] 암석을 획득했습니다.", this);
            if (owner != null) owner.NotifyNodeDestroyed(this);
            Destroy(gameObject);
        }

        public void SetHighlighted(bool highlighted)
        {
            foreach (SpriteRenderer outline in outlines)
                if (outline != null) outline.enabled = highlighted;
        }

        public void SetOccluded(bool occluded)
        {
            if (mainRenderer == null) return;
            Color color = mainRenderer.color;
            color.a = occluded ? occludedAlpha : 1f;
            mainRenderer.color = color;
        }

        private void BuildShadow()
        {
            if (mainRenderer == null || shadowRenderer != null) return;
            var child = new GameObject("Ground Shadow", typeof(SpriteRenderer));
            child.transform.SetParent(transform, false);
            child.transform.localPosition = new Vector3(shadowOffset.x, shadowOffset.y, 0.01f);
            child.transform.localScale = new Vector3(shadowScale.x, shadowScale.y, 1f);
            shadowRenderer = child.GetComponent<SpriteRenderer>();
            shadowRenderer.sprite = mainRenderer.sprite;
            shadowRenderer.color = new Color(0f, 0f, 0f, shadowAlpha);
            shadowRenderer.sortingLayerID = mainRenderer.sortingLayerID;
            shadowRenderer.sortingOrder = mainRenderer.sortingOrder - 2;
        }

        private void BuildOutline()
        {
            if (mainRenderer == null || outlines.Count > 0) return;
            Vector2[] offsets =
            {
                Vector2.left, Vector2.right, Vector2.up, Vector2.down,
                new Vector2(-0.707f, -0.707f), new Vector2(-0.707f, 0.707f),
                new Vector2(0.707f, -0.707f), new Vector2(0.707f, 0.707f)
            };

            foreach (Vector2 direction in offsets)
            {
                var child = new GameObject("White Outline", typeof(SpriteRenderer));
                child.transform.SetParent(transform, false);
                child.transform.localPosition = new Vector3(direction.x, direction.y, 0f) * outlineOffset;
                SpriteRenderer outline = child.GetComponent<SpriteRenderer>();
                outline.sprite = mainRenderer.sprite;
                outline.color = Color.white;
                outline.sortingLayerID = mainRenderer.sortingLayerID;
                outline.sortingOrder = mainRenderer.sortingOrder - 1;
                outlines.Add(outline);
            }
        }
    }
}
