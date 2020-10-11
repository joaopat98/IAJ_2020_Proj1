using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.IAJ.Unity.Movement.Arbitration;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.IAJ.Unity.Util;
using Random = UnityEngine.Random;
using System.Security.Principal;
using Assets.Scripts.IAJ.Unity.Movement.DynamicMovement;
using Boo.Lang.Environments;
using System.Runtime.CompilerServices;
using System;

public class SceneManager : MonoBehaviour
{
    public KeyCode stopKey = KeyCode.S;
    public KeyCode priorityKey = KeyCode.P;
    public KeyCode blendedKey = KeyCode.B;

    public int MAX_CHARACTERS;
    
    public List<GameObject> mainCharacterPrefabs;
    public Text movementText;
    public Slider characterSlider;
    
    public float SpawnTimer;   
    public GameObject SpawnPoints;

    private List<Transform> spawnPoints;
    private List<MainCharacterController> characterControllers;
    private GameObject[] obstacles;
    private List<DynamicCharacter> characters;
    private Text sliderText;
    private int spawnIndex = 0;
    private string CURRENT_MOVEMENT;
    private int NumberOfCharactersSpawning;

    // Use this for initialization
    void Start () 
	{
    
        //Initializing our list variables
        characterControllers = new List<MainCharacterController>();
        characters = new List<DynamicCharacter>();
        sliderText = characterSlider.GetComponentInChildren<Text>();
        
        // Getting the spawnPoints:
        spawnPoints = new List<Transform>();
        for( int i = 0; i< SpawnPoints.transform.childCount; i++ )
        {
            spawnPoints.Add(SpawnPoints.transform.GetChild(i));
        }

        // Retrieving all objects with the tag: Obstacle
        this.obstacles = GameObject.FindGameObjectsWithTag("Obstacle");

        // Starting the Spawn 
        InvokeRepeating("SpawnCharacters", 0.0f, SpawnTimer);

        var textObj = GameObject.Find("InstructionsText");
        if (textObj != null)
        {
            textObj.GetComponent<Text>().text =
                "Instructions\n\n" +
                this.blendedKey + " - Blended\n" +
                this.priorityKey + " - Priority\n" +
                this.stopKey + " - Stop";
        }

        this.NumberOfCharacerSliderChanged();
    }

    public void Update()
    {

        // Handling with User Input       
        if (Input.GetKeyDown(this.stopKey))
        {
            CURRENT_MOVEMENT = "Stop";
            foreach (var c in this.characterControllers)
                c.ChangeMovement(CURRENT_MOVEMENT);

            movementText.text = "Current Movement:" + this.CURRENT_MOVEMENT;
        }
        else if (Input.GetKeyDown(this.blendedKey))
        {
            CURRENT_MOVEMENT = "Blended";
            foreach (var c in this.characterControllers)
                c.ChangeMovement(CURRENT_MOVEMENT);

            movementText.text = "Current Movement:" + this.CURRENT_MOVEMENT;
        }
        else if (Input.GetKeyDown(this.priorityKey))
        {
            CURRENT_MOVEMENT = "Priority";
            foreach (var c in this.characterControllers)
                c.ChangeMovement(CURRENT_MOVEMENT);

            movementText.text = "Current Movement:" + this.CURRENT_MOVEMENT;

        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            CURRENT_MOVEMENT = "RVO";
            foreach (var c in this.characterControllers)
                c.ChangeMovement(CURRENT_MOVEMENT);
            movementText.text = "Current Movement:" + this.CURRENT_MOVEMENT;
        }
        else if(Input.GetKeyDown(KeyCode.M))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }



    }

    void SpawnCharacters()
    {

        if (this.characterControllers.Count <= this.MAX_CHARACTERS - NumberOfCharactersSpawning)
        {

            List<MainCharacterController> newCharacterList = new List<MainCharacterController>();

            for (int i = 0; i < NumberOfCharactersSpawning; i++)
            {
                var car = CreateCharacter();

                this.characters.Add(car.character);
                newCharacterList.Add(car);

            }

            //Initialize the newly created characters
            foreach (var c in newCharacterList)
            {
                c.InitializeMovement(obstacles, characters);
                c.ChangeMovement(CURRENT_MOVEMENT);
            }

            characterControllers.AddRange(newCharacterList);

            //Update the old characters we have in the scene with the new ones
            foreach (var c in characterControllers)
                c.UpdateAvoidCharacterList(newCharacterList, obstacles, characters);

        }
        else Debug.Log("Max Number of characters reached");
    }

    private MainCharacterController CreateCharacter()
    {

        // Random Car from the Prefabs List
        var randomCarPrefab = Random.Range(0, mainCharacterPrefabs.Count);
        var carObject = mainCharacterPrefabs[randomCarPrefab];

        // Spawn according to the current index;
        var spawn = spawnPoints[spawnIndex];


        // Now we need a random destination from the remaining 3

        var remainingSpawnsList = spawnPoints.Where(x => x != spawn).ToList();
        var randomDestination = Random.Range(0, 3);
        var destination = remainingSpawnsList[randomDestination];

        
        // Instantiate the Prefab within the proper location (spawn)
        
        var clone = GameObject.Instantiate(carObject);
        var spawnPosition = new Vector3(spawn.position.x, 0.2f, spawn.position.z);

        // Initiating the Character Controller with the correct information taken from the Editor
        var characterController = clone.GetComponent<MainCharacterController>();
        characterController.character.KinematicData.Position = spawnPosition;
        characterController.destination = destination;
        characterController.spawn = spawn;

        // Really old and rudimentar way of making sure they spawn incrementally
        if (spawnIndex == 3)
            spawnIndex = 0;
        else spawnIndex += 1;

        return characterController;
    }


  

    // Simple method that gets called when the Slider is updated within the Game, it does not have the cleanest name
    public void NumberOfCharacerSliderChanged()
    {
        NumberOfCharactersSpawning = Convert.ToInt32(characterSlider.value);

        sliderText.text = "Cars per " + SpawnTimer + " seconds: " + NumberOfCharactersSpawning;
    }


  

}
