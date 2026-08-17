/// <summary>
/// 농사 세부 행동들이 공통으로 구현해야 하는 인터페이스.
/// 행동 실행 가능 여부 확인과 실제 행동 실행 기능을 정의한다.
/// </summary>
public interface IFarmingAction
{
    bool CanExecute(FarmTile tile);
    void Execute(FarmTile tile, SkullController skull);
}