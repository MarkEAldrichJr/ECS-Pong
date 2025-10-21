using Authoring;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class ScoreUpdater : MonoBehaviour
{
    private TextMeshProUGUI _text;
    private int _scoreLastFrame;
    
    private EntityManager _entityManager;
    private Entity _scoreEntity;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    private void Start()
    {
        var scoreEntityQuery = _entityManager.CreateEntityQuery(
            new EntityQueryBuilder(Allocator.Temp).WithAll<Score>());
        _scoreEntity = scoreEntityQuery.GetSingletonEntity();
    }

    private void Update()
    {
        var scoreThisFrame = _entityManager.GetComponentData<Score>(_scoreEntity).Value;

        if (scoreThisFrame != _scoreLastFrame)
            _text.text = scoreThisFrame.ToString();
        
        _scoreLastFrame = scoreThisFrame;
    }
}
