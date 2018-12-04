﻿using System;
using System.Threading.Tasks;
using System.Timers;
using AElf.Configuration;
using AElf.Configuration.Config.Management;
using AElf.Management.Interfaces;

namespace AElf.Management.Services
{
    public class RecordService : IRecordService
    {
        private readonly IChainService _chainService;
        private readonly ITransactionService _transactionService;
        private readonly INodeService _nodeService;
        private readonly INetworkService _networkService;
        private readonly Timer _timer;

        public RecordService(IChainService chainService, ITransactionService transactionService, INodeService nodeService, INetworkService networkService)
        {
            _chainService = chainService;
            _transactionService = transactionService;
            _nodeService = nodeService;
            _networkService = networkService;
            _timer = new Timer(ManagementConfig.Instance.MonitoringInterval * 1000);
            _timer.Elapsed += TimerOnElapsed;
        }

        public void Start()
        {
            // Todo we should move it to monitor project,management website just receive and record
            _timer.Start();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            var time = DateTime.Now;
//            Parallel.ForEach(ServiceUrlConfig.Instance.ServiceUrls.Keys, chainId =>
//                {
//                    var txPoolSize = _transactionService.GetPoolSize(chainId);
//                    _transactionService.RecordPoolSize(chainId, time, txPoolSize);
//
//                    var isAlive = _nodeService.IsAlive(chainId);
//                    var isForked = _nodeService.IsForked(chainId);
//                    _nodeService.RecordPoolState(chainId, time, isAlive, isForked);
//
////                    var networkState = _networkService.GetPoolState(chainId);
////                    _networkService.RecordPoolState(chainId, time, networkState.RequestPoolSize, networkState.ReceivePoolSize);
//                    
//                    _nodeService.RecordBlockInfo(chainId);
//                }
//            );

            foreach (var chainId in ServiceUrlConfig.Instance.ServiceUrls.Keys)
            {
                try
                {
                    var txPoolSize = _transactionService.GetPoolSize(chainId);
                    _transactionService.RecordPoolSize(chainId, time, txPoolSize);

                    var isAlive = _nodeService.IsAlive(chainId);
                    var isForked = _nodeService.IsForked(chainId);
                    _nodeService.RecordPoolState(chainId, time, isAlive, isForked);
                    
                    _nodeService.RecordBlockInfo(chainId);
                }
                catch (Exception exception)
                {
                    //Console.WriteLine(exception);
                }
            }
        }
    }
}