using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using UnityEngine.UI;
using UnityEngine.Profiling;

//! Need some retakes on the following parts :
// todo: clean up code
// todo: use compute Buffer w/ compute shader to rewrite the input texture (and get rig of the SetPixel)
//based on: https://medium.com/xrpractices/monocular-depth-sensing-point-cloud-from-webcam-feed-using-unity-barracuda-d9f1496b5932
//https://github.com/nianticlabs/monodepth2
//other: https://github.com/ialhashim/DenseDepth
//Fast depth info : http://fastdepth.mit.edu/
namespace Bonjour.AIModel
{
    public class DepthSensor : MonoBehaviour
    {
        [Header("Monocular Depth params")]
        [SerializeField] [Tooltip("Define the model to use")] private NNModel monoDepthONNX;
        [SerializeField] [Tooltip("Define the source to analyze")] private RenderTexture source;

        //Barracuda
        private Model runtimeModel;
        private IWorker worker;

        private int modelwidth = 224;
        private int modelheight = 224;

        private Texture2D rawInput;
        private RenderTexture rawOutput;
        private RenderTexture debugOutput;
        private Material depthViewer;

        [Header("Debug params")]
        public bool showDebug = true;
        public float debugScale = 2;
        [Range(0f, 8f)] 
        [Tooltip("Define the min depth to display")] public float minDist = 0.0f;
        [Range(0f, 8f)] 
        [Tooltip("Define the max depth to display")] public float maxDist = 4.0f;


        private void Start(){
            InitDepthModel();
            InitBuffers();
        }

        private void InitDepthModel(){
            runtimeModel    = ModelLoader.Load(monoDepthONNX);
            worker          = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, runtimeModel);
        }

        private void InitBuffers(){
            rawInput                    = new Texture2D(modelwidth, modelheight, TextureFormat.RGB24, false);
            rawOutput                   = new RenderTexture(modelwidth, modelheight, 0, RenderTextureFormat.RFloat);
            rawOutput.enableRandomWrite = true;
            rawOutput.Create();

            debugOutput                   = new RenderTexture(modelwidth, modelheight, 0, RenderTextureFormat.ARGBFloat);
            debugOutput.enableRandomWrite = true;
            debugOutput.Create();              

            depthViewer = new Material(Shader.Find("Hidden/DepthViewer"));
        }

        private void Update(){
            // Profiler.BeginSample("ResizeSourceToModel");
            ResizeSourceToModel();
            // Profiler.EndSample();
            
            // Profiler.BeginSample("Tensor + Inference");
            //Set tensor with input
            var tensor      = new Tensor(rawInput, 3);
            //inference + grab resultws
            var executer    = worker.Execute(tensor).PeekOutput();
            // Profiler.EndSample();
            
            // Profiler.BeginSample("Tensor to RT");
            //Grab and set results to Temporary RT.
            //Note: FastDepth model return a 1 channel result with a range over 1.0 (0.0 to X)
            executer.ToRenderTexture(rawOutput);
            // Profiler.EndSample();

            if(showDebug){
                // Profiler.BeginSample("Depth viewer");
                depthViewer.SetFloat("_MinDepth", minDist);
                depthViewer.SetFloat("_MaxDepth", maxDist);
                Graphics.Blit(rawOutput, debugOutput, depthViewer);
                // Profiler.EndSample();
            }
            
            // Profiler.BeginSample("Dispose Tensor + Release Temp RT");
            tensor?.Dispose();
            tensor = null;
            // Profiler.EndSample();
        }

        private void ResizeSourceToModel(){
            float aspect            = (float) source.width / (float) source.height;
            int width               = Mathf.FloorToInt(rawInput.height * aspect);
            RenderTexture temp      = RenderTexture.GetTemporary(rawInput.width, rawInput.height, 0, source.format);

            if(source.width >= source.height)   Graphics.Blit(source, temp, new Vector2(1, aspect), new Vector2(0, (1-aspect) * .5f));
            else                                Graphics.Blit(source, temp, new Vector2(1+aspect, 1), new Vector2((1-aspect) * -.5f, 0));

            RenderTexture.active = temp;
            rawInput.ReadPixels(new Rect(0, 0, rawInput.width, rawInput.height), 0, 0);
            rawInput.Apply();
            RenderTexture.ReleaseTemporary(temp);
        }

        private void OnDisable(){
            worker?.Dispose();
            worker = null;
        }

        private void OnGUI() {
            if(rawInput != null && source != null){

                int w = Mathf.RoundToInt(rawInput.width * debugScale);
                int h = Mathf.RoundToInt(rawInput.height * debugScale);

                GUI.DrawTexture(new Rect(0, Screen.height - h, w, h), rawInput);
                GUI.DrawTexture(new Rect(w * 1, Screen.height - h, w, h), debugOutput);
            }
        }

        public RenderTexture GetRawOutput(){
            return rawOutput;
        }
    }

}
