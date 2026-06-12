using System;

namespace TT2026.networking;

public class BadPacketException(string message) : Exception(message);