using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image healthFill;
    [Range(0, 1)]
    [SerializeField] private float lowHealthPercentage = 0.3f;
    [SerializeField] private Color lowHealthColor, normalHealthColor;
    private Coroutine colorBlinkRoutine;
    // Start is called before the first frame update
    void Start()
    {
        HealthEvents.healthEvents.OnHealthChange += OnHealthChange;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator healthBlink(float intervalSeconds)
    {
        bool blink = false;
        while (true)
        {
            yield return new WaitForSeconds(intervalSeconds);

            for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / intervalSeconds)
            {

                healthFill.color = blink ? Color.Lerp(lowHealthColor, Color.white, t) : Color.Lerp(Color.white, lowHealthColor, t);
                yield return null;
            }
            //}

            //    healthFill.color = blink ? Color.Lerp(lowHealthColor, Color.white, intervalSeconds * 10) : Color.Lerp(Color.white, lowHealthColor, intervalSeconds * 10);
            blink = !blink;
        }
        
    }


    private void OnHealthChange(DamageableBase damageable)
    {
        healthFill.fillAmount = damageable.getHealthPercentage();
        if (damageable.getHealthPercentage() < lowHealthPercentage) {
            if (colorBlinkRoutine == null)
                colorBlinkRoutine = StartCoroutine(healthBlink(0.5f));
        } else
        {
            if (colorBlinkRoutine != null)
            {
                StopCoroutine(colorBlinkRoutine);
                colorBlinkRoutine = null;
            }
            healthFill.color = normalHealthColor;
        }
        
    }
}
