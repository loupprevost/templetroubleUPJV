using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float sensibilite = 2.0f;

    public float lissage = 2.0f;

    private Vector3 lissageSouris = Vector3.zero;
    private Vector3 observation = Vector3.zero;

    [SerializeField]
    private GameObject player;

    private bool isPaused = false;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        transform.position = player.transform.position;
        transform.LookAt(player.transform);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
        }

        if (!isPaused){
            // Entr�e de souris
            Vector3 mouseChange = new Vector3(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"), 0);

            // Sensibilit�
            mouseChange = Vector3.Scale(mouseChange, new Vector3(sensibilite * lissage, sensibilite * lissage, 0));
            lissageSouris.x = Mathf.Lerp(lissageSouris.x, mouseChange.x, 1f / lissage);
            lissageSouris.y = Mathf.Lerp(lissageSouris.y, mouseChange.y, 1f / lissage);

            observation += lissageSouris;

            // Rotation cam�ra
            observation.y = Mathf.Clamp(observation.y, -90f, 90f);
            transform.localRotation = Quaternion.AngleAxis(-observation.y, Vector3.right);
            player.transform.localRotation = Quaternion.AngleAxis(observation.x, player.transform.up);
        }
        
    }
}
