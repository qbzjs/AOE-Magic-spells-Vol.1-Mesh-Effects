﻿using Society.Effects;
using Society.GameScreens;

using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Society.Missions.NumeratedMissions
{
    internal sealed class Mission_1 : Mission
    {
        public override int GetMissionNumber() => 1;
        protected override void StartMission()
        {
            OnTaskActions.Add("playbunkerSound", async () =>
            {
                var Aud1 = gameObject.AddComponent<AudioSource>();
                Aud1.PlayOneShot(Resources.Load<AudioClip>("DoorClips\\HermeticDoor\\HermeticDoor_Open"));

                await Task.Delay((int)(Resources.Load<AudioClip>("DoorClips\\HermeticDoor\\HermeticDoor_Open").length * 1000));
                OnTaskActions["onLoadBunker"].Invoke();
            });
            OnTaskActions.Add("onLoadBunker", () =>
            {
                MissionsManager.Instance.FinishMission();
                ScreensManager.SetScreen(null);
                LoadScreensManager.Instance.LoadScene((int)Scenes.Bunker);
            });
            base.StartMission();
        }
        protected override void OnReportTask(bool isLoad = false, bool isMissiomItem = false)
        {
            if (isLoad)
            {
                СleansingScreenEffect lb = new GameObject(nameof(СleansingScreenEffect)).AddComponent<СleansingScreenEffect>();
                lb.OnInit(6, Color.black);

                var Aud = gameObject.AddComponent<AudioSource>();
                Aud.clip = Resources.Load<AudioClip>("DoorClips\\HermeticDoor\\HermeticDoor_Close");
                Aud.Play();
            }
            if (currentTask == 4)
            {
                Report();// пока нет дневника - пропуск
            }
            if (currentTask == 6)
            {
                OnTaskActions["finish"].Invoke();
            }
            base.OnReportTask(isLoad, isMissiomItem);
        }
        public override void FinishMission()
        {
            MissionsManager.Instance.GetTaskDrawer().SetVisible(false);
            DirtyingScreenEffect db = new GameObject(nameof(DirtyingScreenEffect)).AddComponent<DirtyingScreenEffect>();
            db.OnInit(2, Color.black);
            db.SubsctibeOnFinish(OnTaskActions["playbunkerSound"]);
        }
    }
}