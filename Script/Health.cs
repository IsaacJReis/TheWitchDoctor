using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    [SerializeField] private int vidaMaxima = 100;
    [SerializeField] private int vidaAtual;

    public int VidaAtual => vidaAtual;
    public bool Morto { get; private set; }

    public event Action OnMorreu; // avisa quando a vida chega a 0

    void Awake()
    {
        vidaAtual = vidaMaxima;
    }

    public void ReceberDano(int quantidade)
    {
        if (Morto) return;

        vidaAtual -= quantidade;

        if (vidaAtual <= 0)
        {
            vidaAtual = 0;
            Morto = true;
            OnMorreu?.Invoke();
        }
    }
}