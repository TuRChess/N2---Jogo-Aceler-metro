using UnityEngine;
using UnityEngine.SceneManagement;

public class KillCollider : MonoBehaviour
{
    [SerializeField] private Transform player;
    public float offsetColisor; // diferença de distancia entre player e colisor
    private float highestY; // altura anterior do colisor

    [HideInInspector] public bool startChase;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (offsetColisor <= 0f)
            offsetColisor = player.position.y - transform.position.y;

        // garante que colisor inicia na altura correta (não desce)
        float initialTargetY = player.position.y - offsetColisor;
        highestY = Mathf.Max(transform.position.y, initialTargetY);
        transform.position = new Vector3(transform.position.x, highestY, transform.position.z);

        startChase = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (startChase)
        {
            ChasePlayer();
        }
    }

    void ChasePlayer()
    {
        float targetY = player.position.y - offsetColisor; // altura atual do colisor

        // só sobe se targetY for maior que altura anterior
        if (targetY > highestY)
        {
            highestY = targetY; // seta a altura atual como anterior
            transform.position = new Vector3(transform.position.x, highestY, transform.position.z); // Move o colisor de acordo
        }
    }

    void KillPlayer()
    {
        startChase = false;
        SceneManager.LoadScene("GameOver");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            KillPlayer();
        }
    }
}
