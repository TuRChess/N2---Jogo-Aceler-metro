using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Camera cam;
    [SerializeField] private Transform player;
    [SerializeField] private float offsetZ;
    [SerializeField] private BoxCollider2D worldBounds2d; // variavel do box collider que limita o movimento da camera
    private float halfHeight; // altura até onde a camera pode ir em relação ao Y
    private float halfWidth; // os lados até onde a camera pode ir em relação ao X

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = Camera.main;
        cam.transform.position = new Vector3(player.position.x, player.position.y, player.position.z - offsetZ);
    }

    // Update is called once per frame
    void Update()
    {
        cam.transform.position = new Vector3(player.position.x, player.position.y, player.position.z - offsetZ);

        // Calcula o tamanho visível da camera do jogo
        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;

        // Obtém os limites do cenário (do collider)
        Bounds bounds = worldBounds2d.bounds;

        // Calcula as restrições levando em conta o tamanho da câmera
        float minX = bounds.min.x + halfWidth;
        float maxX = bounds.max.x - halfWidth;
        float minY = bounds.min.y + halfHeight;
        float maxY = bounds.max.y - halfHeight;

        // Restringe a posição da câmera
        Vector3 pos = cam.transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        cam.transform.position = new Vector3(pos.x, pos.y, -offsetZ);
    }
}
