using System;
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
        private EntityQuery _scoreEntityQuery;
        

        private void Awake()
        {
            _scoreText = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _scoreEntityQuery = _entityManager.CreateEntityQuery(
                new EntityQueryBuilder(Allocator.Temp).WithAll<Score>());
        }

        private void Update()
        {
            try
            {
                var scoreEntity = _scoreEntityQuery.GetSingletonEntity();
                var scoreThisFrame = _entityManager.GetComponentData<Score>(scoreEntity).Value;

                if (scoreThisFrame != _scoreLastFrame)
                {
                    _scoreText.text = scoreThisFrame.ToString();

                    if (scoreThisFrame is >= 1000 or <= -1000)
                        GameOver(scoreThisFrame);
                }

                _scoreLastFrame = scoreThisFrame;
            }
            catch (Exception e)
            {
                //suppress errors
            }
        }

        private void GameOver(int score)
        {
            winnerText.alignment = score > 0 ? 
                TextAlignmentOptions.BaselineLeft : TextAlignmentOptions.BaselineRight;

            winnerText.gameObject.SetActive(true);
        }
    }
}