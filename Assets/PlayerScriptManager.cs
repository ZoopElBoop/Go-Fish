using UnityEngine;

public class PlayerScriptManager : MonoBehaviour
{
    public static PlayerScriptManager Instance;

    private CharacterController characterController;
    private PlayerMovement playerMovement;
    private Fishing fishing;
    private FishSpawn fishSpawn;

    private void Awake()
    {
        Instance = this;

        if (TryGetComponent<CharacterController>(out var cc))
            characterController = cc;
        else     
            Debug.LogError($"missing character control Component!!!");

        if (TryGetComponent<PlayerMovement>(out var pm))
            playerMovement = pm;
        else
            Debug.LogError($"missing character control script!!!");

        if (TryGetComponent<Fishing>(out var f))
            fishing = f;
        else
            Debug.LogError($"missing fishing script!!!");

        if (GameObject.FindWithTag("Spawner").TryGetComponent<FishSpawn>(out var fs))
            fishSpawn = fs;
        else
            Debug.LogError($"missing fish spawn script!!!");
    }

    public void ShutDown(string script, bool status) 
    {
        switch (script)
        {
            case "Controller":
                characterController.enabled = status;
                    break;
            case "Movement":
                playerMovement.enabled = status;
                break;
            case "Fishing":
                fishing.enabled = status;
                break;
            case "Spawner":
                fishSpawn.enabled = status;
                break;
            default:
                Debug.LogError($"Script/Component {script} not found to shutdown!!!");
                break;
        }    
    }

    public dynamic GetScript(string script)
    {
        switch (script)
        {
            case "Controller":
                return characterController;
            case "Movement":
                return playerMovement;
            case "Fishing":
                return fishing;
            case "Spawner":
                return fishSpawn;
            default:
                Debug.LogError($"Script/Component {script} not found to give!!!");
                break;      
        }
        return null;
    }
}
