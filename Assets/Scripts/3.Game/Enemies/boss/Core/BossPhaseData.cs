using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "BP_Phase",
    menuName = "Boss/Phase Data"
)]
public class BossPhaseData : ScriptableObject
{
    [Header("페이즈 정보")]
    public string phaseName;

    [Header("페이즈 애니메이터")]
    [SerializeField]
    private RuntimeAnimatorController animatorController;

    public RuntimeAnimatorController AnimatorController => animatorController;

    [Header("페이즈 진입 체력 비율")]
    [SerializeField, Range(0f, 1f)]
    private float enterHealthRatio = 1f;

    [Header("사용할 공격 패턴")]
    [SerializeField]
    private List<BossPattern> patterns = new List<BossPattern>();

    public float EnterHealthRatio => enterHealthRatio;

    public IReadOnlyList<BossPattern> Patterns => patterns;
}
