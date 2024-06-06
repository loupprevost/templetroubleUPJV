using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torch : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip torchOnSound;
    private Transform playerTransform;
    private float maxDistance = 3f;
    private float volumePrincipal = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.Log("Cannot find Player");
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // Si l'AudioSource n'est pas déjà attaché, ajoutez-le
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.clip = torchOnSound;
        audioSource.loop = true;
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(playerTransform.position, transform.position);
        float volume = (1f - Mathf.Clamp01(distance / maxDistance)) * volumePrincipal;
        audioSource.volume = volume;
    }
}
