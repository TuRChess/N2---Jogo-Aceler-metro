using System.Collections.Generic;
using UnityEngine;

public class LoopBackground : MonoBehaviour
{
    [Header("Prefab com Sprite do Fundo")]
    public Transform backgroundPrefab;   // Tile com SpriteRenderer

    [Header("Configurações")]
    [Tooltip("Quantas cópias do tile (mínimo 2)")]
    public int copies = 3;

    [Tooltip("Espaço extra entre as peças (normalmente 0)")]
    public float spacing = 0f;

    [Tooltip("Câmera que segue o player")]
    public Transform cameraTransform;

    [Tooltip("Margem antes do tile ser reciclado")]
    public float recycleOffset = 0.2f;

    [Header("Parallax (0 = sem parallax)")]
    [Range(0f, 1f)]
    public float parallaxFactor = 0f;

    private List<Transform> backgrounds = new List<Transform>();
    private float tileHeight;
    private Vector3 lastCamPos;

    void Start()
    {
        if (backgroundPrefab == null)
        {
            Debug.LogError("LoopingBackgroundVertical: atribua um backgroundPrefab!");
            enabled = false;
            return;
        }

        if (cameraTransform == null)
        {
            if (Camera.main != null) cameraTransform = Camera.main.transform;
        }

        if (copies < 2) copies = 2;

        // pega tamanho do sprite
        SpriteRenderer sr = backgroundPrefab.GetComponentInChildren<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("O backgroundPrefab precisa ter um SpriteRenderer.");
            enabled = false;
            return;
        }

        tileHeight = sr.bounds.size.y + spacing;

        // instanciar cópias empilhadas verticalmente
        for (int i = 0; i < copies; i++)
        {
            Transform tile = Instantiate(backgroundPrefab, transform);
            Vector3 pos = backgroundPrefab.position;
            pos.y += i * tileHeight;
            tile.position = pos;

            backgrounds.Add(tile);
        }

        lastCamPos = cameraTransform.position;
    }

    void LateUpdate()
    {
        if (backgrounds.Count == 0) return;

        // PARALLAX – move o grupo inteiro
        if (parallaxFactor > 0f)
        {
            Vector3 delta = cameraTransform.position - lastCamPos;
            transform.position += delta * parallaxFactor;
        }

        lastCamPos = cameraTransform.position;

        RecycleVertical();
    }

    void RecycleVertical()
    {
        // pega a 2° cópia do background
        Transform secondTile = backgrounds[1];

        float camY = cameraTransform.position.y;

        // se a câmera passou do segundo background
        if (camY > secondTile.position.y + tileHeight * 0.5f + recycleOffset)
        {
            // pega o mais baixo (backgrounds[0])
            Transform lowest = backgrounds[0];

            // pega o mais alto (backgrounds[2] se for 3 tiles)
            Transform highest = backgrounds[backgrounds.Count - 1];

            // reposiciona lowest acima do highest
            lowest.position = new Vector3(
                highest.position.x,
                highest.position.y + tileHeight,
                highest.position.z
            );

            // reorganiza a lista
            backgrounds.RemoveAt(0);
            backgrounds.Add(lowest);
        }
    }
}
