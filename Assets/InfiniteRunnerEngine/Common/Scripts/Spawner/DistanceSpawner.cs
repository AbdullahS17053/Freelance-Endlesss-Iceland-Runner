using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using MoreMountains.Tools;

namespace MoreMountains.InfiniteRunnerEngine
{
    public class DistanceSpawner : Spawner
    {
        public enum GapOrigins { Spawner, LastSpawnedObject }

        [Header("Gap between objects")]
        public GapOrigins GapOrigin = GapOrigins.Spawner;
        public Vector3 MinimumGap = new Vector3(1, 1, 1);
        public Vector3 MaximumGap = new Vector3(1, 1, 1);

        [Space(10)]
        [Header("Y Clamp")]
        public float MinimumYClamp;
        public float MaximumYClamp;

        [Header("Z Clamp")]
        public float MinimumZClamp;
        public float MaximumZClamp;

        [Space(10)]
        [Header("Spawn angle")]
        public bool SpawnRotatedToDirection = true;

        [Header("Wave Spawning")]
        public int MaxObjectsPerWave = 5;
        public float SpawnDelay = 2f;

        protected int _spawnCountInWave = 0;
        protected bool _waitingForNextWave = false;

        protected Transform _lastSpawnedTransform;
        protected float _nextSpawnDistance;
        protected Vector3 _gap = Vector3.zero;

        protected MMObjectPooler _pooler;

        protected virtual void Start()
        {
            _pooler = GetComponent<MMObjectPooler>();
        }

        protected virtual void Update()
        {
            CheckSpawn();
        }

        protected virtual void CheckSpawn()
        {
            if (_waitingForNextWave)
                return;

            if (OnlySpawnWhileGameInProgress)
            {
                if ((GameManager.Instance.Status != GameManager.GameStatus.GameInProgress)
                 && (GameManager.Instance.Status != GameManager.GameStatus.Paused))
                {
                    _lastSpawnedTransform = null;
                    return;
                }
            }

            if (_spawnCountInWave >= MaxObjectsPerWave)
            {
                StartCoroutine(WaveDelayCoroutine());
                return;
            }

            if (_lastSpawnedTransform == null || !_lastSpawnedTransform.gameObject.activeInHierarchy)
            {
                DistanceSpawn(transform.position + MMMaths.RandomVector3(MinimumGap, MaximumGap));
                return;
            }

            if (transform.InverseTransformPoint(_lastSpawnedTransform.position).x < -_nextSpawnDistance)
            {
                DistanceSpawn(transform.position);
            }
        }

        protected virtual void DistanceSpawn(Vector3 spawnPosition)
        {
            GameObject spawnedObject = null;

            // Try to get from pool
            if (_pooler != null)
            {
                spawnedObject = _pooler.GetPooledGameObject();
            }

            // Reuse last spawned object if pool is empty
            if (spawnedObject == null && _lastSpawnedTransform != null)
            {
                spawnedObject = _lastSpawnedTransform.gameObject;
            }

            if (spawnedObject == null)
            {
                _nextSpawnDistance = UnityEngine.Random.Range(MinimumGap.x, MaximumGap.x);
                return;
            }

            spawnedObject.SetActive(true);

            if (spawnedObject.GetComponent<MMPoolableObject>() == null)
            {
                throw new Exception(gameObject.name + " is trying to spawn objects without PoolableObject.");
            }

            if (SpawnRotatedToDirection)
                spawnedObject.transform.rotation = transform.rotation;

            if (spawnedObject.GetComponent<MovingObject>() != null)
                spawnedObject.GetComponent<MovingObject>().SetDirection(transform.rotation * Vector3.left);

            if (_lastSpawnedTransform != null)
            {
                spawnedObject.transform.position = transform.position;

                float xDistance = transform.InverseTransformPoint(_lastSpawnedTransform.position).x;

                spawnedObject.transform.position += transform.rotation
                                                    * Vector3.right
                                                    * (xDistance
                                                    + _lastSpawnedTransform.GetComponent<MMPoolableObject>().Size.x / 2
                                                    + spawnedObject.GetComponent<MMPoolableObject>().Size.x / 2);

                if (GapOrigin == GapOrigins.Spawner)
                {
                    spawnedObject.transform.position += transform.rotation *
                        ClampedPosition(MMMaths.RandomVector3(MinimumGap, MaximumGap));
                }
                else
                {
                    _gap.x = UnityEngine.Random.Range(MinimumGap.x, MaximumGap.x);
                    _gap.y = spawnedObject.transform.InverseTransformPoint(_lastSpawnedTransform.position).y
                             + UnityEngine.Random.Range(MinimumGap.y, MaximumGap.y);
                    _gap.z = spawnedObject.transform.InverseTransformPoint(_lastSpawnedTransform.position).z
                             + UnityEngine.Random.Range(MinimumGap.z, MaximumGap.z);

                    spawnedObject.transform.Translate(_gap);
                    spawnedObject.transform.position = transform.rotation *
                        ClampedPositionRelative(spawnedObject.transform.position, transform.position);
                }
            }
            else
            {
                spawnedObject.transform.position = transform.position + transform.rotation *
                    ClampedPosition(MMMaths.RandomVector3(MinimumGap, MaximumGap));
            }

            if (spawnedObject.GetComponent<MovingObject>() != null)
                spawnedObject.GetComponent<MovingObject>().Move();

            spawnedObject.GetComponent<MMPoolableObject>().TriggerOnSpawnComplete();

            foreach (Transform child in spawnedObject.transform)
            {
                if (child.gameObject.GetComponent<ReactivateOnSpawn>() != null)
                    child.gameObject.GetComponent<ReactivateOnSpawn>().Reactivate();
            }

            _nextSpawnDistance = spawnedObject.GetComponent<MMPoolableObject>().Size.x / 2;
            _lastSpawnedTransform = spawnedObject.transform;

            _spawnCountInWave++;
        }

        protected IEnumerator WaveDelayCoroutine()
        {
            _waitingForNextWave = true;
            yield return new WaitForSeconds(SpawnDelay);
            _spawnCountInWave = 0;
            _waitingForNextWave = false;
        }

        protected virtual Vector3 ClampedPosition(Vector3 vectorToClamp)
        {
            vectorToClamp.y = Mathf.Clamp(vectorToClamp.y, MinimumYClamp, MaximumYClamp);
            vectorToClamp.z = Mathf.Clamp(vectorToClamp.z, MinimumZClamp, MaximumZClamp);
            return vectorToClamp;
        }

        protected virtual Vector3 ClampedPositionRelative(Vector3 vectorToClamp, Vector3 clampOrigin)
        {
            vectorToClamp.y = Mathf.Clamp(vectorToClamp.y, MinimumYClamp + clampOrigin.y, MaximumYClamp + clampOrigin.y);
            vectorToClamp.z = Mathf.Clamp(vectorToClamp.z, MinimumZClamp + clampOrigin.z, MaximumZClamp + clampOrigin.z);
            return vectorToClamp;
        }

        protected virtual void OnDrawGizmosSelected()
        {
            DrawClamps();

            GUIStyle style = new GUIStyle();

            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(Vector3.zero, MinimumGap);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Vector3.zero, MaximumGap);

            Gizmos.matrix = Matrix4x4.identity;

            if (MinimumGap != Vector3.zero)
            {
                style.normal.textColor = Color.yellow;
                Vector3 labelPosition = transform.position + (Mathf.Abs(MinimumGap.y / 2) + 1) * Vector3.up + Vector3.left;
                labelPosition = MMMaths.RotatePointAroundPivot(labelPosition, transform.position, transform.rotation.eulerAngles);
#if UNITY_EDITOR
                UnityEditor.Handles.Label(labelPosition, "Minimum Gap", style);
#endif
            }

            if (MaximumGap != Vector3.zero)
            {
                style.normal.textColor = Color.red;
                Vector3 labelPosition = transform.position + (-Mathf.Abs(MaximumGap.y / 2) + 1) * Vector3.up + Vector3.left;
                labelPosition = MMMaths.RotatePointAroundPivot(labelPosition, transform.position, transform.rotation.eulerAngles);
#if UNITY_EDITOR
                UnityEditor.Handles.Label(labelPosition, "Maximum Gap", style);
#endif
            }

            MMDebug.DrawGizmoArrow(transform.position, transform.rotation * Vector3.left * 10, Color.green);
        }

        protected virtual void DrawClamps()
        {
            GUIStyle style = new GUIStyle();
            if (MinimumYClamp != MaximumYClamp)
            {
                style.normal.textColor = Color.cyan;
                Vector3 labelPosition = transform.position + (Mathf.Abs(MaximumYClamp) + 1) * Vector3.up + Vector3.left;
                labelPosition = MMMaths.RotatePointAroundPivot(labelPosition, transform.position, transform.rotation.eulerAngles);
#if UNITY_EDITOR
                UnityEditor.Handles.Label(labelPosition, "Clamp", style);
#endif
            }

            float xMinus5 = transform.position.x - 5;
            float xPlus5 = transform.position.x + 5;

            float minimumYClamp = MinimumYClamp + transform.position.y;
            float maximumYClamp = MaximumYClamp + transform.position.y;
            float minimumZClamp = MinimumZClamp + transform.position.z;
            float maximumZClamp = MaximumZClamp + transform.position.z;

            Gizmos.color = Color.cyan;

            Gizmos.DrawLine(MMMaths.RotatePointAroundPivot(new Vector3(xMinus5, minimumYClamp, minimumZClamp), transform.position, transform.rotation.eulerAngles),
                MMMaths.RotatePointAroundPivot(new Vector3(xPlus5, minimumYClamp, minimumZClamp), transform.position, transform.rotation.eulerAngles));

            Gizmos.DrawLine(MMMaths.RotatePointAroundPivot(new Vector3(xMinus5, maximumYClamp, minimumZClamp), transform.position, transform.rotation.eulerAngles),
                MMMaths.RotatePointAroundPivot(new Vector3(xPlus5, maximumYClamp, minimumZClamp), transform.position, transform.rotation.eulerAngles));

            Gizmos.DrawLine(MMMaths.RotatePointAroundPivot(new Vector3(xMinus5, minimumYClamp, maximumZClamp), transform.position, transform.rotation.eulerAngles),
                MMMaths.RotatePointAroundPivot(new Vector3(xPlus5, minimumYClamp, maximumZClamp), transform.position, transform.rotation.eulerAngles));

            Gizmos.DrawLine(MMMaths.RotatePointAroundPivot(new Vector3(xMinus5, maximumYClamp, maximumZClamp), transform.position, transform.rotation.eulerAngles),
                MMMaths.RotatePointAroundPivot(new Vector3(xPlus5, maximumYClamp, maximumZClamp), transform.position, transform.rotation.eulerAngles));

            Gizmos.DrawLine(MMMaths.RotatePointAroundPivot(new Vector3(xMinus5, maximumYClamp, minimumZClamp), transform.position, transform.rotation.eulerAngles),
                MMMaths.RotatePointAroundPivot(new Vector3(xPlus5, maximumYClamp, maximumZClamp), transform.position, transform.rotation.eulerAngles));

            Gizmos.DrawLine(MMMaths.RotatePointAroundPivot(new Vector3(xPlus5, minimumYClamp, minimumZClamp), transform.position, transform.rotation.eulerAngles),
                MMMaths.RotatePointAroundPivot(new Vector3(xPlus5, minimumYClamp, maximumZClamp), transform.position, transform.rotation.eulerAngles));

            Gizmos.DrawLine(MMMaths.RotatePointAroundPivot(new Vector3(xMinus5, minimumYClamp, maximumZClamp), transform.position, transform.rotation.eulerAngles),
                MMMaths.RotatePointAroundPivot(new Vector3(xMinus5, minimumYClamp, minimumZClamp), transform.position, transform.rotation.eulerAngles));
        }
    }
}
