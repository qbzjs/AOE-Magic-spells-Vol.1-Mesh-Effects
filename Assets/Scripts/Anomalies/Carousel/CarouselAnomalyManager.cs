using Society.Player.Controllers;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace Society.Anomalies.Carousel
{
    public sealed class CarouselAnomalyManager : AnomalyManager
    {
        private readonly List<Rigidbody> items = new List<Rigidbody>();
        private Collider mCollider;
        private Vector3 PointPos;
        private Vector3 targetPos = Vector3.zero;
        private BoxCollider zone;
        private readonly float explosionImpulse = 20;
        [SerializeField] private GameObject piece;
        [SerializeField] private GameObject explosion;
        [SerializeField] private CarouselAnomalyDieHandler carouselDeathHandler;

        [SerializeField] private Transform explosionPoint;
        private void SetHealth(int value)
        {
            health = value;
            if (value == 0) OnDie();
        }

        #region Player Interaction

        #region  Variables

        #region Mechanics
        [Range(0f, 10f)] [SerializeField] private float interactionTime = 2;
        [Range(0f, 20f)] [SerializeField] private float playerMaxHeight = 2;
        [Range(0f, 180f)] [SerializeField] private float degreesPerSecondMax = 90;
        private float degreesPerSec = 0;
        private GameObject player;
        private FirstPersonController playerFPC;
        private Vector3 playerAcceleration;
        #endregion

        #region State Pattern
        private Dictionary<Type, ICarouselBehaviour> behavioursMap;
        private ICarouselBehaviour currentBehaviour;
        #endregion

        #endregion

        #region Methods

        public override void Hit(int value)
        {
            SetHealth(GetHealth() - value);
        }
        protected override void OnDie()
        {
            int piecesNum = UnityEngine.Random.Range(1, 4);
            explosion.SetActive(true);

            enabled = false;

            carouselDeathHandler.OnDie(piecesNum, explosionPoint.position, explosionImpulse, piece);
        }
        public void DropPiece()
        {
            carouselDeathHandler.DropPiece();
        }
        public void DisableAnomaly()
        {
            gameObject.SetActive(true);
        }
        #region State Pattern Methods
        private void InitBehaviours()
        {
            behavioursMap = new Dictionary<Type, ICarouselBehaviour>();
            behavioursMap[typeof(SearchPlayer)] = new SearchPlayer(this, interactionTime);
            behavioursMap[typeof(HoldPlayer)] = new HoldPlayer(this, interactionTime);
            behavioursMap[typeof(FreePlayer)] = new FreePlayer(this, interactionTime);
        }
        private void SetBehaviour(ICarouselBehaviour newBehaviour)
        {
            currentBehaviour = newBehaviour;
            currentBehaviour.Enter();
        }
        private ICarouselBehaviour GetBehaviour<T>() where T : ICarouselBehaviour
        {
            var type = typeof(T);
            return behavioursMap[type];
        }
        private void SetBehaviourByDefault()
        {
            SetBehaviourSearch();
        }
        public void SetBehaviourSearch()
        {
            var behaviour = GetBehaviour<SearchPlayer>();
            SetBehaviour(behaviour);
        }
        public void SetBehaviourHold()
        {
            var behaviour = GetBehaviour<HoldPlayer>();
            SetBehaviour(behaviour);
        }
        public void SetBehaviourFree()
        {
            var behaviour = GetBehaviour<FreePlayer>();
            SetBehaviour(behaviour);
        }
        #endregion

        #region Mechanics Methods
        public void SetPlayer(GameObject player)
        {
            this.player = player;
        }
        public Transform GetPlayerTransform()
        {
            return player.transform;
        }
        public Vector3 GetPointPos()
        {
            return PointPos;
        }
        public void SetPlayerFPC()
        {
            playerFPC = player.GetComponent<FirstPersonController>();
        }
        public void PlayerFixedUpdate()
        {
            if (degreesPerSec == 0) StartCoroutine(DegreesPerSecCalc());
            if (player != null)
            {
                //move:
                playerAcceleration.y += 0.15f * (-player.GetComponent<Rigidbody>().velocity.y) + 0.08f * (playerMaxHeight - player.transform.position.y);
                player.GetComponent<Rigidbody>().AddForce(playerAcceleration * player.GetComponent<Rigidbody>().mass);
                //rotate:
                playerFPC.AdditionalXMouse = degreesPerSec / 500; //не является точным значением в град/сек, ориентировочно
            }

        }

        private System.Collections.IEnumerator DegreesPerSecCalc()
        {
            float times = interactionTime * 10;
            for (int i = 0; i < times; i++)
            {
                degreesPerSec += (degreesPerSecondMax / times);
                yield return new WaitForSeconds(0.1f);
            }
            degreesPerSec = 0;
            playerFPC.AdditionalXMouse = 0;
            yield break;
        }
        #endregion

        #endregion

        #endregion

        public void InitCollider(Collider c)
        {
            mCollider = c;
            PointPos = mCollider.transform.position;
            PointPos.y = (mCollider as CapsuleCollider).height;
        }

        private System.Collections.IEnumerator Start()
        {
            InitBehaviours();
            SetBehaviourByDefault();
            playerAcceleration = new Vector3(0, 0, 0);
            while (true)
            {
                targetPos = Vector3Extensions.CalculateSpawnPositionInRange(zone.transform, zone);
                yield return new WaitForSeconds(4);
            }
        }
        internal void SetZone(BoxCollider mCollider) => zone = mCollider;

        public void AddInList(Rigidbody rb) => items.Add(rb);

        public void RemoveFromList(Rigidbody rb) => items.Remove(rb);

        private void FixedUpdate()
        {
            Vector3 itemPos;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].gameObject.GetComponent<FirstPersonController>() != null)
                {
                    if (player == null) currentBehaviour.PlayerDetected(items[i].gameObject);
                }
                else
                {
                    itemPos = items[i].transform.position;
                    if (itemPos.y < (PointPos.y - 1))
                    {
                        items[i].AddForce((PointPos - itemPos).normalized * items[i].mass * Time.fixedDeltaTime * 1.33f, ForceMode.VelocityChange);
                    }
                    else
                    {
                        items[i].transform.position = Vector3.MoveTowards(items[i].transform.position, PointPos, 1);
                        items[i].transform.localEulerAngles *= 1.01f;
                    }
                    items[i].AddRelativeTorque(new Vector3(0, 36, 0) * Time.fixedDeltaTime * items[i].mass, ForceMode.VelocityChange);
                }

            }
            currentBehaviour.Update();
            MoveToTarget();
        }
        private void MoveToTarget()
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.fixedDeltaTime / 10);
        }
    }
    #region State classes
    public interface ICarouselBehaviour
    {
        void Enter();
        void Exit();
        void Update();
        void PlayerDetected(GameObject p);
    }

    public class HoldPlayer : ICarouselBehaviour
    {
        private CarouselAnomalyManager cm;
        private float t;
        public void PlayerDetected(GameObject p) {/*do nothing*/}
        public HoldPlayer(CarouselAnomalyManager cm, float t)
        {
            this.cm = cm;
            this.t = t;
        }
        public void Enter()
        {
            cm.StartCoroutine(Timer());
        }
        public void Exit()
        {
            cm.SetBehaviourFree();
        }
        public void Update()
        {
            cm.PlayerFixedUpdate();
        }

        private System.Collections.IEnumerator Timer()
        {
            yield return new WaitForSeconds(t);
            Exit();
            yield break;
        }
    }
    public class FreePlayer : ICarouselBehaviour
    {
        private CarouselAnomalyManager cm;
        private float t;
        public void PlayerDetected(GameObject p) {/*do nothing*/}
        public FreePlayer(CarouselAnomalyManager cm, float t)
        {
            this.cm = cm;
            this.t = t;
        }
        public void Enter()
        {
            cm.SetPlayer(null);
            cm.StartCoroutine(Timer());
        }
        public void Exit()
        {
            cm.SetBehaviourSearch();
        }
        public void Update()
        {

        }

        private System.Collections.IEnumerator Timer()
        {
            yield return new WaitForSeconds(t);
            Exit();
            yield break;
        }
    }
    public class SearchPlayer : ICarouselBehaviour
    {
        private CarouselAnomalyManager cm;
        public void PlayerDetected(GameObject p)
        {
            cm.SetPlayer(p);
            cm.SetPlayerFPC();
            Exit();
        }
        public SearchPlayer(CarouselAnomalyManager cm, float t)
        {
            this.cm = cm;
        }
        public void Enter()
        {

        }
        public void Exit()
        {
            cm.SetBehaviourHold();
        }
        public void Update()
        {

        }
    }
    #endregion
}