using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinnerCollider : MonoBehaviour
{
    [SerializeField] private Transform playerPesTransform;
    private new Collider2D collider;
    [SerializeField] private Camera cam;
    [SerializeField] private float limitZoomIn, incrementZoom;

    private bool zoomActive = false; // impede múltiplas execuções

    private void Start()
    {
        collider = GetComponent<Collider2D>();
        collider.enabled = false;
        cam = Camera.main;
    }

    private void Update()
    {
        if (!zoomActive && playerPesTransform.position.y > transform.position.y)
        {
            zoomActive = true;
            collider.enabled = true;

            var player = FindFirstObjectByType<PlayerTiltMove>();
            player.canJump = false;
            player.gameObject.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

            player.audioSource.Play();

            StartCoroutine(ZoomIn());
        }
    }

    IEnumerator ZoomIn()
    {
        // Enquanto o zoom não chegar ao limite
        while (cam.orthographicSize > limitZoomIn)
        {
            cam.orthographicSize -= incrementZoom * Time.deltaTime;
            yield return null;
        }

        var player = FindFirstObjectByType<PlayerTiltMove>();
        player.audioSource.Stop();

        // Garante que não passe do limite
        cam.orthographicSize = limitZoomIn;

        SceneManager.LoadScene("GameWin");
    }
}
