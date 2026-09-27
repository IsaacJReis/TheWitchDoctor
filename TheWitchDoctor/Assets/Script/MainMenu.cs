using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Nomes exatos das cenas (Build Settings)")]
    [SerializeField] private string cenaTraining = "SampleScene";
    [SerializeField] private string cenaBattle = "BossScene";

    

    public void IrParaMenuPrincipal()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void IrParaTraining()
    {
        SceneManager.LoadScene(cenaTraining);
    }

    public void IrParaBattle()
    {
        SceneManager.LoadScene(cenaBattle);
    }
}