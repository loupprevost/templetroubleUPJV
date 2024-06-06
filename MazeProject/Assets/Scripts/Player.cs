using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    public float moveSpeed = 2.0f;

    [SerializeField]
    private CharacterController characterController;

    private int keysCollected;

    private int totalKeys;

    private bool sprint;
    private bool crouched;
    public MazeUIManager mazeUiManager;

    [SerializeField]
    public int[] items = {0, 0, 0};

    public int nbCailloux = 10;
    public int hammerKey = 0;
    public int caillouKey = 1;
    public int plancheKey = 2;

    // Paramètres pour l'effet de mouvement de la tête
    private float bobbingSpeed = 14;
    private float bobbingAmount = 0.05f;
    private float midpoint = 0.6f;

    private float timer = 0.0f;

    // Différents sons
    public AudioClip collectKeySound;
    public AudioClip footstepsSound1;
    public AudioClip footstepsSound2;
    public bool footstepFirst = true;
    private float timeSinceLastFootstep = 0f;
    public float footstepDelay = 0.5f;

    private AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        items[caillouKey] = nbCailloux;
        if(characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }

        if (transform.parent != null)
        {
            Maze maze = transform.parent.GetComponent<Maze>();
            if (maze != null)
            {
                totalKeys = maze.getNumberOfKeys();
                keysCollected = 0;
            }
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // Si l'AudioSource n'est pas déjà attaché, ajoutez-le
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        mazeUiManager = FindObjectOfType<MazeUIManager>();
        mazeUiManager.setItems(items);
        mazeUiManager.UpdateKeyCount(keysCollected, totalKeys);
    }

    // Update is called once per frame
    void Update()
    {
        // Direction de la cam�ra
        Vector3 cameraForward = Camera.main.transform.forward;

        // Suppression du Y pour rester � l'horizontal
        cameraForward.y = 0f;
        cameraForward.Normalize();

        // Player movement
        Vector3 movement = Vector3.zero;

        // Clic gauche ou E pour utiliser l'objet
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E))
        {
            // Récupèration de la position de la caméra
            Vector3 cameraPosition = Camera.main.transform.position;

            // Récupèration de la direction de la caméra
            Vector3 cameraDir = Camera.main.transform.forward;

            // Récupèration des informations sur l'objet touché
            RaycastHit hit;

            // Longueur maximale du rayon lancé
            float maxRaycastDistance = 1.5f;

            // Lance un rayon depuis la position de la caméra dans la direction de la caméra
            if (Physics.Raycast(cameraPosition, cameraDir, out hit, maxRaycastDistance))
            {
                mazeUiManager.useItem(hit.collider.gameObject, items);
            }
        }

        // Touche CTRL ou C pour s'accroupir et diviser la vitesse de déplacement par 2
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.C)) && !crouched)
        {
            moveSpeed = 1.0f;
            crouched = true;
            midpoint = 0.2f;
            movement.y = -0.2f;
        } else if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl) && !Input.GetKey(KeyCode.C) && crouched)
        {
            if (!IsUnderHalfWall())
            {
                midpoint = 0.6f;
                moveSpeed = 2.0f;
                movement.y = 0.2f;
                crouched = false;
            }
        }

        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            movement += cameraForward;
        }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            movement -= cameraForward;
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            movement -= Camera.main.transform.right;
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            movement += Camera.main.transform.right;
        }

        //Touche shift pour courir et doubler la vitesse de déplacement
        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && !sprint)
        {
            moveSpeed = 3.0f;
            footstepDelay = 0.35f;
            sprint = true;
        } else if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift) && sprint) {
            moveSpeed = 2.0f;
            footstepDelay = 0.5f;
            sprint = false;
        }

        // Scroll de la souris pour changer d'objet
        if (Input.GetAxis("Mouse ScrollWheel") != 0f) {
            ChangeItem(Input.GetAxis("Mouse ScrollWheel"));
        }

        // Touche 1, 2 ou 3 pour changer d'objet
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            mazeUiManager.setItem(1);
        } else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            mazeUiManager.setItem(2);
        } else if (Input.GetKeyDown(KeyCode.Alpha3)) {
            mazeUiManager.setItem(3);
        }

        // Normalize the movement vector to avoid faster diagonal movement
        movement.Normalize();

        // Apply the movement to the character controller
        characterController.Move(movement * moveSpeed * Time.deltaTime);

        // Effet de mouvement de la tête
        HeadBobbing();
    }

    private bool IsUnderHalfWall()
    {
        RaycastHit hit;
        float rayLength = 1.0f;

        // Lance un rayon vers le haut depuis la position du joueur
        if (Physics.Raycast(transform.position, Vector3.up, out hit, rayLength))
        {
            // Vérifie si l'objet touché a un tag spécifique indiquant un demi-mur
            if (hit.collider.CompareTag("DemiMur"))
            {
                return true;
            }
        }

        return false;
    }

    private void ChangeItem(float scroll) {
        if (scroll > 0f) {
            //Scroll up
            mazeUiManager.ChangeItem(1);
        } else {
            //Scroll down
            mazeUiManager.ChangeItem(-1);
        }
    }
    void HeadBobbing()
    {
        if (Mathf.Abs(characterController.velocity.x) > 0.1f || Mathf.Abs(characterController.velocity.z) > 0.1f)
        {
            // Si le joueur est en mouvement
            float waveSlice = Mathf.Sin(timer);
            timer += Time.deltaTime * bobbingSpeed;

            if (timer > Mathf.PI * 2)
            {
                timer = (timer + bobbingSpeed) % (Mathf.PI * 2);
            }

            if (waveSlice != 0)
            {
                transform.localPosition = new Vector3(transform.localPosition.x, midpoint + Mathf.Sin(timer) * bobbingAmount, transform.localPosition.z);
            }
            if (Time.time - timeSinceLastFootstep > footstepDelay)
            {
                footstepSound();
                timeSinceLastFootstep = Time.time;
            }

        }
        else
        {
            // Si le joueur est immobile, ramener la tête à la position d'origine
            transform.localPosition = new Vector3(transform.localPosition.x, midpoint, transform.localPosition.z);
            timer = 0.0f;
        }
    }

    private void footstepSound() {
        if (footstepFirst)
        {
            audioSource.PlayOneShot(footstepsSound1);
            footstepFirst = false;
        }
        else
        {
            audioSource.PlayOneShot(footstepsSound2);
            footstepFirst = true;
        }
    }
    public void CollectKey()
    {
        keysCollected += 1;
        mazeUiManager.UpdateKeyCount(keysCollected, totalKeys);
        if (collectKeySound != null)
        {
            audioSource.PlayOneShot(collectKeySound);
        }
        //Si le joueur a toute les cl�s on met � jour l'UI et on active la porte de sortie
        if (keysCollected == totalKeys)
        {
            GameObject[] sas = GameObject.FindGameObjectsWithTag("SAS");
            sas[1].GetComponentInChildren<Porte>().GetComponent<Collider>().isTrigger = true;
            mazeUiManager.UpdateObjective();
        }
    }

    public bool collectItem(GameObject item) {
        int index = 0;
        if (item.GetComponent<Hammer>() != null) {
            index = hammerKey;
        } else if (item.GetComponent<Planche>() != null) {
            index = plancheKey;
        }

        items[index]++;
        if (collectKeySound != null)
        {
            audioSource.PlayOneShot(collectKeySound);
        }
        mazeUiManager.collectItem(item, items[index]);
        return true;
    }

    public void setNbCailloux(int nb) {
        nbCailloux = nb;
    }
}
