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

    [Header("Combate")]
    [SerializeField] private float alcanceAtaque = 1.2f;
    [SerializeField] private float raioAtaque = 0.6f;
    [SerializeField] private LayerMask camadaInimigo;
    [SerializeField] private int danoAtaque1 = 10;
    [SerializeField] private int danoAttackRun = 10;
    [SerializeField] private int danoKick = 15;
    [SerializeField] private int danoPummel = 15;
    [SerializeField] private int danoSpecial1 = 25;
    [SerializeField] private int danoSpecial2 = 25;
    [SerializeField] private int danoCastSpell = 25;
    [SerializeField] private float delayImpacto = 0.2f; // tempo entre apertar e o golpe "acertar"

    private Rigidbody2D rb;
    private Animator animator;
    private Health health;

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
        health = GetComponent<Health>();

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (health != null)
        {
            health.OnMorreu += Morrer;
        }
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
        if (rolando) return;

        // Rolar: Ctrl + movimento
        if (Input.GetKey(KeyCode.LeftControl) && movendo && Time.time >= proximoRollDisponivel)
        {
            StartCoroutine(Rolar());
            return;
        }

        // Ataque: botão direito do mouse
        if (Input.GetMouseButtonDown(1))
        {
            if (movendo)
                Atacar("AttackRun", danoAttackRun);
            else
                Atacar("Attack1", danoAtaque1);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) { Atacar("Special1", danoSpecial1); return; }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { Atacar("Special2", danoSpecial2); return; }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { Atacar("CastSpell", danoCastSpell); return; }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { Atacar("Kick", danoKick); return; }
        if (Input.GetKeyDown(KeyCode.Alpha5)) { Atacar("Pummel", danoPummel); return; }
    }

    private void Atacar(string trigger, int dano)
    {
        animator.SetTrigger(trigger);
        StartCoroutine(AplicarDanoComAtraso(dano));
    }

    private System.Collections.IEnumerator AplicarDanoComAtraso(int dano)
    {
        yield return new WaitForSeconds(delayImpacto);

        Vector2 centroGolpe = (Vector2)transform.position + ultimaDirecao.normalized * alcanceAtaque;
        Collider2D[] atingidos = Physics2D.OverlapCircleAll(centroGolpe, raioAtaque, camadaInimigo);

        foreach (var col in atingidos)
        {
            Health alvo = col.GetComponent<Health>();
            if (alvo == null) continue;

            // Se for o Boss, só causa dano durante a janela de vulnerabilidade
            BossController boss = col.GetComponent<BossController>();
            if (boss != null && !boss.Vulneravel) continue;

            alvo.ReceberDano(dano);
        }
    }

    private void Mover()
    {
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

    public void TomarDano()
    {
        animator.SetTrigger("TakeDamage");
    }

    private void Morrer()
    {
        animator.SetTrigger("Die");
        this.enabled = false;
    }

    // Mostra a área de ataque no editor, para ajustar alcanceAtaque/raioAtaque visualmente
    void OnDrawGizmosSelected()
    {
        Vector2 centro = (Vector2)transform.position + ultimaDirecao.normalized * alcanceAtaque;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(centro, raioAtaque);
    }
}