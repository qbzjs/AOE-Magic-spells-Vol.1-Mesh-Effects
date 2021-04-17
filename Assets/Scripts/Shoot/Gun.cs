﻿using UnityEngine;

/// <summary>
/// оружие
/// </summary>
public abstract class Gun : MonoBehaviour
{
    public delegate void RecoilHandler();
    public event RecoilHandler RecoilEvent;// событие перезарядки
    [SerializeField] protected float damage;// урон оружия
    [SerializeField] protected int ammoCount;// количество патрон
    protected bool possibleShoot;// возможность стрелять
    public virtual float CartridgeDispenser() => 1;// возможная частота нажатия на курок в секунду
    private float currentCartridgeDispenser;

    public virtual float ReloadTime() => 5;// время перезарядки
    private float currentReloadTime = 0;
    private bool isReloaded = true;// перезаряжено ли оружие
    private Dispenser dispenser = new Dispenser(8, 8);
    protected virtual bool Shoot()
    {
        bool canShooting = ammoCount > 0 && currentCartridgeDispenser >= CartridgeDispenser() && dispenser.CountBullets > 0;
        if (canShooting)
        {
            ammoCount--;
            RecoilEvent?.Invoke();
            currentCartridgeDispenser = 0;
            dispenser.Dispens();
        }
        else if (dispenser.CountBullets == 0)
        {
            isReloaded = false;
        }
        return canShooting;
    }
    protected void Update()
    {
        if (Input.GetMouseButtonDown(0))
            Shoot();

        CartridgeDispens();

        if (!isReloaded)
        {
            Reload();
        }
    }
    private void CartridgeDispens()
    {
        if (currentCartridgeDispenser < CartridgeDispenser())
            currentCartridgeDispenser += Time.deltaTime;
    }
    private void Reload()
    {
        if (currentReloadTime < ReloadTime())
        {
            currentReloadTime += Time.deltaTime;
        }
        else
        {
            isReloaded = true;
            dispenser.Reload();
            currentReloadTime = 0;
        }
    }
        
    internal void SetPossibleShooting(bool isAnimFinish)
    {
        possibleShoot = isAnimFinish;
    }
    /// <summary>
    /// "магазин" оружия
    /// </summary>
    class Dispenser
    {
        public int CountBullets { get; private set; }
        private int maxBullets;
        public Dispenser(int cb, int maxB)
        {
            CountBullets = cb;
            maxBullets = maxB;
        }
        public void Dispens()
        {
            CountBullets--;
        }
        public void Reload()
        {
            CountBullets = maxBullets;
        }
    }
}

