﻿namespace JAVS.TFTP;

public enum BlockCounterWrapAround
{
    ToZero,
    ToOne
}

internal static class BlockCounterWrappingHelpers
{
    private const ushort LAST_AVAILABLE_BLOCK_NUMBER = 65535;

    public static ushort CalculateNextBlockNumber(this BlockCounterWrapAround wrapping, ushort previousBlockNumber)
    {
        if (previousBlockNumber == LAST_AVAILABLE_BLOCK_NUMBER)
            return wrapping == BlockCounterWrapAround.ToZero ? (ushort)0 : (ushort)1;

        return (ushort)(previousBlockNumber + 1);
    }
}