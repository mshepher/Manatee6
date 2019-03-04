using System;
using CoreFoundation;
using CoreBluetooth;
using Foundation;
using AVFoundation;
using ObjCRuntime;
using Security;

//[assembly: LinkWith("libGNSMessages.a", LinkTarget.ArmV7 | LinkTarget.Arm64 | LinkTarget.ArmV6 | LinkTarget.x86_64 | LinkTarget.ArmV7s, Frameworks = "CoreBluetooth AVFoundation", ForceLoad = true)]

namespace NearbyMessages
{
	// @interface GNSBeaconStrategyParams : NSObject
	[BaseType (typeof(NSObject))]
	interface GNSBeaconStrategyParams
	{
		// @property (nonatomic) BOOL includeIBeacons;
		[Export ("includeIBeacons")]
		bool IncludeIBeacons { get; set; }

		// @property (nonatomic) BOOL allowInBackground;
		[Export ("allowInBackground")]
		bool AllowInBackground { get; set; }

		// @property (nonatomic) BOOL lowPowerPreferred;
		[Export ("lowPowerPreferred")]
		bool LowPowerPreferred { get; set; }
	}

	// @interface GNSBeaconStrategy : NSObject <NSCopying>
	[BaseType (typeof(NSObject))]
	interface GNSBeaconStrategy : INSCopying
	{
		// @property (readonly, nonatomic) BOOL includeIBeacons;
		[Export ("includeIBeacons")]
		bool IncludeIBeacons { get; }

		// @property (readonly, nonatomic) BOOL allowInBackground;
		[Export ("allowInBackground")]
		bool AllowInBackground { get; }

		// @property (readonly, nonatomic) BOOL lowPowerPreferred;
		[Export ("lowPowerPreferred")]
		bool LowPowerPreferred { get; }

		// +(instancetype)strategy;
		[Static]
		[Export ("strategy")]
		GNSBeaconStrategy Strategy ();

		// +(instancetype)strategyWithParamsBlock:(void (^)(GNSBeaconStrategyParams *))paramsBlock;
		[Static]
		[Export ("strategyWithParamsBlock:")]
		GNSBeaconStrategy StrategyWithParamsBlock (Action<GNSBeaconStrategyParams> paramsBlock);
	}

	// @interface GNSMessage : NSObject
	[BaseType (typeof(NSObject))]
	[DisableDefaultCtor]
	interface GNSMessage
	{
		// @property (readonly, copy, nonatomic) NSString * messageNamespace;
		[Export ("messageNamespace")]
		string MessageNamespace { get; }

		// @property (readonly, copy, nonatomic) NSString * type;
		[Export ("type")]
		string Type { get; }

		// @property (readonly, copy, nonatomic) NSData * content;
		[Export ("content", ArgumentSemantic.Copy)]
		NSData Content { get; }

		// +(instancetype)messageWithContent:(NSData *)content;
		[Static]
		[Export ("messageWithContent:")]
		GNSMessage MessageWithContent (NSData content);

		// +(instancetype)messageWithContent:(NSData *)content type:(NSString *)type;
		[Static]
		[Export ("messageWithContent:type:")]
		GNSMessage MessageWithContent (NSData content, string type);
	}

	// typedef void (^GNSErrorStateHandler)(BOOL);
	delegate void GNSErrorStateHandler (bool error);

	// typedef void (^GNSMessageHandler)(GNSMessage *);
	delegate void GNSMessageHandler (GNSMessage message);

	// typedef void (^GNSMessageManagerParamsBlock)(GNSMessageManagerParams *);
    delegate void GNSMessageManagerParamsBlock (GNSMessageManagerParams managerParams);

	// typedef void (^GNSPublicationParamsBlock)(GNSPublicationParams *);
    delegate void GNSPublicationParamsBlock (GNSPublicationParams publicationParams);

	// typedef void (^GNSSubscriptionParamsBlock)(GNSSubscriptionParams *);
    delegate void GNSSubscriptionParamsBlock (GNSSubscriptionParams subscriptionParams);

	// @protocol GNSPublication <NSObject>
	[Protocol, Model]
	[BaseType (typeof(NSObject))]
	interface GNSPublication
	{
	}

	// @protocol GNSSubscription <NSObject>
	[Protocol, Model]
	[BaseType (typeof(NSObject))]
	interface GNSSubscription
	{
	}

	// @interface GNSMessageManagerParams : NSObject
	[BaseType (typeof(NSObject))]
	interface GNSMessageManagerParams
	{
		// @property (getter = shouldShowBluetoothPowerAlert, nonatomic) BOOL showBluetoothPowerAlert;
		[Export ("showBluetoothPowerAlert")]
		bool ShowBluetoothPowerAlert { [Bind ("shouldShowBluetoothPowerAlert")] get; set; }

		// @property (copy, nonatomic) GNSErrorStateHandler microphonePermissionErrorHandler;
		[Export ("microphonePermissionErrorHandler", ArgumentSemantic.Copy)]
		GNSErrorStateHandler MicrophonePermissionErrorHandler { get; set; }

		// @property (copy, nonatomic) GNSErrorStateHandler bluetoothPermissionErrorHandler;
		[Export ("bluetoothPermissionErrorHandler", ArgumentSemantic.Copy)]
		GNSErrorStateHandler BluetoothPermissionErrorHandler { get; set; }

		// @property (copy, nonatomic) GNSErrorStateHandler bluetoothPowerErrorHandler;
		[Export ("bluetoothPowerErrorHandler", ArgumentSemantic.Copy)]
		GNSErrorStateHandler BluetoothPowerErrorHandler { get; set; }

		// @property (getter = shouldUseBestAudioSessionCategory, nonatomic) BOOL useBestAudioSessionCategory;
		[Export ("useBestAudioSessionCategory")]
		bool UseBestAudioSessionCategory { [Bind ("shouldUseBestAudioSessionCategory")] get; set; }
	}

	// @interface GNSMessageManager : NSObject
	[BaseType (typeof(NSObject))]
	interface GNSMessageManager
	{
		// -(instancetype)initWithAPIKey:(NSString *)apiKey;
		[Export ("initWithAPIKey:")]
		IntPtr Constructor (string apiKey);

		// -(instancetype)initWithAPIKey:(NSString *)apiKey paramsBlock:(GNSMessageManagerParamsBlock)paramsBlock;
		[Export ("initWithAPIKey:paramsBlock:")]
		IntPtr Constructor (string apiKey, GNSMessageManagerParamsBlock paramsBlock);

		// -(id<GNSPublication>)publicationWithMessage:(GNSMessage *)message;
		[Export ("publicationWithMessage:")]
		GNSPublication PublicationWithMessage (GNSMessage message);

		// -(id<GNSPublication>)publicationWithMessage:(GNSMessage *)message paramsBlock:(GNSPublicationParamsBlock)paramsBlock;
		[Export ("publicationWithMessage:paramsBlock:")]
		GNSPublication PublicationWithMessage (GNSMessage message, GNSPublicationParamsBlock paramsBlock);

		// -(id<GNSSubscription>)subscriptionWithMessageFoundHandler:(GNSMessageHandler)messageFoundHandler messageLostHandler:(GNSMessageHandler)messageLostHandler;
		[Export ("subscriptionWithMessageFoundHandler:messageLostHandler:")]
		GNSSubscription SubscriptionWithMessageFoundHandler (GNSMessageHandler messageFoundHandler, GNSMessageHandler messageLostHandler);

		// -(id<GNSSubscription>)subscriptionWithMessageFoundHandler:(GNSMessageHandler)messageFoundHandler messageLostHandler:(GNSMessageHandler)messageLostHandler paramsBlock:(GNSSubscriptionParamsBlock)paramsBlock;
		[Export ("subscriptionWithMessageFoundHandler:messageLostHandler:paramsBlock:")]
		GNSSubscription SubscriptionWithMessageFoundHandler (GNSMessageHandler messageFoundHandler, GNSMessageHandler messageLostHandler, GNSSubscriptionParamsBlock paramsBlock);

		// +(void)setDebugLoggingEnabled:(BOOL)enabled;
		[Static]
		[Export ("setDebugLoggingEnabled:")]
		void SetDebugLoggingEnabled (bool enabled);

		// +(BOOL)isDebugLoggingEnabled;
		[Static]
		[Export ("isDebugLoggingEnabled")]
		bool IsDebugLoggingEnabled { get; }
	}

	// typedef void (^GNSOperationStatusHandler)(GNSOperationStatus);
	delegate void GNSOperationStatusHandler (GNSOperationStatus status);

	// typedef void (^GNSPermissionHandler)(BOOL);
	delegate void GNSPermissionHandler (bool permissionGranted);

	// typedef void (^GNSPermissionRequestHandler)(GNSPermissionHandler);
	delegate void GNSPermissionRequestHandler ([BlockCallback]GNSPermissionHandler handler);

	// @interface GNSPermission : NSObject
	[BaseType (typeof(NSObject))]
	interface GNSPermission
	{
		// -(instancetype)initWithChangedHandler:(GNSPermissionHandler)changedHandler;
		[Export ("initWithChangedHandler:")]
		IntPtr Constructor (GNSPermissionHandler changedHandler);

		// +(BOOL)isGranted;
		[Static]
		[Export ("isGranted")]
		bool IsGranted { get; }

		// +(void)setGranted:(BOOL)granted;
		[Static]
		[Export ("setGranted:")]
		void SetGranted (bool granted);
	}

	// @interface GNSPublicationParams : NSObject <NSCopying>
	[BaseType (typeof(NSObject))]
	interface GNSPublicationParams : INSCopying
	{
		// @property (copy, nonatomic) GNSStrategy * strategy;
		[Export ("strategy", ArgumentSemantic.Copy)]
		GNSStrategy Strategy { get; set; }

		// @property (copy, nonatomic) GNSOperationStatusHandler statusHandler;
		[Export ("statusHandler", ArgumentSemantic.Copy)]
		GNSOperationStatusHandler StatusHandler { get; set; }

		// @property (copy, nonatomic) GNSPermissionRequestHandler permissionRequestHandler;
		[Export ("permissionRequestHandler", ArgumentSemantic.Copy)]
		GNSPermissionRequestHandler PermissionRequestHandler { get; set; }
	}

	// @interface GNSStrategyParams : NSObject
	[BaseType (typeof(NSObject))]
	interface GNSStrategyParams
	{
		// @property (nonatomic) GNSDiscoveryMode discoveryMode;
		[Export ("discoveryMode", ArgumentSemantic.Assign)]
		GNSDiscoveryMode DiscoveryMode { get; set; }

		// @property (nonatomic) GNSDiscoveryMediums discoveryMediums;
		[Export ("discoveryMediums", ArgumentSemantic.Assign)]
		GNSDiscoveryMediums DiscoveryMediums { get; set; }

		// @property (nonatomic) BOOL allowInBackground;
		[Export ("allowInBackground")]
		bool AllowInBackground { get; set; }
	}

	// @interface GNSStrategy : NSObject <NSCopying>
	[BaseType (typeof(NSObject))]
	interface GNSStrategy : INSCopying
	{
		// @property (readonly, nonatomic) GNSDiscoveryMode discoveryMode;
		[Export ("discoveryMode")]
		GNSDiscoveryMode DiscoveryMode { get; }

		// @property (readonly, nonatomic) GNSDiscoveryMediums discoveryMediums;
		[Export ("discoveryMediums")]
		GNSDiscoveryMediums DiscoveryMediums { get; }

		// @property (readonly, nonatomic) BOOL allowInBackground;
		[Export ("allowInBackground")]
		bool AllowInBackground { get; }

		// +(instancetype)strategy;
		[Static]
		[Export ("strategy")]
		GNSStrategy Strategy ();

		// +(instancetype)strategyWithParamsBlock:(void (^)(GNSStrategyParams *))paramsBlock;
		[Static]
		[Export ("strategyWithParamsBlock:")]
		GNSStrategy StrategyWithParamsBlock (Action<GNSStrategyParams> paramsBlock);
	}

	// @interface GNSSubscriptionParams : NSObject <NSCopying>
	[BaseType (typeof(NSObject))]
	interface GNSSubscriptionParams : INSCopying
	{
		// @property (nonatomic) GNSDeviceTypes deviceTypesToDiscover;
		[Export ("deviceTypesToDiscover", ArgumentSemantic.Assign)]
		GNSDeviceTypes DeviceTypesToDiscover { get; set; }

		// @property (copy, nonatomic) NSString * messageNamespace;
		[Export ("messageNamespace")]
		string MessageNamespace { get; set; }

		// @property (copy, nonatomic) NSString * type;
		[Export ("type")]
		string Type { get; set; }

		// @property (copy, nonatomic) GNSStrategy * strategy;
		[Export ("strategy", ArgumentSemantic.Copy)]
		GNSStrategy Strategy { get; set; }

		// @property (copy, nonatomic) GNSBeaconStrategy * beaconStrategy;
		[Export ("beaconStrategy", ArgumentSemantic.Copy)]
		GNSBeaconStrategy BeaconStrategy { get; set; }

		// @property (copy, nonatomic) GNSOperationStatusHandler statusHandler;
		[Export ("statusHandler", ArgumentSemantic.Copy)]
		GNSOperationStatusHandler StatusHandler { get; set; }

		// @property (copy, nonatomic) GNSPermissionRequestHandler permissionRequestHandler;
		[Export ("permissionRequestHandler", ArgumentSemantic.Copy)]
		GNSPermissionRequestHandler PermissionRequestHandler { get; set; }
	}
}
