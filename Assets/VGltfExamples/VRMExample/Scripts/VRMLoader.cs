﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VGltf;
using VGltf.Unity;

namespace VGltfExamples.VRMExample
{
    public sealed class VRMLoader : MonoBehaviour
    {
        [SerializeField] InputField filePathInput;
        [SerializeField] Button loadButton;
        [SerializeField] Button unloadButton;

        [SerializeField] InputField outputFilePathInput;
        [SerializeField] Button exportButton;

        [SerializeField] public RuntimeAnimatorController RuntimeAnimatorController;

        sealed class VRMResource : IDisposable
        {
            public IImporterContext Context;
            public GameObject Go;

            public void Dispose()
            {
                if (Go != null)
                {
                    GameObject.Destroy(Go);
                }
                Context?.Dispose();
            }
        }

        readonly List<VRMResource> _vrmResources = new List<VRMResource>();

        void Start()
        {
            loadButton.onClick.AddListener(UIOnLoadButtonClick);
            unloadButton.onClick.AddListener(UIOnUnloadButtonClick);

            exportButton.onClick.AddListener(UIOnExportButtonClicked);
        }

        // Update is called once per frame
        void Update()
        {
        }

        void OnDestroy()
        {
            foreach (var disposable in _vrmResources)
            {
                disposable.Dispose();
            }
        }

        async UniTask<VRMResource> LoadVRM()
        {
            var filePath = filePathInput.text;

            // Read the glTF container (unity-independent)
            var gltfContainer = default(GltfContainer);
            using (var sr = Common.StreamReaderFactory.Create(filePath)) // get-path can be called on only main-thread...
            {
                gltfContainer = await Task.Run(() =>
                {
                    return GltfContainer.FromGlb(sr);
                });
            }

            var res = new VRMResource();
            try
            {
                // Create a GameObject that points to the glTF scene.
                // The GameObject of the glTF's child Node will be created under this object.
                var go = new GameObject();
                res.Go = go;

                // For some reason, VRM0 inverts the coordinate system of glTF in the Z axis.
                var config = new Importer.Config
                {
                    FlipZAxisInsteadOfXAsix = true,
                };

                // Create a glTF Importer for Unity.
                // The resources will be cached in the internal Context of this Importer.
                // Resources can be released by calling Dispose of the Importer (or the internal Context).
                var timeSlicer = new Common.TimeSlicer();
                using (var gltfImporter = new Importer(gltfContainer, timeSlicer, config))
                {
                    var bridge = new VRM0ImporterBridge();
                    // VRM has GameObjects packed flat in glTF nodes, and there is no root Go, so it is solved by hook.
                    gltfImporter.AddHook(new VGltf.Ext.Vrm0.Unity.Hooks.ImporterHook(go, bridge));

                    // Load the Scene.
                    res.Context = await gltfImporter.ImportSceneNodes(System.Threading.CancellationToken.None);
                }
            }
            catch (Exception)
            {
                res.Dispose();
                throw;
            }

            return res;
        }

        // UI

        void UIOnLoadButtonClick()
        {
            UIOnLoadButtonClickAsync().Forget();
        }

        async UniTaskVoid UIOnLoadButtonClickAsync()
        {
            var p0 = Common.MemoryProfile.Now;
            DebugLogProfile(p0);

            var res = await LoadVRM();
            _vrmResources.Insert(0, res);

            // Start animations
            var anim = res.Go.GetComponentInChildren<Animator>();
            anim.runtimeAnimatorController = RuntimeAnimatorController;

            var p1 = Common.MemoryProfile.Now;
            DebugLogProfile(p1, p0);
        }

        void UIOnUnloadButtonClick()
        {
            if (_vrmResources.Count == 0)
            {
                return;
            }

            var p0 = Common.MemoryProfile.Now;
            DebugLogProfile(p0);

            var head = _vrmResources[0];
            _vrmResources.RemoveAt(0);

            head.Dispose();

            var p1 = Common.MemoryProfile.Now;
            DebugLogProfile(p1, p0);
        }

        void UIOnExportButtonClicked()
        {
            UIOnExportButtonClickedAsync().Forget();
        }

        async UniTaskVoid UIOnExportButtonClickedAsync()
        {
            if (_vrmResources.Count == 0)
            {
                return;
            }

            var head = _vrmResources[0];

            var anim = head.Go.GetComponentInChildren<Animator>();
            var animCtrl = anim.runtimeAnimatorController;
            anim.runtimeAnimatorController = null; // Make the model to the rest pose

            GltfContainer gltfContainer = null;
            try
            {
                // For some reason, VRM0 inverts the coordinate system of glTF in the Z axis.
                var config = new Exporter.Config
                {
                    FlipZAxisInsteadOfXAsix = true,
                };

                using (var gltfExporter = new Exporter(config))
                {
                    var bridge = new VRM0ExporterBridge();
                    gltfExporter.AddHook(new VGltf.Ext.Vrm0.Unity.Hooks.ExporterHook(bridge));

                    // In some implementations of VRM, specifying multiple shape keys in the blendshape proxy may not work correctly, so they should be unified.
                    using (var unifier = new VGltf.Ext.Vrm0.Unity.Filter.BlendshapeUnifier())
                    {
                        unifier.Unify(head.Go);

                        gltfExporter.ExportGameObjectAsScene(unifier.Go);
                    }

                    gltfContainer = gltfExporter.IntoGlbContainer();
                }
            }
            finally
            {
                anim.runtimeAnimatorController = animCtrl;
            }

            var filePath = outputFilePathInput.text;
            await Task.Run(() =>
            {
                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    GltfContainer.ToGlb(fs, gltfContainer);
                }

                Debug.Log("exported");
            });
        }

        void DebugLogProfile(Common.MemoryProfile now, Common.MemoryProfile prev = null)
        {
            Debug.Log($"----------");
            Debug.Log($"(totalReservedMB, totalAllocatedMB, totalUnusedReservedMB)");
            Debug.Log($"({now.TotalReservedMB}MB,  {now.TotalAllocatedMB}MB, {now.TotalUnusedReservedMB}MB");

            if (prev != null)
            {
                Debug.Log($"delta ({now.TotalReservedMB - prev.TotalReservedMB}MB, {now.TotalAllocatedMB - prev.TotalAllocatedMB}MB, {now.TotalUnusedReservedMB - prev.TotalUnusedReservedMB}MB)");
            }
        }
    }
}

