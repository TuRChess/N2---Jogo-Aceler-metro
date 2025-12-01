using System.Collections;
using UnityEngine;

public class Platform : MonoBehaviour
{

    private new Collider2D collider;
    private Animator anim;
    private Rigidbody2D rb;
    private bool colidiuPlayer = false;
    private SpriteRenderer spriteRenderer;

    [Header("Sprites Plataformas")]
    [SerializeField] private Sprite[] spritesPlataformaTypes;

    [Header("Configurações do Jogador")]
    [SerializeField] private Transform playerPesTransform;
    private Rigidbody2D playerRb;
    private PlayerTiltMove playerScript;
    [SerializeField] private float minHeightJump;

    [Header("Configurações das Plataformas de Gelo")]
    public float slideForce = 4f;
    private bool playerOnTop = false;

    [Header("Configurações da Plataforma de Pedra")]
    [SerializeField] private float timeToReturn;
    private bool isBlinking = false; // está desaparecendo
    private bool isDisabled = false; // desapareceu

    private enum PlatformTypes
    {
        Grama,
        Pedra,
        Gelo,
        Neve
    }

    [SerializeField] private PlatformTypes types;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        collider = GetComponent<Collider2D>();
        collider.enabled = false;

        spriteRenderer = GetComponent<SpriteRenderer>();

        anim = GetComponent<Animator>();
        anim.enabled = false;

        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch (types) {
            case PlatformTypes.Grama:
                spriteRenderer.sprite = spritesPlataformaTypes[0];
                GrassPlatform();
                break;
            case PlatformTypes.Pedra:
                spriteRenderer.sprite = spritesPlataformaTypes[1];
                RockPlatform();
                break;
            case PlatformTypes.Gelo:
                spriteRenderer.sprite = spritesPlataformaTypes[2];
                IcePlatform();
                break;
            case PlatformTypes.Neve:
                spriteRenderer.sprite = spritesPlataformaTypes[3];
                SnowPlatform();
                break;
        }
    }

    void GrassPlatform()
    {
        // Verifica se o y do player é maior que o da plataforma
        if (playerPesTransform.position.y > this.transform.position.y)
        {
            // Torna a plataforma ativa enquanto estiver acima dela
            PlatformActivated();
        }
        else
        {
            // Se estiver abaixo da plataforma
            PlatformDisabled();
        }
    }

    void RockPlatform()
    {
        // Verifica se o y do player é maior que o da plataforma
        if (playerPesTransform.position.y > this.transform.position.y)
        {
            if (!isDisabled)  // só ativa se NÃO estiver desativada
                PlatformActivated();

            if (colidiuPlayer && !isBlinking) // só iniciar se não estiver piscando
            {
                // Controla a desativação e a ativação
                StartCoroutine(DisableAfterTime());
            }
        }
        else
        {
            PlatformDisabled();
        }
    }

    void IcePlatform()
    {
        // Verifica se o y do player é maior que o da plataforma
        if (playerPesTransform.position.y > this.transform.position.y)
        {
            // Torna a plataforma ativa enquanto estiver acima dela
            PlatformActivated();

            // Libera a rotação
            rb.constraints &= ~RigidbodyConstraints2D.FreezeRotation;

            ApplySlidingEffect();
        }
        else
        {
            PlatformDisabled();
        }
    }

    void SnowPlatform()
    {
        if (playerPesTransform.position.y > this.transform.position.y)
        {
            // Torna a plataforma ativa enquanto estiver acima dela
            PlatformActivated();

            if (playerScript != null && colidiuPlayer) {
                var diferenca = playerScript.jumpForce - minHeightJump;
                playerScript.jumpForce -= diferenca;

                if (playerScript.jumpForce <= minHeightJump)
                    playerScript.jumpForce = minHeightJump;
            }
        }
        else
        {
            PlatformDisabled();
            if (playerScript != null)
            {
                playerScript.jumpForce = playerScript.jumpInitial;
            }
        }
    }

    void PlatformActivated()
    {
        collider.enabled = true;
    }

    void PlatformDisabled()
    {
        collider.enabled = false;
    }

    IEnumerator DisableAfterTime()
    {
        if(isBlinking) yield break; // impede multiplas chamadas
        isBlinking = true;

        anim.enabled = true;

        int blinkCount = 5;         // número total de piscadas
        float speed = 1f;            // velocidade inicial
        float acceleration = 0.2f;   // aceleração por ciclo
        float minDelay = 0.15f;      // tempo mínimo entre piscadas

        for (int i = 0; i < blinkCount; i++)
        {
            anim.speed = speed;
            anim.Play("PiscarPlataforma", 3, 0f);

            yield return null; // 1 frame para atualizar

            AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
            float clipLength = info.length;

            // Calcula tempo real da animação com speed
            float animTime = clipLength / speed;

            // Nunca deixar a piscada ser instantânea
            float waitTime = Mathf.Max(minDelay, animTime);

            yield return new WaitForSeconds(waitTime);

            speed += acceleration;
        }

        anim.speed = 1f;
        anim.enabled = false;

        collider.enabled = false;
        spriteRenderer.enabled = false;
        isDisabled = true;

        yield return new WaitForSeconds(timeToReturn);

        collider.enabled = true;
        spriteRenderer.enabled = true;
        spriteRenderer.color = Color.white;
        isDisabled = false;

        isBlinking = false;
    }

    void ApplySlidingEffect()
    {
        if (!playerOnTop || playerRb == null) return;

        // força suave lateral baseada na movimentação da plataforma
        float slide = rb.linearVelocity.x * slideForce;

        playerRb.AddForce(new Vector2(slide, 0f), ForceMode2D.Force);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            colidiuPlayer = true;
            playerOnTop = true;

            playerRb = collision.collider.GetComponent<Rigidbody2D>();
            playerScript = collision.gameObject.GetComponent<PlayerTiltMove>();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            colidiuPlayer = false;
            playerOnTop = false;

            playerScript.jumpForce = playerScript.jumpInitial;
        }
    }
}
