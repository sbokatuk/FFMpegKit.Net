using System;
using CoreFoundation;
using Foundation;
using ObjCRuntime;

namespace Ffmpegkit.Ios
{
    // @interface Log : NSObject
    [BaseType(typeof(NSObject))]
    interface Log
    {
        // -(instancetype)init:(long)sessionId :(int)level :(NSString *)message;
        [Export("init:::")]
        NativeHandle Constructor(nint sessionId, int level, string message);

        // -(long)getSessionId;
        [Export("getSessionId")]
        nint SessionId { get; }

        // -(int)getLevel;
        [Export("getLevel")]
        int Level { get; }

        // -(NSString *)getMessage;
        [Export("getMessage")]
        string Message { get; }
    }

    // typedef void (^LogCallback)(Log *);
    delegate void LogCallback(Log arg0);

    // @interface ReturnCode : NSObject
    [BaseType(typeof(NSObject))]
    interface ReturnCode
    {
        // -(instancetype)init:(int)value;
        [Export("init:")]
        NativeHandle Constructor(int value);

        // +(BOOL)isSuccess:(ReturnCode *)value;
        [Static]
        [Export("isSuccess:")]
        bool IsSuccess(ReturnCode value);

        // +(BOOL)isCancel:(ReturnCode *)value;
        [Static]
        [Export("isCancel:")]
        bool IsCancel(ReturnCode value);

        // -(int)getValue;
        [Export("getValue")]
        int Value { get; }

        // -(BOOL)isValueSuccess;
        [Export("isValueSuccess")]
        bool IsValueSuccess { get; }

        // -(BOOL)isValueError;
        [Export("isValueError")]
        bool IsValueError { get; }

        // -(BOOL)isValueCancel;
        [Export("isValueCancel")]
        bool IsValueCancel { get; }
    }

    // Placeholder so the API definition itself compiles: the generator emits the real ISession
    // from the [Protocol] declaration below and discards this one. Members returning
    // id<Session> must use the protocol interface rather than the generated Session class,
    // because the concrete instances are AbstractSession subclasses.
    interface ISession
    {
    }

    // @protocol Session
    [Protocol]
    [BaseType(typeof(NSObject))]
    interface Session
    {
        // @required -(LogCallback)getLogCallback;
        [Abstract]
        [Export("getLogCallback")]
        LogCallback LogCallback { get; }

        // @required -(long)getSessionId;
        [Abstract]
        [Export("getSessionId")]
        nint SessionId { get; }

        // @required -(NSDate *)getCreateTime;
        [Abstract]
        [Export("getCreateTime")]
        NSDate CreateTime { get; }

        // @required -(NSDate *)getStartTime;
        [Abstract]
        [Export("getStartTime")]
        NSDate StartTime { get; }

        // @required -(NSDate *)getEndTime;
        [Abstract]
        [Export("getEndTime")]
        NSDate EndTime { get; }

        // @required -(long)getDuration;
        [Abstract]
        [Export("getDuration")]
        nint Duration { get; }

        // @required -(NSArray *)getArguments;
        [Abstract]
        [Export("getArguments")]
        string[] Arguments { get; }

        // @required -(NSString *)getCommand;
        [Abstract]
        [Export("getCommand")]
        string Command { get; }

        // @required -(NSArray *)getAllLogsWithTimeout:(int)waitTimeout;
        [Abstract]
        [Export("getAllLogsWithTimeout:")]
        Log[] GetAllLogsWithTimeout(int waitTimeout);

        // @required -(NSArray *)getAllLogs;
        [Abstract]
        [Export("getAllLogs")]
        Log[] AllLogs { get; }

        // @required -(NSArray *)getLogs;
        [Abstract]
        [Export("getLogs")]
        Log[] Logs { get; }

        // @required -(NSString *)getAllLogsAsStringWithTimeout:(int)waitTimeout;
        [Abstract]
        [Export("getAllLogsAsStringWithTimeout:")]
        string GetAllLogsAsStringWithTimeout(int waitTimeout);

        // @required -(NSString *)getAllLogsAsString;
        [Abstract]
        [Export("getAllLogsAsString")]
        string AllLogsAsString { get; }

        // @required -(NSString *)getLogsAsString;
        [Abstract]
        [Export("getLogsAsString")]
        string LogsAsString { get; }

        // @required -(NSString *)getOutput;
        [Abstract]
        [Export("getOutput")]
        string Output { get; }

        // @required -(SessionState)getState;
        [Abstract]
        [Export("getState")]
        SessionState State { get; }

        // @required -(ReturnCode *)getReturnCode;
        [Abstract]
        [Export("getReturnCode")]
        ReturnCode ReturnCode { get; }

        // @required -(NSString *)getFailStackTrace;
        [Abstract]
        [Export("getFailStackTrace")]
        string FailStackTrace { get; }

        // @required -(LogRedirectionStrategy)getLogRedirectionStrategy;
        [Abstract]
        [Export("getLogRedirectionStrategy")]
        LogRedirectionStrategy LogRedirectionStrategy { get; }

        // @required -(BOOL)thereAreAsynchronousMessagesInTransmit;
        [Abstract]
        [Export("thereAreAsynchronousMessagesInTransmit")]
        bool ThereAreAsynchronousMessagesInTransmit { get; }

        // @required -(void)addLog:(Log *)log;
        [Abstract]
        [Export("addLog:")]
        void AddLog(Log log);

        // @required -(void)startRunning;
        [Abstract]
        [Export("startRunning")]
        void StartRunning();

        // @required -(void)complete:(ReturnCode *)returnCode;
        [Abstract]
        [Export("complete:")]
        void Complete(ReturnCode returnCode);

        // @required -(void)fail:(NSException *)exception;
        [Abstract]
        [Export("fail:")]
        void Fail(NSException exception);

        // @required -(BOOL)isFFmpeg;
        [Abstract]
        [Export("isFFmpeg")]
        bool IsFFmpeg { get; }

        // @required -(BOOL)isFFprobe;
        [Abstract]
        [Export("isFFprobe")]
        bool IsFFprobe { get; }

        // @required -(BOOL)isMediaInformation;
        [Abstract]
        [Export("isMediaInformation")]
        bool IsMediaInformation { get; }

        // @required -(void)cancel;
        [Abstract]
        [Export("cancel")]
        void Cancel();
    }

    // @interface AbstractSession : NSObject <Session>
    [BaseType(typeof(NSObject))]
    interface AbstractSession : Session
    {
        // extern const int AbstractSessionDefaultTimeoutForAsynchronousMessagesInTransmit;
        [Field("AbstractSessionDefaultTimeoutForAsynchronousMessagesInTransmit", "__Internal")]
        int DefaultTimeoutForAsynchronousMessagesInTransmit { get; }

        // -(instancetype)init:(NSArray *)arguments withLogCallback:(LogCallback)logCallback withLogRedirectionStrategy:(LogRedirectionStrategy)logRedirectionStrategy;
        [Export("init:withLogCallback:withLogRedirectionStrategy:")]
        NativeHandle Constructor(string[] arguments, LogCallback logCallback, LogRedirectionStrategy logRedirectionStrategy);

        // -(void)waitForAsynchronousMessagesInTransmit:(int)timeout;
        [Export("waitForAsynchronousMessagesInTransmit:")]
        void WaitForAsynchronousMessagesInTransmit(int timeout);
    }

    // @interface ArchDetect : NSObject
    [BaseType(typeof(NSObject))]
    interface ArchDetect
    {
        // +(NSString *)getCpuArch;
        [Static]
        [Export("getCpuArch")]
        string CpuArch { get; }

        // +(NSString *)getArch;
        [Static]
        [Export("getArch")]
        string Arch { get; }
    }

    // @interface AtomicLong : NSObject
    [BaseType(typeof(NSObject))]
    interface AtomicLong
    {
        // -(instancetype)initWithValue:(long)value;
        [Export("initWithValue:")]
        NativeHandle Constructor(nint value);

        // -(long)incrementAndGet;
        [Export("incrementAndGet")]
        nint IncrementAndGet();

        // -(long)getAndIncrement;
        [Export("getAndIncrement")]
        nint GetAndIncrement();
    }

    // @interface Chapter : NSObject
    [BaseType(typeof(NSObject))]
    interface Chapter
    {
        // extern NSString *const ChapterKeyId;
        [Field("ChapterKeyId", "__Internal")]
        NSString KeyId { get; }

        // extern NSString *const ChapterKeyTimeBase;
        [Field("ChapterKeyTimeBase", "__Internal")]
        NSString KeyTimeBase { get; }

        // extern NSString *const ChapterKeyStart;
        [Field("ChapterKeyStart", "__Internal")]
        NSString KeyStart { get; }

        // extern NSString *const ChapterKeyStartTime;
        [Field("ChapterKeyStartTime", "__Internal")]
        NSString KeyStartTime { get; }

        // extern NSString *const ChapterKeyEnd;
        [Field("ChapterKeyEnd", "__Internal")]
        NSString KeyEnd { get; }

        // extern NSString *const ChapterKeyEndTime;
        [Field("ChapterKeyEndTime", "__Internal")]
        NSString KeyEndTime { get; }

        // extern NSString *const ChapterKeyTags;
        [Field("ChapterKeyTags", "__Internal")]
        NSString KeyTags { get; }

        // -(instancetype)init:(NSDictionary *)chapterDictionary;
        [Export("init:")]
        NativeHandle Constructor(NSDictionary chapterDictionary);

        // -(NSNumber *)getId;
        [Export("getId")]
        NSNumber Id { get; }

        // -(NSString *)getTimeBase;
        [Export("getTimeBase")]
        string TimeBase { get; }

        // -(NSNumber *)getStart;
        [Export("getStart")]
        NSNumber Start { get; }

        // -(NSString *)getStartTime;
        [Export("getStartTime")]
        string StartTime { get; }

        // -(NSNumber *)getEnd;
        [Export("getEnd")]
        NSNumber End { get; }

        // -(NSString *)getEndTime;
        [Export("getEndTime")]
        string EndTime { get; }

        // -(NSDictionary *)getTags;
        [Export("getTags")]
        NSDictionary Tags { get; }

        // -(NSString *)getStringProperty:(NSString *)key;
        [Export("getStringProperty:")]
        string GetStringProperty(string key);

        // -(NSNumber *)getNumberProperty:(NSString *)key;
        [Export("getNumberProperty:")]
        NSNumber GetNumberProperty(string key);

        // -(id)getProperty:(NSString *)key;
        [Export("getProperty:")]
        NSObject GetProperty(string key);

        // -(NSDictionary *)getAllProperties;
        [Export("getAllProperties")]
        NSDictionary AllProperties { get; }
    }

    // @interface Statistics : NSObject
    [BaseType(typeof(NSObject))]
    interface Statistics
    {
        // -(instancetype)init:(long)sessionId videoFrameNumber:(int)videoFrameNumber videoFps:(float)videoFps videoQuality:(float)videoQuality size:(int64_t)size time:(double)time bitrate:(double)bitrate speed:(double)speed;
        [Export("init:videoFrameNumber:videoFps:videoQuality:size:time:bitrate:speed:")]
        NativeHandle Constructor(nint sessionId, int videoFrameNumber, float videoFps, float videoQuality, long size, double time, double bitrate, double speed);

        // -(long)getSessionId;
        [Export("getSessionId")]
        nint SessionId { get; }

        // -(int)getVideoFrameNumber;
        [Export("getVideoFrameNumber")]
        int VideoFrameNumber { get; }

        // -(float)getVideoFps;
        [Export("getVideoFps")]
        float VideoFps { get; }

        // -(float)getVideoQuality;
        [Export("getVideoQuality")]
        float VideoQuality { get; }

        // -(long)getSize;
        [Export("getSize")]
        nint Size { get; }

        // -(double)getTime;
        [Export("getTime")]
        double Time { get; }

        // -(double)getBitrate;
        [Export("getBitrate")]
        double Bitrate { get; }

        // -(double)getSpeed;
        [Export("getSpeed")]
        double Speed { get; }
    }

    // typedef void (^StatisticsCallback)(Statistics *);
    delegate void StatisticsCallback(Statistics arg0);

    // typedef void (^FFmpegSessionCompleteCallback)(FFmpegSession *);
    delegate void FFmpegSessionCompleteCallback(FFmpegSession arg0);

    // @interface FFmpegSession : AbstractSession
    [BaseType(typeof(AbstractSession))]
    interface FFmpegSession
    {
        // +(instancetype)create:(NSArray *)arguments;
        [Static]
        [Export("create:")]
        FFmpegSession Create(string[] arguments);

        // +(instancetype)create:(NSArray *)arguments withCompleteCallback:(FFmpegSessionCompleteCallback)completeCallback;
        [Static]
        [Export("create:withCompleteCallback:")]
        FFmpegSession Create(string[] arguments, FFmpegSessionCompleteCallback completeCallback);

        // +(instancetype)create:(NSArray *)arguments withCompleteCallback:(FFmpegSessionCompleteCallback)completeCallback withLogCallback:(LogCallback)logCallback withStatisticsCallback:(StatisticsCallback)statisticsCallback;
        [Static]
        [Export("create:withCompleteCallback:withLogCallback:withStatisticsCallback:")]
        FFmpegSession Create(string[] arguments, FFmpegSessionCompleteCallback completeCallback, LogCallback logCallback, StatisticsCallback statisticsCallback);

        // +(instancetype)create:(NSArray *)arguments withCompleteCallback:(FFmpegSessionCompleteCallback)completeCallback withLogCallback:(LogCallback)logCallback withStatisticsCallback:(StatisticsCallback)statisticsCallback withLogRedirectionStrategy:(LogRedirectionStrategy)logRedirectionStrategy;
        [Static]
        [Export("create:withCompleteCallback:withLogCallback:withStatisticsCallback:withLogRedirectionStrategy:")]
        FFmpegSession Create(string[] arguments, FFmpegSessionCompleteCallback completeCallback, LogCallback logCallback, StatisticsCallback statisticsCallback, LogRedirectionStrategy logRedirectionStrategy);

        // -(StatisticsCallback)getStatisticsCallback;
        [Export("getStatisticsCallback")]
        StatisticsCallback StatisticsCallback { get; }

        // -(FFmpegSessionCompleteCallback)getCompleteCallback;
        [Export("getCompleteCallback")]
        FFmpegSessionCompleteCallback CompleteCallback { get; }

        // -(NSArray *)getAllStatisticsWithTimeout:(int)waitTimeout;
        [Export("getAllStatisticsWithTimeout:")]
        Statistics[] GetAllStatisticsWithTimeout(int waitTimeout);

        // -(NSArray *)getAllStatistics;
        [Export("getAllStatistics")]
        Statistics[] AllStatistics { get; }

        // -(NSArray *)getStatistics;
        [Export("getStatistics")]
        Statistics[] Statistics { get; }

        // -(Statistics *)getLastReceivedStatistics;
        [Export("getLastReceivedStatistics")]
        Statistics LastReceivedStatistics { get; }

        // -(void)addStatistics:(Statistics *)statistics;
        [Export("addStatistics:")]
        void AddStatistics(Statistics statistics);
    }

    // @interface FFmpegKit : NSObject
    [BaseType(typeof(NSObject))]
    interface FFmpegKit
    {
        // +(FFmpegSession *)executeWithArguments:(NSArray *)arguments;
        [Static]
        [Export("executeWithArguments:")]
        FFmpegSession ExecuteWithArguments(string[] arguments);

        // +(FFmpegSession *)executeWithArgumentsAsync:(NSArray *)arguments withCompleteCallback:(FFmpegSessionCompleteCallback)completeCallback;
        [Static]
        [Export("executeWithArgumentsAsync:withCompleteCallback:")]
        FFmpegSession ExecuteWithArgumentsAsync(string[] arguments, FFmpegSessionCompleteCallback completeCallback);

        // +(FFmpegSession *)executeWithArgumentsAsync:(NSArray *)arguments withCompleteCallback:(FFmpegSessionCompleteCallback)completeCallback withLogCallback:(LogCallback)logCallback withStatisticsCallback:(StatisticsCallback)statisticsCallback;
        [Static]
        [Export("executeWithArgumentsAsync:withCompleteCallback:withLogCallback:withStatisticsCallback:")]
        FFmpegSession ExecuteWithArgumentsAsync(string[] arguments, FFmpegSessionCompleteCallback completeCallback, LogCallback logCallback, StatisticsCallback statisticsCallback);

        // +(FFmpegSession *)executeWithArgumentsAsync:(NSArray *)arguments withCompleteCallback:(FFmpegSessionCompleteCallback)completeCallback onDispatchQueue:(dispatch_queue_t)queue;
        [Static]
        [Export("executeWithArgumentsAsync:withCompleteCallback:onDispatchQueue:")]
        FFmpegSession ExecuteWithArgumentsAsync(string[] arguments, FFmpegSessionCompleteCallback completeCallback, DispatchQueue queue);

        // +(FFmpegSession *)executeWithArgumentsAsync:(NSArray *)arguments withCompleteCallback:(FFmpegSessionCompleteCallback)completeCallback withLogCallback:(LogCallback)logCallback withStatisticsCallback:(StatisticsCallback)statisticsCallback onDispatchQueue:(dispatch_queue_t)queue;
        [Static]
        [Export("executeWithArgumentsAsync:withCompleteCallback:withLogCallback:withStatisticsCallback:onDispatchQueue:")]
        FFmpegSession ExecuteWithArgumentsAsync(string[] arguments, FFmpegSessionCompleteCallback completeCallback, LogCallback logCallback, StatisticsCallback statisticsCallback, DispatchQueue queue);

        // +(FFmpegSession *)execute:(NSString *)command;
        [Static]
        [Export("execute:")]
        FFmpegSession Execute(string command);

        // +(FFmpegSession *)executeAsync:(NSString *)command withCompleteCallback:(FFmpegSessionCompleteCallback)completeCallback;
        [Static]
        [Export("executeAsync:withCompleteCallback:")]
        FFmpegSession ExecuteAsync(string command, FFmpegSessionCompleteCallback completeCallback);

        // +(FFmpegSession *)executeAsync:(NSString *)command withCompleteCallback:(FFmpegSessionCompleteCallback)completeCallback withLogCallback:(LogCallback)logCallback withStatisticsCallback:(StatisticsCallback)statisticsCallback;
        [Static]
        [Export("executeAsync:withCompleteCallback:withLogCallback:withStatisticsCallback:")]
        FFmpegSession ExecuteAsync(string command, FFmpegSessionCompleteCallback completeCallback, LogCallback logCallback, StatisticsCallback statisticsCallback);

        // +(FFmpegSession *)executeAsync:(NSString *)command withCompleteCallback:(FFmpegSessionCompleteCallback)completeCallback onDispatchQueue:(dispatch_queue_t)queue;
        [Static]
        [Export("executeAsync:withCompleteCallback:onDispatchQueue:")]
        FFmpegSession ExecuteAsync(string command, FFmpegSessionCompleteCallback completeCallback, DispatchQueue queue);

        // +(FFmpegSession *)executeAsync:(NSString *)command withCompleteCallback:(FFmpegSessionCompleteCallback)completeCallback withLogCallback:(LogCallback)logCallback withStatisticsCallback:(StatisticsCallback)statisticsCallback onDispatchQueue:(dispatch_queue_t)queue;
        [Static]
        [Export("executeAsync:withCompleteCallback:withLogCallback:withStatisticsCallback:onDispatchQueue:")]
        FFmpegSession ExecuteAsync(string command, FFmpegSessionCompleteCallback completeCallback, LogCallback logCallback, StatisticsCallback statisticsCallback, DispatchQueue queue);

        // +(void)cancel;
        [Static]
        [Export("cancel")]
        void Cancel();

        // +(void)cancel:(long)sessionId;
        [Static]
        [Export("cancel:")]
        void Cancel(nint sessionId);

        // +(NSArray *)listSessions;
        [Static]
        [Export("listSessions")]
        FFmpegSession[] ListSessions { get; }
    }

    // typedef void (^FFprobeSessionCompleteCallback)(FFprobeSession *);
    delegate void FFprobeSessionCompleteCallback(FFprobeSession arg0);

    // @interface FFprobeSession : AbstractSession
    [BaseType(typeof(AbstractSession))]
    interface FFprobeSession
    {
        // +(instancetype)create:(NSArray *)arguments;
        [Static]
        [Export("create:")]
        FFprobeSession Create(string[] arguments);

        // +(instancetype)create:(NSArray *)arguments withCompleteCallback:(FFprobeSessionCompleteCallback)completeCallback;
        [Static]
        [Export("create:withCompleteCallback:")]
        FFprobeSession Create(string[] arguments, FFprobeSessionCompleteCallback completeCallback);

        // +(instancetype)create:(NSArray *)arguments withCompleteCallback:(FFprobeSessionCompleteCallback)completeCallback withLogCallback:(LogCallback)logCallback;
        [Static]
        [Export("create:withCompleteCallback:withLogCallback:")]
        FFprobeSession Create(string[] arguments, FFprobeSessionCompleteCallback completeCallback, LogCallback logCallback);

        // +(instancetype)create:(NSArray *)arguments withCompleteCallback:(FFprobeSessionCompleteCallback)completeCallback withLogCallback:(LogCallback)logCallback withLogRedirectionStrategy:(LogRedirectionStrategy)logRedirectionStrategy;
        [Static]
        [Export("create:withCompleteCallback:withLogCallback:withLogRedirectionStrategy:")]
        FFprobeSession Create(string[] arguments, FFprobeSessionCompleteCallback completeCallback, LogCallback logCallback, LogRedirectionStrategy logRedirectionStrategy);

        // -(FFprobeSessionCompleteCallback)getCompleteCallback;
        [Export("getCompleteCallback")]
        FFprobeSessionCompleteCallback CompleteCallback { get; }
    }

    // @interface StreamInformation : NSObject
    [BaseType(typeof(NSObject))]
    interface StreamInformation
    {
        // extern NSString *const StreamKeyIndex;
        [Field("StreamKeyIndex", "__Internal")]
        NSString KeyIndex { get; }

        // extern NSString *const StreamKeyType;
        [Field("StreamKeyType", "__Internal")]
        NSString KeyType { get; }

        // extern NSString *const StreamKeyCodec;
        [Field("StreamKeyCodec", "__Internal")]
        NSString KeyCodec { get; }

        // extern NSString *const StreamKeyCodecLong;
        [Field("StreamKeyCodecLong", "__Internal")]
        NSString KeyCodecLong { get; }

        // extern NSString *const StreamKeyFormat;
        [Field("StreamKeyFormat", "__Internal")]
        NSString KeyFormat { get; }

        // extern NSString *const StreamKeyWidth;
        [Field("StreamKeyWidth", "__Internal")]
        NSString KeyWidth { get; }

        // extern NSString *const StreamKeyHeight;
        [Field("StreamKeyHeight", "__Internal")]
        NSString KeyHeight { get; }

        // extern NSString *const StreamKeyBitRate;
        [Field("StreamKeyBitRate", "__Internal")]
        NSString KeyBitRate { get; }

        // extern NSString *const StreamKeySampleRate;
        [Field("StreamKeySampleRate", "__Internal")]
        NSString KeySampleRate { get; }

        // extern NSString *const StreamKeySampleFormat;
        [Field("StreamKeySampleFormat", "__Internal")]
        NSString KeySampleFormat { get; }

        // extern NSString *const StreamKeyChannelLayout;
        [Field("StreamKeyChannelLayout", "__Internal")]
        NSString KeyChannelLayout { get; }

        // extern NSString *const StreamKeySampleAspectRatio;
        [Field("StreamKeySampleAspectRatio", "__Internal")]
        NSString KeySampleAspectRatio { get; }

        // extern NSString *const StreamKeyDisplayAspectRatio;
        [Field("StreamKeyDisplayAspectRatio", "__Internal")]
        NSString KeyDisplayAspectRatio { get; }

        // extern NSString *const StreamKeyAverageFrameRate;
        [Field("StreamKeyAverageFrameRate", "__Internal")]
        NSString KeyAverageFrameRate { get; }

        // extern NSString *const StreamKeyRealFrameRate;
        [Field("StreamKeyRealFrameRate", "__Internal")]
        NSString KeyRealFrameRate { get; }

        // extern NSString *const StreamKeyTimeBase;
        [Field("StreamKeyTimeBase", "__Internal")]
        NSString KeyTimeBase { get; }

        // extern NSString *const StreamKeyCodecTimeBase;
        [Field("StreamKeyCodecTimeBase", "__Internal")]
        NSString KeyCodecTimeBase { get; }

        // extern NSString *const StreamKeyTags;
        [Field("StreamKeyTags", "__Internal")]
        NSString KeyTags { get; }

        // -(instancetype)init:(NSDictionary *)streamDictionary;
        [Export("init:")]
        NativeHandle Constructor(NSDictionary streamDictionary);

        // -(NSNumber *)getIndex;
        [Export("getIndex")]
        NSNumber Index { get; }

        // -(NSString *)getType;
        [Export("getType")]
        string Type { get; }

        // -(NSString *)getCodec;
        [Export("getCodec")]
        string Codec { get; }

        // -(NSString *)getCodecLong;
        [Export("getCodecLong")]
        string CodecLong { get; }

        // -(NSString *)getFormat;
        [Export("getFormat")]
        string Format { get; }

        // -(NSNumber *)getWidth;
        [Export("getWidth")]
        NSNumber Width { get; }

        // -(NSNumber *)getHeight;
        [Export("getHeight")]
        NSNumber Height { get; }

        // -(NSString *)getBitrate;
        [Export("getBitrate")]
        string Bitrate { get; }

        // -(NSString *)getSampleRate;
        [Export("getSampleRate")]
        string SampleRate { get; }

        // -(NSString *)getSampleFormat;
        [Export("getSampleFormat")]
        string SampleFormat { get; }

        // -(NSString *)getChannelLayout;
        [Export("getChannelLayout")]
        string ChannelLayout { get; }

        // -(NSString *)getSampleAspectRatio;
        [Export("getSampleAspectRatio")]
        string SampleAspectRatio { get; }

        // -(NSString *)getDisplayAspectRatio;
        [Export("getDisplayAspectRatio")]
        string DisplayAspectRatio { get; }

        // -(NSString *)getAverageFrameRate;
        [Export("getAverageFrameRate")]
        string AverageFrameRate { get; }

        // -(NSString *)getRealFrameRate;
        [Export("getRealFrameRate")]
        string RealFrameRate { get; }

        // -(NSString *)getTimeBase;
        [Export("getTimeBase")]
        string TimeBase { get; }

        // -(NSString *)getCodecTimeBase;
        [Export("getCodecTimeBase")]
        string CodecTimeBase { get; }

        // -(NSDictionary *)getTags;
        [Export("getTags")]
        NSDictionary Tags { get; }

        // -(NSString *)getStringProperty:(NSString *)key;
        [Export("getStringProperty:")]
        string GetStringProperty(string key);

        // -(NSNumber *)getNumberProperty:(NSString *)key;
        [Export("getNumberProperty:")]
        NSNumber GetNumberProperty(string key);

        // -(id)getProperty:(NSString *)key;
        [Export("getProperty:")]
        NSObject GetProperty(string key);

        // -(NSDictionary *)getAllProperties;
        [Export("getAllProperties")]
        NSDictionary AllProperties { get; }
    }

    // @interface MediaInformation : NSObject
    [BaseType(typeof(NSObject))]
    interface MediaInformation
    {
        // extern NSString *const MediaKeyMediaProperties;
        //
        // Deliberately NOT bound. MediaInformation.h declares this constant, but the compiled
        // library never defines it - `nm -gU ffmpegkit` lists every other MediaKey* and not this
        // one. Binding it links the app against a symbol that does not exist, and the failure is
        // an "Undefined symbols for architecture arm64" error in the *consuming* app rather than
        // anything visible when building this binding. Re-add it only once upstream exports it.
        // The nearest real constant is MediaKeyFormatProperties, bound below.

        // extern NSString *const MediaKeyFilename;
        [Field("MediaKeyFilename", "__Internal")]
        NSString KeyFilename { get; }

        // extern NSString *const MediaKeyFormat;
        [Field("MediaKeyFormat", "__Internal")]
        NSString KeyFormat { get; }

        // extern NSString *const MediaKeyFormatLong;
        [Field("MediaKeyFormatLong", "__Internal")]
        NSString KeyFormatLong { get; }

        // extern NSString *const MediaKeyStartTime;
        [Field("MediaKeyStartTime", "__Internal")]
        NSString KeyStartTime { get; }

        // extern NSString *const MediaKeyDuration;
        [Field("MediaKeyDuration", "__Internal")]
        NSString KeyDuration { get; }

        // extern NSString *const MediaKeySize;
        [Field("MediaKeySize", "__Internal")]
        NSString KeySize { get; }

        // extern NSString *const MediaKeyBitRate;
        [Field("MediaKeyBitRate", "__Internal")]
        NSString KeyBitRate { get; }

        // extern NSString *const MediaKeyTags;
        [Field("MediaKeyTags", "__Internal")]
        NSString KeyTags { get; }

        // -(instancetype)init:(NSDictionary *)mediaDictionary withStreams:(NSArray *)streams withChapters:(NSArray *)chapters;
        [Export("init:withStreams:withChapters:")]
        NativeHandle Constructor(NSDictionary mediaDictionary, StreamInformation[] streams, Chapter[] chapters);

        // -(NSString *)getFilename;
        [Export("getFilename")]
        string Filename { get; }

        // -(NSString *)getFormat;
        [Export("getFormat")]
        string Format { get; }

        // -(NSString *)getLongFormat;
        [Export("getLongFormat")]
        string LongFormat { get; }

        // -(NSString *)getDuration;
        [Export("getDuration")]
        string Duration { get; }

        // -(NSString *)getStartTime;
        [Export("getStartTime")]
        string StartTime { get; }

        // -(NSString *)getSize;
        [Export("getSize")]
        string Size { get; }

        // -(NSString *)getBitrate;
        [Export("getBitrate")]
        string Bitrate { get; }

        // -(NSDictionary *)getTags;
        [Export("getTags")]
        NSDictionary Tags { get; }

        // -(NSArray *)getStreams;
        [Export("getStreams")]
        StreamInformation[] Streams { get; }

        // -(NSArray *)getChapters;
        [Export("getChapters")]
        Chapter[] Chapters { get; }

        // -(NSString *)getStringProperty:(NSString *)key;
        [Export("getStringProperty:")]
        string GetStringProperty(string key);

        // -(NSNumber *)getNumberProperty:(NSString *)key;
        [Export("getNumberProperty:")]
        NSNumber GetNumberProperty(string key);

        // -(id)getProperty:(NSString *)key;
        [Export("getProperty:")]
        NSObject GetProperty(string key);

        // -(NSString *)getStringFormatProperty:(NSString *)key;
        [Export("getStringFormatProperty:")]
        string GetStringFormatProperty(string key);

        // -(NSNumber *)getNumberFormatProperty:(NSString *)key;
        [Export("getNumberFormatProperty:")]
        NSNumber GetNumberFormatProperty(string key);

        // -(id)getFormatProperty:(NSString *)key;
        [Export("getFormatProperty:")]
        NSObject GetFormatProperty(string key);

        // -(NSDictionary *)getFormatProperties;
        [Export("getFormatProperties")]
        NSDictionary FormatProperties { get; }

        // -(NSDictionary *)getAllProperties;
        [Export("getAllProperties")]
        NSDictionary AllProperties { get; }
    }

    // typedef void (^MediaInformationSessionCompleteCallback)(MediaInformationSession *);
    delegate void MediaInformationSessionCompleteCallback(MediaInformationSession arg0);

    // @interface MediaInformationSession : AbstractSession
    [BaseType(typeof(AbstractSession))]
    interface MediaInformationSession
    {
        // +(instancetype)create:(NSArray *)arguments;
        [Static]
        [Export("create:")]
        MediaInformationSession Create(string[] arguments);

        // +(instancetype)create:(NSArray *)arguments withCompleteCallback:(MediaInformationSessionCompleteCallback)completeCallback;
        [Static]
        [Export("create:withCompleteCallback:")]
        MediaInformationSession Create(string[] arguments, MediaInformationSessionCompleteCallback completeCallback);

        // +(instancetype)create:(NSArray *)arguments withCompleteCallback:(MediaInformationSessionCompleteCallback)completeCallback withLogCallback:(LogCallback)logCallback;
        [Static]
        [Export("create:withCompleteCallback:withLogCallback:")]
        MediaInformationSession Create(string[] arguments, MediaInformationSessionCompleteCallback completeCallback, LogCallback logCallback);

        // -(MediaInformation *)getMediaInformation;
        // -(void)setMediaInformation:(MediaInformation *)mediaInformation;
        [Export("getMediaInformation")]
        MediaInformation MediaInformation { get; [Bind("setMediaInformation:")] set; }

        // -(MediaInformationSessionCompleteCallback)getCompleteCallback;
        [Export("getCompleteCallback")]
        MediaInformationSessionCompleteCallback CompleteCallback { get; }
    }

    // @interface FFmpegKitConfig : NSObject
    [BaseType(typeof(NSObject))]
    interface FFmpegKitConfig
    {
        // extern NSString *const FFmpegKitVersion;
        [Field("FFmpegKitVersion", "__Internal")]
        NSString FFmpegKitVersion { get; }

        // +(void)enableRedirection;
        [Static]
        [Export("enableRedirection")]
        void EnableRedirection();

        // +(void)disableRedirection;
        [Static]
        [Export("disableRedirection")]
        void DisableRedirection();

        // +(int)setFontconfigConfigurationPath:(NSString *)path;
        [Static]
        [Export("setFontconfigConfigurationPath:")]
        int SetFontconfigConfigurationPath(string path);

        // +(void)setFontDirectory:(NSString *)fontDirectoryPath with:(NSDictionary *)fontNameMapping;
        [Static]
        [Export("setFontDirectory:with:")]
        void SetFontDirectory(string fontDirectoryPath, NSDictionary fontNameMapping);

        // +(void)setFontDirectoryList:(NSArray *)fontDirectoryList with:(NSDictionary *)fontNameMapping;
        [Static]
        [Export("setFontDirectoryList:with:")]
        void SetFontDirectoryList(string[] fontDirectoryList, NSDictionary fontNameMapping);

        // +(NSString *)registerNewFFmpegPipe;
        [Static]
        [Export("registerNewFFmpegPipe")]
        string RegisterNewFFmpegPipe();

        // +(void)closeFFmpegPipe:(NSString *)ffmpegPipePath;
        [Static]
        [Export("closeFFmpegPipe:")]
        void CloseFFmpegPipe(string ffmpegPipePath);

        // +(NSString *)getFFmpegVersion;
        [Static]
        [Export("getFFmpegVersion")]
        string FFmpegVersion { get; }

        // +(NSString *)getVersion;
        [Static]
        [Export("getVersion")]
        string Version { get; }

        // +(int)isLTSBuild;
        [Static]
        [Export("isLTSBuild")]
        int IsLTSBuild { get; }

        // +(NSString *)getBuildDate;
        [Static]
        [Export("getBuildDate")]
        string BuildDate { get; }

        // +(int)setEnvironmentVariable:(NSString *)variableName value:(NSString *)variableValue;
        [Static]
        [Export("setEnvironmentVariable:value:")]
        int SetEnvironmentVariable(string variableName, string variableValue);

        // +(void)ignoreSignal:(Signal)signal;
        [Static]
        [Export("ignoreSignal:")]
        void IgnoreSignal(Signal signal);

        // +(void)ffmpegExecute:(FFmpegSession *)ffmpegSession;
        [Static]
        [Export("ffmpegExecute:")]
        void FfmpegExecute(FFmpegSession ffmpegSession);

        // +(void)ffprobeExecute:(FFprobeSession *)ffprobeSession;
        [Static]
        [Export("ffprobeExecute:")]
        void FfprobeExecute(FFprobeSession ffprobeSession);

        // +(void)getMediaInformationExecute:(MediaInformationSession *)mediaInformationSession withTimeout:(int)waitTimeout;
        [Static]
        [Export("getMediaInformationExecute:withTimeout:")]
        void GetMediaInformationExecute(MediaInformationSession mediaInformationSession, int waitTimeout);

        // +(void)asyncFFmpegExecute:(FFmpegSession *)ffmpegSession;
        [Static]
        [Export("asyncFFmpegExecute:")]
        void AsyncFFmpegExecute(FFmpegSession ffmpegSession);

        // +(void)asyncFFmpegExecute:(FFmpegSession *)ffmpegSession onDispatchQueue:(dispatch_queue_t)queue;
        [Static]
        [Export("asyncFFmpegExecute:onDispatchQueue:")]
        void AsyncFFmpegExecute(FFmpegSession ffmpegSession, DispatchQueue queue);

        // +(void)asyncFFprobeExecute:(FFprobeSession *)ffprobeSession;
        [Static]
        [Export("asyncFFprobeExecute:")]
        void AsyncFFprobeExecute(FFprobeSession ffprobeSession);

        // +(void)asyncFFprobeExecute:(FFprobeSession *)ffprobeSession onDispatchQueue:(dispatch_queue_t)queue;
        [Static]
        [Export("asyncFFprobeExecute:onDispatchQueue:")]
        void AsyncFFprobeExecute(FFprobeSession ffprobeSession, DispatchQueue queue);

        // +(void)asyncGetMediaInformationExecute:(MediaInformationSession *)mediaInformationSession withTimeout:(int)waitTimeout;
        [Static]
        [Export("asyncGetMediaInformationExecute:withTimeout:")]
        void AsyncGetMediaInformationExecute(MediaInformationSession mediaInformationSession, int waitTimeout);

        // +(void)asyncGetMediaInformationExecute:(MediaInformationSession *)mediaInformationSession onDispatchQueue:(dispatch_queue_t)queue withTimeout:(int)waitTimeout;
        [Static]
        [Export("asyncGetMediaInformationExecute:onDispatchQueue:withTimeout:")]
        void AsyncGetMediaInformationExecute(MediaInformationSession mediaInformationSession, DispatchQueue queue, int waitTimeout);

        // +(void)enableLogCallback:(LogCallback)logCallback;
        [Static]
        [Export("enableLogCallback:")]
        void EnableLogCallback([NullAllowed] LogCallback logCallback);

        // +(void)enableStatisticsCallback:(StatisticsCallback)statisticsCallback;
        [Static]
        [Export("enableStatisticsCallback:")]
        void EnableStatisticsCallback([NullAllowed] StatisticsCallback statisticsCallback);

        // +(void)enableFFmpegSessionCompleteCallback:(FFmpegSessionCompleteCallback)ffmpegSessionCompleteCallback;
        [Static]
        [Export("enableFFmpegSessionCompleteCallback:")]
        void EnableFFmpegSessionCompleteCallback([NullAllowed] FFmpegSessionCompleteCallback ffmpegSessionCompleteCallback);

        // +(FFmpegSessionCompleteCallback)getFFmpegSessionCompleteCallback;
        [Static]
        [Export("getFFmpegSessionCompleteCallback")]
        FFmpegSessionCompleteCallback FFmpegSessionCompleteCallback { get; }

        // +(void)enableFFprobeSessionCompleteCallback:(FFprobeSessionCompleteCallback)ffprobeSessionCompleteCallback;
        [Static]
        [Export("enableFFprobeSessionCompleteCallback:")]
        void EnableFFprobeSessionCompleteCallback([NullAllowed] FFprobeSessionCompleteCallback ffprobeSessionCompleteCallback);

        // +(FFprobeSessionCompleteCallback)getFFprobeSessionCompleteCallback;
        [Static]
        [Export("getFFprobeSessionCompleteCallback")]
        FFprobeSessionCompleteCallback FFprobeSessionCompleteCallback { get; }

        // +(void)enableMediaInformationSessionCompleteCallback:(MediaInformationSessionCompleteCallback)mediaInformationSessionCompleteCallback;
        [Static]
        [Export("enableMediaInformationSessionCompleteCallback:")]
        void EnableMediaInformationSessionCompleteCallback([NullAllowed] MediaInformationSessionCompleteCallback mediaInformationSessionCompleteCallback);

        // +(MediaInformationSessionCompleteCallback)getMediaInformationSessionCompleteCallback;
        [Static]
        [Export("getMediaInformationSessionCompleteCallback")]
        MediaInformationSessionCompleteCallback MediaInformationSessionCompleteCallback { get; }

        // +(int)getLogLevel;
        // +(void)setLogLevel:(int)level;
        [Static]
        [Export("getLogLevel")]
        int LogLevel { get; [Bind("setLogLevel:")] set; }

        // +(NSString *)logLevelToString:(int)level;
        [Static]
        [Export("logLevelToString:")]
        string LogLevelToString(int level);

        // +(int)getSessionHistorySize;
        // +(void)setSessionHistorySize:(int)sessionHistorySize;
        [Static]
        [Export("getSessionHistorySize")]
        int SessionHistorySize { get; [Bind("setSessionHistorySize:")] set; }

        // +(id<Session>)getSession:(long)sessionId;
        [Static]
        [Export("getSession:")]
        ISession GetSession(nint sessionId);

        // +(id<Session>)getLastSession;
        [Static]
        [Export("getLastSession")]
        ISession LastSession { get; }

        // +(id<Session>)getLastCompletedSession;
        [Static]
        [Export("getLastCompletedSession")]
        ISession LastCompletedSession { get; }

        // +(NSArray *)getSessions;
        [Static]
        [Export("getSessions")]
        ISession[] Sessions { get; }

        // +(void)clearSessions;
        [Static]
        [Export("clearSessions")]
        void ClearSessions();

        // +(NSArray *)getFFmpegSessions;
        [Static]
        [Export("getFFmpegSessions")]
        FFmpegSession[] FFmpegSessions { get; }

        // +(NSArray *)getFFprobeSessions;
        [Static]
        [Export("getFFprobeSessions")]
        FFprobeSession[] FFprobeSessions { get; }

        // +(NSArray *)getMediaInformationSessions;
        [Static]
        [Export("getMediaInformationSessions")]
        MediaInformationSession[] MediaInformationSessions { get; }

        // +(NSArray *)getSessionsByState:(SessionState)state;
        [Static]
        [Export("getSessionsByState:")]
        ISession[] GetSessionsByState(SessionState state);

        // +(LogRedirectionStrategy)getLogRedirectionStrategy;
        // +(void)setLogRedirectionStrategy:(LogRedirectionStrategy)logRedirectionStrategy;
        [Static]
        [Export("getLogRedirectionStrategy")]
        LogRedirectionStrategy LogRedirectionStrategy { get; [Bind("setLogRedirectionStrategy:")] set; }

        // +(int)messagesInTransmit:(long)sessionId;
        [Static]
        [Export("messagesInTransmit:")]
        int MessagesInTransmit(nint sessionId);

        // +(NSString *)sessionStateToString:(SessionState)state;
        [Static]
        [Export("sessionStateToString:")]
        string SessionStateToString(SessionState state);

        // +(NSArray *)parseArguments:(NSString *)command;
        [Static]
        [Export("parseArguments:")]
        string[] ParseArguments(string command);

        // +(NSString *)argumentsToString:(NSArray *)arguments;
        [Static]
        [Export("argumentsToString:")]
        string ArgumentsToString(string[] arguments);
    }

    // @interface MediaInformationJsonParser : NSObject
    [BaseType(typeof(NSObject))]
    interface MediaInformationJsonParser
    {
        // +(MediaInformation *)from:(NSString *)ffprobeJsonOutput;
        [Static]
        [Export("from:")]
        MediaInformation From(string ffprobeJsonOutput);

        // +(MediaInformation *)fromWithError:(NSString *)ffprobeJsonOutput;
        [Static]
        [Export("fromWithError:")]
        MediaInformation FromWithError(string ffprobeJsonOutput);
    }

    // @interface FFprobeKit : NSObject
    [BaseType(typeof(NSObject))]
    interface FFprobeKit
    {
        // +(FFprobeSession *)executeWithArguments:(NSArray *)arguments;
        [Static]
        [Export("executeWithArguments:")]
        FFprobeSession ExecuteWithArguments(string[] arguments);

        // +(FFprobeSession *)executeWithArgumentsAsync:(NSArray *)arguments withCompleteCallback:(FFprobeSessionCompleteCallback)completeCallback;
        [Static]
        [Export("executeWithArgumentsAsync:withCompleteCallback:")]
        FFprobeSession ExecuteWithArgumentsAsync(string[] arguments, FFprobeSessionCompleteCallback completeCallback);

        // +(FFprobeSession *)executeWithArgumentsAsync:(NSArray *)arguments withCompleteCallback:(FFprobeSessionCompleteCallback)completeCallback withLogCallback:(LogCallback)logCallback;
        [Static]
        [Export("executeWithArgumentsAsync:withCompleteCallback:withLogCallback:")]
        FFprobeSession ExecuteWithArgumentsAsync(string[] arguments, FFprobeSessionCompleteCallback completeCallback, LogCallback logCallback);

        // +(FFprobeSession *)executeWithArgumentsAsync:(NSArray *)arguments withCompleteCallback:(FFprobeSessionCompleteCallback)completeCallback onDispatchQueue:(dispatch_queue_t)queue;
        [Static]
        [Export("executeWithArgumentsAsync:withCompleteCallback:onDispatchQueue:")]
        FFprobeSession ExecuteWithArgumentsAsync(string[] arguments, FFprobeSessionCompleteCallback completeCallback, DispatchQueue queue);

        // +(FFprobeSession *)executeWithArgumentsAsync:(NSArray *)arguments withCompleteCallback:(FFprobeSessionCompleteCallback)completeCallback withLogCallback:(LogCallback)logCallback onDispatchQueue:(dispatch_queue_t)queue;
        [Static]
        [Export("executeWithArgumentsAsync:withCompleteCallback:withLogCallback:onDispatchQueue:")]
        FFprobeSession ExecuteWithArgumentsAsync(string[] arguments, FFprobeSessionCompleteCallback completeCallback, LogCallback logCallback, DispatchQueue queue);

        // +(FFprobeSession *)execute:(NSString *)command;
        [Static]
        [Export("execute:")]
        FFprobeSession Execute(string command);

        // +(FFprobeSession *)executeAsync:(NSString *)command withCompleteCallback:(FFprobeSessionCompleteCallback)completeCallback;
        [Static]
        [Export("executeAsync:withCompleteCallback:")]
        FFprobeSession ExecuteAsync(string command, FFprobeSessionCompleteCallback completeCallback);

        // +(FFprobeSession *)executeAsync:(NSString *)command withCompleteCallback:(FFprobeSessionCompleteCallback)completeCallback withLogCallback:(LogCallback)logCallback;
        [Static]
        [Export("executeAsync:withCompleteCallback:withLogCallback:")]
        FFprobeSession ExecuteAsync(string command, FFprobeSessionCompleteCallback completeCallback, LogCallback logCallback);

        // +(FFprobeSession *)executeAsync:(NSString *)command withCompleteCallback:(FFprobeSessionCompleteCallback)completeCallback onDispatchQueue:(dispatch_queue_t)queue;
        [Static]
        [Export("executeAsync:withCompleteCallback:onDispatchQueue:")]
        FFprobeSession ExecuteAsync(string command, FFprobeSessionCompleteCallback completeCallback, DispatchQueue queue);

        // +(FFprobeSession *)executeAsync:(NSString *)command withCompleteCallback:(FFprobeSessionCompleteCallback)completeCallback withLogCallback:(LogCallback)logCallback onDispatchQueue:(dispatch_queue_t)queue;
        [Static]
        [Export("executeAsync:withCompleteCallback:withLogCallback:onDispatchQueue:")]
        FFprobeSession ExecuteAsync(string command, FFprobeSessionCompleteCallback completeCallback, LogCallback logCallback, DispatchQueue queue);

        // +(MediaInformationSession *)getMediaInformation:(NSString *)path;
        [Static]
        [Export("getMediaInformation:")]
        MediaInformationSession GetMediaInformation(string path);

        // +(MediaInformationSession *)getMediaInformation:(NSString *)path withTimeout:(int)waitTimeout;
        [Static]
        [Export("getMediaInformation:withTimeout:")]
        MediaInformationSession GetMediaInformation(string path, int waitTimeout);

        // +(MediaInformationSession *)getMediaInformationAsync:(NSString *)path withCompleteCallback:(MediaInformationSessionCompleteCallback)completeCallback;
        [Static]
        [Export("getMediaInformationAsync:withCompleteCallback:")]
        MediaInformationSession GetMediaInformationAsync(string path, MediaInformationSessionCompleteCallback completeCallback);

        // +(MediaInformationSession *)getMediaInformationAsync:(NSString *)path withCompleteCallback:(MediaInformationSessionCompleteCallback)completeCallback withLogCallback:(LogCallback)logCallback withTimeout:(int)waitTimeout;
        [Static]
        [Export("getMediaInformationAsync:withCompleteCallback:withLogCallback:withTimeout:")]
        MediaInformationSession GetMediaInformationAsync(string path, MediaInformationSessionCompleteCallback completeCallback, LogCallback logCallback, int waitTimeout);

        // +(MediaInformationSession *)getMediaInformationAsync:(NSString *)path withCompleteCallback:(MediaInformationSessionCompleteCallback)completeCallback onDispatchQueue:(dispatch_queue_t)queue;
        [Static]
        [Export("getMediaInformationAsync:withCompleteCallback:onDispatchQueue:")]
        MediaInformationSession GetMediaInformationAsync(string path, MediaInformationSessionCompleteCallback completeCallback, DispatchQueue queue);

        // +(MediaInformationSession *)getMediaInformationAsync:(NSString *)path withCompleteCallback:(MediaInformationSessionCompleteCallback)completeCallback withLogCallback:(LogCallback)logCallback onDispatchQueue:(dispatch_queue_t)queue withTimeout:(int)waitTimeout;
        [Static]
        [Export("getMediaInformationAsync:withCompleteCallback:withLogCallback:onDispatchQueue:withTimeout:")]
        MediaInformationSession GetMediaInformationAsync(string path, MediaInformationSessionCompleteCallback completeCallback, LogCallback logCallback, DispatchQueue queue, int waitTimeout);

        // +(MediaInformationSession *)getMediaInformationFromCommand:(NSString *)command;
        [Static]
        [Export("getMediaInformationFromCommand:")]
        MediaInformationSession GetMediaInformationFromCommand(string command);

        // +(MediaInformationSession *)getMediaInformationFromCommandAsync:(NSString *)command withCompleteCallback:(MediaInformationSessionCompleteCallback)completeCallback withLogCallback:(LogCallback)logCallback onDispatchQueue:(dispatch_queue_t)queue withTimeout:(int)waitTimeout;
        [Static]
        [Export("getMediaInformationFromCommandAsync:withCompleteCallback:withLogCallback:onDispatchQueue:withTimeout:")]
        MediaInformationSession GetMediaInformationFromCommandAsync(string command, MediaInformationSessionCompleteCallback completeCallback, LogCallback logCallback, DispatchQueue queue, int waitTimeout);

        // +(MediaInformationSession *)getMediaInformationFromCommandArgumentsAsync:(NSArray *)arguments withCompleteCallback:(MediaInformationSessionCompleteCallback)completeCallback withLogCallback:(LogCallback)logCallback onDispatchQueue:(dispatch_queue_t)queue withTimeout:(int)waitTimeout;
        [Static]
        [Export("getMediaInformationFromCommandArgumentsAsync:withCompleteCallback:withLogCallback:onDispatchQueue:withTimeout:")]
        MediaInformationSession GetMediaInformationFromCommandArgumentsAsync(string[] arguments, MediaInformationSessionCompleteCallback completeCallback, LogCallback logCallback, DispatchQueue queue, int waitTimeout);

        // +(NSArray *)listFFprobeSessions;
        [Static]
        [Export("listFFprobeSessions")]
        FFprobeSession[] ListFFprobeSessions { get; }

        // +(NSArray *)listMediaInformationSessions;
        [Static]
        [Export("listMediaInformationSessions")]
        MediaInformationSession[] ListMediaInformationSessions { get; }
    }

    // @interface Packages : NSObject
    [BaseType(typeof(NSObject))]
    interface Packages
    {
        // +(NSString *)getPackageName;
        [Static]
        [Export("getPackageName")]
        string PackageName { get; }

        // +(NSArray *)getExternalLibraries;
        [Static]
        [Export("getExternalLibraries")]
        string[] ExternalLibraries { get; }
    }
}
