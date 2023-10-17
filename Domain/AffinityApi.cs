using System;
using Domain.Infrastructure;

namespace Domain;

public static class AffinityApi
{
  public static long BitmaskFrom(AffinityMode affinityMode, long value)
    => affinityMode switch
    {
      AffinityMode.AllEven => FillFirstNEvenOnly(Environment.ProcessorCount / 2),
      AffinityMode.FirstNEven => FillFirstNEvenOnly((int)value),
      AffinityMode.FirstN => FillFirstN((int)value),
      AffinityMode.LastN => FillLastN((int)value),
      AffinityMode.CustomBitmask or _ => FromCustom(value)
    };

  public static long FillFirstNEvenOnly(int count)
  {
    long result = 0;

    for (byte i = 0; count != 0; i += 2)
    {
      result |= 1L << i;
      count--;
    }

    return result;
  }

  public static long FillFirstN(int bitNumber)
  {
    long result = 0;

    for (byte i = 0; i < bitNumber; i++)
    {
      result |= 1L << i;
    }

    return result;
  }

  public static long FillLastN(int bitNumberFromTop)
  {
    long n = FillFirstN(bitNumberFromTop);
    int shiftLength = Environment.ProcessorCount - bitNumberFromTop;
    return n << shiftLength;
  }

  public static long FromCustom(long customMask)
  {
    var clearUnallowedBitsMask = FillFirstN((byte)Environment.ProcessorCount);
    return clearUnallowedBitsMask & customMask;
  }
}
