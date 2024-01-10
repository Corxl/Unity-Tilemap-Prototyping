using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthEvents : MonoBehaviour
{
    public static HealthEvents healthEvents;
    public delegate void ChangedHealth(DamageableBase entity);
    public event ChangedHealth OnHealthChange = delegate { };
    private void Awake()
    {
        healthEvents = this;
    }

    public void OnHealthChanged(DamageableBase entity)
    {
        if (OnHealthChange != null) OnHealthChange(entity);
    }

    public void log(string message)
    {
        Debug.Log(message);
    }

}
