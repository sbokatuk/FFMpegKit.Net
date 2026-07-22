using System;
using ObjCRuntime;

namespace Ffmpegkit.Ios
{
    [Native]
    public enum LogRedirectionStrategy : ulong
    {
        AlwaysPrintLogs,
        PrintLogsWhenNoCallbacksDefined,
        PrintLogsWhenGlobalCallbackNotDefined,
        PrintLogsWhenSessionCallbackNotDefined,
        NeverPrintLogs
    }

    [Native]
    public enum ReturnCodeEnum : ulong
    {
        Success = 0,
        Cancel = 255
    }

    [Native]
    public enum SessionState : ulong
    {
        Created,
        Running,
        Failed,
        Completed
    }

    [Native]
    public enum Signal : ulong
    {
        Int = 2,
        Quit = 3,
        Pipe = 13,
        Term = 15,
        Xcpu = 24
    }

    // Declared as NS_ENUM(NSUInteger, Level) natively, but two members are negative, so the
    // managed enum has to be signed.
    [Native]
    public enum Level : long
    {
        StdErr = -16,
        Quiet = -8,
        Panic = 0,
        Fatal = 8,
        Error = 16,
        Warning = 24,
        Info = 32,
        Verbose = 40,
        Debug = 48,
        Trace = 56
    }
}
