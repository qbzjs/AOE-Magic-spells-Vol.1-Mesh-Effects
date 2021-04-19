﻿using System.Collections.Generic;
using UnityEngine;
namespace Shoots
{
    public class Pistol : Gun
    {       
        protected override void LoadAssets()
        {
            bullet = Resources.Load<Bullet>("Guns\\NotNormal\\9.27 bullet");
            upBullet = Resources.Load<UsedUpBullet>("Guns\\NotNormal\\9.27 bullet(usedUp)");
        }
        protected override void Awake()
        {
            dispenser = new Dispenser(8, 8);
            LoadAssets();
        }
        public override float CartridgeDispenser()
        {
            return 0.25f;
        }
        protected override void Update()
        {
            if (Input.GetMouseButtonDown(0))
                Shoot();
            base.Update();
        }
        protected override bool Shoot()
        {
            bool canShoot = base.Shoot() && possibleShoot;
            if (!canShoot)
                return canShoot;
            CreateBullet();
            PlayFlashEffect();
            DropUsedBullet();
            CallRecoilEvent();
            return canShoot;
        }
        /// <summary>
        /// создание пули
        /// </summary>
        protected override void CreateBullet()
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            Bullet newBullet = Instantiate(bullet, spawnBulletPlace.position, spawnBulletPlace.rotation);
            BulletValues bv = new BulletValues(0, maxDistance, caliber, bulletSpeed);

            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, layerMask, QueryTriggerInteraction.Ignore))
            {
                bv.CurrentDistance = hit.distance;
                Enemy e = null;
                bool enemyFound = hit.transform.parent && hit.transform.parent.TryGetComponent(out e);

                newBullet.Init(bv, hit, enemyFound ? ImpactsContainer.Impacts["Enemy"] : ImpactsContainer.Impacts["Default"], e);

                return;
            }
            newBullet.Init(bv, ray.GetPoint(maxDistance));
        }
        protected override void PlayFlashEffect()
        {
            flashEffect.Play();
        }
        protected override void DropUsedBullet()
        {
            if (Instantiate(upBullet, droppingPlace.position, droppingPlace.rotation).TryGetComponent<Rigidbody>(out var rb))
            {
                rb.AddForce(droppingPlace.right, ForceMode.Impulse);
                rb.AddForce(-droppingPlace.forward, ForceMode.Impulse);
            }
        }
        public override float getRecoilPower()
        {
            return 0.125f;
        }
    }   
}