using UnityEngine;

public class BossContext
{
    private SpikeWarningVFX activeSpikeWarning;

    public void RegisterSpikeWarning(SpikeWarningVFX warning)
    {
        ClearSpikeWarning();
        activeSpikeWarning = warning;
    }

    public void ClearSpikeWarning()
    {
        if (activeSpikeWarning == null)
            return;

        activeSpikeWarning.Hide();
        Object.Destroy(activeSpikeWarning.gameObject);
        activeSpikeWarning = null;
    }

    public Transform Boss { get; }
    public Transform Player { get; }

    public Rigidbody BossRigidbody { get; }
    public Animator BossAnimator { get; }
    public BossDamageHitbox DamageHitbox { get; }
    public ChargeWarningVFX ChargeWarning { get; }
    public BossTelegraphMotion TelegraphMotion { get; }
    public HoofSlamWarningVFX HoofSlamWarning { get; }
    public HoofSlamShockwaveVFX HoofSlamShockwave { get; }
    public ExecutionWarningVFX ExecutionWarning { get; }
    public ExecutionSlashVFX ExecutionSlash { get; }

    public BossContext(
        Transform boss,
        Transform player,
        Rigidbody bossRigidbody,
        Animator bossAnimator,
        BossDamageHitbox damageHitbox,
        ChargeWarningVFX chargeWarning,
        BossTelegraphMotion telegraphMotion,
        HoofSlamWarningVFX hoofSlamWarning,
        HoofSlamShockwaveVFX hoofSlamShockwave,
        ExecutionWarningVFX executionWarning,
        ExecutionSlashVFX executionSlash)
    {
        Boss = boss;
        Player = player;
        BossRigidbody = bossRigidbody;
        BossAnimator = bossAnimator;
        DamageHitbox = damageHitbox;
        ChargeWarning = chargeWarning;
        TelegraphMotion = telegraphMotion;
        HoofSlamWarning = hoofSlamWarning;
        HoofSlamShockwave = hoofSlamShockwave;
        ExecutionWarning = executionWarning;
        ExecutionSlash = executionSlash;
    }
}
