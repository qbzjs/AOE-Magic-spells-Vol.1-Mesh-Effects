﻿using System.Linq;

using Society.Patterns;

using UnityEngine;
namespace Society.Missions
{
    /// <summary>
    /// Трекер миссий
    /// </summary>
    public class MissionInteractiveObject : InteractiveObject
    {
        /// <summary>
        /// Номер миссии трекера
        /// </summary>
        [SerializeField]
        [Range(MissionsManager.MinMissions, MissionsManager.MaxMissions)] private int missionNumber = 0;
#if UNITY_EDITOR
        /// <summary>
        /// Вспомогательное поле для разработчика - отображает название миссии
        /// </summary>
        [ReadOnlyField] [SerializeField] private string missionTitle = "N/A";
#endif
        /// <summary>
        /// Номер задачи трекера
        /// </summary>
        [SerializeField, Range(0, 10)] private int task;
#if UNITY_EDITOR
        /// <summary>
        /// Вспомогательное поле для разработчика - отображает название задачи
        /// </summary>
        [ReadOnlyField] [SerializeField] private string taskTitle = "N/A";
#endif
        /// <summary>
        /// Миссия трекера
        /// </summary>
        private Mission mMission;

        /// <summary>
        /// Трекер сработал раннее?
        /// </summary>
        private bool hasInteracted;

        protected virtual void Start()
        {
            //Определение нужной мисии из всех доступных на сцене
            mMission = FindObjectsOfType<Mission>().First(m => m.GetMissionNumber() == missionNumber);
        }

        public override void Interact() => Report();

        /// <summary>
        /// Обработка трекинга
        /// </summary>
        protected void Report()
        {
            //защита от нажатия не по сценарию
            if (!CanInteract())
                return;
            if (hasInteracted)
                return;
            hasInteracted = true;

            if (ThisTaskIsTheLastInMission())
                mMission.FinishMission();
            else
                mMission.Report();
        }

        /// <summary>
        /// Трекер может сработать?
        /// </summary>
        /// <returns></returns>
        internal bool CanInteract()
        {
            return (mMission == MissionsManager.Instance.GetActiveMission()) && //Миссия трекера это активная миссия?
                    (task == mMission.GetCurrentTask());// Задача трекера это активная задача?
        }

        /// <summary>
        /// Насильный пропуск трекера из <see cref="Debugger.DebugConsole"/>
        /// </summary>
        internal void ForceInteract()
        {
            hasInteracted = true;

            mMission.Report();
        }
        public Mission GetMission() => mMission;
        public int GetTask() => task;

        /// <summary>
        /// Задача трекера - последняя в миссии?
        /// </summary>
        /// <returns></returns>
        private bool ThisTaskIsTheLastInMission()
        {
            return Localization.LocalizationManager.GetNumberOfMissionTasks(missionNumber) == (task + 1);
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            missionTitle = MissionsManager.MissionInfo.GetMissionTitleByIndex(missionNumber);
            taskTitle = MissionsManager.MissionInfo.GetMissionTaskTitleByIndex(missionNumber, task);
        }
#endif        
    }
}