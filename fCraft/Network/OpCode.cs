namespace fCraft
{
    /// <summary> Classic Protocol Extension opcodes. </summary>
    public enum OpCode
    {
        Handshake = 0,
        Ping = 1,
        MapBegin = 2,
        MapChunk = 3,
        MapEnd = 4,
        SetBlockClient = 5,
        SetBlockServer = 6,
        AddEntity = 7,
        Teleport = 8,
        MoveRotate = 9,
        Move = 10,
        Rotate = 11,
        RemoveEntity = 12,
        Message = 13,
        Kick = 14,
        SetPermission = 15,

        ExtInfo = 16,
        ExtEntry = 17,
        SetClickDistance = 18,
        CustomBlockSupportLevel = 19,
        HoldThis = 20,
        SetTextHotKey = 21,
        ExtAddPlayerName = 22,
        ExtAddEntity = 23,
        ExtRemovePlayerName = 24,
        EnvSetColor = 25,
        MakeSelection = 26,
        RemoveSelection = 27,
        SetBlockPermissions = 28,
        ChangeModel = 29,
        EnvMapAppearance = 30,
        EnvWeatherType = 31,
        HackControl = 32
    }
}