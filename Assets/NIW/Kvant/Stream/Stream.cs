//
// Stream - line particle system
//
using UnityEngine;

namespace Kvant
{
    [ExecuteInEditMode, AddComponentMenu("Kvant/Stream")]
    public class Stream : MonoBehaviour
    {
        #region Parameters Exposed To Editor

        [SerializeField]
        int _maxParticles = 32768;

        [SerializeField]
        Texture2D _footprintTex;

        [SerializeField]
        Vector3 _footPosition;

        [SerializeField]
        Vector3 _cavePosition;

        [SerializeField]
        bool _debug;

        #endregion

        #region Public Properties

        public int maxParticles {
            // Returns the actual number of particles.
            get { return BufferWidth * BufferHeight; }
        }

        public Vector3 footPosition
        {
            get { return _footPosition; }
            set { _footPosition = value; }
        }

        public Vector3 cavePosition
        {
            get { return _cavePosition; }
            set { _cavePosition = value; }
        }

        #endregion

        #region Shader And Materials

        [SerializeField] Shader _kernelShader;
        [SerializeField] Shader _debugShader;

        Material _kernelMaterial;
        Material _debugMaterial;

        #endregion

        #region Private Variables And Objects

        RenderTexture _particleBuffer1;
        RenderTexture _particleBuffer2;
        bool _needsReset = true;

        #endregion

        #region Private Properties

        public int BufferWidth { get { //return 256;
                return (int)Mathf.Sqrt(_maxParticles);
            }
        }

        public int BufferHeight {
            get {
                //return Mathf.Clamp(_maxParticles / BufferWidth + 1, 1, 127);
                return (int)Mathf.Sqrt(_maxParticles);
            }
        }

        static float deltaTime {
            get {
                return Application.isPlaying && Time.frameCount > 1 ? Time.deltaTime : 1.0f / 10;
            }
        }

        #endregion

        #region Resource Management

        public void NotifyConfigChange()
        {
            _needsReset = true;
        }

        Material CreateMaterial(Shader shader)
        {
            var material = new Material(shader);
            material.hideFlags = HideFlags.DontSave;
            return material;
        }

        RenderTexture CreateBuffer()
        {
            var buffer = new RenderTexture(BufferWidth, BufferHeight, 0, RenderTextureFormat.ARGBFloat);
            buffer.hideFlags = HideFlags.DontSave;
            buffer.filterMode = FilterMode.Point;
            buffer.wrapMode = TextureWrapMode.Repeat;
            return buffer;
        }

        void UpdateKernelShader()
        {
            var m = _kernelMaterial;
            m.SetTexture("_FootprintTex", _footprintTex);

            //_footPosition = new Vector3(10, 0, 0);
            m.SetVector("_FootPos", _footPosition);
            m.SetVector("_CavePosPrev", m.GetVector("_CavePos"));
            m.SetVector("_CavePos", _cavePosition);
        }

        void ResetResources()
        {
            // Particle buffers.
            if (_particleBuffer1) DestroyImmediate(_particleBuffer1);
            if (_particleBuffer2) DestroyImmediate(_particleBuffer2);

            _particleBuffer1 = CreateBuffer();
            _particleBuffer2 = CreateBuffer();

            // Shader materials.
            if (!_kernelMaterial) _kernelMaterial = CreateMaterial(_kernelShader);
            if (!_debugMaterial)  _debugMaterial  = CreateMaterial(_debugShader);

            // Warming up.
            UpdateKernelShader();
            InitializeAndPrewarmBuffers();

            gameObject.GetComponent<Renderer>().material.SetTexture("_HeightMap", _particleBuffer2);

            _needsReset = false;
        }

        void InitializeAndPrewarmBuffers()
        {
            // Initialization.
            Graphics.Blit(null, _particleBuffer2, _kernelMaterial, 0);

            // Execute the kernel shader repeatedly.
            for (var i = 0; i < 8; i++) {
                Graphics.Blit(_particleBuffer2, _particleBuffer1, _kernelMaterial, 1);
                Graphics.Blit(_particleBuffer1, _particleBuffer2, _kernelMaterial, 1);
            }
        }

        #endregion

        #region MonoBehaviour Functions

        void Reset()
        {
            _needsReset = true;
        }

        void OnDestroy()
        {
            if (_particleBuffer1) DestroyImmediate(_particleBuffer1);
            if (_particleBuffer2) DestroyImmediate(_particleBuffer2);
            if (_kernelMaterial)  DestroyImmediate(_kernelMaterial);
            if (_debugMaterial)   DestroyImmediate(_debugMaterial);
        }

        void Update()
        {
            if (_needsReset) ResetResources();

            UpdateKernelShader();

            if (Application.isPlaying)
            {
                // Swap the particle buffers.
                var temp = _particleBuffer1;
                _particleBuffer1 = _particleBuffer2;
                _particleBuffer2 = temp;

                // Execute the kernel shader.
                Graphics.Blit(_particleBuffer1, _particleBuffer2, _kernelMaterial, 1);

                gameObject.transform.GetChild(0).GetComponent<Projector>().material.SetTexture("_ShadowTex", _particleBuffer2);
                // reset foot position
                _footPosition = new Vector3(10, 0);
            }
            else
            {
                InitializeAndPrewarmBuffers();
            }
        }

        void OnGUI()
        {
            if (_debug && Event.current.type.Equals(EventType.Repaint))
            {
                if (_debugMaterial && _particleBuffer2)
                {
                    var rect = new Rect(0, 0, BufferWidth, BufferHeight);
                    Graphics.DrawTexture(rect, _particleBuffer2, _debugMaterial);
                }
                //if (_debugMaterial && _particleBuffer1)
                //{
                //    var rect = new Rect(0, BufferHeight, BufferWidth, BufferHeight);
                //    Graphics.DrawTexture(rect, _particleBuffer1, _debugMaterial);
                //}
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
        }

        #endregion
    }
}
