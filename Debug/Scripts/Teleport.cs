namespace UnityEngine.ProBuilder.Debug
{
    public class Teleport : MonoBehaviour
    {
        [SerializeField]
        Transform m_Destination;

        void OnTriggerEnter(Collider collider)
        {
            transform.position = m_Destination.position;
            GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        }
    }
}
