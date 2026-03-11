using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum State { Patrol, Chase }
    public State currentState;

    [Header("Detection Ranges")]
    public float detectionRange = 6f;     // normal detection (plays SFX)
    public float runDetectRange = 12f;    // hearing range when player runs

    [Header("Movement")]
    public float patrolRadius = 10f;
    public float patrolSpeed = 3.5f;
    public float chaseSpeed = 3.5f;
    public float chaseSpeedFast = 6f;

    Transform player;
    NavMeshAgent agent;
    Vector3 startPosition;

    [Header("Audio")]
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
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        FPSController fps = player.GetComponent<FPSController>();
        bool playerRunning = fps != null && fps.IsRunning;

        // PLAYER IN CLOSE DETECTION RANGE
        if (distance <= detectionRange)
        {
            currentState = State.Chase;

            if (!hasPlayedDetectSFX && detectSFX != null)
            {
                detectSFX.Play();
                hasPlayedDetectSFX = true;
            }

            if (chaseMusicSFX != null && !chaseMusicSFX.isPlaying)
                chaseMusicSFX.Play();
        }

        // PLAYER RUNNING IN HEARING RANGE
        else if (playerRunning && distance <= runDetectRange)
        {
            currentState = State.Chase;

            hasPlayedDetectSFX = false;

            if (chaseMusicSFX != null && chaseMusicSFX.isPlaying)
                chaseMusicSFX.Stop();
        }

        // PATROL
        else
        {
            currentState = State.Patrol;

            hasPlayedDetectSFX = false;

            if (chaseMusicSFX != null && chaseMusicSFX.isPlaying)
                chaseMusicSFX.Stop();
        }

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

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, runDetectRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);
    }

    //Collide with Player
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
}
