using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIEntity : MonoBehaviour
{
    // Start is called before the first frame update
    public TextMeshProUGUI text;
    public EntityHealth entity;


    void Start()
    {
        HealthEvents.healthEvents.OnHealthChange += HealthChange;
        this.HealthChange(entity);
    }

    public void HealthChange(DamageableBase e)
    {
        if (!this.entity.Equals(entity)) return;
        text.text = $"{this.entity.health} / {this.entity.max_health}";
    }




}
