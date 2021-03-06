using System.Collections.Generic;
using Server.Commands.Generic;
using Server.Items;
using Server.Targeting;
using System.Globalization;

namespace Server.Commands
{
  public class LocationCommand : BaseCommand
  {
    private static readonly List<int> m_DefaultGraphics = new List<int>{ 0x17AF, 0x17B0, 0x17B1, 0x17B2 };

    public LocationCommand()
    {
      AccessLevel = AccessLevel.Counselor;
      Commands = new[] { "Location", "Loc", "Pos" };
      ObjectTypes = ObjectTypes.All;
      Supports = CommandSupport.Single | CommandSupport.Multi;
      Usage = "Location [itemIds ...]";
      Description = "Retrieves the positional coordinates of a target. Displaying a message above the item, "
                    + "mobile, or environment. Environment targets will also have a graphic temporarily displayed "
                    + "on the tile. This graphic can be changed to the provided list of itemIds.";
    }

    public override void Execute(CommandEventArgs e, object obj)
    {
      if (!(obj is IPoint3D point))
      {
        LogFailure("That cannot be located.");
        return;
      }

      var label = $"(x:{point.X}, y:{point.Y}, z:{point.Z})";
      if (obj is LandTarget || obj is StaticTarget)
      {
        List<int> graphics;
        if (e.Arguments.Length == 0)
          graphics = m_DefaultGraphics;
        else
        {
          graphics = new List<int>();
          foreach (var arg in e.Arguments)
          {
            var numberStyles = arg.ToLower().StartsWith("0x") ? NumberStyles.HexNumber : NumberStyles.Integer;

            if (int.TryParse(arg, numberStyles, CultureInfo.InvariantCulture, out int result))
              graphics.Add(result);
          }
        }

        var item = EffectItem.Create(new Point3D(point), e.Mobile.Map, EffectItem.DefaultDuration);
        foreach (int graphic in graphics)
          Effects.SendLocationParticles(item, graphic, 10, 50, 2023);

        item.LabelTo(e.Mobile, label);
      }
      else if (obj is Mobile entity) entity.SayTo(e.Mobile, label);
      else if (obj is Item item) item.LabelTo(e.Mobile, label);

      AddResponse($"Location: {label}");
    }
  }
}
