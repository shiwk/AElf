﻿namespace AElf.Network.Peers
{
    public interface ITimedRequest
    {
        void Start();
        bool IsCanceled { get; }
    }
}