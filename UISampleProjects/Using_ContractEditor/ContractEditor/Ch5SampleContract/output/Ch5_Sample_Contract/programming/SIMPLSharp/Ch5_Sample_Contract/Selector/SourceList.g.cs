using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro;

namespace Ch5_Sample_Contract.Selector
{
    /// <summary>
    /// The number of active sources in a list
    /// </summary>
    public interface ISourceList
    {
        object UserObject { get; set; }

        void NumberOfSources(SourceListUShortInputSigDelegate callback);

        Ch5_Sample_Contract.Selector.ISource[] Sources { get; }
    }

    public delegate void SourceListUShortInputSigDelegate(UShortInputSig uShortInputSig, ISourceList sourceList);

    internal class SourceList : ISourceList, IDisposable
    {
        #region Standard CH5 Component members

        private ComponentMediator ComponentMediator { get; set; }

        public object UserObject { get; set; }

        public uint ControlJoinId { get; private set; }

        private IList<BasicTriListWithSmartObject> _devices;
        public IList<BasicTriListWithSmartObject> Devices { get { return _devices; } }

        #endregion

        #region Joins

        private static class Joins
        {
            internal static class Numerics
            {

                public const uint NumberOfSources = 1;
            }
        }

        #endregion

        #region Construction and Initialization

        internal SourceList(ComponentMediator componentMediator, uint controlJoinId)
        {
            ComponentMediator = componentMediator;
            Initialize(controlJoinId);
        }

        private static readonly IDictionary<uint, List<uint>> SourcesSmartObjectIdMappings = new Dictionary<uint, List<uint>> {
            { 32, new List<uint> { 33, 34, 35, 36, 37, 38, 39, 40 } }};

        internal static void ClearDictionaries()
        {
            SourcesSmartObjectIdMappings.Clear();
        }

        private void Initialize(uint controlJoinId)
        {
            ControlJoinId = controlJoinId; 
 
            _devices = new List<BasicTriListWithSmartObject>(); 
 
            List<uint> sourcesList = SourcesSmartObjectIdMappings[controlJoinId];
            Sources = new Ch5_Sample_Contract.Selector.ISource[sourcesList.Count];
            for (int index = 0; index < sourcesList.Count; index++)
            {
                Sources[index] = new Ch5_Sample_Contract.Selector.Source(ComponentMediator, sourcesList[index]); 
            }

        }

        public void AddDevice(BasicTriListWithSmartObject device)
        {
            Devices.Add(device);
            ComponentMediator.HookSmartObjectEvents(device.SmartObjects[ControlJoinId]);
            for (int index = 0; index < Sources.Length; index++)
            {
                ((Ch5_Sample_Contract.Selector.Source)Sources[index]).AddDevice(device);
            }
        }

        public void RemoveDevice(BasicTriListWithSmartObject device)
        {
            Devices.Remove(device);
            ComponentMediator.UnHookSmartObjectEvents(device.SmartObjects[ControlJoinId]);
            for (int index = 0; index < Sources.Length; index++)
            {
                ((Ch5_Sample_Contract.Selector.Source)Sources[index]).RemoveDevice(device);
            }
        }

        #endregion

        #region CH5 Contract

        public Ch5_Sample_Contract.Selector.ISource[] Sources { get; private set; }


        public void NumberOfSources(SourceListUShortInputSigDelegate callback)
        {
            for (int index = 0; index < Devices.Count; index++)
            {
                callback(Devices[index].SmartObjects[ControlJoinId].UShortInput[Joins.Numerics.NumberOfSources], this);
            }
        }

        #endregion

        #region Overrides

        public override int GetHashCode()
        {
            return (int)ControlJoinId;
        }

        public override string ToString()
        {
            return string.Format("Contract: {0} Component: {1} HashCode: {2} {3}", "SourceList", GetType().Name, GetHashCode(), UserObject != null ? "UserObject: " + UserObject : null);
        }

        #endregion

        #region IDisposable

        public bool IsDisposed { get; set; }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            for (int index = 0; index < Sources.Length; index++)
            {
                ((Ch5_Sample_Contract.Selector.Source)Sources[index]).Dispose();
            }

        }

        #endregion

    }
}
