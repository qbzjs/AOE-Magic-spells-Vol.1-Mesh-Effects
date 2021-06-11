﻿using UnityEngine;
using TMPro;
using System;

namespace SMG
{
    /// <summary>
    /// класс - рисовщик хар. об оружии
    /// </summary>
    class SMGGunCharsDrawer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleTextDrawing;
        [SerializeField] private TextMeshProUGUI titleTextPreview;

        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private TextMeshProUGUI maxFlyDistText;
        [SerializeField] private TextMeshProUGUI optFlyDistText;
        [SerializeField] private TextMeshProUGUI caliberText;
        [SerializeField] private TextMeshProUGUI dispVolText;

        private SMGEventReceiver eventReceiver;
        private void OnEnable()
        {
            eventReceiver = FindObjectOfType<SMGMain>().EventReceiver;

            eventReceiver.UpdateModfiersEvent += OnChangeSelectedGun;
        }
        public void OnChangeSelectedGun(ModifierCell modCell)
        {
            var cell = modCell.Ic;
            int id = cell.Id;
            if (!Inventory.ItemStates.ItsGun(id))
                return;
            var title = GunCharacteristics.GetGunTitle(id);
            titleTextDrawing.text = title;
            titleTextPreview.text = title;
            damageText.text = 
                $"Урон: {GunCharacteristics.GetDamage(id)}";
            maxFlyDistText.text = $"Максимальная дистанция поражения: {GunCharacteristics.GetMaximumDistanceFromTitle(id)}";
            optFlyDistText.text = $"Оптимальная дистанция поражения: {GunCharacteristics.GetOptimalDistanceFromTitle(id)}";
            caliberText.text = $"Калибр: {GunCharacteristics.GetCaliberFromTitle(id)}";
            dispVolText.text = $"Объём магазина: {ModifierCharacteristics.GetAmmoCountFromDispenser(cell.MGun.Title, cell.MGun.Mag)}";
        }
        private void OnDisable()
        {
            if (eventReceiver != null)
                eventReceiver.UpdateModfiersEvent -= OnChangeSelectedGun;
        }
    }
}