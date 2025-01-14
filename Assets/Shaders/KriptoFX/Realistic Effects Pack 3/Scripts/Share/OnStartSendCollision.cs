﻿using UnityEngine;

public class OnStartSendCollision : MonoBehaviour
{

    private EffectSettings effectSettings;
    private bool isInitialized;

    private void GetEffectSettingsComponent(Transform tr)
    {
        var parent = tr.parent;
        if (parent != null)
        {
            effectSettings = parent.GetComponentInChildren<EffectSettings>();
            if (effectSettings == null)
                GetEffectSettingsComponent(parent.transform);
        }
    }

    private void Start()
    {
        GetEffectSettingsComponent(transform);
        effectSettings.OnCollisionHandler(new CollisionInfo());
        isInitialized = true;
    }

    private void OnEnable()
    {
        if (isInitialized) effectSettings.OnCollisionHandler(new CollisionInfo());
    }
}
