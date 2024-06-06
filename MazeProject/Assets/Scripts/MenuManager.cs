using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    // Boutons du menu principals
    public Button boutonJouer;
    public Button boutonParametres;
    public Button boutonQuitter;
    public Button boutonEnregistrer;

    // Parametres
    public Button texteTailleLabirynthe;
    public Text tailleLabiryntheTexte;
    private int tailleLabirynthe = 20;
    public Slider sliderTailleLabirynthe;
    public Button texteNbCles;
    public Slider sliderNbCles;
    public Text nbClesAffiche;
    private int nbCles = 3;

    public Button texteNbCailloux;
    public Slider sliderNbCailloux;
    public Text nbCaillouxAffiche;
    private int nbCailloux = 10;

    // Sons
    public AudioClip buttonClickSound;

    private AudioSource audioSource;
    /**
     * Clic sur le bouton Jouer
    */
    public void Jouer()
    {
        playSound();
        // V�rifier si la taille est dans la plage de 10 � 50.
        if (tailleLabirynthe >= 10 && tailleLabirynthe <= 50)
        {
            PlayerPrefs.SetInt("tailleLabirynthe", tailleLabirynthe);
            PlayerPrefs.SetInt("nbCles", nbCles);
            PlayerPrefs.SetInt("nbCailloux", nbCailloux);
            PlayerPrefs.Save();
            // Charger la sc�ne avec la taille du labyrinthe s�lectionn�e.
            SceneManager.LoadScene("MazeScene");
        }
        else
        {
            // Afficher un message d'erreur ou prendre une autre action appropri�e.
            Debug.LogError("La taille du labyrinthe doit �tre entre 10 et 50.");
        }
    }

    private int GetTailleLabirynthe()
    {
        return (int)sliderTailleLabirynthe.value;
    }

    private int GetNbCles()
    {
        return (int)sliderNbCles.value;
    }

    private int GetNbCailloux()
    {
        return (int)sliderNbCailloux.value;
    }

    public void Awake() 
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // Si l'AudioSource n'est pas déjà attaché, ajoutez-le
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        tailleLabiryntheTexte.text = GetTailleLabirynthe().ToString() + " blocs";
        nbClesAffiche.text = GetNbCles().ToString() + " clés";
        nbCaillouxAffiche.text = GetNbCailloux().ToString() + " cailloux";
        sliderNbCles.onValueChanged.AddListener(delegate { nbClesAffiche.text = GetNbCles().ToString() + " clés"; });
        sliderTailleLabirynthe.onValueChanged.AddListener(delegate { tailleLabiryntheTexte.text = GetTailleLabirynthe().ToString() + " blocs"; });
        sliderNbCailloux.onValueChanged.AddListener(delegate { nbCaillouxAffiche.text = GetNbCailloux().ToString() + " cailloux"; });
    }

    public void Update()
    {

    }

    /**
     * Clic sur le bouton de param�tres
    */
    public void Parametres()
    {
        ToggleParametres(false);
    }

    public void EnregistrerTailleLabirynthe()
    {
        // Enregistrer la nouvelle taille du labyrinthe et le nombre de cles puis retour au menu.
        tailleLabirynthe = GetTailleLabirynthe();
        nbCles = GetNbCles();
        nbCailloux = GetNbCailloux();
        ToggleParametres(true);
    }

    private void ToggleParametres(bool menuParametres)
    {
        playSound();
        boutonJouer.gameObject.SetActive(menuParametres);
        boutonParametres.gameObject.SetActive(menuParametres);
        boutonQuitter.gameObject.SetActive(menuParametres);
        boutonEnregistrer.gameObject.SetActive(!menuParametres);
        texteTailleLabirynthe.gameObject.SetActive(!menuParametres);
        sliderTailleLabirynthe.gameObject.SetActive(!menuParametres);
        tailleLabiryntheTexte.gameObject.SetActive(!menuParametres);
        texteNbCles.gameObject.SetActive(!menuParametres);
        nbClesAffiche.gameObject.SetActive(!menuParametres);
        sliderNbCles.gameObject.SetActive(!menuParametres);
        sliderNbCailloux.gameObject.SetActive(!menuParametres);
        nbCaillouxAffiche.gameObject.SetActive(!menuParametres);
        texteNbCailloux.gameObject.SetActive(!menuParametres);
    }
    /**
     * Clic sur le bouton de quitter
    */
    public void Quitter()
    {
        playSound();
        #if UNITY_EDITOR
            // Code sp�cifique � l'�diteur Unity (par exemple, arr�ter le mode de lecture dans l'�diteur).
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // Code sp�cifique � une version autonome (par exemple, build pour Windows, Mac, Linux).
            Application.Quit();
        #endif
    }

    private void playSound()
    {
        if (buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }
}
