using UnityEngine;

namespace lottehime.BrickBreak
{
    public class excavatorCanBreak : MonoBehaviour
    {
        public float RequiredVelocity = 3.0f;

        Vector3 _breakerVelocity;
        Breakable _Obj;

        ExcavatorController controller;

        Excavator excavator;
        void Start()
        {
            controller = GetComponent<ExcavatorController>();
            excavator = controller.excavator;
        }

        void FixedUpdate()
        {
            _breakerVelocity = GetComponent<Rigidbody>().linearVelocity;
        }

        void OnCollisionEnter(Collision col)
        {
            if (col.gameObject == null) return;
            if (col.gameObject != null && col.gameObject.GetComponent<Breakable>() == null) return;

            _Obj = col.gameObject.GetComponent<Breakable>();
            GetComponent<Rigidbody>().linearVelocity = _breakerVelocity * _Obj.ObjectSlowdownFactor;

            Collider ourColliderThatGotHit = col.contacts[0].thisCollider;

            if (col.relativeVelocity.magnitude >= RequiredVelocity)
            {
                col.gameObject.GetComponent<Breakable>().Break();
            }
            else if (ourColliderThatGotHit.gameObject.transform.parent.name == "Bucket_Assembly")
            {
                if (Mathf.Abs(excavator.swingVelocity) >= 17)
                {
                    controller.lockSwing = true;
                    excavator.swingVelocity = 0;
                    col.gameObject.GetComponent<Breakable>().Break();
                }
                else if (Mathf.Abs(excavator.boomVelocity) >= 17.3)
                {
                    controller.lockBoom = true;
                    excavator.boomVelocity = 0;
                    col.gameObject.GetComponent<Breakable>().Break();
                }
                else if (Mathf.Abs(excavator.armVelocity) >= 48)
                {
                    controller.lockArm = true;
                    excavator.armVelocity = 0;
                    col.gameObject.GetComponent<Breakable>().Break();
                }
                else if (Mathf.Abs(excavator.bucketVelocity) >= 72)
                {
                    controller.lockBucket = true;
                    excavator.bucketVelocity = 0;
                    col.gameObject.GetComponent<Breakable>().Break();
                }
            }
            else if (ourColliderThatGotHit.gameObject.transform.name == "Vert.001" || ourColliderThatGotHit.gameObject.transform.name == "Cube.022" || ourColliderThatGotHit.gameObject.transform.name == "Cylinder.038" || ourColliderThatGotHit.gameObject.transform.name == "Cylinder.054" || ourColliderThatGotHit.gameObject.transform.name == "Sphere.007")
            {
                if (Mathf.Abs(excavator.swingVelocity) >= 17)
                {
                    controller.lockSwing = true;
                    excavator.swingVelocity = 0;
                    col.gameObject.GetComponent<Breakable>().Break();
                }
                else if (Mathf.Abs(excavator.boomVelocity) >= 17.3)
                {
                    controller.lockBoom = true;
                    excavator.boomVelocity = 0;
                    col.gameObject.GetComponent<Breakable>().Break();
                }
                else if (Mathf.Abs(excavator.armVelocity) >= 48)
                {
                    controller.lockArm = true;
                    excavator.armVelocity = 0;
                    col.gameObject.GetComponent<Breakable>().Break();
                }
            }
            else if (ourColliderThatGotHit.gameObject.transform.name == "Cube.010" || ourColliderThatGotHit.gameObject.transform.name == "Cylinder.039" || ourColliderThatGotHit.gameObject.transform.name == "Cylinder.050")
            {
                if (Mathf.Abs(excavator.swingVelocity) >= 17)
                {
                    controller.lockSwing = true;
                    excavator.swingVelocity = 0;
                    col.gameObject.GetComponent<Breakable>().Break();
                }
                else if (Mathf.Abs(excavator.boomVelocity) >= 17.3)
                {
                    controller.lockBoom = true;
                    excavator.boomVelocity = 0;
                    col.gameObject.GetComponent<Breakable>().Break();
                }
            }
        }
    }
}