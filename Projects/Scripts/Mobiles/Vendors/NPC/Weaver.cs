using System;
using System.Collections.Generic;
using Server.Engines.BulkOrders;

namespace Server.Mobiles
{
  public class Weaver : BaseVendor
  {
    private List<SBInfo> m_SBInfos = new List<SBInfo>();

    [Constructible]
    public Weaver() : base("the weaver")
    {
      SetSkill(SkillName.Tailoring, 65.0, 88.0);
    }

    public Weaver(Serial serial) : base(serial)
    {
    }

    protected override List<SBInfo> SBInfos => m_SBInfos;

    public override NpcGuild NpcGuild => NpcGuild.TailorsGuild;

    public override VendorShoeType ShoeType => VendorShoeType.Sandals;

    public override void InitSBInfo()
    {
      m_SBInfos.Add(new SBWeaver());
    }

    public override void Serialize(GenericWriter writer)
    {
      base.Serialize(writer);

      writer.Write(0); // version
    }

    public override void Deserialize(GenericReader reader)
    {
      base.Deserialize(reader);

      int version = reader.ReadInt();
    }

    #region Bulk Orders

    public override Item CreateBulkOrder(Mobile from, bool fromContextMenu)
    {
      if (from is PlayerMobile pm && pm.NextTailorBulkOrder == TimeSpan.Zero && (fromContextMenu || 0.2 > Utility.RandomDouble()))
      {
        double theirSkill = pm.Skills.Tailoring.Base;

        if (theirSkill >= 70.1)
          pm.NextTailorBulkOrder = TimeSpan.FromHours(6.0);
        else if (theirSkill >= 50.1)
          pm.NextTailorBulkOrder = TimeSpan.FromHours(2.0);
        else
          pm.NextTailorBulkOrder = TimeSpan.FromHours(1.0);

        if (theirSkill >= 70.1 && (theirSkill - 40.0) / 300.0 > Utility.RandomDouble())
          return new LargeTailorBOD();

        return SmallTailorBOD.CreateRandomFor(from);
      }

      return null;
    }

    public override bool IsValidBulkOrder(Item item)
    {
      return item is SmallTailorBOD || item is LargeTailorBOD;
    }

    public override bool SupportsBulkOrders(Mobile from)
    {
      return from is PlayerMobile && from.Skills.Tailoring.Base > 0;
    }

    public override TimeSpan GetNextBulkOrder(Mobile from)
    {
      if (from is PlayerMobile mobile)
        return mobile.NextTailorBulkOrder;

      return TimeSpan.Zero;
    }

    public override void OnSuccessfulBulkOrderReceive(Mobile from)
    {
      if (Core.SE && from is PlayerMobile mobile)
        mobile.NextTailorBulkOrder = TimeSpan.Zero;
    }

    #endregion
  }
}