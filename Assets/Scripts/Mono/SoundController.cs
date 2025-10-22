using System;
using Authoring;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Mono
{
    public class SoundController : MonoBehaviour
    {
        [SerializeField] private AudioClip audioClip;
        [SerializeField] private Vector2 goalPitch = new(0.7f, 1.2f);
        [SerializeField] private Vector2 wallPitch = new(0.7f, 1.2f);
        [SerializeField] private Vector2 paddlePitch = new(0.7f, 1.2f);
    
        private AudioSource _audioSource;
        private EntityManager _entityManager;
        private EntityQuery _soundEntityQuery;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _soundEntityQuery = _entityManager.CreateEntityQuery(new EntityQueryBuilder(
                Allocator.Temp).WithAll<BounceSound>());
        }

        private void Update()
        {
            try
            {
                var bounceSoundEntity = _soundEntityQuery.GetSingletonEntity();
                var sound = _entityManager.GetComponentData<BounceSound>(bounceSoundEntity);

                if (sound.Goal) PlayAudioClip(audioClip, _audioSource, goalPitch);
                else if (sound.Paddle) PlayAudioClip(audioClip, _audioSource, paddlePitch);
                else if (sound.Wall) PlayAudioClip(audioClip, _audioSource, wallPitch);

                _entityManager.SetComponentData(bounceSoundEntity, new BounceSound
                {
                    Goal = false,
                    Wall = false,
                    Paddle = false
                });
            }
            catch (Exception)
            {
                //suppress errors
            }
        }

        private static void PlayAudioClip(AudioClip clip, AudioSource source, Vector2 pitch)
        {
            source.pitch = Random.Range(pitch.x, pitch.y);
            source.PlayOneShot(clip);
        }
    }
}