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

        // 렌더러와 체력을 초기화하고 그림자와 선택 외곽선을 생성한다.
        private void Awake()
        {
            mainRenderer = GetComponent<SpriteRenderer>();
            currentHealth = maxHealth;
            BuildShadow();
            BuildOutline();
            SetHighlighted(false);
        }

        // 생성한 MiningManager를 소유자로 등록하고 광물 노드 상태를 초기화한다.
        public void Initialize(MiningManager manager)
        {
            owner = manager;
            currentHealth = maxHealth;
            destroyed = false;
            
            if (randomJewelColor && mainRenderer != null)
            {
                mainRenderer.color = JewelColors[Random.Range(0, JewelColors.Length)];
            }
        }

        // 채광 피해를 적용하고 체력이 소진되면 로그 출력, 재생성 통지, 파괴를 수행한다.
        public void TakeDamage(int amount)
        {
            if (destroyed || amount <= 0) 
            {
                return;
            }

            currentHealth -= amount;

            Debug.Log($"[MineTest] {name} 채광 피해 {amount}, 남은 체력 {Mathf.Max(0, currentHealth)}/{maxHealth}", this);

            if (currentHealth > 0) 
            {
                return;
            }

            destroyed = true;
            SetHighlighted(false);

            Debug.Log(CompareTag("jewel")
                ? "[MineTest] 광석을 획득했습니다."
                : "[MineTest] 암석을 획득했습니다.", this);
                
            if (owner != null) 
            {
                owner.NotifyNodeDestroyed(this);
            }

            Destroy(gameObject);
        }

        // 마우스로 선택된 광물의 흰색 외곽선 표시 여부를 변경한다.
        public void SetHighlighted(bool highlighted)
        {
            foreach (SpriteRenderer outline in outlines)
            {
                if (outline != null) 
                {
                    outline.enabled = highlighted;
                }
            }
        }

        // 광물이 캐릭터를 가릴 때 본체 스프라이트의 투명도를 조절한다.
        public void SetOccluded(bool occluded)
        {
            if (mainRenderer == null) 
            {
                return;
            }

            Color color = mainRenderer.color;
            color.a = occluded ? occludedAlpha : 1f;

            mainRenderer.color = color;
        }

        // 본체 스프라이트를 복제하여 바닥 그림자 렌더러를 생성한다.
        private void BuildShadow()
        {
            if (mainRenderer == null || shadowRenderer != null) 
            {
                return;
            }
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

        // 본체 주변 여덟 방향에 흰색 스프라이트를 배치해 선택 외곽선을 만든다.
        private void BuildOutline()
        {
            if (mainRenderer == null || outlines.Count > 0) 
            {
                return;
            }

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
