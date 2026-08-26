using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace WormWars.Network
{
    public enum WormSoundEvent { FireWeapon, Explosion, WormJump, CastleCollapse, WormDamage, TurnStart }

    // Server-authoritative sound triggers. Nothing plays until the server says so, and
    // every client hears the same event at the same world position at the same moment -
    // no client can trigger a sound on its own, and no rendering/pipeline code is involved.
    public class NetworkAudioManager : NetworkBehaviour
    {
        [Serializable]
        struct SoundEntry
        {
            public WormSoundEvent soundEvent;
            public AudioClip clip;
        }

        [SerializeField] SoundEntry[] soundLibrary;
        [SerializeField, Range(0f, 1f)] float volume = 1f;

        // One audio manager per match, so other server-side systems (explosions, weapon
        // fire, turn transitions, ...) have a simple call site instead of needing their own
        // serialized reference wired up in every prefab.
        public static NetworkAudioManager Instance { get; private set; }

        readonly Dictionary<WormSoundEvent, AudioClip> _clipLookup = new Dictionary<WormSoundEvent, AudioClip>();

        void Awake()
        {
            Instance = this;

            if (soundLibrary == null) return;

            foreach (SoundEntry entry in soundLibrary)
            {
                if (entry.clip != null) _clipLookup[entry.soundEvent] = entry.clip;
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // Server-only. Call this wherever an event happens server-side (a shot fired, an
        // explosion, a worm landing a jump, ...) to have every client play the matching
        // sound at that world position.
        public void PlaySoundEffect(WormSoundEvent soundType, Vector3 position)
        {
            if (!IsServer) return;
            PlaySpatialSoundClientRpc(soundType, position);
        }

        [ClientRpc]
        void PlaySpatialSoundClientRpc(WormSoundEvent soundType, Vector3 position)
        {
            if (!_clipLookup.TryGetValue(soundType, out AudioClip clip) || clip == null) return;
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }
    }
}
