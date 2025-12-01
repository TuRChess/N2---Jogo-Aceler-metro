using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTiltMove : MonoBehaviour
{
    [Header("Pulo Automático")]
    public float jumpForce = 12f;
    [HideInInspector] public float jumpInitial;
    [SerializeField] private Transform groundCheck;
    private bool isGrounded;
    [SerializeField] private float radiusWidth = 1f;
    [SerializeField] private float radiusHeight = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [HideInInspector] public bool canJump = true;
    [SerializeField] private float intervalJump = 1f;

    [Header("Movimento Lateral")]
    [SerializeField] private float horizontalSpeed = 5f;
    [SerializeField] private float deadZone = 0.05f;

    [Header("Sprites Estado")]
    [SerializeField] private Sprite[] sprites;

    private bool autoCalibrateOnStart = true;
    private int calibrationSamples = 30;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    [HideInInspector] public AudioSource audioSource;

    private Vector3 accelZero; // offset calibrar
    private Vector2 velTarget;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (autoCalibrateOnStart && Accelerometer.current != null)
            StartCoroutine(Calibrate());

        jumpInitial = jumpForce;

        spriteRenderer.sprite = sprites[0];

        audioSource = GetComponent<AudioSource>();
        audioSource.Stop();
    }

    private void Update()
    {
        if (rb.linearVelocity.y > deadZone)
        {
            spriteRenderer.sprite = sprites[2]; // subindo
        }
        else if (rb.linearVelocity.y < -deadZone)
        {
            spriteRenderer.sprite = sprites[1]; // caindo
        }
        else 
        {
            spriteRenderer.sprite = sprites[0]; // parado
        }

        CheckGround();

        if (isGrounded)
        {
            AutoJump();
        }
        
        TiltMovement();
    }

    void CheckGround()
    {
        // Verifica se está tocando no chão
        isGrounded = Physics2D.OverlapCapsule(groundCheck.position, new Vector2(radiusWidth, radiusHeight), 
            CapsuleDirection2D.Horizontal, 0f, groundLayer);
    }

    void AutoJump()
    {
        if (!canJump) return;

        // Só pula se estiver descendo (velocidade Y < 0)
        if (rb.linearVelocity.y > 0) return;

        // Zera a velocidade vertical para não acumular força
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);

        // Aplica impulso para cima
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        // inicia cooldown
        StartCoroutine(JumpCooldown());
    }

    IEnumerator JumpCooldown()
    {
        canJump = false;
        yield return new WaitForSeconds(intervalJump);
        canJump = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("KillTrigger"))
        {
            KillCollider kill = FindFirstObjectByType<KillCollider>();
            
            if(kill != null)
            {
                kill.offsetColisor = (transform.position.y - kill.transform.position.y) + 3;
                kill.startChase = true;
            }
        }
    }

    void TiltMovement()
    {
        if (Accelerometer.current == null) return;

        Vector3 a = Accelerometer.current.acceleration.ReadValue() - accelZero;
        Vector2 tilt = new Vector2(a.x, 0f);

        if (tilt.magnitude < deadZone) tilt = Vector2.zero;
        tilt = Vector2.ClampMagnitude(tilt, 1f);

        velTarget = tilt.normalized * horizontalSpeed;

        rb.AddForce(velTarget, ForceMode2D.Force);

        if (tilt.x > 0f)
        {
            spriteRenderer.flipX = false;
        }
        else
        {
            spriteRenderer.flipX = true;
        }

            // limita a velocidade lateral para não ficar descontrolado
            float maxSpeed = 6f;
        if (Mathf.Abs(rb.linearVelocity.x) > maxSpeed)
        {
            rb.linearVelocity = new Vector2(Mathf.Sign(rb.linearVelocity.x) * maxSpeed, rb.linearVelocity.y);
        }
    }

    System.Collections.IEnumerator Calibrate()
    {
        accelZero = Vector3.zero;
        int n = Mathf.Max(1, calibrationSamples);
        for (int i = 0; i < n; i++)
        {
            accelZero += Accelerometer.current.acceleration.ReadValue();
            yield return null;
        }
        accelZero /= n;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector2 size = new Vector2(radiusWidth, radiusHeight);
        Gizmos.DrawWireCube(groundCheck.position, size);
    }
}
