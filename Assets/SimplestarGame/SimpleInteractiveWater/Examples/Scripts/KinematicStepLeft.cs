using UnityEngine;

namespace SimplestarGame
{
    public class KinematicStepLeft : MonoBehaviour
    {
        [SerializeField, Tooltip("Rotation Speed")] float rotationSpeed = 2.5f;
        [SerializeField, Tooltip("Moves Forward")] bool moveForward = true;
        [SerializeField, Tooltip("Linear Speed")] float linearSpeed = -0.5f;
        void Start()
        {
            this.centor = this.transform.position - Vector3.up;
        }

        void Update()
        {
            if (moveForward) {
                this.transform.position = this.centor + new Vector3(0, 
                    Mathf.Sin(Time.realtimeSinceStartup * rotationSpeed) * 0.5f,
                    this.transform.position.z + (linearSpeed/100) - this.centor.z);
            }
            else {
                this.transform.position = this.centor + new Vector3(0, 
                    Mathf.Sin(Time.realtimeSinceStartup * rotationSpeed) * 0.5f,
                    this.transform.position.z - this.centor.z);
            }
        }

        Vector3 centor = Vector3.zero;
    }
}