using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Jobs;
using Unity.Burst;

public class JobSystemSample : MonoBehaviour
{
    struct SampleJob : IJob
    {
        public int value1;
        public int value2;
        public NativeArray<int> result;

        public void Execute()
        {
            for(var idx = 0; idx < result.Length; ++idx)
            {
                result[idx] = value1 + value2;
            }
        }
    }

    struct SampleParallelJob : IJobParallelFor
    {
        public NativeArray<int> value1;
        public NativeArray<int> value2;
        public NativeArray<int> result;

        public void Execute(int i)
        {
            result[i] = value1[i] + value2[i];
        }
    }

    // [BurstCompile]
    struct SampleParallelTransformJob : IJobParallelForTransform
    {
        public NativeArray<Vector3> positions;
        public void Execute(int idx, TransformAccess transform)
        {
            var pos = positions[idx];
            transform.localPosition = pos;
        }
    }

    [SerializeField]
    private int processCount = 1000;

    [SerializeField]
    private Transform prefab = null;

    private TransformAccessArray prefabTransformAccessArray;

    void Start()
    {
        CreatePrefab();
    }

    /// <summary>
    /// Prefab生成。
    /// </summary>
    void CreatePrefab()
    {
        var transforms = new Transform[processCount];
        for(var idx = 0; idx < processCount; ++idx)
        {
            var prefabInstance = Instantiate(prefab);
            transforms[idx] = prefabInstance;
        }
        prefabTransformAccessArray = new TransformAccessArray(transforms);
    }

    void OnDestroy()
    {
        prefabTransformAccessArray.Dispose();
    }

    void Update()
    {
        // ExecuteSample();
        // ExecuteSampleJob();
        // ExecuteSampleParallel();
        // ExecuteSampleParallelJob();
        // ExecuteSampleParallelTransform();
        // ExecuteSampleParallelTransformJob();
    }

    /// <summary>
    /// IJobの実行。
    /// </summary>
    void ExecuteSampleJob()
    {
        // バッファ生成。
        var result = new NativeArray<int>(processCount, Allocator.TempJob);

        // ジョブ生成。
        var sampleJob = new SampleJob();
        sampleJob.value1 = 10;
        sampleJob.value2 = 20;
        sampleJob.result = result;

        // ジョブ実行。
        var jobHandle = sampleJob.Schedule();

        // ジョブ完了待機。
        jobHandle.Complete();

        // バッファの破棄。
        result.Dispose();
    }

    /// <summary>
    /// IJobのJobなし実行。
    /// </summary>
    void ExecuteSample()
    {
        var result = new NativeArray<int>(processCount, Allocator.TempJob);
        var value1 = 10;
        var value2 = 20;

        for(var idx = 0; idx < result.Length; ++idx)
        {
            result[idx] = value1 + value2;
        }
        result.Dispose();
    }

    /// <summary>
    /// IJobPrallelForの実行。
    /// </summary>
    void ExecuteSampleParallelJob()
    {
        // バッファ生成。
        var result = new NativeArray<int>(processCount, Allocator.TempJob);
        var value1 = new NativeArray<int>(processCount, Allocator.TempJob);
        var value2 = new NativeArray<int>(processCount, Allocator.TempJob);

        for(var idx = 0; idx < value1.Length; ++idx)
        {
            value1[idx] = idx;
        }

        for(var idx = 0; idx < value2.Length; ++idx)
        {
            value2[idx] = idx;
        }

        // ジョブ生成。
        var parallelJob = new SampleParallelJob();

        parallelJob.value1 = value1;
        parallelJob.value2 = value2;
        parallelJob.result = result;

        // ジョブ実行。
        var jobHandle = parallelJob.Schedule(result.Length, 1);

        // ジョブ完了待機。
        jobHandle.Complete();

        // バッファの破棄。
        value1.Dispose();
        value2.Dispose();
        result.Dispose();
    }

    /// <summary>
    /// IJobPrallelForのJobなし実行。
    /// </summary>
    void ExecuteSampleParallel()
    {
        // バッファ生成。
        var result = new NativeArray<int>(processCount, Allocator.TempJob);
        var value1 = new NativeArray<int>(processCount, Allocator.TempJob);
        var value2 = new NativeArray<int>(processCount, Allocator.TempJob);

        for(var idx = 0; idx < value1.Length; ++idx)
        {
            value1[idx] = idx;
        }

        for(var idx = 0; idx < value2.Length; ++idx)
        {
            value2[idx] = idx;
        }

        for(var idx = 0; idx < result.Length; ++idx)
        {
            result[idx] = value1[idx] + value2[idx];
        }

        // バッファの破棄。
        value1.Dispose();
        value2.Dispose();
        result.Dispose();
    }

    /// <summary>
    /// IJobParallelForTransformの実行。
    /// </summary>
    void ExecuteSampleParallelTransformJob()
    {
        var positions = new NativeArray<Vector3>(prefabTransformAccessArray.length, Allocator.TempJob);

        for(var idx = 0; idx < prefabTransformAccessArray.length; ++idx)
        {
            positions[idx] = new Vector3(Random.Range(-15, 15), Random.Range(-15, 15), Random.Range(-15, 15));
        }

        var sampleParallelTransformJob = new SampleParallelTransformJob();
        sampleParallelTransformJob.positions = positions;

        var jobHandle = sampleParallelTransformJob.Schedule(prefabTransformAccessArray);
        jobHandle.Complete();

        positions.Dispose();
    }

    /// <summary>
    /// IJobParallelForTransformのJobなし実行。
    /// </summary>
    void ExecuteSampleParallelTransform()
    {
        for(var idx = 0; idx < prefabTransformAccessArray.length; ++idx)
        {
            prefabTransformAccessArray[idx].localPosition = new Vector3(Random.Range(-15, 15), Random.Range(-15, 15), Random.Range(-15, 15));
        }
    }
}
