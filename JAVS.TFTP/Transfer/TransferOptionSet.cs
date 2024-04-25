using System.Collections.Generic;
using JAVS.TFTP.Commands;

namespace JAVS.TFTP.Transfer;

internal class TransferOptionSet
{
    public const int DEFAULT_BLOCKSIZE = 512;
    public const int DEFAULT_TIMEOUT_SECS = 5;

    public bool IncludesBlockSizeOption;
    public int BlockSize = DEFAULT_BLOCKSIZE;

    public bool IncludesTimeoutOption;
    public int Timeout = DEFAULT_TIMEOUT_SECS;

    public bool IncludesTransferSizeOption;
    public long TransferSize;

    public static TransferOptionSet NewDefaultSet() =>
        new() { IncludesBlockSizeOption = true, IncludesTimeoutOption = true, IncludesTransferSizeOption = true };

    public static TransferOptionSet NewEmptySet() =>
        new() { IncludesBlockSizeOption = false, IncludesTimeoutOption = false, IncludesTransferSizeOption = false };

    private TransferOptionSet()
    {
    }

    public TransferOptionSet(IEnumerable<TransferOption> options)
    {
        IncludesBlockSizeOption = IncludesTimeoutOption = IncludesTransferSizeOption = false;

        foreach (var option in options)
        {
            Parse(option);
        }
    }

    private void Parse(TransferOption option)
    {
        switch (option.Name)
        {
            case "blksize":
                IncludesBlockSizeOption = ParseBlockSizeOption(option.Value);
                break;

            case "timeout":
                IncludesTimeoutOption = ParseTimeoutOption(option.Value);
                break;

            case "tsize":
                IncludesTransferSizeOption = ParseTransferSizeOption(option.Value);
                break;
        }
    }

    public List<TransferOption> ToOptionList()
    {
        List<TransferOption> result = [];
        if (IncludesBlockSizeOption)
            result.Add(new TransferOption("blksize", BlockSize.ToString()));

        if (IncludesTimeoutOption)
            result.Add(new TransferOption("timeout", Timeout.ToString()));

        if (IncludesTransferSizeOption)
            result.Add(new TransferOption("tsize", TransferSize.ToString()));

        return result;
    }

    private bool ParseTransferSizeOption(string value)
    {
        return long.TryParse(value, out TransferSize) && TransferSize >= 0;
    }

    private bool ParseTimeoutOption(string value)
    {
        if (!int.TryParse(value, out var timeout))
            return false;

        //Only accept timeouts in the range [1, 255]
        if (timeout < 1 || timeout > 255)
            return false;

        Timeout = timeout;
        return true;
    }

    private bool ParseBlockSizeOption(string value)
    {
        if (!int.TryParse(value, out var blockSize))
            return false;

        //Only accept block sizes in the range [8, 65464]
        if (blockSize < 8 || blockSize > 65464)
            return false;

        BlockSize = blockSize;
        return true;
    }
}