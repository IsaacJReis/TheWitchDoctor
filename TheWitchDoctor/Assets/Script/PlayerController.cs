using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float velocidadeAndando = 2f;
    [SerializeField] private float velocidadeCorrida = 4f;
    [SerializeField] private float velocidadeAgachado = 1f;

    private Rigidbody2D rb;
    private Animator animator;

    private Vector2 input;
    private Vector2 ultimaDirecao = Vector2.down; // direção inicial (S)
    private bool movendo;
    private bool correndo;
    private bool agachado;
    private bool travado; // true durante ações que não podem ser interrompidas (ataque, roll, etc.)

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Personagem 2D top-down não deve sofrer gravidade
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

        // Guarda a última direção para o Idle e ataques olharem para o lado certo
        if (movendo)
        {
            ultimaDirecao = input;
        }

        agachado = Input.GetKey(KeyCode.C);

        // Só corre se estiver se movendo e segurando Shift, e não estiver agachado
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
        // Enquanto uma ação estiver travando o personagem, ignora novos inputs de ação
        if (travado) return;

        // Rolar: Ctrl + movimento
        if (Input.GetKey(KeyCode.LeftControl) && movendo)
        {
            AcionarAcao("Roll");
            return;
        }

        // Ataque: botão direito do mouse (Attack1 parado, AttackRun se movendo)
        if (Input.GetMouseButtonDown(1))
        {
            AcionarAcao(movendo ? "AttackRun" : "Attack1");
            return;
        }

        // Habilidades especiais
        if (Input.GetKeyDown(KeyCode.Alpha1)) { AcionarAcao("Special1"); return; }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { AcionarAcao("Special2"); return; }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { AcionarAcao("CastSpell"); return; }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { AcionarAcao("Kick"); return; }
        if (Input.GetKeyDown(KeyCode.Alpha5)) { AcionarAcao("Pummel"); return; }
    }

    private void AcionarAcao(string trigger)
    {
        travado = true;
        animator.SetTrigger(trigger);
    }

    
    public void FimDaAcao()
    {
        travado = false;
    }

    private void Mover()
    {
        // Durante uma ação travada, o personagem não se desloca
        if (travado)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float velocidade = agachado ? velocidadeAgachado
                          : correndo ? velocidadeCorrida
                          : velocidadeAndando;

        rb.linearVelocity = input.normalized * velocidade;
    }

    // ---- Métodos públicos para ligar depois a um sistema de vida/inimigos ----

    public void TomarDano()
    {
        if (travado) return; // opcional: pode remover se quiser que dano sempre interrompa
        travado = true;
        animator.SetTrigger("TakeDamage");
    }

    public void Morrer()
    {
        travado = true;
        animator.SetTrigger("Die");
        this.enabled = false; // desliga o script, o personagem para de responder a input
    }
}