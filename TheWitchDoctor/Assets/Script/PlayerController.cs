using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float velocidadeAndando = 2f;
    [SerializeField] private float velocidadeCorrida = 4f;
    [SerializeField] private float velocidadeAgachado = 1f;

    [Header("Esquiva (Roll)")]
    [SerializeField] private float velocidadeRoll = 6f;
    [SerializeField] private float duracaoRoll = 0.3f;
    [SerializeField] private float cooldownRoll = 0.5f;

    private Rigidbody2D rb;
    private Animator animator;

    private Vector2 input;
    private Vector2 ultimaDirecao = Vector2.down; // direção inicial (S)
    private bool movendo;
    private bool correndo;
    private bool agachado;

    private bool rolando;
    private float proximoRollDisponivel;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        LerInput();
        AtualizarAnimator();
        LerAcoes();
    }

    void FixedUpdate()
    {
        Mover();
    }

    private void LerInput()
    {
        input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        movendo = input != Vector2.zero;

        if (movendo)
        {
            ultimaDirecao = input;
        }

        agachado = Input.GetKey(KeyCode.C);
        correndo = movendo && !agachado && Input.GetKey(KeyCode.LeftShift);
    }

    private void AtualizarAnimator()
    {
        animator.SetBool("IsMoving", movendo);
        animator.SetBool("IsRunning", correndo);
        animator.SetBool("IsCrouching", agachado);

        animator.SetFloat("MoveX", ultimaDirecao.x);
        animator.SetFloat("MoveY", ultimaDirecao.y);
    }

    private void LerAcoes()
    {
        if (rolando) return; // ignora outras ações enquanto esquiva

        // Rolar: Ctrl + movimento
        if (Input.GetKey(KeyCode.LeftControl) && movendo && Time.time >= proximoRollDisponivel)
        {
            StartCoroutine(Rolar());
            return;
        }

        // Ataque: botão direito do mouse
        if (Input.GetMouseButtonDown(1))
        {
            animator.SetTrigger(movendo ? "AttackRun" : "Attack1");
            return;
        }

        // Habilidades especiais
        if (Input.GetKeyDown(KeyCode.Alpha1)) { animator.SetTrigger("Special1"); return; }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { animator.SetTrigger("Special2"); return; }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { animator.SetTrigger("CastSpell"); return; }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { animator.SetTrigger("Kick"); return; }
        if (Input.GetKeyDown(KeyCode.Alpha5)) { animator.SetTrigger("Pummel"); return; }
    }

    private void Mover()
    {
        // Durante o Roll, a velocidade é controlada pela coroutine Rolar()
        if (rolando) return;

        float velocidade = agachado ? velocidadeAgachado
                          : correndo ? velocidadeCorrida
                          : velocidadeAndando;

        rb.linearVelocity = input.normalized * velocidade;
    }

    private System.Collections.IEnumerator Rolar()
    {
        rolando = true;
        proximoRollDisponivel = Time.time + cooldownRoll;

        Vector2 direcaoDash = ultimaDirecao.normalized;
        animator.SetTrigger("Roll");

        float tempoDecorrido = 0f;
        while (tempoDecorrido < duracaoRoll)
        {
            rb.linearVelocity = direcaoDash * velocidadeRoll;
            tempoDecorrido += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        rb.linearVelocity = Vector2.zero;
        rolando = false;
    }

    // Métodos públicos para ligar depois a um sistema de vida/inimigos
    public void TomarDano()
    {
        animator.SetTrigger("TakeDamage");
    }

    public void Morrer()
    {
        animator.SetTrigger("Die");
        this.enabled = false;
    }
}