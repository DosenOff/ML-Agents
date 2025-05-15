using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    LavaAgent lavaAgent;

    private void Start()
    {
        lavaAgent = transform.parent.GetComponent<LavaAgent>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            lavaAgent.isGrounded = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            lavaAgent.isGrounded = false;
        }
    }
}
