using UnityEngine;

// 무기의 공격 방식
public enum WeaponType { Melee, Ranged }

// 무기가 사용하는 탄약 종류. None은 탄약을 사용하지 않음
public enum AmmoType { None, Bullet, Arrow }

// 프로젝트 창의 Create → Combat → Weapon Data 메뉴에서 에셋 생성
[CreateAssetMenu(fileName = "WeaponData", menuName = "Combat/Weapon Data")]
public class WeaponData : ScriptableObject
{
    // 무기를 식별하는 아이템 ID. 무기 테이블의 ITEM_ID와 대응
    [SerializeField] private int _itemId;

    // 근접 무기인지 원거리 무기인지 구분, 공격 실행기를 선택할 때 사용하는 설정
    [SerializeField] private WeaponType _weaponType;

    // 원거리 공격 시 검사하고 소비할 탄약 종류, 근접 무기는 None으로 설정
    [SerializeField] private AmmoType _ammoType;

    // 이 무기가 사용할 콤보 데이터 에셋, 검이라면 ComboId가 "COMBO_SWORD"인 에셋을 연결
    [SerializeField] private AttackComboData _comboData;

    // 이 무기가 연속으로 실행할 수 있는 최대 타수, 콤보 데이터에 정의된 타수 개수 이하로 설정해야 함
    [SerializeField, Min(1)] private int _maxComboCount = 1;

    // 무기의 기본 공격력, 타격 피해량 = 기본 공격력 × 해당 타수의 DamageMultiplier
    [SerializeField, Min(0f)] private float _baseDamage = 10f;

    // 무기의 최대 내구도. 개별 무기 생성 시 초기 내구도로 사용할 기준값, 사용하면서 감소하는 현재 내구도는 개별 아이템 데이터에서 관리
    [SerializeField, Min(0)] private int _maxDurability = 100;

    // 외부에서 무기의 설정값을 읽기 위한 읽기 전용 프로퍼티
    public int ItemId => _itemId;
    public WeaponType WeaponType => _weaponType;
    public AmmoType AmmoType => _ammoType;
    public AttackComboData ComboData => _comboData;
    public int MaxComboCount => _maxComboCount;
    public float BaseDamage => _baseDamage;
    public int MaxDurability => _maxDurability;

    // 콤보 에셋이 연결돼 있고, 사용할 타수만큼 데이터가 준비됐는지 검사, 각 타수의 애니메이션 이름이나 범위 값까지 검사하는 것은 아님
    public bool HasValidCombo => _comboData != null && _maxComboCount > 0 && _maxComboCount <= _comboData.StepCount;
}