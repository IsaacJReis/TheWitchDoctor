using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Animator))]
public class BossController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Transform player;

    [Header("Movimento")]
    [SerializeField] private float velocidadeAndar = 1.5f;
    [SerializeField] private float distanciaParaAtacar = 3f;

    [Header("Ataque (Pulo)")]
    [SerializeField] private float duracaoPulo = 0.6f;
    [SerializeField] private float raioImpacto = 1.5f;
    [SerializeField] private int danoImpacto = 20;
    [SerializeField] private float duracaoVulneravel = 3f;
    [SerializeField] private float cooldownEntreAtaques = 1.5f;
    [SerializeField] private LayerMask camadaPlayer;

    private Animator animator;
    private Health health;
    private Rigidbody2D rb;
    private Vector2 ultimaDirecao = Vector2.down;

    private enum Estado { Andando, Pulando, Vulneravel, Pausado }
    private Estado estadoAtual;

    // O Player só consegue causar dano enquanto isso for true
    public bool Vulneravel => estadoAtual == Estado.Vulneravel;

    void Awake()
    {
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
        }

        health.OnMorreu += Morrer;

        if (player == null)
        {
            GameObject alvo = GameObject.FindGameObjectWithTag("Player");
            if (alvo != null) player = alvo.transform;
        }
    }

    void Update()
    {
        if (player == null || estadoAtual == Estado.Pulando || estadoAtual == Estado.Vulneravel)
        {
            return;
        }

        float distancia = Vector2.Distance(transform.position, player.position);

        if (distancia <= distanciaParaAtacar)
        {
            StartCoroutine(AtacarComPulo());
        }
        else
        {
            AndarEmDirecaoAoPlayer();
        }
    }

    private void AndarEmDirecaoAoPlayer()
    {
        estadoAtual = Estado.Andando;

        Vector2 direcao = ((Vector2)player.position - (Vector2)transform.position).normalized;
        transform.position += (Vector3)(direcao * velocidadeAndar * Time.deltaTime);

        ultimaDirecao = direcao;
        animator.SetBool("IsWalking", true);
        animator.SetFloat("MoveX", ultimaDirecao.x);
        animator.SetFloat("MoveY", ultimaDirecao.y);
    }

    private System.Collections.IEnumerator AtacarComPulo()
    {
        estadoAtual = Estado.Pulando;
        animator.SetBool("IsWalking", false);

        // Mantém a direção do pulo apontando para onde o player está
        Vector3 posicaoInicial = transform.position;
        Vector3 posicaoAlvo = player.position;
        ultimaDirecao = ((Vector2)posicaoAlvo - (Vector2)posicaoInicial).normalized;
        animator.SetFloat("MoveX", ultimaDirecao.x);
        animator.SetFloat("MoveY", ultimaDirecao.y);

        animator.SetTrigger("JumpAttack");

        float tempoDecorrido = 0f;
        while (tempoDecorrido < duracaoPulo)
        {
            float progresso = tempoDecorrido / duracaoPulo;
            transform.position = Vector3.Lerp(posicaoInicial, posicaoAlvo, progresso);
            tempoDecorrido += Time.deltaTime;
            yield return null;
        }

        transform.position = posicaoAlvo;

        // Dano de impacto, se o player ainda estiver perto do ponto de queda
        Collider2D[] atingidos = Physics2D.OverlapCircleAll(transform.position, raioImpacto, camadaPlayer);
        foreach (var col in atingidos)
        {
            Health alvoHealth = col.GetComponent<Health>();
            if (alvoHealth != null)
            {
                alvoHealth.ReceberDano(danoImpacto);
            }
        }

        yield return StartCoroutine(FicarVulneravel());
    }

    private System.Collections.IEnumerator FicarVulneravel()
    {
        estadoAtual = Estado.Vulneravel;
        //para o Boss ficar parado durante 3 segundos
        yield return new WaitForSeconds(duracaoVulneravel);

        estadoAtual = Estado.Pausado;
        yield return new WaitForSeconds(cooldownEntreAtaques);

        estadoAtual = Estado.Andando;
    }

    private void Morrer()
    {
         StopAllCoroutines();
        animator.SetBool("IsWalking", false);
        animator.SetTrigger("Die");
        
        this.enabled = false;
        
       
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaParaAtacar);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, raioImpacto);
    }
}