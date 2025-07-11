/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Meta.XR.Samples;
using UnityEngine;

/// <summary>
/// Demonstrates loading existing spatial anchors from storage.
/// </summary>
/// <remarks>
/// Loading existing anchors involves two asynchronous methods:
/// 1. Call <see cref="OVRSpatialAnchor.LoadUnboundAnchorsAsync"/>
/// 2. For each unbound anchor you wish to localize, invoke <see cref="OVRSpatialAnchor.UnboundAnchor.Localize"/>.
/// 3. Once localized, your callback will receive an <see cref="OVRSpatialAnchor.UnboundAnchor"/>. Instantiate an
/// <see cref="OVRSpatialAnchor"/> component and bind it to the `UnboundAnchor` by calling
/// <see cref="OVRSpatialAnchor.UnboundAnchor.BindTo"/>.
/// </remarks>
[MetaCodeSample("StarterSample-SpatialAnchor")]
public class SpatialAnchorLoader : MonoBehaviour
{
    [SerializeField]
    OVRSpatialAnchor _anchorPrefab;

    Action<bool, OVRSpatialAnchor.UnboundAnchor> _onAnchorLocalized;

    readonly List<OVRSpatialAnchor.UnboundAnchor> _unboundAnchors = new();

    public async void LoadAnchorsByUuid()
    {
        var uuids = AnchorUuidStore.Uuids;
        if (uuids.Count == 0)
        {
            LogWarning($"There are no anchors to load.");
            return;
        }

        var batchCount = Math.Ceiling((float)uuids.Count / 50);
        for (int i = 0, batchIndex = 1; i < uuids.Count; i += 50, batchIndex++)
        {
            var uuidBatch = uuids.Skip(i).Take(50);
            Log($"Attempting to load batch {batchIndex} of {batchCount} with {uuidBatch.Count()} anchor(s) by UUID: " +
                $"[{string.Join($", ", uuidBatch.Select(uuid => uuid.ToString()))}]");

            var result = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(uuidBatch, _unboundAnchors);
            if (result.Success)
            {
                ProcessUnboundAnchors(result.Value);
            }
            else
            {
                LogError($"{nameof(OVRSpatialAnchor.LoadUnboundAnchorsAsync)} failed with error {result.Status}.");
            }
        }
    }

    private void Awake()
    {
        _onAnchorLocalized = OnLocalized;
    }

    private void ProcessUnboundAnchors(IReadOnlyList<OVRSpatialAnchor.UnboundAnchor> unboundAnchors)
    {
        Log($"{nameof(OVRSpatialAnchor.LoadUnboundAnchorsAsync)} found {unboundAnchors.Count} unbound anchors: " +
            $"[{string.Join(", ", unboundAnchors.Select(a => a.Uuid.ToString()))}]");

        foreach (var anchor in unboundAnchors)
        {
            if (anchor.Localized)
            {
                _onAnchorLocalized(true, anchor);
            }
            else if (!anchor.Localizing)
            {
                anchor.LocalizeAsync().ContinueWith(_onAnchorLocalized, anchor);
            }
        }
    }

    private void OnLocalized(bool success, OVRSpatialAnchor.UnboundAnchor unboundAnchor)
    {
        if (!success)
        {
            LogError($"{unboundAnchor} Localization failed!");
            return;
        }

        string uuidKey = unboundAnchor.Uuid.ToString();
        Debug.Log($"Localized anchor with UUID: {uuidKey}");
        GameObject prefab = null;
        if (PlayerPrefs.HasKey(uuidKey))
        {
            string prefName = PlayerPrefs.GetString(uuidKey);
            Debug.Log($"PlayerPrefs HAS uuid {uuidKey} with value {prefName}");
            prefab = Resources.Load<GameObject>($"prefabs/{prefName}");
        }

        Debug.Log($"Prefab is null ? {prefab == null}");

        OVRSpatialAnchor objAnchor = prefab != null ? prefab.GetComponent<OVRSpatialAnchor>() : _anchorPrefab;



        var isPoseValid = unboundAnchor.TryGetPose(out var pose);
        if (!isPoseValid)
        {
            Debug.LogWarning("Unable to acquire initial anchor pose. Instantiating prefab at the origin.");
        }

        var spatialAnchor = isPoseValid
            ? Instantiate(objAnchor, pose.position, pose.rotation)
            : Instantiate(objAnchor);
        unboundAnchor.BindTo(spatialAnchor);

        if (spatialAnchor.TryGetComponent<Anchor>(out var anchor))
        {
            // We just loaded it, so we know it exists in persistent storage.
            anchor.ShowSaveIcon = true;
        }
    }

    private static void Log(LogType logType, object message)
        => Debug.unityLogger.Log(logType, "[SpatialAnchorSample]", message);

    private static void Log(object message) => Log(LogType.Log, message);

    private static void LogWarning(object message) => Log(LogType.Warning, message);

    private static void LogError(object message) => Log(LogType.Error, message);
}
