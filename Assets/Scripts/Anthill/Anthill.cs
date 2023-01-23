using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anthill : MonoBehaviour
{
    [System.Serializable]
    public struct AgentState
    {
        public int[] pheromoneSense;
        public int pheromoneDropped;
    }
    public Transform target;

    public List<AgentState> states;
    public Ant agentPrefab;
    private List<Ant> ants = new List<Ant>();

    [Range(1, 500)]
    public int startingCount = 250;

    [Range(0f, 1f)]
    public float turnSpeed = 1f;
    [Range(0f, 100f)]
    public float maxSpeed = 5f;
    [Range(1f, 5f)]
    public float stuborness = 1f;

    public bool dropPheromones = true;
    private float lastDropped = 0;
    public float pheromoneDropRate = .5f;
    public int gatheredFood = 0;


    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < startingCount; i++)
        {
            Ant newAnt = Instantiate(
                agentPrefab,
                (Vector2)transform.position + (Random.insideUnitCircle),
                Quaternion.Euler(Vector3.forward * Random.Range(0f, 360f)),
                transform
            );
            newAnt.name = "Ant " + i;
            newAnt.Initialize(this);
            ants.Add(newAnt);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        bool drop = dropPheromones && Time.time - lastDropped > pheromoneDropRate;
        if (drop) lastDropped = Time.time;
        foreach (Ant ant in ants)
        {
            // Perception
            // Movement
            Vector2 move = Smell(ant) + Visuals(ant);
            move = HandleMapCollision(ant, move);
            ant.Move(move);
            if (drop) DropPheromone(ant);
        }
    }

    private Vector2 HandleMapCollision(Ant ant, Vector2 move)
    {
        RaycastHit2D hit = Physics2D.Raycast(ant.transform.position, move, 0.5f, (1 << 7));
        if (hit.collider != null)
        {
            move = Vector2.Reflect(move, hit.normal);
        }
        return move;
    }

    Vector2 Visuals(Ant ant)
    {
        RaycastHit2D hitRigth = Physics2D.Raycast(ant.transform.position, 3*ant.transform.up + ant.transform.right, 1f, 1 << 7);
        Debug.DrawRay(ant.transform.position, 3 * ant.transform.up + ant.transform.right, Color.red);
        RaycastHit2D hitLeft = Physics2D.Raycast(ant.transform.position, 3*ant.transform.up - ant.transform.right, 1f, 1 << 7);
        Debug.DrawRay(ant.transform.position, 3 * ant.transform.up - ant.transform.right, Color.red);
        Vector2 avoidanceMove = Vector2.zero;
        if (hitRigth.collider != null)
        {
            avoidanceMove += ((Vector2)ant.transform.position - (Vector2)hitRigth.point) / (hitRigth.distance == 0 ? .1f : hitRigth.distance);
        }
        if (hitLeft.collider != null)
        {
            avoidanceMove += ((Vector2)ant.transform.position - (Vector2)hitLeft.point) / (hitLeft.distance == 0 ? .1f : hitLeft.distance);
        }
        return avoidanceMove;
    }

    Vector2 Smell(Ant ant)
    {
        Collider2D food = Physics2D.OverlapCircle(ant.transform.position, 1f, 1 << 8);
        Collider2D home = Physics2D.OverlapCircle(ant.transform.position, 1f, 1 << 10);
        if (ant.state == 0 && food != null) return (Vector2)food.transform.position - (Vector2)ant.transform.position;
        if (ant.state == 1 && home != null) return (Vector2)home.transform.position - (Vector2)ant.transform.position;
        for (int i = 0; i < states[ant.state].pheromoneSense.Length; i++) {
            if (states[ant.state].pheromoneSense[i] == 0) continue;
            List<ColliderPheromoneNode> nodes = ColliderPheromoneField.instance.GetPheromoneContext(ant, i);
            if (nodes.Count > 0)
            {
                Vector2 move = Vector2.zero;
                for (int j = 0; j < nodes.Count; j++)
                {
                    move += ((Vector2)nodes[j].position - (Vector2)ant.transform.position) * states[ant.state].pheromoneSense[i] * nodes[j].Potency();
                    // ColliderPheromoneField.instance.GetNode(nodes[j].position).UpdatePheromone(ant.Home.states[ant.state].pheromoneDropped);
                }
                return move;
            }
        }
        // if (target != null) return (Vector2)target.position - (Vector2)ant.transform.position;
        return stuborness * ant.transform.up + ant.transform.right * Random.Range(-1f, 1f);
    }

    private void DropPheromone(Ant ant)
    {
        ColliderPheromoneField.instance.GetNode(ant.transform.position).UpdatePheromone(ant.Home.states[ant.state].pheromoneDropped);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Change state from carrying to searching
        Ant ant = other.GetComponent<Ant>();
        if (ant != null && ant.Home == this && ant.state == 1)
        {
            ant.DropFood();
            gatheredFood++;
        }
    }
}
