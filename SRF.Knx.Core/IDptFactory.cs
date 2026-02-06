using System;
using SRF.Knx.Core.DPT;

namespace SRF.Knx.Core;

public interface IDptFactory
{
    DptBase Get(int main, int sub);
}
