"""Encode the project owner's existing logo computer as platform icons. Requires Pillow."""
from pathlib import Path
from PIL import Image

assets = Path(__file__).resolve().parent.parent / "GruetzeToaster" / "Assets"
with Image.open(assets / "logo.gif") as logo:
    frame = logo.convert("RGBA")

def sized(size):
    return frame.resize((size, size), Image.Resampling.NEAREST)

sized(256).save(assets / "app-icon.png")
ico_sizes = (16, 24, 32, 48, 64, 128, 256)
sized(256).save(assets / "app-icon.ico", sizes=[(s, s) for s in ico_sizes],
                append_images=[sized(s) for s in ico_sizes[:-1]])
sized(1024).save(assets / "app-icon.icns",
                 append_images=[sized(s) for s in (16, 32, 64, 128, 256, 512)])
print("Created app-icon.png, app-icon.ico and app-icon.icns from the owner's logo computer.")
