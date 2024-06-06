using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    [SerializeField]
    private GameObject expectedObject = null;

    [SerializeField]
    private string playerTag = "Player";

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (expectedObject != null && other.CompareTag(playerTag))
        {
            gameObject.SetActive(false);

            Player playerScript = other.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.CollectKey();
            }
        }
    }
}
