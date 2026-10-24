using System.Collections.Generic;

namespace GruetzeToaster;

public enum SpriteKind { Toaster, Toast, Logo, C64, Amiga, Floppy }

public static class SpriteCatalog
{
    // Computers join the configured total rather than adding extra objects.
    public static IReadOnlyList<SpriteKind> GetSpawnPool(ToasterSettings settings)
    {
        var pool = new List<SpriteKind>();
        if (settings.ShowToasters) for (int i = 0; i < 12; i++) pool.Add(SpriteKind.Toaster);
        if (settings.ShowToast) for (int i = 0; i < 4; i++) pool.Add(SpriteKind.Toast);
        if (settings.ShowFloppies) for (int i = 0; i < 4; i++) pool.Add(SpriteKind.Floppy);
        if (settings.ShowLogo) pool.Add(SpriteKind.Logo);
        if (settings.ShowC64) pool.Add(SpriteKind.C64);
        if (settings.ShowAmiga) pool.Add(SpriteKind.Amiga);
        return pool;
    }
}
