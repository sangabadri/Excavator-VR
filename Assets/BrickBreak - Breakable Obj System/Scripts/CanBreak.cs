using UnityEngine;

namespace lottehime.BrickBreak
{
    public class CanBreak : MonoBehaviour
    {
        public float RequiredVelocity = 2.0f;

        Vector3 _breakerVelocity;
        Breakable _Obj;

        void FixedUpdate()
        {
            _breakerVelocity = GetComponent<Rigidbody>().linearVelocity;
        }

        void OnCollisionEnter(Collision col)
        {
            if (col.gameObject != null && col.gameObject.GetComponent<Breakable>() == null) return;

            if (col.gameObject == null) return;
            _Obj = col.gameObject.GetComponent<Breakable>();
            GetComponent<Rigidbody>().linearVelocity = _breakerVelocity * _Obj.ObjectSlowdownFactor;

            if (!_Obj.VelocityBreaksObject) return;

            if (!(col.relativeVelocity.magnitude >= RequiredVelocity)) return;

            col.gameObject.GetComponent<Breakable>().Break();
        }
    }
}