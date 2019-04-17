using System;
using System.Runtime.InteropServices;
using CoreFoundation;
using CoreBluetooth;
using AVFoundation;
using Foundation;
using ObjCRuntime;
using Security;

namespace NearbyMessages
{

    [Native]
    public enum GNSOperationStatus : ulong
    {
        Inactive,
        Starting,
        Active
    }

    [Native, Flags]
    public enum GNSDiscoveryMode : long
    {
        Broadcast = 1 << 0,
        Scan = 1 << 1,
        Default = Broadcast | Scan
    }

    [Native, Flags]
    public enum GNSDiscoveryMediums : long
    {
        Audio = 1 << 0,
        Ble = 1 << 1,
        Default = Audio | Ble
    }

    [Native]
    public enum GNSDeviceTypes : long
    {
        UsingNearby = 1 << 0,
        BLEBeacon = 1 << 1
    }
}