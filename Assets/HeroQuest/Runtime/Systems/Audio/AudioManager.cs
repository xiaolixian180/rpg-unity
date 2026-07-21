using UnityEngine;

namespace HeroQuest.Systems.Audio
{
    /// <summary>
    /// 全局音频管理器 — 管理 BGM 淡入淡出切换 + SFX 播放（含音量/音调随机化）。
    /// 单例模式，通过 Ensure() 获取。
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;

        private AudioSource _bgmSource;
        private AudioSource _sfxSource;
        private float _bgmVolume = 0.5f;
        private float _sfxVolume = 0.8f;
        private float _fadeTimer;
        private float _fadeDuration;
        private AudioClip _fadeTarget;
        private bool _fading;

        public static AudioManager Ensure()
        {
            if (_instance != null) return _instance;
            var existing = FindFirstObjectByType<AudioManager>();
            if (existing != null)
            {
                _instance = existing;
                return _instance;
            }
            var go = new GameObject("AudioManager");
            _instance = go.AddComponent<AudioManager>();
            DontDestroyOnLoad(go);
            return _instance;
        }

        private void Awake()
        {
            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.playOnAwake = false;
            _bgmSource.volume = _bgmVolume;
            _bgmSource.spatialBlend = 0f;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.loop = false;
            _sfxSource.playOnAwake = false;
            _sfxSource.volume = _sfxVolume;
            _sfxSource.spatialBlend = 0f;
        }

        private void Update()
        {
            if (!_fading) return;
            _fadeTimer += Time.deltaTime;
            var t = _fadeDuration > 0f ? _fadeTimer / _fadeDuration : 1f;
            if (t >= 1f)
            {
                if (_fadeTarget != null)
                {
                    _bgmSource.clip = _fadeTarget;
                    _bgmSource.volume = _bgmVolume;
                    _bgmSource.Play();
                }
                _fading = false;
                return;
            }
            // 淡出旧 BGM
            _bgmSource.volume = _bgmVolume * (1f - t);
        }

        /// <summary>
        /// 切换 BGM，带淡入淡出。
        /// </summary>
        public void PlayBGM(AudioClip clip, float fadeDuration = 1f)
        {
            if (clip == null) return;
            if (_bgmSource.clip == clip) return;
            _fadeTarget = clip;
            _fadeDuration = fadeDuration;
            _fadeTimer = 0f;
            _fading = true;
        }

        /// <summary>
        /// 播放一次性 SFX，支持音调随机化避免重复感。
        /// </summary>
        public void PlaySFX(AudioClip clip, float volumeScale = 1f, float pitchVariance = 0f)
        {
            if (clip == null) return;
            _sfxSource.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
            _sfxSource.PlayOneShot(clip, volumeScale);
        }

        /// <summary>
        /// 设置 BGM 音量 (0-1)。
        /// </summary>
        public void SetBGMVolume(float vol)
        {
            _bgmVolume = Mathf.Clamp01(vol);
            if (!_fading) _bgmSource.volume = _bgmVolume;
        }

        /// <summary>
        /// 设置 SFX 音量 (0-1)。
        /// </summary>
        public void SetSFXVolume(float vol)
        {
            _sfxVolume = Mathf.Clamp01(vol);
            _sfxSource.volume = _sfxVolume;
        }

        /// <summary>
        /// 停止 BGM。
        /// </summary>
        public void StopBGM()
        {
            _bgmSource.Stop();
            _fading = false;
        }
    }
}
