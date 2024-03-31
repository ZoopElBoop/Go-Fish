using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Fish Data Block", menuName = "ScriptableObjects/Fish Data Object")]
public class FishData : ScriptableObject
{
    [Header("Fish Prefab")]
    public GameObject _Fishk;

    [Header("Base Fish Values")]
    [Min(1)] public int _Health = 1;
    [Min(0.1f)] public float _Speed = 1;
    [Min(0.1f)] public float _rotationSpeed = 1;

    [Header("Can It Be Caught By A Rod?")]
    public bool _canBeCaught = false;

    [Header("Probability Spawning Points")]
    [Range(-100f, -1.0f)] public float _spawnDepthStart = -1f;
    [Range(-100f, -2.0f)] public float _spawnDepthHigh = -5f;
    [Range(-100f, -3.0f)] public float _spawnDepthEnd = -10f;

    [Header("Rarity Markiplier")]
    [Range(0.1f, 2.0f)] public float _Rarity = 1f;

    [Header("Limits To How High & Low Fish Can Swim")]

    [Range(-100f, 0.0f)] public float _moveHeightLimit = -1f;
    [Range(-100f, -1.0f)] public float _moveDepthLimit = -10f;

    [Header("Area Needed To Be Clear To Spawn")]
    [Range(0.1f, 10.0f)] public float _collisionRange = 1;

    [Header("Despawn Range Multiplier")]
    [Range(0.5f, 1.5f)] public float _despawnRangeMulti = 1;

    [Header("Base Value & Range To Price Changes")]
    [Min(1f)] public int _baseValue = 1;
    [Min(0.1f)] public float _valueChangeCoefficient = 0.1f;

    [Header("Fish Information")]
    public Sprite _FishkImage;
    [Multiline] public string _FishkDescription;

    private void OnValidate()
    {
        //ik this is horrific amount of ifs but this be for checking i am not stupid

        if (_Fishk == null)
        {
            StopPlay();
            Debug.LogError($"Missing Fish Prefab On {name}, pls fix");
        }

        if (_spawnDepthStart <= _spawnDepthEnd)
        {
            StopPlay();
            Debug.LogError($"Start Spawn Depth Below End On {name}, pls fix");
        }
        else if (_spawnDepthHigh >= _spawnDepthStart || _spawnDepthHigh <= _spawnDepthEnd)
        {
            StopPlay();
            Debug.LogError($"Spawn High Not Between Start & End On {name}, pls fix");
        }
        else if (_spawnDepthEnd >= _spawnDepthStart)
        {
            StopPlay();
            Debug.LogError($"End Spawn Depth Above Start On {name}, pls fix");
        }

        if (_moveDepthLimit >= _moveHeightLimit)
        {
            StopPlay();
            Debug.LogError($"Depth Limit Above Height Limit On {name}, pls fix");
        }
    }

    private void StopPlay()
    {
#if UNITY_EDITOR
        if (EditorApplication.isPlaying)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
#endif
    }
}