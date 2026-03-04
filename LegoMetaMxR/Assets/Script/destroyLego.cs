using UnityEngine;

public class destroyLego : MonoBehaviour
{
    [SerializeField] private string trashTag = "Trash";
    [SerializeField] private bool matchByName = true;
    private bool destroyed;

    private void OnTriggerEnter(Collider other)
    {
        TryDestroy(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryDestroy(collision.gameObject);
    }

    private void TryDestroy(GameObject other)
    {
        if (destroyed) return;
        if (!IsTrash(other)) return;
        destroyed = true;
        Destroy(gameObject);
    }

    private bool IsTrash(GameObject other)
    {
        if (other == null) return false;
        if (!string.IsNullOrEmpty(trashTag) && other.CompareTag(trashTag)) return true;
        if (matchByName && other.name == trashTag) return true;
        return false;
    }
}
