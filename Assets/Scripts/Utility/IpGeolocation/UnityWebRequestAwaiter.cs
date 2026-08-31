using System;
using System.Runtime.CompilerServices;
using UnityEngine.Networking;

namespace WormWars.Core
{
    // UnityWebRequestAsyncOperation has no built-in awaiter on older Unity/.NET Standard
    // targets, so `await request.SendWebRequest()` needs this GetAwaiter extension.
    public static class UnityWebRequestAwaiterExtensions
    {
        public static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp) => new(asyncOp);
    }

    public readonly struct UnityWebRequestAwaiter : INotifyCompletion
    {
        readonly UnityWebRequestAsyncOperation _asyncOp;

        public UnityWebRequestAwaiter(UnityWebRequestAsyncOperation asyncOp) => _asyncOp = asyncOp;

        public bool IsCompleted => _asyncOp.isDone;

        public void GetResult() { }

        public void OnCompleted(Action continuation) => _asyncOp.completed += _ => continuation();
    }
}
