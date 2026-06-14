using UnityEngine;
using UnityEngine.AI;

public class NavMeshDebugger : MonoBehaviour
{
    void Start()
    {
        var agents = FindObjectsOfType<NavMeshAgent>();
        foreach (var agent in agents)
        {
            if (!agent.isOnNavMesh)
            {
                Debug.LogError($"? {agent.gameObject.name} NavMesh üzerinde DEÐÝL!", agent.gameObject);
            }
            else
            {
                Debug.Log($"? {agent.gameObject.name} NavMesh üzerinde.", agent.gameObject);
            }
        }
    }
}