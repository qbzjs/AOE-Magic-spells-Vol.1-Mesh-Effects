﻿using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Inventory
{
    /// <summary>
    /// главный контейнер инвентаря
    /// </summary>
    public sealed class InventoryContainer : MonoBehaviour
    {                
        [SerializeField] private Transform mainParent;// родитель для отрисовки поверх всего

        private List<InventoryCell> Cells = new List<InventoryCell>();//слоты инвентаря
        public List<InventoryCell> GetCells() => Cells;
        public readonly List<RectTransform> CellsRect = new List<RectTransform>();        
        public InventoryEventReceiver EventReceiver { get; private set; }
        private readonly List<InventoryCell> HotCells = new List<InventoryCell>();
        public List<InventoryCell> GetHotCells() => HotCells;
        private InventoryEffects inventoryEffects;
        private readonly InventorySaver inventorySaver = new InventorySaver();

        [SerializeField] private Transform freeCellsContainer;
        [SerializeField] private Transform busyCellsContainer;
        [SerializeField] private GameObject cellPrefab;
        private PlayerClasses.PlayerStatements playerStatements;
        [SerializeField] private GameObject ItemsLabelDescription;
        [SerializeField] private TextMeshProUGUI weightText;
        [SerializeField] private Button takeAllButton;// кнопка у слотов контейнеров, забирает всё, что можно
        private InventoryInput inventoryInput;
        private InventoryDrawer inventoryDrawer;
        public delegate void TakeItem(int id, int count);
        public event TakeItem TakeItemEvent;
        public event TakeItem ActivateItemEvent;
        public bool IsInitialized { get; private set; }

        private void Awake()
        {
            inventoryInput = gameObject.AddComponent<InventoryInput>();
        }
        private void OnEnable()
        {
            inventoryEffects = new InventoryEffects(gameObject);
            playerStatements = FindObjectOfType<PlayerClasses.PlayerStatements>();
            inventoryDrawer = FindObjectOfType<InventoryDrawer>();

            EventReceiver = new InventoryEventReceiver(mainParent, FindObjectOfType<FirstPersonController>(), freeCellsContainer,
                busyCellsContainer, this, ItemsLabelDescription, inventoryInput, inventoryDrawer, weightText, takeAllButton);
            EventReceiver.OnEnable();
        }
        private void Start()
        {
            // добавление всех ячеек в список

            var sc = inventoryDrawer.GetSupportContainer().GetComponentsInChildren<InventoryCell>();

            foreach (var cell in sc)
            {
                Cells.Add(cell);
                HotCells.Add(cell);
            }
            var mc = inventoryDrawer.GetMainContainer().GetComponentsInChildren<InventoryCell>();
            foreach (var cell in mc)
            {
                Cells.Add(cell);
            }
            foreach (var c in Cells)
            {
                c.Init(this);
            }
            inventorySaver.Load(ref Cells);// загрузка сохранённого инвентаря            

            foreach (var c in Cells)
            {
                CellsRect.Add(c.GetComponent<RectTransform>());
                TakeItemEvent?.Invoke(c.MItemContainer.Id, c.MItemContainer.Count);
            }

            //загрузка слотов для сундуков
            for (int i = 0; i < ItemsContainer.maxCells; i++)
            {
                Instantiate(cellPrefab, freeCellsContainer);
            }

            itemPrefabs = new Dictionary<int, InventoryItem>
            {
                {ItemStates.AxeId, Resources.Load<InventoryItem>("InventoryItems\\Axe_Item_1") },
                {ItemStates.MakarovId, Resources.Load<InventoryItem>("InventoryItems\\Makarov_Item_1") },
                {ItemStates.Ak_74Id, Resources.Load<InventoryItem>("InventoryItems\\AK-74u_Item_1") },
                {ItemStates.CannedFoodId, Resources.Load<InventoryItem>("InventoryItems\\CannedFood_Item_1") },
                {ItemStates.MilkId, Resources.Load<InventoryItem>("InventoryItems\\Milk_Item_1") }

            };
            IsInitialized = true;
        }

        /// <summary>
        /// добавление поднятого предмета в очередь
        /// </summary>
        /// <param name="item"></param>    
        public void AddItem(InventoryItem item)
        {
            if (Cells.FindAll(c => c.MItemContainer.IsEmpty).Count == 0)// если не нашлись свободные слоты
                return;

            // поиск слота, с предметом того же типа, и не заполненным   
            var cell = Cells.Find(c => !c.MItemContainer.IsFilled && c.MItemContainer.Id.Equals(item.Id));

            if (cell == null) cell = Cells.Find(c => c.MItemContainer.IsEmpty);// если слот не нашёлся то запись в пустой слот

            cell.SetItem(item.Id, item.GetCount());                        

            TakeItemEvent?.Invoke(item.Id, item.GetCount());
        }
        public void SpendOnCell()
        {
            inventoryEffects.PlaySpendClip();
        }        
        public void ActivateItem() => EventReceiver.ActivateItem();

        public void CallItemEvent(int id, int count) => ActivateItemEvent?.Invoke(id, count);
        public void MealPlayer(int food, int water) => playerStatements.MealPlayer(food, water);

        private void OnDisable()
        {
            Save(Cells);
            EventReceiver.OnDisable();
        }
        /// <summary>
        /// сохранение инвентаря
        /// </summary>
        /// <param name="cells"></param>
        private void Save(List<InventoryCell> cells) => inventorySaver.Save(cells);

        public Dictionary<int, InventoryItem> itemPrefabs;
        public InventoryItem GetItemPrefab(int id)=> itemPrefabs[id];

        public class InventoryEffects
        {
            private readonly AudioClip spendOnCellClip;// звук при наведении на слот
            private readonly AudioSource inventorySpeaker;
            public InventoryEffects(GameObject main)
            {
                spendOnCellClip = Resources.Load<AudioClip>("Inventory\\tic");
                inventorySpeaker = main.AddComponent<AudioSource>();
            }
            public void PlaySpendClip() => inventorySpeaker.PlayOneShot(spendOnCellClip);
        }
    }
}