using System;

namespace TT2026.libraries.NetworkedBoardGameEntitySystem.Networking;

public class BadPacketException(string message) : Exception(message);