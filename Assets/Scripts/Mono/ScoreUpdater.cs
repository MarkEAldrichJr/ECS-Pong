using Authoring;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Mono
{
    public class ScoreUpdater : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI winnerText;
        private TextMeshProUGUI _scoreText;
    
        private int _scoreLastFrame;
    
        private EntityManager _entityManager;
        private Entity _scoreEntity;

        private void Awake()
        {
            _scoreText = GetComponent<TextMeshProUGUI>();
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
            {
                _scoreText.text = scoreThisFrame.ToString();
            
                if (scoreThisFrame is > 1000 or < -1000)
                    GameOver(scoreThisFrame);
            }

            _scoreLastFrame = scoreThisFrame;
        }

        private void GameOver(int score)
        {
            winnerText.alignment = score > 0 ? 
                TextAlignmentOptions.BaselineLeft : TextAlignmentOptions.BaselineRight;

            winnerText.gameObject.SetActive(true);
        }
    }
}