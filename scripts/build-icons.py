"""Encode the rendered propeller-toaster master as platform icons. Requires Pillow."""
from pathlib import Path
from PIL import Image

root = Path(__file__).resolve().parent.parent
assets = root / "GruetzeToaster" / "Assets"
with Image.open(root / "branding" / "gruetzetoaster.png") as logo:
    frame = logo.convert("RGBA")

def sized(size):
    return frame.resize((size, size), Image.Resampling.LANCZOS)

sized(256).save(assets / "app-icon.png")
sized(1024).save(assets / "app-logo.png")
ico_sizes = (16, 24, 32, 48, 64, 128, 256)
sized(256).save(assets / "app-icon.ico", sizes=[(s, s) for s in ico_sizes],
                append_images=[sized(s) for s in ico_sizes[:-1]])
sized(1024).save(assets / "app-icon.icns",
                 append_images=[sized(s) for s in (16, 32, 64, 128, 256, 512)])
print("Created app-logo.png and PNG/ICO/ICNS icons from the propeller toaster master.")
