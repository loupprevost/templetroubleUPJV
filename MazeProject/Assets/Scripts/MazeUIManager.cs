using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;

public class MazeUIManager : MonoBehaviour
{
    [SerializeField]
    private Text keyCountText;

    [SerializeField]
    private Text objective;

    [SerializeField]
    private Text center;

    [SerializeField]
    private RawImage selectedImage;

    [SerializeField]
    public AudioClip winSound;

    [SerializeField]
    public AudioClip marteauSound;

    [SerializeField]
    public AudioClip murSound;

    [SerializeField]
    public AudioClip caillouSound;

    [SerializeField]
    public AudioClip plancheSound;

    [SerializeField]
    public AudioClip BoutonSound;
    private AudioSource audioSource;
    private int selectedItem = 1;

    private Transform canvasTransform;

    [SerializeField]
    private GameObject marteauImg;

    [SerializeField]
    private GameObject marteauNb;

    [SerializeField]
    private GameObject caillouImg;

    [SerializeField]
    private GameObject caillouNb;
    [SerializeField]
    private GameObject plancheImg;

    [SerializeField]
    private GameObject plancheNb;
    [SerializeField]
    private Caillou caillou;

    [SerializeField]
    private Plancher plancher;

    public Text pauseText;

    public Button boutonMenuPrincipal;
    public Button boutonQuitter;
    private bool isPaused = false;

    // Start is called before the first frame update
    void Start()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            canvasTransform = canvas.GetComponent<Transform>();
        }
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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    public void UpdateKeyCount(int keysCollected, int totalKeys)
    {
        if (keyCountText != null)
        {
            keyCountText.text = "Cles obtenues : " + keysCollected + "/" + totalKeys;
        }
    }

    public void UpdateObjective()
    {
        if (objective != null)
        {
            objective.text = "Objectif : Trouvez la sortie !";
        }
    }

    public void ShowWin()
    {
        center.gameObject.SetActive(true);
        StartCoroutine(ClearWinAndQuit());

    }

    private IEnumerator ClearWinAndQuit()
    {
        yield return new WaitForSeconds(2f);
        audioSource.PlayOneShot(winSound);
        center.gameObject.SetActive(false);
        SceneManager.LoadScene("MenuScene");
    }

    public int getItem() {
        return selectedItem;
    }
    public void setItem(int item) {
        if (item != selectedItem) {
            selectedItem = item;
            moveSelectedItem();
        }
    }
    public void moveSelectedItem() {
        Vector3 newVector = new Vector3(0, selectedImage.rectTransform.localPosition.y, selectedImage.rectTransform.localPosition.z);
        switch (selectedItem) {
            case 1:
                newVector.x = -69.2f; 
                break;
            case 2:
                newVector.x = 0f;
                break;
            case 3:
                newVector.x = 68.8f;
                break;
        }
        selectedImage.rectTransform.localPosition = newVector;
    }
    public void ChangeItem(int scroll) {
        selectedItem += scroll;
        if (selectedItem > 3) {
            selectedItem = 1;
        }
        if (selectedItem < 1) {
            selectedItem = 3;
        }
        moveSelectedItem();
    }

    public void collectItem(GameObject item, int number)
    {
        if (item.GetComponent<Hammer>() != null)
        {
            marteauNb.GetComponent<Text>().text = number.ToString();
        } else if (item.GetComponent<Planche>() != null)
        {
            plancheNb.GetComponent<Text>().text = number.ToString();
        }
    }

    public void useItem(GameObject hitObject, int[] itemsNb)
    {
        int index = selectedItem - 1;
        if (index == 0)
        {
            audioSource.PlayOneShot(marteauSound);
            if (itemsNb[index] > 0 && System.Array.IndexOf(Bloc.murNames, hitObject.name) > -1)
            {
                GameObject bloc = hitObject.transform.parent.gameObject;
                string orientation = hitObject.name.Split("Mur")[1];
                GameObject murCasse = bloc.GetComponent<Bloc>().getMureCasse(orientation);
                murCasse.SetActive(true);
                
                // Détruisez l'ancien objet
                hitObject.SetActive(false);

                // Récupèration de la position de la caméra
                Vector3 cameraPosition = Camera.main.transform.position;

                // Récupèration de la direction de la caméra
                Vector3 cameraDir = Camera.main.transform.forward;

                // Récupèration des informations sur l'objet touché
                RaycastHit hit;

                // Longueur maximale du rayon lancé
                float maxRaycastDistance = 1.5f;

                // Lance un rayon depuis la position de la caméra dans la direction de la caméra
                // Si il y a bien un autre objet derrière le mur cassé on le casse bien
                // Sinon on le remet car on est au bord du labyrinthe
                if (Physics.Raycast(cameraPosition, cameraDir, out hit, maxRaycastDistance)) {
                    hit.collider.gameObject.SetActive(false);
                    string orientation2 = hit.collider.gameObject.name.Split("Mur")[1];
                    GameObject murCasse2 = hit.collider.gameObject.transform.parent.gameObject.GetComponent<Bloc>().getMureCasse(orientation2);
                    murCasse2.SetActive(true);
                } else {
                    hitObject.SetActive(true);
                    return;
                }
                audioSource.PlayOneShot(murSound);
                itemsNb[index]--;
                marteauNb.GetComponent<Text>().text = itemsNb[index].ToString();
            }
        } else if (index == 1) {
            // Lâcher de caillou là où le joueur est
            if (itemsNb[index] > 0) {
                Vector3 cameraPosition = Camera.main.transform.position;
                Vector3 cameraDir = Camera.main.transform.forward;
                RaycastHit hit;
                float maxRaycastDistance = 1.5f;
                if (Physics.Raycast(cameraPosition, cameraDir, out hit, maxRaycastDistance)) {
                    Vector3 position = hit.point;
                    position.y = 0.05f;
                    Caillou caillouInst = Instantiate(caillou, position, Quaternion.identity);
                    if (audioSource != null) {
                        audioSource.PlayOneShot(caillouSound);
                    }
                    caillouInst.transform.SetParent(hit.collider.gameObject.transform);

                    itemsNb[index]--;
                    caillouNb.GetComponent<Text>().text = itemsNb[index].ToString();
                }
            }
        } else if (index == 2) {
            bool meshRendererEnabled = hitObject.GetComponent<MeshRenderer>().enabled;
            if (itemsNb[index] > 0 && hitObject.name == "Sol" && !meshRendererEnabled && hitObject.transform.childCount == 0) {
                Plancher plancherInst = Instantiate(plancher, new Vector3(hitObject.transform.position.x + 0.45f, hitObject.transform.position.y + 0.2f, hitObject.transform.position.z + -0.05f), Quaternion.identity);
                plancherInst.transform.SetParent(hitObject.transform);
                BoxCollider collider = hitObject.GetComponent<BoxCollider>();
                collider.size = new Vector3(collider.size.x, 1, collider.size.z);
                if (audioSource != null) {
                    audioSource.PlayOneShot(plancheSound);
                }
                itemsNb[index]--;
                plancheNb.GetComponent<Text>().text = itemsNb[index].ToString();
            }
        }
    }

    public void setItems(int[] itemsNb)
    {
        marteauNb.GetComponent<Text>().text = itemsNb[0].ToString();
        caillouNb.GetComponent<Text>().text = itemsNb[1].ToString();
        plancheNb.GetComponent<Text>().text = itemsNb[2].ToString();
    }

    public void TogglePauseMenu()
    {
        isPaused = !isPaused;
        pauseText.gameObject.SetActive(isPaused);
        boutonMenuPrincipal.gameObject.SetActive(isPaused);
        boutonQuitter.gameObject.SetActive(isPaused);

        if (isPaused)
        {
            Time.timeScale = 0f; // Mettre en pause le temps
            Cursor.lockState = CursorLockMode.None; // Déverrouiller le curseur
            Cursor.visible = true; // Rendre le curseur visible
        }
        else
        {
            Time.timeScale = 1f; // Reprise du temps
            Cursor.lockState = CursorLockMode.Locked; // Verrouiller le curseur
            Cursor.visible = false; // Rendre le curseur invisible
        }
    }

    public void MenuPrincipal()
    {
        if (audioSource != null) {
            audioSource.PlayOneShot(BoutonSound);
        }
        
        SceneManager.LoadScene("MenuScene");
    }

    public void Quitter()
    {
        if (audioSource != null) {
            audioSource.PlayOneShot(BoutonSound);
        }
        #if UNITY_EDITOR
            // Code sp�cifique � l'�diteur Unity (par exemple, arr�ter le mode de lecture dans l'�diteur).
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // Code sp�cifique � une version autonome (par exemple, build pour Windows, Mac, Linux).
            Application.Quit();
        #endif
    }
}
