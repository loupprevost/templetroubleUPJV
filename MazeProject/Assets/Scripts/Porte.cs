using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Porte : MonoBehaviour
{
    private MazeUIManager uiManager;

    public AudioClip doorOpenSound;

    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        uiManager = FindObjectOfType<MazeUIManager>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // Si l'AudioSource n'est pas déjà attaché, ajoutez-le
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Porte");
        if (uiManager != null && other.CompareTag("Player"))
        {
            if (doorOpenSound != null) {
                audioSource.PlayOneShot(doorOpenSound);
            }
            this.GetComponent<MeshRenderer>().enabled = false;
            uiManager.ShowWin();
        }
    }
}
