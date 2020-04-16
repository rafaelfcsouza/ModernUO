using Server.Engines.ConPVP;
using Server.Spells;
using Server.Spells.Necromancy;

namespace Server.Items
{
  public class CureLevelInfo
  {
    public CureLevelInfo(Poison poison, double chance)
    {
      Poison = poison;
      Chance = chance;
    }

    public Poison Poison { get; }

    public double Chance { get; }
  }

  public abstract class BaseCurePotion : BasePotion
  {
    public BaseCurePotion(PotionEffect effect) : base(0xF07, effect)
    {
    }

    public BaseCurePotion(Serial serial) : base(serial)
    {
    }

    public abstract CureLevelInfo[] LevelInfo { get; }

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

    public void DoCure(Mobile from)
    {
      bool cure = false;

      CureLevelInfo[] info = LevelInfo;

      for (int i = 0; i < info.Length; ++i)
      {
        CureLevelInfo li = info[i];

        if (li.Poison == from.Poison && Scale(from, li.Chance) > Utility.RandomDouble())
        {
          cure = true;
          break;
        }
      }

      if (cure && from.CurePoison(from))
      {
        from.SendLocalizedMessage(500231); // You feel cured of poison!

        from.FixedEffect(0x373A, 10, 15);
        from.PlaySound(0x1E0);
      }
      else if (!cure)
      {
        from.SendLocalizedMessage(500232); // That potion was not strong enough to cure your ailment!
      }
    }

    public override void Drink(Mobile from)
    {
      if (TransformationSpellHelper.UnderTransformation(from, typeof(VampiricEmbraceSpell)))
      {
        from.SendLocalizedMessage(1061652); // The garlic in the potion would surely kill you.
      }
      else if (from.Poisoned)
      {
        DoCure(from);

        PlayDrinkEffect(from);

        from.FixedParticles(0x373A, 10, 15, 5012, EffectLayer.Waist);
        from.PlaySound(0x1E0);

        if (!DuelContext.IsFreeConsume(from))
          Consume();
      }
      else
      {
        from.SendLocalizedMessage(1042000); // You are not poisoned.
      }
    }
  }
}