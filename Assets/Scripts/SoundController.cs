using Authoring;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    [SerializeField] private AudioClip audioClip;
    
    [SerializeField] private Vector2 goalPitch = new(0.7f, 1.2f);
    [SerializeField] private Vector2 wallPitch = new(0.7f, 1.2f);
    [SerializeField] private Vector2 paddlePitch = new(0.7f, 1.2f);
    
    private AudioSource _audioSource;
    private Entity _bounceSoundEntity;
    private EntityManager _entityManager;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    private void Start()
    {
        var soundEntityQuery = _entityManager.CreateEntityQuery(new EntityQueryBuilder(
            Allocator.Temp).WithAll<BounceSound>());
        _bounceSoundEntity = soundEntityQuery.GetSingletonEntity();
    }

    private void Update()
    {
        var sound = _entityManager.GetComponentData<BounceSound>(_bounceSoundEntity);

        if (sound.Goal) PlayAudioClip(audioClip, _audioSource, goalPitch);
        if (sound.Wall) PlayAudioClip(audioClip, _audioSource, wallPitch);
        if (sound.Paddle) PlayAudioClip(audioClip, _audioSource, paddlePitch);
        
        _entityManager.SetComponentData(_bounceSoundEntity, new BounceSound
        {
            Goal = false,
            Wall = false,
            Paddle = false
        });
    }

    private static void PlayAudioClip(AudioClip clip, AudioSource source, Vector2 pitch)
    {
        source.pitch = Random.Range(pitch.x, pitch.y);
        source.PlayOneShot(clip);
    }
}