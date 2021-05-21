﻿using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Inventory
{/// <summary>
/// класс - слота в инвентаре
/// </summary>
    public sealed class InventoryCell : MonoBehaviour, IPointerEnterHandler,
        IPointerExitHandler, IDragHandler, IEndDragHandler, IBeginDragHandler, IPointerClickHandler
    {
        [SerializeField] private Image background;
        [SerializeField] private Image mImage;// картинка

        [SerializeField] private RectTransform mItem;// трансформация предмета
        [SerializeField] private TextMeshProUGUI mText;// счётчик предметов
        public Item MItemContainer { get; private set; } = new Item();
        private AdditionalSettins additionalSettins;
        private InventoryEventReceiver eventReceiver;
        private InventoryContainer inventoryContainer;
        public bool CellIsInventorySon { get; private set; } = false;
        public int Id => MItemContainer.Id;
        public int Count => MItemContainer.Count;
        public sealed class AdditionalSettins
        {
            public Vector3 DefaultScale { get; } // обычный размер
            public Vector3 AnimatedScale { get; } // анимированный размер
            public Color FocusedColor { get; }// цвет при выделении
            public Color UnfocusedColor { get; }// обычный цвет
            public AdditionalSettins(Image bg)
            {
                DefaultScale = bg.GetComponent<RectTransform>().localScale;
                AnimatedScale = DefaultScale * 1.1f;
                FocusedColor = new Color(0, 1, 0, bg.color.a);
                UnfocusedColor = bg.color;
            }
        }

        private void Awake()
        {
            if (additionalSettins == null)
                Init(null);
        }
        public void Init(InventoryContainer ic)
        {
            additionalSettins = new AdditionalSettins(background);
            if (!ic)// инициализация проходящая для слотов контейнеров
            {
                inventoryContainer = FindObjectOfType<InventoryContainer>();
            }
            else// инициализация проходящая для слотов инвентаря
            {
                inventoryContainer = ic;
                CellIsInventorySon = true;
            }
            eventReceiver = inventoryContainer.EventReceiver;
        }
        /// <summary>
        /// вызывается для записи предмета в ячейку после загрузки
        /// </summary>
        /// <param name="id"></param>
        /// <param name="count"></param>
        /// <param name="pos"></param>
        public int SetItem(int id, int count, bool isMerge = true)
        {
            int outOfRange = MItemContainer.SetItem(id, count + Count, isMerge);
            ChangeSprite();
            return outOfRange;
        }
        /// <summary>
        /// вызывается для смены предмета другим предметом
        /// </summary>
        /// <param name="cell"></param>
        public int SetItem(CopyPasteCell copyPaste, bool isMerge = true)
        {
            int outRangeCount = MItemContainer.SetItem(copyPaste.id, copyPaste.count, isMerge);//запись в свободную ячейку кол-во и возвращение излишка

            mItem = copyPaste.mItem;// присвоение новых транс-ов
            mImage = copyPaste.mImage;// и новых image                        
            mText = copyPaste.mText;

            ChangeSprite();
            mItem.localScale = additionalSettins.DefaultScale;
            return outRangeCount;
        }
        private void UpdateText() => mText.SetText(Count > 1 ? Count.ToString() : string.Empty);// если кол-во > 1 то пишется число предметов           

        public void Clear()
        {
            MItemContainer.DelItem(Count);
            ChangeSprite();
        }
        /// <summary>
        /// смена изображения на картинке (в зависимости от типа предмета)
        /// </summary>
        /// <param name="type"></param>
        public void ChangeSprite()
        {
            mImage.sprite = InventorySpriteData.GetSprite(Id);
            mImage.color = MItemContainer.IsEmpty ? new Color(1, 1, 1, 0) : Color.white;
            UpdateText();
            ///если контейнер пуст
            if (MItemContainer.IsEmpty)
                eventReceiver.UnfocusSelectedCell(this);//снимается фокус со слота
        }
        #region Events
        /// <summary>
        /// вызывается при входе курсором в пространство ячейки
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            eventReceiver.InsideCursorCell(this);
            StartCoroutine(nameof(BackgroundAnimate));
        }

        /// <summary>
        /// вызывается при выходе курсора из пространства ячейки
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerExit(PointerEventData eventData)
        {
            eventReceiver.OutsideCursorCell();
        }


        /// <summary>
        /// вызывается в начале удержания
        /// </summary>
        /// <param name="eventData"></param>
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            if (MItemContainer.IsEmpty)
                return;

            eventReceiver.BeginDrag(this);
            mImage.raycastTarget = false;//отключение чувствительности предмета            
        }

        /// <summary>
        /// вызывается при удержании предмета
        /// </summary>
        /// <param name="eventData"></param>
        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            //кнопка для удержания обязательно должна быть левой

            if (MItemContainer.IsEmpty)
                return;

            eventReceiver.DragCell(eventData);
        }

        /// <summary>
        /// вызывается при отмене удержания
        /// </summary>
        /// <param name="eventData"></param>
        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (MItemContainer.IsEmpty)
                return;

            eventReceiver.EndDrag();
            mImage.raycastTarget = true;//возврат чувствительности предмету
        }

        /// <summary>
        /// вызывается при нажатии на слот
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            eventReceiver.FocusCell(this);
        }
        private IEnumerator BackgroundAnimate()
        {
            bool wasAnimated = false;
            inventoryContainer.SpendOnCell();
            while (true)
            {
                var rt = background.GetComponent<RectTransform>();
                Vector3 nextState = wasAnimated ? additionalSettins.DefaultScale : additionalSettins.AnimatedScale;
                rt.localScale = Vector3.MoveTowards(rt.localScale, nextState, 0.5f);
                if (rt.localScale == additionalSettins.AnimatedScale)
                {
                    wasAnimated = true;
                }
                if (wasAnimated && rt.localScale == additionalSettins.DefaultScale)
                    break;
                yield return null;
            }
        }


        #endregion
        /// <summary>
        /// удаляет указанное кол-во предметов в слоте
        /// </summary>
        /// <param name="outOfRange"></param>
        internal void DelItem(int outOfRange)
        {
            MItemContainer.DelItem(outOfRange);
            ChangeSprite();
        }

        public RectTransform GetItemTransform() => mItem;
        public Image GetImage() => mImage;
        public void SetFocus(bool v) => background.color = v ? additionalSettins.FocusedColor : additionalSettins.UnfocusedColor;


        public class Item
        {
            public bool IsFilled { get => Count > MaxCount - 1; }
            public int Id { get; set; }
            public int Count { get; private set; } = 0;
            public int MaxCount { get => ItemStates.GetMaxCount(Id); }
            public bool IsEmpty { get => Count == 0; }
            public void DelItem(int count)
            {
                Count -= count;
                if (IsEmpty)
                    Id = (int)ItemStates.ItemsID.Default;
            }
            public int SetItem(int nid, int ncount, bool isMerge = true)
            {
                int outRange = 0;
                if (Id == nid && isMerge)// если тип предмета тот же, что и был в слоте
                {
                    outRange = MaxCount - (Count += ncount);// получаем выход за границу
                    if (Count > MaxCount)
                        Count = MaxCount;
                }
                else// иначе просто замена
                    Count = ncount;

                Id = IsEmpty ? (int)ItemStates.ItemsID.Default : nid;

                return outRange;
            }
        }
        /// <summary>
        /// структура для копирования слота
        /// </summary>
        public struct CopyPasteCell
        {
            public TextMeshProUGUI mText { get; set; }
            public RectTransform mItem { get; set; }
            public Image mImage { get; set; }
            public int count { get; set; }
            public int id { get; set; }

            public CopyPasteCell(InventoryCell c)
            {
                mItem = c.GetItemTransform();
                mImage = c.GetImage();
                count = c.Count;
                mText = c.mText;
                id = c.Id;
            }
            public bool Equals(CopyPasteCell obj) => obj.id == id && obj.count < ItemStates.GetMaxCount(id) && count < ItemStates.GetMaxCount(id);

        }

        /// <summary>
        /// метод "активации предмета (например поедание еды)"
        /// </summary>
        public bool Activate()
        {
            if (Id == 5 || Id == 6)
            {
                var meal = ItemStates.GetMeatNutrition(Id);
                inventoryContainer.MealPlayer(meal.Item1, meal.Item2);
                DelItem(1);
                return true;
            }
            return false;
        }
    }
}