using System.Collections.Generic;

namespace Server.Mobiles
{
  public class VarietyDealer : BaseVendor
  {
    private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();

    [Constructible]
    public VarietyDealer() : base("the variety dealer")
    {
    }

    public VarietyDealer(Serial serial) : base(serial)
    {
    }

    protected override List<SBInfo> SBInfos => m_SBInfos;

    public override void InitSBInfo()
    {
      m_SBInfos.Add(new SBVarietyDealer());
    }

    public override void Serialize(IGenericWriter writer)
    {
      base.Serialize(writer);

      writer.Write(0); // version
    }

    public override void Deserialize(IGenericReader reader)
    {
      base.Deserialize(reader);

      int version = reader.ReadInt();
    }
  }
}