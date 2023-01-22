using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PheromoneField : MonoBehaviour
{
    static public PheromoneField instance { get; private set;}
    public GameObject nodePrefab;
    public float[] pheromoneDuration;
    public float proximityUpdate = 0.1f;
    public LayerMask layerMask;
    [SerializeField] private int nodeCount = 0;
    private float minFeromoneTime;
    private List<PheromoneNode> nodes = new List<PheromoneNode>();

    // Start is called before the first frame update
    void Start()
    {
        minFeromoneTime = 10;
        for (int i = 0; i < pheromoneDuration.Length; i++)
        {
            if (pheromoneDuration[i] < minFeromoneTime) minFeromoneTime = pheromoneDuration[i];
        }
        if (instance == null) instance = this;
        else Debug.LogError("FeromoneField already exists");
    }

    public PheromoneNode GetNode(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, proximityUpdate, layerMask);
        PheromoneNode node = null;
        for (int i = 0; node == null && i < colliders.Length; i++)
        {
            node = colliders[i].GetComponent<PheromoneNode>();
        }
        for (int i = 0; node == null && i < nodes.Count; i++)
        {
            if (Time.time - nodes[i].lastUpdate > minFeromoneTime) node = nodes[i];
        }
        if (node == null) node = CreateNode();
        node.transform.position = position;
        return node;
    }

    void Update()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].UpdateColor();
        }
    }

    public PheromoneNode CreateNode()
    {
        GameObject nodeObject = Instantiate(nodePrefab, transform);
        PheromoneNode node = nodeObject.GetComponent<PheromoneNode>();
        node.feromoneTimes = new float[pheromoneDuration.Length];
        nodes.Add(node);
        nodeCount++;
        return node;
    }
}
