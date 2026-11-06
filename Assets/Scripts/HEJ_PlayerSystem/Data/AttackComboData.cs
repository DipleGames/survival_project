using UnityEngine;

// 프로젝트 창의 Create → Combat → Attack Combo Data 메뉴에서 에셋 생성
[CreateAssetMenu(fileName = "AttackComboData", menuName = "Combat/Attack Combo Data")]
public class AttackComboData : ScriptableObject
{
    // 콤보를 구분하는 ID. 무기 테이블의 COMBO_ID와 대응 / 예: "COMBO_SWORD", "COMBO_GUN", "COMBO_BOW"
    [SerializeField] private string _comboId;

    // 각 타수의 설정을 공격 순서대로 저장 / steps[0] = 1타, steps[1] = 2타, steps[2] = 3타
    [SerializeField] private AttackStepData[] _steps = new AttackStepData[0];

    // 외부에서 콤보 ID를 읽을 수 있도록 제공
    public string ComboId => _comboId;

    // 이 콤보에 정의된 타수의 개수. 배열에 데이터가 3개 있으면 3 반환
    public int StepCount => _steps.Length;

    // 지정한 순서의 타수 데이터를 반환 / index는 0 이상, StepCount 미만이어야 함
    public AttackStepData GetStep(int index)
    {
        return _steps[index];
    }
}