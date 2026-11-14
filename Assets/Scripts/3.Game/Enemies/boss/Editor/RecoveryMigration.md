# Recovery field migration

Values recorded from serialized SO assets before moving recoveryDuration to BossPattern:

| Asset | recoveryDuration |
| --- | --- |
| Assets/Scripts/3.Game/Enemies/boss/Patterns/Charge/BP_Charge.asset | 0.5 seconds |
| Assets/Scripts/3.Game/Enemies/boss/Patterns/HoofSlam/BP_HoofSlam.asset | 0.6 seconds |

Existing YAML fields are retained without changing the assets. New pattern assets
default to the BossPattern value of 1.2 seconds. After Unity recompiles, inspect
both existing assets to confirm these values remain 0.5 and 0.6 respectively.

BossController now owns the recovery wait. Execute ends after the attack;
pattern subclasses must not wait for recoveryDuration themselves.
During Recovery, IsAttacking is false and basic chase remains stopped.
After Recovery, CurrentState returns to Chase and movement resumes, followed
by the existing patternInterval wait.
