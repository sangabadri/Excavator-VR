using UnityEngine;


public class bucketCanBreak : MonoBehaviour
{

    void OnCollisionEnter(Collision col)
    {
        Debug.Log("Bucket hit");
    }
}
