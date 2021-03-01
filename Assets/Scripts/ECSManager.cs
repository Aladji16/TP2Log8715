using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ECSManager : MonoBehaviour
{
    [SerializeField]
#pragma warning disable 0649
    private ParticlePool _particlePool;
#pragma warning restore 0649

    #region Public API

    public Config Config
    {
        get
        {
            return _config;
        }
    }

    public void Start()
    {
        _config.LoadSave();
        _config.SaveToJson();
    }

    [SerializeField]
#pragma warning disable 0649
    private GameObject text;
#pragma warning restore 0649

    private Dictionary<uint, Text> allTexts = new Dictionary<uint, Text>();

    public List<ISystem> AllSystems { get { return _allSystems; } }

    [SerializeField]
    public bool ShouldDisplayEntityIDs = true;

    public void CreateShape(uint id, Config.ShapeConfig entityConfig)
    {
        switch (entityConfig.shape)
        {
            case Config.Shape.Circle:
                if (Instance.ShouldDisplayGraphics)
                {
                    _particlePool.CreateParticle(id, entityConfig.initialPos, entityConfig.size, entityConfig.color);
                }

                break;
            default:
                Debug.LogError("Unhandled shape " + entityConfig.shape);
                break;
        }
        if (ShouldDisplayGraphics && ShouldDisplayEntityIDs)
        {
            var newText = Instantiate(text);
            newText.transform.SetParent(text.transform.parent, false);
            newText.transform.localScale = Vector3.one;
            newText.transform.position = entityConfig.initialPos;
            newText.transform.SetAsLastSibling();
            Text t = newText.GetComponent<Text>();
            t.text = id.ToString();
            t.color = Color.white;
            allTexts[id] = t;
        }
    }

    public void InitDisplay()
    {
        if (Instance.ShouldDisplayGraphics)
        {
            _particlePool.DisplayParticlesFirst();
        }
    }

    public void UpdateShapePosition(uint id, Vector2 position)
    {
        if (Instance.ShouldDisplayGraphics)
        {
            _particlePool.SetParticlePosition(id, position);
            if (Instance.ShouldDisplayEntityIDs)
            {
                allTexts[id].transform.position = position;
            }
        }
    }

    public void UpdateShapeSize(uint id, float size)
    {
        if (Instance.ShouldDisplayGraphics)
        {
            _particlePool.SetParticleSize(id, size);
        }
    }

    public void UpdateShapeColor(uint id, Color color)
    {
        if (Instance.ShouldDisplayGraphics)
        {
            _particlePool.SetParticleColor(id, color);
        }
    }
    #endregion

    #region System Management
    private List<ISystem> _allSystems = new List<ISystem>();

    public bool ShouldDisplayGraphics
    {
        get
        {
            return !Config.headless;
        }
    }

    private void Awake()
    {
        _allSystems = RegisterSystems.GetListOfSystems();

        // If system missing from config, add it and enable it.
        foreach (var system in _allSystems)
        {
            if (!Config.SystemsEnabled.ContainsKey(system.Name))
            {
                Config.SystemsEnabled[system.Name] = true;
            }
        }
    }

    private float timeLastPrint = 0f;
    private const float updateInterval = 2f;
    [SerializeField]
    private bool _debugPrint = false;
    // Update is called once per frame
    private void Update()
    {
        if (_debugPrint)
        {
            ComponentsManager.Instance.DebugPrint();
        }
        foreach (var system in _allSystems)
        {
            if (Config.SystemsEnabled[system.Name])
            {
                system.UpdateSystem();
            }
        }
        if (Instance.ShouldDisplayGraphics)
        {
            _particlePool.DisplayParticles();
        }

        if (Time.unscaledTime - timeLastPrint > updateInterval)
        {
            timeLastPrint = Time.unscaledTime;
            PrintStats();
        }
    }

    public void PrintStats()
    {
        Debug.Log(" FPS: " + (int)(1.0f / Time.smoothDeltaTime));
    }
    #endregion

    #region Singleton
    private static ECSManager _instance;
    private static bool _instanceInitialized = false;
    public static ECSManager Instance
    {
        get
        {
            if (!_instanceInitialized)
            {
                _instance = FindObjectOfType<ECSManager>();
                _instanceInitialized = true;
            }
            return _instance;
        }
    }
    private ECSManager() { }
    #endregion

    #region Private attributes
    [SerializeField]
#pragma warning disable 0649
    private Config _config;
#pragma warning restore 0649
    #endregion
}

public interface ISystem
{
    void UpdateSystem();
    string Name { get; }
}

public interface IComponent
{
    int GetRandomNumber();
}