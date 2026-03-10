using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum State { Patrol, Chase }
    public State currentState;

    [Header("Detection & Patrol")]
    public float detectionRange = 6f;
    public float patrolRadius = 10f;
    public float patrolSpeed = 3.5f;
    public float chaseSpeed = 3.5f;
    public float chaseSpeedFast = 6f;

    Transform player;
    NavMeshAgent agent;
    Vector3 startPosition;

    [Header("Audio (Optional)")]
    public AudioSource detectSFX;
    public AudioSource chaseMusicSFX;
    bool hasPlayedDetectSFX = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;

        currentState = State.Patrol;
        PickRandomPatrolPoint();
        agent.speed = patrolSpeed;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Get player running status
        FPSController fps = player.GetComponent<FPSController>();
        bool playerRunning = fps != null && fps.IsRunning;

        if (distance <= detectionRange)
        {
            // Only play SFX if player is in proximity
            currentState = State.Chase;

            if (!hasPlayedDetectSFX && detectSFX != null)
            {
                detectSFX.Play();
                hasPlayedDetectSFX = true;
            }

            if (chaseMusicSFX != null && !chaseMusicSFX.isPlaying)
            {
                chaseMusicSFX.Play();
            }
        }
        else if (playerRunning)
        {
            // Chase silently if player is running but outside detection range
            currentState = State.Chase;
            hasPlayedDetectSFX = false;

            if (chaseMusicSFX != null && chaseMusicSFX.isPlaying)
                chaseMusicSFX.Stop();
        }
        else
        {
            // Patrol
            currentState = State.Patrol;
            hasPlayedDetectSFX = false;

            if (chaseMusicSFX != null && chaseMusicSFX.isPlaying)
                chaseMusicSFX.Stop();
        }

        // Perform behavior
        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                break;
            case State.Chase:
                Chase(playerRunning);
                break;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
            }
        }
    }

    void Patrol()
    {
        agent.speed = patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            PickRandomPatrolPoint();
        }
    }

    void Chase(bool playerRunning)
    {
        agent.speed = playerRunning ? chaseSpeedFast : chaseSpeed;
        agent.SetDestination(player.position);
    }

    void PickRandomPatrolPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius + startPosition;
        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);
    }
}
