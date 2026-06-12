using System;
using System.Collections.Generic;
using Godot;

namespace TT2026;

public static class Logger
{
    private static HashSet<LoggingContexts> _enabledContexts = new HashSet<LoggingContexts>();

    
    
    public static void SetEnabledContext(LoggingContexts context,  bool enabled = true)
    {
        if (enabled) _enabledContexts.Add(context);
        else _enabledContexts.Remove(context);
    }
    public static void Log(string message, LoggingContexts onlyShowInContext = LoggingContexts.Always)
    {
        if (onlyShowInContext != LoggingContexts.Always && !_enabledContexts.Contains(onlyShowInContext)) return;
        GD.Print($"{message}");
    }
    public static void Log(Node source, string message, LoggingContexts onlyShowInContext = LoggingContexts.Always)
    {
        if (onlyShowInContext != LoggingContexts.Always && !_enabledContexts.Contains(onlyShowInContext)) return;
        GD.Print($"{source.Name}: {message}");
    }
    
    public static void Error(Exception exception, string message)
    {
        GD.PrintErr($"{message}\n{exception}");
    }
    
    public static void Error(Node source, Exception exception, string message)
    {
        GD.PrintErr($"{source.Name}: {message}\n{exception}");
    }
    
    public static void Error(string message)
    {
        GD.PrintErr($"ERROR: {message}");
    }
    
    public static void Error(Node source, string message)
    {
        GD.PrintErr($"{source.Name}: {message}");
    }
}