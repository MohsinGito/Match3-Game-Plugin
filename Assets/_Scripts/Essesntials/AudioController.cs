using System.Collections;
using UnityEngine;

namespace Utilities
{
    namespace Audio
    {
        public class AudioController : Singleton<AudioController>
        {

            #region Main Attributes

            public bool debug;
            public AudioTrack[] tracks;

            private Hashtable m_AudioTable;
            private Hashtable m_JobTable;

            #endregion

            #region Unity Functions

            private void Start()
            {
                m_AudioTable = new Hashtable();
                m_JobTable = new Hashtable();
                GenerateAudioTable();
            }

            private void OnDisable()
            {
                foreach (DictionaryEntry _entry in m_JobTable)
                {
                    IEnumerator _job = (IEnumerator)_entry.Value;
                    StopCoroutine(_job);
                }
            }

            #endregion

            #region Public Functions

            public void PlayAudio(AudioName type, bool _fade = false, float _delay = 0.0f)
            {
                AddJob(new AudioJob(AudioAction.START, type, _fade, _delay));
            }

            public void StopAudio(AudioName type, bool _fade = false, float _delay = 0.0f)
            {
                AddJob(new AudioJob(AudioAction.STOP, type, _fade, _delay));
            }

            public void RestartAudio(AudioName type, bool _fade = false, float _delay = 0.0f)
            {
                AddJob(new AudioJob(AudioAction.RESTART, type, _fade, _delay));
            }

            public void PlayProvidedAudio(string _trackName, AudioClip _clip)
            {
                foreach (AudioTrack _track in tracks)
                {
                    if (!_track.trackName.Equals(_trackName))
                        continue;

                    if (_track.AdSource != null)
                    {
                        _track.AdSource.clip = _clip;
                        _track.AdSource.Play();
                    }

                    break;
                }
            }

            public void MuteTrack(string _trackName)
            {
                foreach(AudioTrack track in tracks)
                {
                    if(track.trackName.Equals(_trackName))
                    {
                        track.Mute = true;
                        break;
                    }
                }
            }

            public void UnMuteTrack(string _trackName)
            {
                foreach (AudioTrack track in tracks)
                {
                    if (track.trackName.Equals(_trackName))
                    {
                        track.Mute = false;
                        break;
                    }
                }
            }

            #endregion

            #region Private Functions

            private void GenerateAudioTable()
            {
                foreach(AudioTrack _track in tracks)
                {
                    _track.AdSource = new GameObject().AddComponent<AudioSource>();
                    _track.AdSource.transform.parent = transform;

                    foreach (AudioObject _obj in _track.audio)
                    {
                        if(m_AudioTable.ContainsKey(_obj.type))
                        {
                            LogWarning("You Are Trying To Register Audio ["+ _obj.type+"] That Has Already Been Registered");
                        }
                        else
                        {
                            m_AudioTable.Add(_obj.type, _track);
                            Log("Audio [" + _obj.type + "] Has Been Registered");
                        }
                    }
                }
            }

            private void AddJob(AudioJob _job)
            {
                RemoveConflictingJobs(_job.type);
                IEnumerator _jobRunner = RunAduioJOb(_job);
                m_JobTable.Add(_job.type, _jobRunner);
                StartCoroutine(_jobRunner);
                Log("Starting Job On ["+ _job .type+ "] With Operation ["+ _job.action+ "]");
            }

            private void RemoveJob(AudioName _type)
            {
                if (!m_JobTable.ContainsKey(_type))
                {
                    LogWarning("Trying To Stop The Job [" + _type + "] That Is Not Running.");
                    return;
                }

                IEnumerator _jobRunner = (IEnumerator)m_JobTable[_type];
                StopCoroutine(_jobRunner);
                m_JobTable.Remove(_type);
            }

            private void RemoveConflictingJobs(AudioName _type)
            {
                if (!m_JobTable.ContainsKey(_type))
                    RemoveJob(_type);
                AudioName _conflictAudio = AudioName.NONE;
                foreach (DictionaryEntry _entry in m_JobTable) /// here we are looking for all the audio's that are in job table
                {
                    AudioName _audioType = (AudioName)_entry.Key;
                    AudioTrack _audioTrackInUse = (AudioTrack)m_AudioTable[_audioType]; // this is the track currently utilized by the addjob
                    AudioTrack _audioTrackNeeded = (AudioTrack)m_AudioTable[_type];// this is the actual audio track to be player if this track and _audioTrackInUse are equal, it means we will have a conflict
                    if (_audioTrackNeeded.AdSource == _audioTrackInUse.AdSource)
                    {
                        /// it means we have a conflict
                        _conflictAudio = _audioType;
                    }
                }
                if(_conflictAudio != AudioName.NONE)
                    RemoveJob(_conflictAudio);
            }

            private IEnumerator RunAduioJOb(AudioJob _job)
            {
                yield return new WaitForSeconds(_job.delay);
                AudioTrack _track = (AudioTrack)m_AudioTable[_job.type];
                _track.AdSource.clip = GetAudioClipFromAudioTrack(_job.type, _track);

                switch(_job.action)
                {
                    case AudioAction.START:
                        _track.AdSource.Play();
                        break;
                    case AudioAction.STOP:
                        if (!_job.fade)
                        {
                            _track.AdSource.Stop();
                        }
                        break;
                    case AudioAction.RESTART:
                        _track.AdSource.Stop();
                        _track.AdSource.Play();
                        break;
                }

                if (_job.fade && !_track.Mute)
                {
                    float _initial = _job.action == AudioAction.START || _job.action == AudioAction.RESTART ? 0.0f : 1f;
                    float _target = _initial == 0 ? _track.volume : 0;
                    float _duration = 1.0f;
                    float _timer = 0.0f;

                    while (_timer < _duration)
                    {
                        _track.AdSource.volume = Mathf.Lerp(_initial, _target, _timer / _duration);
                        _timer += Time.deltaTime;
                        yield return null;
                    }
                    if (_job.action == AudioAction.STOP)
                    {
                        _track.AdSource.Stop();
                    }
                }
                else
                {
                    _track.AdSource.volume = _track.Mute ? 0 : _track.volume;
                }

                m_JobTable.Remove(_job.type);
                Log("Job Count : "+m_JobTable.Count);
            }

            private AudioClip GetAudioClipFromAudioTrack(AudioName _type ,AudioTrack _track)
            {
                foreach(AudioObject _obj in _track.audio)
                {
                    if (_obj.type == _type)
                        return _obj.clip;
                }
                return null;
            }

            private void Log(string msg)
            { 
                if(!debug) return;
                Debug.Log("[Audio Controller] : " + msg);
            }

            private void LogWarning(string msg)
            {
                if (!debug) return;
                Debug.LogWarning("[Audio Controller] : " + msg);
            }

            #endregion

            #region In Script Calsses

            [System.Serializable]
            public class AudioObject
            {
                public AudioName type;
                public AudioClip clip;
            }

            [System.Serializable]
            public class AudioTrack
            {
                public string trackName;
                public float volume;
                public bool onLoop;
                public AudioObject[] audio;

                private bool isMute;
                private AudioSource source;

                public AudioSource AdSource
                {
                    set
                    {
                        source = value;
                        source.loop = onLoop;
                        source.playOnAwake = false;
                        source.transform.name = trackName + " AD SRC";
                    }
                    get
                    {
                        return source;
                    }
                }

                public bool Mute
                {
                    set
                    {
                        isMute = value;
                        source.volume = value ? 0 : volume;
                    }
                    get
                    {
                        return isMute;
                    }
                }
            }

            public class AudioJob
            {
                public AudioAction action;
                public AudioName type;
                public bool fade;
                public float delay;

                public AudioJob(AudioAction _action, AudioName _type, bool _fade, float _delay)
                {
                    action = _action;
                    type = _type;
                    fade = _fade;
                    delay = _delay;
                }
            }

            public enum AudioAction
            {
                START,
                STOP,
                RESTART
            }

            #endregion
        }
    }
}