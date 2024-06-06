using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Maze : MonoBehaviour
{
    [SerializeField]
    private Bloc bloc; //pour appeler la preFab bloc

    [SerializeField]
    private Torch torch; //pour appeler la preFab torch

    [SerializeField]
    private Key key; //pour appeler la preFab key

    [SerializeField]
    private Hammer hammer; //pour appeler la preFab hammer

    [SerializeField]
    private Planche planche; //pour appeler la preFab planche

    [SerializeField]
    private HalfWall halfWall; //pour appeler la preFab halfWall

    [SerializeField]
    private SAS sas;

    [SerializeField]
    public Player joueur;

    private Bloc entree;

    private int xEntree;

    private Bloc sortie;

    private int xSortie;

    System.Random random = new();

    private int taille = 20;

    private int numberOfKeys = 3;
    private int numberOfCailloux = 10;

    private Bloc[,] grille;

    private int[,] grillePathNumber;

    // Sons
    public AudioClip backgroundMusic;

    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // Si l'AudioSource n'est pas déjà attaché, ajoutez-le
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.PlayOneShot(backgroundMusic, 0.2f);

        // Assurez-vous que l'audio est configuré pour être en boucle
        audioSource.clip = backgroundMusic;
        audioSource.loop = true;
        audioSource.volume = 0.2f;
        audioSource.Play();

        taille = PlayerPrefs.GetInt("tailleLabirynthe", 20);
        numberOfKeys = PlayerPrefs.GetInt("nbCles", 3);
        numberOfCailloux = PlayerPrefs.GetInt("nbCailloux", 10);
        //On cr�e la grille et on la remplie de la prefab Bloc
        grille = new Bloc[taille, taille];
        for (int x = 0; x < taille; ++x)
        {
            for (int z = 0; z < taille; ++z)
            {
                Bloc newBloc = Instantiate(bloc, new Vector3(x*2, (float)-1.6, z*2), Quaternion.identity);
                grille[x, z] = newBloc;
                grille[x, z].transform.SetParent(this.transform);
            }
        }

        //On choisit un bloc de d�part
        int startX = random.Next(taille);
        int startZ = random.Next(taille);

        generationMaze(startX, startZ, 0);
        addEntreeSortie();

        // Grille contenant les valeur de pas (bloc d'entrée=0, bloc de sortie=nombreMinimumDePasPourFinirLabyrinthe)
        grillePathNumber = new int[taille,taille];

        mazePathNumber(xEntree,0,1);
        removeTorchEntreeSortie();
        addPlayer();
        addHoles();
        addHalfWalls();
        addKeys();
        addHammers();


        affMazePathNumber();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*
     * M�thode r�cursive, permet de parcourir la grille en profondeur
     * Dans un bloc qu'on explore, on regarde si l'un des 4 blocs adjacents est libre
     * Si oui, on l'explore � son tour
     * Sinon, on revient au bloc d'avant et on cherche une autre direction
     * Au final, on sera revenu au premier bloc avec tous les blocs d'explor�
     */
    public void generationMaze(int x, int z, int cpt)
    {
        grille[x, z].Explore();
        int[] directions = { 0, 1, 2, 3 };
        ShuffleArray(directions); // On m�lange les directions aux hasard

        foreach (int direction in directions)
        {
            int nextX = x;
            int nextZ = z;

            switch (direction)
            {
                case 0: nextZ = z + 1; 
                        break; // Nord
                case 1: nextX = x - 1; 
                        break; // Ouest
                case 2: nextZ = z - 1; 
                        break; // Sud
                case 3: nextX = x + 1; 
                        break; // Est
            }

            // Si on est bien dans la grille
            if (nextX >= 0 && nextX < taille && nextZ >= 0 && nextZ < taille)
            {
                // Si le bloc suivant n'a pas encore �t� explor�
                if(!grille[nextX, nextZ].getExplore())
                {
                    hideWalls(direction, x, nextX, z, nextZ);
                    generationMaze(nextX, nextZ, cpt+1);
                }
            }
        }

        if (cpt % 8 == 0)
        {
            //Ajout des torches
            generationTorchInMaze(x, z, directions);
        }
    }

    public void affMazePathNumber()
    {
        for(int y=0;y<taille;y++)
        {
            string result = "";
            for(int x=0;x<taille;x++)
            {
                result += "|" + grillePathNumber[x,y];
                int espaces = 3 - grillePathNumber[x,y].ToString().Length;
                result += "|";
                if(espaces > 0)
                {
                    for(int u = 0;u<espaces;u++)
                    {
                        result += " ";
                    }
                }
            }
        }   
    }

    public void mazePathNumber(int x, int y, int number)
    {
        grillePathNumber[x,y] = number;

        // S'il n'y a pas de mur nord
        if(!grille[x,y].murNordIsActive())
        {
            if(y+1<taille && y+1 >= 0)
            {
                if(grillePathNumber[x,y+1] == 0)
                {
                    mazePathNumber(x,y+1,number+1);
                }
            }
        }
        // S'il n'y a pas de mur est
        if(!grille[x,y].murEstIsActive())
        {
            if(x+1<taille && x+1 >= 0)
            {
                if(grillePathNumber[x+1,y] == 0)
                {
                    mazePathNumber(x+1,y,number+1);
                }
            }
        }
        // S'il n'y a pas de mur sud
        if(!grille[x,y].murSudIsActive())
        {
            if(y-1<taille && y-1 >= 0)
            {
                if(grillePathNumber[x,y-1] == 0)
                {
                    mazePathNumber(x,y-1,number+1);
                }
            }
        }
        // S'il n'y a pas de mur ouest
        if(!grille[x,y].murOuestIsActive())
        {
            if(x-1<taille && x-1 >= 0)
            {
                if(grillePathNumber[x-1,y] == 0)
                {
                    mazePathNumber(x-1,y,number+1);
                }
            }
        }
    }

    //Fonction d'ajout des torches
    public void generationTorchInMaze(int x, int z, int[] directions)
    {
        Torch newTorch = null;
        bool isTorched = false;
        
        foreach (int direction in  directions)
        {
            switch (direction)
            {
                case 0:
                    if (grille[x, z].murNordIsActive()){
                        newTorch = Instantiate(torch, new Vector3((x * 2) - (float)0.5, (float)1.533, (z * 2)+(float)0.9), Quaternion.Euler(0, 180, 0));
                        isTorched = true;
                    }
                    break; // Nord
                case 1:
                    if (grille[x, z].murOuestIsActive()){
                        newTorch = Instantiate(torch, new Vector3((x * 2) - (float)1.4, (float)1.533, (z * 2)), Quaternion.Euler(0, 90, 0));
                        isTorched = true;
                    }
                    break; // Ouest
                case 2:
                    if (grille[x, z].murSudIsActive()){
                        newTorch = Instantiate(torch, new Vector3((x * 2) - (float)0.5, (float)1.533, (z * 2)-(float)0.9), Quaternion.Euler(0, 0, 0));
                        isTorched = true;
                    }
                    break; // Sud
                case 3:
                    if (grille[x, z].murEstIsActive()){
                        newTorch = Instantiate(torch, new Vector3((x * 2) + (float)0.4, (float)1.533, (z * 2)), Quaternion.Euler(0, -90, 0));
                        isTorched = true;
                    }
                    break; // Est
            }
            if (isTorched)
            {
                newTorch.transform.SetParent(grille[x, z].transform);
                break;
            }
        }
    }

    
    //M�lange au hasard les �l�ments un tableau d'entiers
    public void ShuffleArray(int[] array)
    {
        for (int i = 0; i < 4; i++)
        {
            int j = i + random.Next(4 - i);
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }

    //Cacher deux murs (celui du bloc d'avant et l'actuel)
    public void hideWalls(int direction, int x, int nextX, int z, int nextZ)
    {
        switch (direction)
        {
            case 0:
                grille[x, z].RemoveMurNord();
                grille[nextX, nextZ].RemoveMurSud();
                break;
            case 1:
                grille[x, z].RemoveMurOuest();
                grille[nextX, nextZ].RemoveMurEst();
                break;
            case 2:
                grille[x, z].RemoveMurSud();
                grille[nextX, nextZ].RemoveMurNord();
                break;
            case 3:
                grille[x, z].RemoveMurEst();
                grille[nextX, nextZ].RemoveMurOuest();
                break;
        }
    }

    //Cr�ation et placement d'un bloc avec sp�cification des murs souhait�s
    public void blocWithSpecificWalls(int x, int z, bool nord, bool est, bool sud, bool ouest)
    {
        Bloc instBloc = Instantiate(bloc, new Vector3(x, -1, z), Quaternion.identity);
        instBloc.setWall(nord, est, sud, ouest);
        instBloc.setCorner(false, false, false, false);
    }


    //Ajouter une entr�e et une sortie
    public void addEntreeSortie()
    {
        // Placement de l'entr�e
        xEntree = random.Next(1,taille - 2);
        entree = grille[xEntree, 0];
        entree.RemoveMurSud();

        // Placement de la sortie
        xSortie= random.Next(1,taille - 2);
        sortie = grille[xSortie, taille - 1];
        sortie.RemoveMurNord();

        //Cr�ation des SAS d'entr�e et de sortie
        SAS instEntreeSAS = Instantiate(sas, new Vector3((xEntree*2)+2, (float)-1.6, -4), Quaternion.identity); //Entr�e
        SAS instSortieSAS = Instantiate(sas, new Vector3((xSortie*2)-3, (float)-1.6, (taille*2)+2), Quaternion.Euler(0, 180, 0)); //Sortie

        //Association des SAS au maze
        instEntreeSAS.transform.SetParent(this.transform);
        instSortieSAS.transform.SetParent(this.transform);

        //On ajoute un tag au SAS de sortie (utile pour g�rer sa porte plus tard)
        //instSortieSAS.gameObject.tag = "SASS";

        //On d�sactive la porte du SAS d'entr�e
        instEntreeSAS.GetComponentInChildren<Porte>().gameObject.SetActive(false);

    }

    //On recherche si des torches existent � l'entr�e et sortie, si oui on les rend invisibles.
    public void removeTorchEntreeSortie()
    {
        Torch torcheE = entree.GetComponentInChildren<Torch>();
        Torch torcheS = sortie.GetComponentInChildren<Torch>();
        if (torcheE != null) torcheE.gameObject.SetActive(false);
        if (torcheS != null) torcheS.gameObject.SetActive(false);
    }

    public void addPlayer()
    {
        Player j = Instantiate(joueur, new Vector3((xEntree * 2) - (float)0.5, (float)0.6, -4), Quaternion.identity);
        j.setNbCailloux(numberOfCailloux);
        j.transform.SetParent(this.transform);
    }

    public int getNumberOfKeys()
    {
        return this.numberOfKeys;
    }

    public void addHoles()
    {
        int holeX, holeZ;

        // Nombre de demi-murs -> taille / 8 (entier le plus proche)
        int numberOfHolesInMap = (int)Mathf.RoundToInt(taille / 4);
        bool positionOK = false;

        holeX = random.Next(0, taille - 1);
        holeZ = random.Next(0, taille - 1);

        // Placement des trous
        for (int i = 0; i < numberOfHolesInMap; i++)
        {
            positionOK = false;

            // On cherche une nouvelle position pour le trou
            while (!positionOK)
            {
                if(grille[holeX,holeZ].solIsActive() && holeX != xEntree && holeX != xSortie)
                {
                    Bloc blocConcerne = grille[holeX,holeZ];
                    blocConcerne.RemoveSol();
                    addPlanche(grillePathNumber[holeX,holeZ]);
                    positionOK = true;
                }
                else
                {
                    holeX = random.Next(0, taille - 1);
                    holeZ = random.Next(0, taille - 1);
                }
            }
        }
    }

    // Apparition d'une planche
    public void addPlanche(int indiceBloc)
    {
        int plancheX, plancheZ;

        int numberOfPlancheForHole = 3;

        plancheX = random.Next(0, taille - 1);
        plancheZ = random.Next(0, taille - 1);

        for(int i=0;i<numberOfPlancheForHole;i++)
        {
            // On cherche une position pour la clé
            while (grillePathNumber[plancheX,plancheZ] >= indiceBloc || !grille[plancheX,plancheZ].solIsActive() || grille[plancheX,plancheZ].GetComponentInChildren<Planche>() || grille[plancheX,plancheZ].GetComponentInChildren<Key>() || grille[plancheX,plancheZ].GetComponentInChildren<Hammer>() || (plancheX == xEntree && plancheZ == 0) || (plancheX == xSortie && (plancheZ == taille-1)))
            {
                plancheX = random.Next(0, taille - 1);
                plancheZ = random.Next(0, taille - 1);
            }

            Quaternion rotation = Quaternion.Euler(0, random.Next(0, 360), 0);     
            Planche instPlanche = Instantiate(planche, new Vector3((plancheX * 2)- (float)0.5, (float)0.0, (plancheZ * 2)), rotation);

            instPlanche.transform.SetParent(grille[plancheX,plancheZ].transform);
        }
    }

    // Apparition des demi-murs
    public void addHalfWalls()
    {
        int halfWallX, halfWallZ;

        // Nombre de demi-murs -> taille / 8 (entier le plus proche)
        int numberOfHalfWallsInMap = (int)Mathf.RoundToInt(taille / 3);
        bool positionOK = false;

        halfWallX = random.Next(0, taille - 1);
        halfWallZ = random.Next(0, taille - 1);

        // Placement des demi-murs
        for (int i = 0; i < numberOfHalfWallsInMap; i++)
        {
            string corridor = null;
            positionOK = false;
            Quaternion rotation;

            // On cherche une nouvelle position pour le demi-mur
            while (!positionOK)
            {
                if(!grille[halfWallX,halfWallZ].GetComponentInChildren<HalfWall>() && halfWallX != xEntree && halfWallX != xSortie)
                {
                    Bloc blocConcerne = grille[halfWallX,halfWallZ];
                    corridor = blocConcerne.corridor();
                    if(corridor != null)
                    {
                        positionOK = true;
                    }
                    else
                    {
                        halfWallX = random.Next(0, taille - 1);
                        halfWallZ = random.Next(0, taille - 1);
                    }
                }
                else
                {
                    halfWallX = random.Next(0, taille - 1);
                    halfWallZ = random.Next(0, taille - 1);
                }
            }

            if(corridor == "NS")    
            {
                rotation = Quaternion.Euler(90, 0, 180);
            } 
            else 
            {
                rotation = Quaternion.Euler(90, 90, 0);
            }

            HalfWall instHalfWall = Instantiate(halfWall, new Vector3((halfWallX * 2) - 0.5f, 1.4f, (halfWallZ * 2)), rotation);
            instHalfWall.transform.SetParent(grille[halfWallX,halfWallZ].transform);

            //On rend invisible la torche si elle existe dans le bloc où se situe le demi-mur
            Torch torche = grille[halfWallX, halfWallZ].GetComponentInChildren<Torch>();
            if (torche != null) torche.gameObject.SetActive(false);
        }
    }

    public void addKeys()
    {
        int keyX, keyZ;

        // Seulement 3 quarts des clés nécessaire pour ouvrir la porte
        int numberOfKeysInMap = (int)Mathf.RoundToInt(numberOfKeys + (numberOfKeys/3));

        keyX = random.Next(0, taille - 1);
        keyZ = random.Next(0, taille - 1);

        // Placement des clés
        for (int i = 0; i < numberOfKeysInMap; i++)
        {
            // On cherche une position pour la clé
            while (grille[keyX,keyZ].GetComponentInChildren<Planche>() || grille[keyX,keyZ].GetComponentInChildren<Key>() || grille[keyX,keyZ].GetComponentInChildren<Hammer>() || keyX == xEntree || keyX == xSortie)
            {
                keyX = random.Next(0, taille - 1);
                keyZ = random.Next(0, taille - 1);
            }

            Quaternion rotation = Quaternion.Euler(0, random.Next(0, 360), 0);     
            Key instKey = Instantiate(key, new Vector3((keyX * 2)- (float)0.5, (float)0.0, (keyZ * 2)), rotation);

            instKey.transform.SetParent(grille[keyX,keyZ].transform);
        }
    }

    // Apparition des marteaux
    public void addHammers()
    {
        int hammerX, hammerZ;

        // Nombre de marteaux -> taille / 10 (entier le plus proche)
        int numberOfHammersInMap = (int)Mathf.RoundToInt(taille / 3);

        hammerX = random.Next(0, taille - 1);
        hammerZ = random.Next(0, taille - 1);

        // Placement des marteaux
        for (int i = 0; i < numberOfHammersInMap; i++)
        {
            // On cherche une nouvelle position pour le marteau
            while (grille[hammerX,hammerZ].GetComponentInChildren<Planche>() || grille[hammerX,hammerZ].GetComponentInChildren<Key>() || grille[hammerX,hammerZ].GetComponentInChildren<Hammer>() || hammerX == xEntree || hammerX == xSortie)
            {
                hammerX = random.Next(0, taille - 1);
                hammerZ = random.Next(0, taille - 1);
            }

            Quaternion rotation = Quaternion.Euler(0, random.Next(0, 360), 90); 
            Hammer instHammer = Instantiate(hammer, new Vector3((hammerX * 2) - 0.5f, 0.02f, (hammerZ * 2)), rotation);

            instHammer.transform.SetParent(grille[hammerX,hammerZ].transform);
        }
    }
}
