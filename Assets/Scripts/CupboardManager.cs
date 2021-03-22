﻿using System.Collections.Generic;
using UnityEngine;

public sealed class CupboardManager : MonoBehaviour// класс реализует взаимодействие и движение ящиков шкафа
{
    private List<CupboardMesh> cases { get; set; } = new List<CupboardMesh>();// ящики
    private List<Transform> currentMovingCases { get; set; } = new List<Transform>();// ящики в движении
    [SerializeField] private float targetX;// конечная точка открытых ящиков
    private float startX;// начальная точка закрытых ящиков
    private float deltaRate = 1;// скорость движения ящиков
    private void Start()
    {
        if (cases.Count > 0)
            startX = cases[0].transform.localPosition.x;
    }
    public void AddCase(CupboardMesh m)
    {
        cases.Add(m);
    }
    public void Interact(Transform currentCase)
    {
        currentMovingCases.Add(currentCase);
    }
    private void OpenCloseCases()
    {
        foreach (var cs in currentMovingCases.ToArray())
        {
            Vector3 localTargetPlace = cs.localPosition;
            for (int i = 0; i < cases.Count; i++)
            {
                if (cs.gameObject.GetInstanceID() == cases[i].gameObject.GetInstanceID())
                    localTargetPlace.x = cases[i].IsOpen ? startX : targetX;// установка точки назначения
            }
            cs.localPosition = Vector3.MoveTowards(cs.localPosition, localTargetPlace, deltaRate * Time.fixedDeltaTime);// движение ящика назад или вперёд
            if (cs.localPosition == localTargetPlace)
            {
                currentMovingCases.Remove(cs);// удаление из списка, при достижении точки назначения
                for (int i = 0; i < cases.Count; i++)
                {
                    if (cs.gameObject.GetInstanceID() == cases[i].gameObject.GetInstanceID())
                        cases[i].SetOpened(!cases[i].IsOpen);
                }
            }
        }
    }
    private void FixedUpdate()
    {
        if (currentMovingCases.Count > 0)
            OpenCloseCases();
    }
}