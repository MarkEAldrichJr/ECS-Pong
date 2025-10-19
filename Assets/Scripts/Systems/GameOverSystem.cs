using Authoring;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Systems
{
    public partial struct GameOverSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Score>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var score = SystemAPI.GetSingleton<Score>();

            if (math.abs(score.Value) > score.MaxScore)
            {
                Debug.Log("Game Over!");
                //end game sequence
            }
        }
    }
}