using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class LavaAgent : Agent
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float jump = 3f;

    public bool isLava;
    public bool isGrounded;
    private Rigidbody rb;

    [SerializeField] private MeshRenderer[] floorMeshRenderers;
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material partialMaterial;
    [SerializeField] private Material loseMaterial;

    private bool canTouch;

    public void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void FixedUpdate()
    {
        RaycastHit hit;
        isLava = Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, 15f, LayerMask.GetMask("Hazard"));

        if (isLava)
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.blue);
        }
    }

    public override void OnEpisodeBegin()
    {
        Vector3 thisLocation = new Vector3(Random.Range(-4.4f, 2.9f), 0, Random.Range(-2.5f, 2.5f));
        Vector3 targetLocation = new Vector3(Random.Range(6.6f, 14.9f), 0, Random.Range(-2.5f, 2.5f));

        transform.localPosition = thisLocation;
        targetTransform.localPosition = targetLocation;

        canTouch = true;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
        sensor.AddObservation(isGrounded);
        sensor.AddObservation(isLava);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        int jumpNow = actions.DiscreteActions[0];

        if (isGrounded)
        {
            rb.linearVelocity = new Vector3(moveX, 0, moveZ).normalized * moveSpeed;

            if (jumpNow == 1)
            {
                TryJump(moveX, moveZ);
            }
        }

        AddReward(-0.001f);
    }

    private void TryJump(float moveX, float moveZ)
    {
        Vector3 horizontalVelocity = new Vector3(moveX, 0, moveZ).normalized;
        rb.linearVelocity = new Vector3(horizontalVelocity.x * moveSpeed, jump, horizontalVelocity.z * moveSpeed);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");

        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = Input.GetKey("space") ? 1 : 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Goal>(out Goal goal))
        {
            SetReward(3f);
            SetColor("win");
            EndEpisode();
        }

        if (other.TryGetComponent<GoalFloor>(out GoalFloor goalFloor) && canTouch)
        {
            AddReward(0.25f);
            SetColor("partial");
            canTouch = false;
        }

        if (other.TryGetComponent<Wall>(out Wall wall))
        {
            SetReward(-1f);
            SetColor("lose");
            EndEpisode();
        }
    }

    private void SetColor(string condition)
    {
        foreach (MeshRenderer n in floorMeshRenderers)  
        {
            switch (condition)
            {
                case "win":
                    n.material = winMaterial;
                    break;

                case "partial":
                    n.material = partialMaterial;
                    break;

                case "lose":
                    n.material = loseMaterial;
                    break;
            }
        }
    }
}
