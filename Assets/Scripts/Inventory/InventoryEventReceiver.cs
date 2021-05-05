﻿using System.Collections.Generic;
using UnityEngine;
namespace Inventory
{
    /// <summary>
    /// класс - обработчик событий инвентаря
    /// </summary>
    public sealed class InventoryEventReceiver : IGameScreen
    {
        private readonly InventoryContainer inventoryContainer;
        public InventoryEventReceiver(Transform mp, FirstPersonController fps, Transform freeCellsContainer, Transform busyCellsContainer, InventoryContainer ic)
        {
            mainParent = mp;
            Instance = this;
            this.fps = fps;
            this.freeCellsContainer = freeCellsContainer;
            this.busyCellsContainer = busyCellsContainer;
            inventoryContainer = ic;
        }
        public void SetPoints(List<(Vector2 left, Vector2 up)> outPlc)
        {
            outPlaces = outPlc;
        }
        public static InventoryEventReceiver Instance { get; private set; }
        private readonly Transform mainParent;
        private readonly FirstPersonController fps;
        private readonly Transform busyCellsContainer;
        private readonly Transform freeCellsContainer;

        private ItemsContainer lastItemContainer;
        public void OpenContainer(List<(int id, int count)> content, int countSlots, ItemsContainer it)
        {
            for (int i = 0; i < countSlots; i++)
            {
                var child = freeCellsContainer.GetChild(0);
                child.SetParent(busyCellsContainer);

                if (content != null)
                    child.GetComponent<InventoryCell>().SetItem(content[i].id, content[i].count);

            }
            InventoryInput.Instance.EnableInventory();
            lastItemContainer = it;
        }
        public void CloseContainer()
        {
            List<Transform> childs = new List<Transform>();
            for (int i = 0; i < busyCellsContainer.childCount; i++)
            {
                childs.Add(busyCellsContainer.GetChild(i));
            }
            foreach (var c in childs)
            {
                c.SetParent(freeCellsContainer);
            }

            var cells = new List<(int id, int count)>();

            for (int i = 0; i < childs.Count; i++)
            {
                var item = childs[i].GetComponent<InventoryCell>().MItemContainer;
                cells.Add((item.Id, item.Count));
            }
            lastItemContainer.Close(cells);
            lastItemContainer = null;
        }

        private Transform candidateForReplaceItem;// кандидат для смены местами в инветаре(предмет)
        private InventoryCell candidateForReplaceCell;// кандидат для смены местами в инветаре(ячейка)
        private InventoryCell draggedCell;// удерживаемая ячейка
        private RectTransform draggedItem;// удерживаемый предмет
        private bool isDragged;// происходит ли удержание

        private List<(Vector2 left, Vector2 up)> outPlaces;// места выхода 
        public void OnEnable()
        {
            InventoryInput.ChangeActiveEvent += ChangeActiveEvent;
            InventoryInput.InputKeyEvent += SelectCell;
        }
        public void OnDisable()
        {
            InventoryInput.ChangeActiveEvent -= ChangeActiveEvent;
            InventoryInput.InputKeyEvent -= SelectCell;
        }
        private void ChangeActiveEvent(bool value) => SetPause(InventoryDrawer.Instance.ChangeActiveMainField(value));

        private void SetPause(bool enabled)
        {
            Cursor.visible = enabled;
            if (!enabled)
            {
                Cursor.lockState = CursorLockMode.Locked;
                fps.SetState(State.unlocked);
                if (lastItemContainer != null)
                    CloseContainer();
                ScreensManager.SetScreen(null);
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                fps.SetState(State.locked);
                ScreensManager.SetScreen(this);
            }
            EndDrag();
        }
        public void InsideCursorCell(InventoryCell cell)
        {
            // событие входа курсора в сектор ячейки
            candidateForReplaceCell = cell;
            candidateForReplaceItem = cell.GetItemTransform();

            if (isDragged)
                return;
            draggedCell = cell;
            draggedItem = cell.GetItemTransform();
        }

        public void OutsideCursorCell()
        {
            // событие выхода курсора из сектора ячейки
            candidateForReplaceItem = null;
            candidateForReplaceCell = null;

            if (isDragged)
                return;
            draggedCell = null;
            draggedItem = null;
        }
        public void DragCell(UnityEngine.EventSystems.PointerEventData eventData)
        {
            //удержание предмета
            draggedItem.position = eventData.position;            
        }

        public void BeginDrag(InventoryCell cell)
        {
            //начало удержания
            isDragged = true;
            draggedCell = cell;
            draggedItem = cell.GetItemTransform();
            draggedItem.SetParent(mainParent);
        }
        public void EndDrag()
        {
            if (isDragged)
                UnfocusAllCells();
            //конец удержания
            isDragged = false;

            ParentingDraggedObject();

            draggedCell = null;
            draggedItem = null;
            candidateForReplaceCell = null;
            candidateForReplaceItem = null;
        }
        private void ParentingDraggedObject()
        {
            // смена местами с кандидатом на ячейку или возврат на место
            if (candidateForReplaceItem && candidateForReplaceItem != draggedItem)
            {

                var bufferingSelectItemParent = draggedCell.transform;

                draggedItem.SetParent(candidateForReplaceItem.parent);
                candidateForReplaceItem.SetParent(bufferingSelectItemParent);

                candidateForReplaceItem.localPosition = draggedItem.localPosition = Vector3.zero;

                InventoryCell.CopyPasteCell candidateCopy = new InventoryCell.CopyPasteCell(candidateForReplaceCell);
                InventoryCell.CopyPasteCell draggedCopy = new InventoryCell.CopyPasteCell(draggedCell);

                if (candidateCopy.Equals(draggedCopy))
                {
                    draggedCopy.count += candidateCopy.count;
                    candidateCopy.count = candidateForReplaceCell.SetItem(draggedCopy);
                    Debug.LogWarning(12);
                }
                else candidateForReplaceCell.SetItem(draggedCopy);


                draggedCell.SetItem(candidateCopy);
                return;
            }
            if (!draggedCell)
                return;

            //если игрок хочет выкинуть предмет
            if (!IsIntersected(draggedItem.position))
            {
                DropItem(draggedCell.MItemContainer.Id, draggedCell.MItemContainer.Count);                
            }

            draggedItem.SetParent(draggedCell.transform);
            draggedItem.localPosition = Vector3.zero;
        }

        /// <summary>
        /// выделение слота по нажатию на слот
        /// </summary>
        /// <param name="ic"></param>
        public void FocusCell(InventoryCell ic)
        {
            UnfocusAllCells();
            ic.SetFocus(true);
        }
        /// <summary>
        /// снятие выделения со слотов
        /// </summary>
        private void UnfocusAllCells()
        {
            var cells = inventoryContainer.Cells;
            foreach (var cell in cells)
            {
                cell.SetFocus(false);
            }
        }
        /// <summary>
        /// выделение по нажатию на клавишу
        /// </summary>
        /// <param name="c"></param>
        private void SelectCell(int c)
        {
            if (c > 0 && c <= inventoryContainer.HotCells.Count)
                FocusCell(inventoryContainer.HotCells[c - 1]);
        }

        public void DropItem(int id, int count)
        {
            InventoryInput.Instance.DropItem(inventoryContainer.GetItemPrefab(id), count);
        }
        private bool IsIntersected(Vector2 obj)
        {
            foreach (var c in inventoryContainer.Cells)
            {
                Debug.Log(Vector2.Distance(obj, c.GetComponent<RectTransform>().position));
                if (Vector2.Distance(obj, c.GetComponent<RectTransform>().position) < 100)
                    return true;
            }
            return false;
        }
    }
}