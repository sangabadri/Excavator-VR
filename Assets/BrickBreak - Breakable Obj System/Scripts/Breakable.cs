using UnityEngine;
using System.Collections.Generic;

namespace lottehime.BrickBreak
{
    public class Breakable : MonoBehaviour
    {
        public bool VelocityBreaksObject = true;
        public bool BrokenPiecesExpire = false;
        public bool BreakSound = true;
        public GameObject BreakAudioOneShot;
        public float OneShotAudioLifetime = 2.0f;
        public List<GameObject> BrokenObjectVariants;
        public float ObjectSlowdownFactor = 0.65f;
        public float BrokenObjectPieceMass = 0.35f;
        public float maxDistanceBeforeMassChange = 0.5f;

        public int wallNum;

        Vector3 initialPos;
        bool massChanged = false;

        private float _brokenPieceDecayTime;
        private GameObject _brokenObj;
        Rigidbody rb;
        Breakable breakable;

        void Awake()
        {
            _brokenPieceDecayTime = BrokenPiecesExpire ? 4.0f : 0.0f;
            rb = GetComponent<Rigidbody>();
            initialPos = transform.position;
            breakable = GetComponent<Breakable>();
        }

        void Update()
        {
            if(massChanged)
            {
                return;
            }

            float distanceTraveled = Vector3.Distance(initialPos, transform.position);
            if(distanceTraveled > maxDistanceBeforeMassChange)
            {
                rb.mass = 10f;
                massChanged = true;
                if(wallNum != -1)
                {
                    if(Task2Tracker.Instance != null)if(Task2Tracker.Instance != null)
                    {
                        Task2Tracker.Instance.UpdateSlider(wallNum);
                    }
                    wallNum = -1; // Ensure this only happens once
                }
            }
        }

        void OnCollisionEnter(Collision col)
        {
            if (col.gameObject.CompareTag("Excavator"))
            {
                rb.mass = 1000f;
                rb.isKinematic = false;
            }
        }

        public void Break()
        {
            if (BrokenObjectVariants != null)
            {
                _brokenObj =
                    Instantiate(BrokenObjectVariants[Random.Range(0, BrokenObjectVariants.Count)], transform.position,
                        transform.rotation) as GameObject;

                if(wallNum != -1)
                {
                    if(Task2Tracker.Instance != null)
                    {
                        Task2Tracker.Instance.UpdateSlider(wallNum);
                    }
                    wallNum = -1; // Ensure this only happens once
                }

                if (_brokenObj != null)
                {
                    _brokenObj.transform.localScale = transform.lossyScale;

                    foreach (Transform shardObj in _brokenObj.transform)
                    {
                        shardObj.GetComponent<Rigidbody>().mass = BrokenObjectPieceMass;
                    }

                    if (BreakSound)
                        Destroy(Instantiate(BreakAudioOneShot, transform.position, transform.rotation) as GameObject,
                            OneShotAudioLifetime);

                    if (_brokenPieceDecayTime > 0) Destroy(_brokenObj, _brokenPieceDecayTime);
                }

                Destroy(gameObject);
            }
            else
            {
                Debug.Log("You have not assigned a broken object variant to this object: " +
                            gameObject +
                            "\nPlease assign one under 'Broken Object Variants' in the inspector.");
            }
        }

    }
}