using System.Collections.Generic;

namespace GruetzeToaster;

public enum SpriteKind { Toaster, Toast, Logo, C64, Amiga, Floppy }

public static class SpriteCatalog
{
    // Computers join the configured total rather than adding extra objects.
    public static IReadOnlyList<SpriteKind> GetSpawnPool(ToasterSettings settings)
    {
        var pool = new List<SpriteKind>
        {
            SpriteKind.Toaster, SpriteKind.Toaster, SpriteKind.Toaster,
            SpriteKind.Toaster, SpriteKind.Toaster, SpriteKind.Toaster,
            SpriteKind.Toaster, SpriteKind.Toaster, SpriteKind.Toaster,
            SpriteKind.Toaster, SpriteKind.Toaster, SpriteKind.Toaster,
            SpriteKind.Toast, SpriteKind.Toast, SpriteKind.Toast, SpriteKind.Toast,
            SpriteKind.Floppy, SpriteKind.Floppy, SpriteKind.Floppy, SpriteKind.Floppy,
            SpriteKind.Logo
        };
        if (settings.ShowC64) pool.Add(SpriteKind.C64);
        if (settings.ShowAmiga) pool.Add(SpriteKind.Amiga);
        return pool;
    }
}
