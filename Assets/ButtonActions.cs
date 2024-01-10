using State;
using State.HealthState;
using State.HealthState.States;
using UnityEngine;
using UnityEngine.UI;

public class ButtonActions : MonoBehaviour
{

    public Button regenerate, poison, respawn;
    public DamageableBase damageable;
    private HealthStateManager manager;
    void Start()
    {
        this.manager = damageable.stateManager;
        regenerate.onClick.AddListener(() =>
        {
            manager.SwitchState(manager.INJURED);
            //damageable.Heal(damageable.max_health);
        });
        poison.onClick.AddListener(() =>
        {
            manager.SwitchState(manager.POISONED);
            Poisoned poisionedStatus = (Poisoned) manager.POISONED;
            poisionedStatus.damagePerTick = 7;
            poisionedStatus.duration = 3;
            //damageable.Heal(damageable.max_health);
        });
        respawn.onClick.AddListener(() =>
        {
            manager.SwitchState(manager.HEALTHY);
            damageable.Heal(damageable.max_health);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
